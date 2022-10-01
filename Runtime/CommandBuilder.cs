#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
#define HAS_CONTEXT_RENDERING
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if VERTX_URP
using UnityEngine.Rendering.Universal;
#endif
using Vertx.Debugging.PlayerLoop;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging.PlayerLoop
{
	public struct VertxDebugging { }
}

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		private const string ProfilerName = "Vertx.Debugging";
		private const string RemoveShapesByDurationProfilerName = ProfilerName + " " + nameof(RemoveShapesByDuration);
		private const string FillCommandBufferProfilerName = ProfilerName + " " + nameof(FillCommandBuffer);
		private const string ExecuteProfilerName = ProfilerName + " Execute";

		internal static CommandBuilder Instance { get; }

		private readonly int _unityMatrixVPKey = Shader.PropertyToID("unity_MatrixVP");
		private readonly BufferGroup _defaultGroup = new BufferGroup(true, "Vertx.Debugging");
		private readonly BufferGroup _gizmosGroup = new BufferGroup(false, "Vertx.Debugging.Gizmos");
		private Camera _lastRenderingCamera;

		internal TextDataLists DefaultTexts => _defaultGroup.Texts;
		internal TextDataLists GizmoTexts => _gizmosGroup.Texts;
		
		private sealed class BufferGroup : IDisposable
		{
			private readonly string _commandBufferName;
			public readonly ShapeBuffersWithData<Shapes.Line> Lines;
			public readonly ShapeBuffersWithData<Shapes.Arc> Arcs;
			public readonly ShapeBuffersWithData<Shapes.Box> Boxes;
			public readonly ShapeBuffersWithData<Shapes.Box2D> Box2Ds;
			public readonly ShapeBuffersWithData<Shapes.Outline> Outlines;
			public readonly ShapeBuffersWithData<Shapes.Cast> Casts;
			public readonly TextDataLists Texts = new TextDataLists();

			private CommandBuffer _commandBuffer;

			public BufferGroup(bool usesDurations, string commandBufferName)
			{
				_commandBufferName = commandBufferName;
				Lines = new ShapeBuffersWithData<Shapes.Line>("line_buffer", usesDurations);
				Arcs = new ShapeBuffersWithData<Shapes.Arc>("arc_buffer", usesDurations);
				Boxes = new ShapeBuffersWithData<Shapes.Box>("box_buffer", usesDurations);
				Box2Ds = new ShapeBuffersWithData<Shapes.Box2D>("mesh_buffer", usesDurations);
				Outlines = new ShapeBuffersWithData<Shapes.Outline>("outline_buffer", usesDurations);
				Casts = new ShapeBuffersWithData<Shapes.Cast>("cast_buffer", usesDurations);
			}
			
			public CommandBuffer ReadyResources()
			{
				if (_commandBuffer == null)
				{
					_commandBuffer = new CommandBuffer
					{
						name = _commandBufferName
					};
				}
				else 
					_commandBuffer.Clear();

				return _commandBuffer;
			}

			public void Clear()
			{
				Lines.Clear();
				Arcs.Clear();
				Boxes.Clear();
				Box2Ds.Clear();
				Outlines.Clear();
				Casts.Clear();
				Texts.Clear();
			}

			public void Dispose()
			{
				Lines.Dispose();
				Arcs.Dispose();
				Boxes.Dispose();
				Box2Ds.Dispose();
				Outlines.Dispose();
				Casts.Dispose();
				_commandBuffer?.Dispose();
			}
		}
		
#if VERTX_URP
		private VertxDebuggingRendererFeature _pass;
#endif
		private bool _disposeIsQueued;
		private float _timeThisFrame;

		static CommandBuilder() => Instance = new CommandBuilder();

		private CommandBuilder()
		{
			Camera.onPostRender += OnPostRender;
#if HAS_CONTEXT_RENDERING
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
			RenderPipelineManager.endContextRendering += OnEndContextRendering;
#else
			RenderPipelineManager.beginFrameRendering += (context, cameras) =>
			{
				
#if !UNITY_2021_1_OR_NEWER
				using (Vertx.Debugging.Internal.ListPool<Camera>.Get(out var list))
#else
				using (UnityEngine.Pool.ListPool<Camera>.Get(out var list))
#endif
				{
					list.AddRange(cameras);
					OnBeginContextRendering(context, list);
				}
			};
			RenderPipelineManager.endFrameRendering += (context, cameras) => OnEndContextRendering(context, null);
#endif
			EditorApplication.update = OnUpdate + EditorApplication.update;
		}

		[InitializeOnLoadMethod, RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitialiseUpdate()
		{
			// Queue RuntimeEarlyUpdate into the EarlyUpdate portion of the player loop.

			PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subsystems = playerLoop.subSystemList.ToArray();
			Type earlyUpdate = typeof(EarlyUpdate);
			InjectFirstIn(earlyUpdate, typeof(VertxDebugging), Instance.EarlyUpdate);

			playerLoop.subSystemList = subsystems;
			UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);

			void InjectFirstIn(Type type, Type actionType, PlayerLoopSystem.UpdateFunction action)
			{
				for (int i = 0; i < subsystems.Length; i++)
				{
					if (subsystems[i].type != type)
						continue;

					var earlyUpdateSystem = subsystems[i];
					PlayerLoopSystem[] source = earlyUpdateSystem.subSystemList;
					for (int j = 0; j < source.Length; j++)
					{
						// Already appended time callback.
						if (source[j].type == actionType)
							return;
					}

					PlayerLoopSystem[] dest = new PlayerLoopSystem[source.Length + 1];
					Array.Copy(source, 0, dest, 1, source.Length);
					dest[0] = new PlayerLoopSystem
					{
						type = actionType,
						updateDelegate = action
					};
					subsystems[i].subSystemList = dest;
				}
			}
		}

		private void EarlyUpdate()
		{
			UpdateContext.ForceStateToUpdate();
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (Time.deltaTime == 0)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				RemoveShapesByDuration(Time.deltaTime, null);
			}

			_timeThisFrame = Time.time;
		}

		private static bool CombineDependencies(ref JobHandle? handle, JobHandle? other)
		{
			if (!other.HasValue)
				return false;
			handle = handle.HasValue
				? JobHandle.CombineDependencies(handle.Value, other.Value)
				: other.Value;
			return true;
		}

		/// <summary>
		/// Remove data where the duration has been met.
		/// </summary>
		private void RemoveShapesByDuration(float deltaTime, JobHandle? dependency)
		{
			// _lastRemovedEditorFrame = _editorFrame;

			Profiler.BeginSample(RemoveShapesByDurationProfilerName);
			
			_defaultGroup.Texts.RemoveByDeltaTime(deltaTime);
			
			int oldLineCount = QueueRemovalJob(_defaultGroup.Lines, dependency, out JobHandle? lineHandle);
			int oldArcCount = QueueRemovalJob(_defaultGroup.Arcs, dependency, out JobHandle? arcHandle);
			int oldBoxCount = QueueRemovalJob(_defaultGroup.Boxes, dependency, out JobHandle? boxHandle);
			int oldBox2DCount = QueueRemovalJob(_defaultGroup.Box2Ds, dependency, out JobHandle? box2DHandle);
			int oldOutlineCount = QueueRemovalJob(_defaultGroup.Outlines, dependency, out JobHandle? outlineHandle);
			int oldMatrixAndVectorsCount = QueueRemovalJob(_defaultGroup.Casts, dependency, out JobHandle? castsHandle);

			JobHandle? coreHandle = null;
			if (!CombineDependencies(ref coreHandle, lineHandle) & // Purposely an &, so each branch gets executed.
			    !CombineDependencies(ref coreHandle, arcHandle) &
			    !CombineDependencies(ref coreHandle, boxHandle) &
			    !CombineDependencies(ref coreHandle, box2DHandle) &
			    !CombineDependencies(ref coreHandle, outlineHandle) &
			    !CombineDependencies(ref coreHandle, castsHandle))
				coreHandle = dependency;

			if (coreHandle.HasValue)
			{
				coreHandle.Value.Complete();

				if (_defaultGroup.Lines.Count != oldLineCount)
					_defaultGroup.Lines.SetDirty();

				if (_defaultGroup.Arcs.Count != oldArcCount)
					_defaultGroup.Arcs.SetDirty();

				if (_defaultGroup.Boxes.Count != oldBoxCount)
					_defaultGroup.Boxes.SetDirty();

				if (_defaultGroup.Box2Ds.Count != oldBox2DCount)
					_defaultGroup.Box2Ds.SetDirty();

				if (_defaultGroup.Outlines.Count != oldOutlineCount)
					_defaultGroup.Outlines.SetDirty();
				
				if (_defaultGroup.Casts.Count != oldMatrixAndVectorsCount)
					_defaultGroup.Casts.SetDirty();
			}

			int QueueRemovalJob<T>(ShapeBuffersWithData<T> data, JobHandle? handleIn, out JobHandle? handleOut) where T : unmanaged
			{
				int length = data.Count;
				if (length == 0)
				{
					handleOut = null;
					return 0;
				}

				var removalJob = new RemovalJob<T>
				{
					Elements = data.InternalList,
					Durations = data.DurationsInternalList,
					Modifications = data.ModificationsInternalList,
					Colors = data.ColorsInternalList,
					DeltaTime = deltaTime
				};
				handleOut = removalJob.Schedule(handleIn ?? default);
				return length;
			}

			Profiler.EndSample();
		}

#if UNITY_2021_1_OR_NEWER
		[Unity.Burst.BurstCompile]
#endif
		private struct RemovalJob<T> : IJob where T : unmanaged
		{
			public NativeList<T> Elements;
			public NativeList<float> Durations;
			public NativeList<Shapes.DrawModifications> Modifications;
			public NativeList<Color> Colors;
			public float DeltaTime;

			/// <summary>
			/// Removes indices in an unordered fashion,
			/// But the removals happen identically across the buffers, so this is not a concern.
			/// </summary>
			public void Execute()
			{
				for (int index = Elements.Length - 1; index >= 0; index--)
				{
					float oldDuration = Durations[index];
					float newDuration = oldDuration - DeltaTime;
					if (newDuration > 0)
					{
						Durations[index] = newDuration;
						// ! Remember to change this when swapping between IJob and IJobFor
						continue;
					}

					// RemoveUnorderedAt, shared logic:
					int endIndex = Durations.Length - 1;

					Durations[index] = Durations[endIndex];
					Durations.RemoveAt(endIndex);

					Elements[index] = Elements[endIndex];
					Elements.RemoveAt(endIndex);

					Modifications[index] = Modifications[endIndex];
					Modifications.RemoveAt(endIndex);

					Colors[index] = Colors[endIndex];
					Colors.RemoveAt(endIndex);
				}
			}
		}

		private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
		{
#if VERTX_URP
			if (RenderPipelineUtility.Pipeline != CurrentPipeline.URP)
				return;

			foreach (Camera camera in cameras)
			{
				UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
				if (cameraData == null)
					continue;

				ScriptableRenderer renderer = cameraData.scriptableRenderer;
				if (_pass == null)
					_pass = ScriptableObject.CreateInstance<VertxDebuggingRendererFeature>();

				_pass.AddRenderPasses(renderer);
			}
#endif
		}

		private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
		{
			// If `cameras` becomes used, change subscription to this method.
		}

		private void OnUpdate()
		{
			// TODO cleanup if things aren't running and stuff is getting out of hand...
		}

		private void OnPostRender(Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;
			
			if (!SharedRenderingDetails(camera, _defaultGroup, out CommandBuffer commandBuffer, type))
				return;
			Profiler.BeginSample(ExecuteProfilerName);
			Graphics.ExecuteCommandBuffer(commandBuffer);
			Profiler.EndSample();
		}

		internal void ExecuteDrawRenderPass(ScriptableRenderContext context, Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;
			
			if (!SharedRenderingDetails(camera, _defaultGroup, out CommandBuffer commandBuffer, type))
				return;
			Profiler.BeginSample(ExecuteProfilerName);
			context.ExecuteCommandBuffer(commandBuffer);
			Profiler.EndSample();
		}

		internal void RenderGizmosGroup(bool isSceneView)
		{
			RenderingType type = RenderingType.Gizmos;
			if (isSceneView)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;
			
			if (!SharedRenderingDetails(_lastRenderingCamera, _gizmosGroup, out CommandBuffer commandBuffer, type))
				return;
			Profiler.BeginSample(ExecuteProfilerName);
			Graphics.ExecuteCommandBuffer(commandBuffer);
			Profiler.EndSample();
		}
		
		internal void ClearGizmoGroup() => _gizmosGroup.Clear();

		[Flags]
		private enum RenderingType
		{
			Unknown = 0,
			// Call origin
			Default = 1,
			Gizmos = 1 << 1,
			// Rendering view
			Scene = 1 << 2,
			Game = 1 << 3,
			// -
			GizmosAndGame = Gizmos | Game
		}
		
		private bool SharedRenderingDetails(Camera camera, BufferGroup group, out CommandBuffer commandBuffer, RenderingType renderingType)
		{
			_lastRenderingCamera = camera;
			UpdateContext.ForceStateToUpdate();
			
			if (!ShouldRenderCamera(camera, renderingType))
			{
				commandBuffer = null;
				return false;
			}

			InitialiseIfRequired();
			commandBuffer = group.ReadyResources();
			return FillCommandBuffer(commandBuffer, camera, group, renderingType);
		}

		private static bool ShouldRenderCamera(Camera camera, RenderingType renderingType)
		{
			if (!Handles.ShouldRenderGizmos())
				return false;

			if ((renderingType & RenderingType.Gizmos) != 0)
				return true;

			bool isRenderingSceneView = (renderingType & RenderingType.Scene) != 0;

			// Don't render cameras that render render textures. Always render scene view cameras.
			if (!isRenderingSceneView && camera.targetTexture != null)
				return false;

			return true;
		}

		private bool FillCommandBuffer(CommandBuffer commandBuffer, Camera camera, BufferGroup group, RenderingType renderingType)
		{
			Profiler.BeginSample(FillCommandBufferProfilerName);

			bool render;
			if (renderingType == RenderingType.GizmosAndGame)
			{
				Matrix4x4 oldMatrix = Shader.GetGlobalMatrix(_unityMatrixVPKey);
				commandBuffer.SetGlobalMatrix(_unityMatrixVPKey, GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix);
				RenderShapes();
				commandBuffer.SetGlobalMatrix(_unityMatrixVPKey, oldMatrix);
			}
			else
			{
				RenderShapes();
			}


			void RenderShapes()
			{
				render = RenderShape(AssetsUtility.Line, AssetsUtility.LineMaterial, group.Lines);
				render |= RenderShape(AssetsUtility.Circle, AssetsUtility.ArcMaterial, group.Arcs);
				render |= RenderShape(AssetsUtility.Box, AssetsUtility.BoxMaterial, group.Boxes);
				render |= RenderShape(AssetsUtility.Box2D, AssetsUtility.DefaultMaterial, group.Box2Ds);
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.OutlineMaterial, group.Outlines);
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.CastMaterial, group.Casts);

				bool RenderShape<T>(
					AssetsUtility.Asset<Mesh> mesh,
					AssetsUtility.Asset<Material> material,
					ShapeBuffersWithData<T> shape) where T : unmanaged
				{
					int shapeCount = shape.Count;
					if (shapeCount <= 0)
						return false;

					MaterialPropertyBlock propertyBlock = shape.PropertyBlock;
					// Set the buffers to be used by the property block
					// Synchronise the GraphicsBuffer with the data in the line buffer.
					shape.Set(commandBuffer, propertyBlock);

					// Render boxes
					commandBuffer.DrawMeshInstancedProcedural(mesh.Value, 0, material.Value, -1, shapeCount, propertyBlock);
					return true;
				}
			}

			Profiler.EndSample();
			return render;
		}

		private static bool IsInFixedUpdate()
#if UNITY_2020_3_OR_NEWER
			=> Time.inFixedTimeStep;
#else
			=> Time.deltaTime == Time.fixedDeltaTime;
#endif

		private float GetDuration(float duration)
		{
			// Calls from FixedUpdate should hang around until the next FixedUpdate, at minimum.
			if (IsInFixedUpdate() && duration < Time.fixedDeltaTime)
			{
				// Time from the last 
				// ReSharper disable once ArrangeRedundantParentheses
				duration += (Time.fixedTime + Time.fixedDeltaTime) - _timeThisFrame;
			}

			return duration;
		}

		private void InitialiseIfRequired()
		{
			if (_disposeIsQueued) return;
			_disposeIsQueued = true;
			AssemblyReloadEvents.beforeAssemblyReload += Dispose;
		}

		private void Dispose()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= Dispose;

			_defaultGroup?.Dispose();
			_gizmosGroup?.Dispose();

#if VERTX_URP
			if (_pass != null)
				UnityEngine.Object.DestroyImmediate(_pass, true);
#endif
		}
	}
}
#endif