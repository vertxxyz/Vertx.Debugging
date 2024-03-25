#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if VERTX_URP
using UnityEngine.Rendering.Universal;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging.PlayerLoop
{
	public struct VertxDebuggingInitialization
	{
	}

	public struct VertxDebuggingFixedUpdate
	{
	}
}

namespace Vertx.Debugging
{
	internal sealed partial class CommandBuilder
	{
		internal const string Name = "Vertx.Debugging";
		internal const string GizmosName = Name + ".Gizmos";

		internal static CommandBuilder Instance { get; }

		internal static readonly int s_InstanceCountKey = Shader.PropertyToID("_InstanceCount");
		private static readonly int s_UnityMatrixVPKey = Shader.PropertyToID("unity_MatrixVP");
		private static readonly int s_ZWriteKey = Shader.PropertyToID("_ZWrite");
		private static readonly int s_ZTestKey = Shader.PropertyToID("_ZTest");

		private BufferGroup _defaultGroup;
		private BufferGroup _gizmosGroup;

		internal BufferGroup GetDefaultBufferGroup() => _defaultGroup;
		internal BufferGroup GetGizmosBufferGroup() => _gizmosGroup;

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


		[InitializeOnLoadMethod]
		private static void Initialise()
		{
			InitialiseUpdate();
			CommandBuilder instance = Instance;
			ref UnmanagedCommandBuilder unmanaged = ref UnmanagedCommandBuilder.Instance.Data;
			unmanaged.Initialise();
			instance._defaultGroup = new BufferGroup(Name, unmanaged.Standard);
			instance._gizmosGroup = new BufferGroup(GizmosName, unmanaged.Gizmos);
		}

		private CommandBuilder()
		{
			Camera.onPostRender += OnPostRender;
			RenderPipelineManager.endContextRendering += OnEndContextRendering;
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
			EditorApplication.update = OnUpdate + EditorApplication.update;
			EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
			EditorApplication.pauseStateChanged -= EditorApplicationOnPauseStateChanged;
			EditorApplication.pauseStateChanged += EditorApplicationOnPauseStateChanged;
			InitialiseDisposal();
		}

		private void EditorApplicationOnPauseStateChanged(PauseState obj) => _isPaused = obj == PauseState.Paused;


		private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange obj)
		{
			switch (obj)
			{
				case PlayModeStateChange.ExitingEditMode:
					_defaultGroup.Clear();
					_gizmosGroup.Clear();
					UnmanagedCommandBuilder.Instance.Data.Clear();
					_isPlaying = true;
					break;
				case PlayModeStateChange.ExitingPlayMode:
					_defaultGroup.Clear();
					_gizmosGroup.Clear();
					UnmanagedCommandBuilder.Instance.Data.Clear();
					_isPlaying = false;
					break;
				case PlayModeStateChange.EnteredPlayMode:
				case PlayModeStateChange.EnteredEditMode:
					break;
				default:
					return;
			}
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
				RenderGizmosGroup(camera, SceneView.currentDrawingSceneView != null ? RenderingType.Scene : RenderingType.Game);
			}
		}

		private void OnPostRender(Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			Profiler.BeginSample(Name);
			ref var unmanaged = ref UnmanagedCommandBuilder.Instance.Data;
			if (!SharedRenderingDetails(camera, ref unmanaged.Standard, _defaultGroup, null, type))
			{
				Profiler.EndSample();
				return;
			}

			Graphics.ExecuteCommandBuffer(_defaultGroup.BuiltInCommandBuffer.CommandBuffer);
			Profiler.EndSample();
		}

		private readonly Stack<CommandBufferWrapper> _wrappers = new Stack<CommandBufferWrapper>();

		internal void ExecuteDrawRenderPass(CommandBuffer commandBuffer, Camera camera)
		{
			CommandBufferWrapper wrapper;
			if (_wrappers.Count == 0)
				wrapper = new CommandBufferWrapper(commandBuffer);
			else
			{
				wrapper = _wrappers.Pop();
				wrapper.OverrideCommandBuffer(commandBuffer);
			}

			ExecuteDrawRenderPass(wrapper, camera);
			_wrappers.Push(wrapper);
		}

		internal void ExecuteDrawRenderPass(ICommandBuffer commandBuffer, Camera camera)
		{
			RenderingType type = RenderingType.Default;
			if (SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera)
				type |= RenderingType.Scene;
			else
				type |= RenderingType.Game;

			ref var unmanaged = ref UnmanagedCommandBuilder.Instance.Data;
			SharedRenderingDetails(camera, ref unmanaged.Standard, _defaultGroup, commandBuffer, type);
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

			ref var unmanaged = ref UnmanagedCommandBuilder.Instance.Data;
			if (!SharedRenderingDetails(camera, ref unmanaged.Gizmos, _gizmosGroup, null, type))
				return;
			Graphics.ExecuteCommandBuffer(_gizmosGroup.BuiltInCommandBuffer.CommandBuffer);
		}

		internal void ClearGizmoGroup()
		{
			_gizmosGroup.Clear();
			UnmanagedCommandBuilder.Instance.Data.Gizmos.Clear();
		}

		/// <summary>
		/// This must match <see cref="DebuggingSettings.Location"/>'s Scene and Game
		/// </summary>
		[Flags]
		private enum RenderingType
		{
			Unset = 0,

			// Rendering view
			Scene = 1,
			Game = 1 << 1,

			// Call origin
			Default = 1 << 2,
			Gizmos = 1 << 3,

			// -
			GizmosAndGame = Gizmos | Game
		}

		private bool SharedRenderingDetails(Camera camera, ref UnmanagedCommandGroup commandGroup, BufferGroup bufferGroup, ICommandBuffer commandBuffer, RenderingType renderingType)
		{
			_pauseCapture.CommitCurrentPausedFrame();
			_lastRenderingCamera = camera;
			UpdateContext.ForceStateToUpdate();

			if (!ShouldRenderCamera(camera, renderingType))
				return false;

			bufferGroup.ReadyResources(ref commandBuffer);
			return FillCommandBuffer(commandBuffer, camera, ref commandGroup, bufferGroup, renderingType);
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
		private bool FillCommandBuffer(ICommandBuffer commandBuffer, Camera camera, ref UnmanagedCommandGroup commandGroup, BufferGroup group, RenderingType renderingType)
		{
			bool render;
			if (renderingType == RenderingType.GizmosAndGame)
			{
				Matrix4x4 oldMatrix = Shader.GetGlobalMatrix(s_UnityMatrixVPKey);
				commandBuffer.SetGlobalMatrix(s_UnityMatrixVPKey, GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix);
				RenderShapes(ref commandGroup);
				commandBuffer.SetGlobalMatrix(s_UnityMatrixVPKey, oldMatrix);
			}
			else
			{
				RenderShapes(ref commandGroup);
			}

			// commandBuffer.SetGlobalDepthBias(-1, -1);

			return render;

			void RenderShapes(ref UnmanagedCommandGroup commandGroup)
			{
				DebuggingSettings settings = DebuggingSettings.instance;
				bool depthTest = ((int)settings.DepthTest & (int)renderingType) != 0;
				bool depthWrite = ((int)settings.DepthWrite & (int)renderingType) != 0;
				render = RenderShape(AssetsUtility.Line, AssetsUtility.LineMaterial, commandGroup.Lines, group.Lines, 128); // 256 vert target, 2 verts per line. 128 lines.
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.DashedLineMaterial, commandGroup.DashedLines, group.DashedLines, 128);
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.OutlineMaterial, commandGroup.Outlines, group.Outlines, 128);
				render |= RenderShape(AssetsUtility.Line, AssetsUtility.CastMaterial, commandGroup.Casts, group.Casts, 128);
				render |= RenderShape(AssetsUtility.Circle, AssetsUtility.ArcMaterial, commandGroup.Arcs, group.Arcs, 4); // 256 vert target, 64 verts per circle. 4 circles.
				render |= RenderShape(AssetsUtility.Box, AssetsUtility.BoxMaterial, commandGroup.Boxes, group.Boxes, 11); // 256 vert target, 12 edges, 2 verts each. 11 boxes.
				return;

				bool RenderShape<T>(AssetsUtility.Asset<Mesh> mesh,
					AssetsUtility.MaterialAsset material,
					UnmanagedCommandContainer<T> elements,
					ShapeBuffer<T> buffer,
					int groupCount = 1) where T : unmanaged
				{
					int shapeCount = elements.Count;
					if (shapeCount <= 0)
						return false;

					Material mat = material.Value;

					// Don't render this shape until it's compiled.
					// (It looks very strange to the user when a cyan box appears for a millisecond at 0,0,0)
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

					MaterialPropertyBlock propertyBlock = buffer.PropertyBlock;
					// Set the buffers to be used by the property block
					// Synchronise the GraphicsBuffer with the data in the shape buffer.
					buffer.Set(commandBuffer, propertyBlock, elements.Values, shapeCount, elements.Dirty);
					mat.SetFloat(s_ZWriteKey, depthWrite ? 1f : 0f);
					mat.SetFloat(s_ZTestKey, (float)(depthTest ? CompareFunction.LessEqual : CompareFunction.Always));
					commandBuffer.DrawMeshInstancedProcedural(mesh.Value, 0, mat, depthTest ? -1 : 1, (int)math.ceil(shapeCount / (float)groupCount), propertyBlock);
					return true;
				}
			}
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

			ref var unmanagedCommandBuilder = ref UnmanagedCommandBuilder.Instance.Data;
			unmanagedCommandBuilder.Dispose();
		}
	}
}
#endif