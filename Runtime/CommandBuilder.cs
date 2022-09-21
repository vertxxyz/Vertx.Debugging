#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
#define HAS_CONTEXT_RENDERING
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		private const string ProfilerName = "Vertx.Debugging";
		private const string RuntimeEarlyUpdateProfilerName = ProfilerName + " " + nameof(RuntimeEarlyUpdate);
		private const string FillCommandBufferProfilerName = ProfilerName + " " + nameof(FillCommandBuffer);
		private const string ExecuteProfilerName = ProfilerName + " Execute";

		public static CommandBuilder Instance { get; }

		private CommandBuffer _commandBuffer;
		private readonly ShapeBuffersWithData<Shapes.Line> _lines = new ShapeBuffersWithData<Shapes.Line>("line_buffer");
		private readonly ShapeBuffersWithData<Shapes.Arc> _arcs = new ShapeBuffersWithData<Shapes.Arc>("arc_buffer");
		private readonly ShapeBuffersWithData<Shapes.Box> _boxes = new ShapeBuffersWithData<Shapes.Box>("box_buffer");
		private readonly ShapeBuffersWithData<Shapes.Box2D> _box2Ds = new ShapeBuffersWithData<Shapes.Box2D>("box_buffer");
		private readonly ShapeBuffersWithData<Shapes.Outline> _outlines = new ShapeBuffersWithData<Shapes.Outline>("outline_buffer");
		private bool _queuedDispose;
		private VertxDebuggingRendererFeature _pass;
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
				using (UnityEngine.Pool.ListPool<Camera>.Get(out var list))
				{
					list.AddRange(cameras);
					OnBeginContextRendering(context, list);
				}
			};
			RenderPipelineManager.endFrameRendering += (context, cameras) => OnEndContextRendering(context, null);
#endif
			EditorApplication.update += OnUpdate;
		}

		private struct VertxDebugging { }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitialiseRuntime()
		{
			// Queue RuntimeEarlyUpdate into the EarlyUpdate portion of the player loop.

			PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subsystems = playerLoop.subSystemList.ToArray();
			Type earlyUpdate = typeof(EarlyUpdate);
			InjectFirstIn(earlyUpdate, typeof(VertxDebugging), Instance.RuntimeEarlyUpdate);

			playerLoop.subSystemList = subsystems;
			PlayerLoop.SetPlayerLoop(playerLoop);

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
					;
					subsystems[i].subSystemList = dest;
				}
			}
		}

		private void RuntimeEarlyUpdate()
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (Time.deltaTime == 0)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				Profiler.BeginSample(RuntimeEarlyUpdateProfilerName);
				RemoveShapesByDuration(Time.deltaTime, null);
				_timeThisFrame = Time.time;
				Profiler.EndSample();
			}
		}

		private void ClearAllShapes()
		{
			_lines.Clear();
			_arcs.Clear();
			_boxes.Clear();
			_box2Ds.Clear();
			_outlines.Clear();
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
			int oldLineCount = QueueRemovalJob(_lines, dependency, out JobHandle? lineHandle);
			int oldArcCount = QueueRemovalJob(_arcs, dependency, out JobHandle? arcHandle);
			int oldBoxCount = QueueRemovalJob(_boxes, dependency, out JobHandle? boxHandle);
			int oldBox2DCount = QueueRemovalJob(_box2Ds, dependency, out JobHandle? box2DHandle);
			int oldOutlineCount = QueueRemovalJob(_outlines, dependency, out JobHandle? outlineHandle);

			JobHandle? coreHandle = null;
			if (!CombineDependencies(ref coreHandle, lineHandle) & // Purposely an &, so each branch gets executed.
			    !CombineDependencies(ref coreHandle, arcHandle) &
			    !CombineDependencies(ref coreHandle, boxHandle) &
			    !CombineDependencies(ref coreHandle, box2DHandle) &
			    !CombineDependencies(ref coreHandle, outlineHandle))
				coreHandle = dependency;

			if (coreHandle.HasValue)
			{
				coreHandle.Value.Complete();

				if (_lines.Count != oldLineCount)
					_lines.SetDirty();

				if (_arcs.Count != oldArcCount)
					_arcs.SetDirty();

				if (_boxes.Count != oldBoxCount)
					_boxes.SetDirty();
				
				if (_box2Ds.Count != oldBox2DCount)
					_box2Ds.SetDirty();
				
				if(_outlines.Count != oldOutlineCount)
					_outlines.SetDirty();
			}

			int QueueRemovalJob<T>(ShapeBuffersWithData<T> data, JobHandle? handleIn, out JobHandle? handleOut) where T : unmanaged
			{
				int length = data.Count;
				if (length == 0)
				{
					handleOut = null;
					return length;
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
		}

		[BurstCompile]
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

		private void OnUpdate() { }

		private void OnPostRender(Camera camera)
		{
			if (!SharedRenderingDetails(camera))
				return;
			Profiler.BeginSample(ExecuteProfilerName);
			Graphics.ExecuteCommandBuffer(_commandBuffer);
			Profiler.EndSample();
		}

		public void ExecuteDrawRenderPass(ScriptableRenderContext context, Camera camera)
		{
			if (!SharedRenderingDetails(camera))
				return;
			Profiler.BeginSample(ExecuteProfilerName);
			context.ExecuteCommandBuffer(_commandBuffer);
			Profiler.EndSample();
		}

		private bool SharedRenderingDetails(Camera camera)
		{
			if (!ShouldRenderCamera(camera))
				return false;

			InitialiseIfRequired();

			if (_commandBuffer == null)
			{
				_commandBuffer = new CommandBuffer
				{
					name = "Vertx.Debugging"
				};
			}
			else
				_commandBuffer.Clear();

			FillCommandBuffer(_commandBuffer, camera);
			return true;
		}

		private static bool ShouldRenderCamera(Camera camera)
		{
			if (!Handles.ShouldRenderGizmos())
				return false;

			bool isRenderingSceneView = SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera;

			// Don't render cameras that render render textures. Always render scene view cameras.
			if (!isRenderingSceneView && camera.targetTexture != null)
				return false;

			return true;
		}

		private void FillCommandBuffer(CommandBuffer commandBuffer, Camera camera)
		{
			Profiler.BeginSample(RuntimeEarlyUpdateProfilerName);
			RenderShape(AssetsUtility.Line, AssetsUtility.LineMaterial, _lines);
			RenderShape(AssetsUtility.Circle, AssetsUtility.ArcMaterial, _arcs);
			RenderShape(AssetsUtility.Box, AssetsUtility.BoxMaterial, _boxes);
			RenderShape(AssetsUtility.Box2D, AssetsUtility.DefaultMaterial, _box2Ds);
			RenderShape(AssetsUtility.Line, AssetsUtility.OutlineMaterial, _outlines);

			void RenderShape<T>(
				AssetsUtility.Asset<Mesh> mesh,
				AssetsUtility.Asset<Material> material,
				ShapeBuffersWithData<T> shape) where T : unmanaged
			{
				int shapeCount = shape.Count;
				if (shapeCount <= 0)
					return;

				MaterialPropertyBlock propertyBlock = shape.PropertyBlock;
				// Set the buffers to be used by the property block
				// Synchronise the GraphicsBuffer with the data in the line buffer.
				shape.Set(commandBuffer, propertyBlock);

				// Render boxes
				commandBuffer.DrawMeshInstancedProcedural(mesh.Value, 0, material.Value, -1, shapeCount, propertyBlock);
			}

			Profiler.EndSample();
		}

		public void AppendRay(Shapes.Ray ray, Color color, float duration) => AppendLine(new Shapes.Line(ray), color, duration);

		public void AppendLine(Shapes.Line line, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			duration = GetDuration(duration);
			if (duration < 0)
				return;
			InitialiseIfRequired();
			_lines.Add(line, color, modifications, duration);
		}

		public void AppendArc(Shapes.Arc arc, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			duration = GetDuration(duration);
			if (duration < 0)
				return;
			InitialiseIfRequired();
			_arcs.Add(arc, color, modifications, duration);
		}

		public void AppendBox(Shapes.Box box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			duration = GetDuration(duration);
			if (duration < 0)
				return;
			InitialiseIfRequired();
			_boxes.Add(box, color, modifications, duration);
		}
		
		public void AppendBox2D(Shapes.Box2D box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			duration = GetDuration(duration);
			if (duration < 0)
				return;
			InitialiseIfRequired();
			_box2Ds.Add(box, color, modifications, duration);
		}

		internal void AppendOutline(Shapes.Outline outline, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			duration = GetDuration(duration);
			if (duration < 0)
				return;
			InitialiseIfRequired();
			_outlines.Add(outline, color, modifications, duration);
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
			if (IsInFixedUpdate() && duration == 0)
				duration += Time.fixedDeltaTime - (_timeThisFrame - Time.fixedTime);

			return duration;
		}

		private void InitialiseIfRequired()
		{
			if (_queuedDispose) return;
			_queuedDispose = true;
			AssemblyReloadEvents.beforeAssemblyReload += Dispose;
		}

		private void Dispose()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= Dispose;

			_commandBuffer?.Dispose();
			_lines.Dispose();
			_arcs.Dispose();
			_boxes.Dispose();
			_box2Ds.Dispose();
			_outlines.Dispose();

			if (_pass != null)
				Object.DestroyImmediate(_pass, true);
		}
	}
}
#endif