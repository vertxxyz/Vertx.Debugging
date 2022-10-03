#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
#define HAS_CONTEXT_RENDERING
#endif
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if VERTX_URP
using UnityEngine.Rendering.Universal;
using Vertx.Debugging.Internal;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
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
		private const string GizmosProfilerName = ProfilerName + ".Gizmos";

		internal static CommandBuilder Instance { get; }

		private readonly int _unityMatrixVPKey = Shader.PropertyToID("unity_MatrixVP");
		private readonly BufferGroup _defaultGroup = new BufferGroup(true, ProfilerName);
		private readonly BufferGroup _gizmosGroup = new BufferGroup(false, GizmosProfilerName);
#if VERTX_URP || VERTX_HDRP
		private readonly ProfilingSampler _defaultProfilingSampler = new ProfilingSampler(ProfilerName);
		private readonly ProfilingSampler _gizmosProfilingSampler = new ProfilingSampler(GizmosProfilerName);
#endif
#if VERTX_URP
		private VertxDebuggingRenderPass _pass;
#endif
		private Camera _lastRenderingCamera;
		private bool _disposeIsQueued;

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

		private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
		{
#if VERTX_URP
			if (RenderPipelineUtility.Pipeline != CurrentPipeline.URP)
				return;

			if (_pass == null)
			{
				_pass = new VertxDebuggingRenderPass { renderPassEvent = RenderPassEvent.AfterRendering + 10 };
			}

			//_pass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

			foreach (Camera camera in cameras)
			{
				UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
				if (cameraData == null)
					continue;

				cameraData.scriptableRenderer.EnqueuePass(_pass);
			}
#endif
		}

		private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
		{
			// After cameras are rendered, we have collected gizmos and it is safe to render them.
			foreach (Camera camera in cameras)
			{
				RenderGizmosGroup(camera, SceneView.currentDrawingSceneView != null);
			}
		}

		private void OnPostRender(Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			Profiler.BeginSample(ProfilerName);
			if (!SharedRenderingDetails(camera, _defaultGroup, out CommandBuffer commandBuffer, type))
				return;
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
#if VERTX_URP || VERTX_HDRP
			// TODO fix profiling scope issues.
			// using (new ProfilingScope(commandBuffer, _defaultProfilingSampler))
#endif
			{
				context.ExecuteCommandBuffer(commandBuffer);
			}
		}

		// Called by Built-in (from OnGUI)
		internal void RenderGizmosGroup(bool sceneViewCamera) => RenderGizmosGroup(_lastRenderingCamera, sceneViewCamera);

		// Called by render pipelines (From EndContextRendering)
		private void RenderGizmosGroup(Camera camera, bool isSceneViewCamera)
		{
			UpdateContext.ForceStateToUpdate();
			RenderingType type = RenderingType.Gizmos;
			
			if (isSceneViewCamera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			if (!SharedRenderingDetails(camera, _gizmosGroup, out CommandBuffer commandBuffer, type))
				return;
#if VERTX_URP || VERTX_HDRP
			// TODO fix profiling scope issues.
			//using (new ProfilingScope(commandBuffer, _gizmosProfilingSampler))
#endif
			{
				Graphics.ExecuteCommandBuffer(commandBuffer);
			}
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

			return render;
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
		}
	}
}
#endif