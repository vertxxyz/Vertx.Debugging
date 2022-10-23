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
		internal const string Name = "Vertx.Debugging";
		internal const string GizmosName = Name + ".Gizmos";

		internal static CommandBuilder Instance { get; }

		private readonly int _unityMatrixVPKey = Shader.PropertyToID("unity_MatrixVP");
		private readonly BufferGroup _defaultGroup = new BufferGroup(true, Name);
		private readonly BufferGroup _gizmosGroup = new BufferGroup(false, GizmosName);
#if VERTX_URP
		private VertxDebuggingRenderPass _pass;
#endif
		private Camera _lastRenderingCamera;
		private bool _disposeIsQueued;
		// Application.isPlaying, but without the native code transition.
		private bool _isPlaying;
		// UnityEditor.EditorApplication.isPaused, but without the native code transition.
		private bool _isPaused;

		internal TextDataLists DefaultTexts => _defaultGroup.Texts;
		internal ScreenTextDataLists DefaultScreenTexts => _defaultGroup.ScreenTexts;
		internal TextDataLists GizmoTexts => _gizmosGroup.Texts;
		internal ScreenTextDataLists GizmoScreenTexts => _gizmosGroup.ScreenTexts;

		static CommandBuilder() => Instance = new CommandBuilder();

		private CommandBuilder()
		{
			Camera.onPostRender += OnPostRender;
			RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
#if HAS_CONTEXT_RENDERING
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
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
#endif
			EditorApplication.update = OnUpdate + EditorApplication.update;
			EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
			EditorApplication.pauseStateChanged -= EditorApplicationOnPauseStateChanged;
			EditorApplication.pauseStateChanged += EditorApplicationOnPauseStateChanged;
		}

		private void EditorApplicationOnPauseStateChanged(PauseState obj) => _isPaused = obj == PauseState.Paused;

		private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange obj)
		{
			_isPlaying = obj == PlayModeStateChange.EnteredPlayMode;
			if (obj != PlayModeStateChange.EnteredPlayMode && obj != PlayModeStateChange.EnteredEditMode)
				return;
			_defaultGroup.Clear();
			_gizmosGroup.Clear();
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

		private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
			// After cameras are rendered, we have collected gizmos and it is safe to render them.
			=> RenderGizmosGroup(camera, SceneView.currentDrawingSceneView != null ? RenderingType.Scene : RenderingType.Game);

		private void OnPostRender(Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			Profiler.BeginSample(Name);
			CommandBuffer commandBuffer = null;
			if (!SharedRenderingDetails(camera, _defaultGroup, ref commandBuffer, type))
			{
				Profiler.EndSample();
				return;
			}

			Graphics.ExecuteCommandBuffer(commandBuffer);
			Profiler.EndSample();
		}

		internal void ExecuteDrawRenderPass(ScriptableRenderContext context, CommandBuffer commandBuffer, Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			SharedRenderingDetails(camera, _defaultGroup, ref commandBuffer, type);
		}

		/// <summary>
		/// Called by Built-in (from OnGUI)
		/// </summary>
		internal void RenderGizmosGroup(bool sceneViewCamera) => RenderGizmosGroup(_lastRenderingCamera, sceneViewCamera ? RenderingType.Scene : RenderingType.Game);

		/// <summary>
		/// Called by render pipelines (From EndContextRendering)
		/// </summary>
		private void RenderGizmosGroup(Camera camera, RenderingType type)
		{
			type |= RenderingType.Gizmos;

			CommandBuffer commandBuffer = null;
			if (!SharedRenderingDetails(camera, _gizmosGroup, ref commandBuffer, type))
				return;
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}

		internal void ClearGizmoGroup() => _gizmosGroup.Clear();

		[Flags]
		private enum RenderingType
		{
			Unset = 0,
			// Call origin
			Default = 1,
			Gizmos = 1 << 1,
			// Rendering view
			Scene = 1 << 2,
			Game = 1 << 3,
			// -
			GizmosAndGame = Gizmos | Game
		}

		private bool SharedRenderingDetails(Camera camera, BufferGroup group, ref CommandBuffer commandBuffer, RenderingType renderingType)
		{
			_pauseCapture.CommitCurrentPausedFrame();
			_lastRenderingCamera = camera;
			UpdateContext.ForceStateToUpdate();

			if (!ShouldRenderCamera(camera, renderingType))
				return false;

			InitialiseDisposal();
			group.ReadyResources(ref commandBuffer);
			return FillCommandBuffer(commandBuffer, camera, group, renderingType);
		}

		private static bool ShouldRenderCamera(Camera camera, RenderingType renderingType)
		{
			if (!Handles.ShouldRenderGizmos())
				return false;

			// If we're rendering from a gizmo context, always render.
			if ((renderingType & RenderingType.Gizmos) != 0)
				return true;

			bool isRenderingSceneView = (renderingType & RenderingType.Scene) != 0;

			// Don't render cameras that render render textures. Always render scene view cameras.
			if (!isRenderingSceneView && camera.targetTexture != null)
				return false;

			return true;
		}

		/// <summary>
		/// The core rendering loop.
		/// </summary>
		/// <returns>True if relevant rendering commands were issued.</returns>
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
			
			// commandBuffer.SetGlobalDepthBias(-1, -1);

			void RenderShapes()
			{
				render = RenderShape(AssetsUtility.Line, AssetsUtility.LineMaterial, group.Lines, 128); // 256 vert target, 2 verts per line. 128 lines.
				render |= RenderShape(AssetsUtility.Circle, AssetsUtility.ArcMaterial, group.Arcs, 4); // 256 vert target, 64 verts per circle. 4 circles.
				render |= RenderShape(AssetsUtility.Box, AssetsUtility.BoxMaterial, group.Boxes, 11); // 256 vert target, 12 edges, 2 verts each. 11 boxes.
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.OutlineMaterial, group.Outlines, 128);
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.CastMaterial, group.Casts, 128);

				bool RenderShape<T>(
					AssetsUtility.Asset<Mesh> mesh,
					AssetsUtility.MaterialAsset material,
					ShapeBuffersWithData<T> shape,
					int groupCount = 1) where T : unmanaged
				{
					int shapeCount = shape.Count;
					if (shapeCount <= 0)
						return false;

					// Don't render this shape until it's compiled.
					// (It looks very strange to the user when a cyan box appears for a millisecond at 0,0,0)
					Material mat = material.Value;
					int passCount = mat.passCount;
					bool isCompiling = false;
					for (int i = 0; i < passCount; i++)
					{
						if (ShaderUtil.IsPassCompiled(mat, i))
							continue;
						if (ShaderUtil.anythingCompiling)
							return false;
						ShaderUtil.CompilePass(mat, i);
						isCompiling = true;
					}

					if (isCompiling)
						return false;

					MaterialPropertyBlock propertyBlock = shape.PropertyBlock;
					// Set the buffers to be used by the property block
					// Synchronise the GraphicsBuffer with the data in the shape buffer.
					shape.Set(commandBuffer, propertyBlock);
					commandBuffer.DrawMeshInstancedProcedural(mesh.Value, 0, mat, -1, Mathf.CeilToInt(shapeCount / (float)groupCount), propertyBlock);
					return true;
				}
			}

			return render;
		}

		/// <summary>
		/// Ensures Dispose is called for allocated resources.
		/// </summary>
		private void InitialiseDisposal()
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