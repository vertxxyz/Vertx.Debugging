#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		public static CommandBuilder Instance { get; }

		private struct Duration
		{
			public float Value;
		}

		private CommandBuffer _commandBuffer;
		private NativeList<Duration> _durations;
		private static readonly int _colorBufferStartId = Shader.PropertyToID("shared_buffer_start");
		private readonly ListAndBuffer<Color> _colors = new ListAndBuffer<Color>("color_buffer");
		private readonly ListAndBuffer<Shapes.DrawModifications> _modifications = new ListAndBuffer<Shapes.DrawModifications>("modifications_buffer");
		private readonly ListBufferAndMpb<Shapes.Line> _lines = new ListBufferAndMpb<Shapes.Line>("line_buffer");
		private readonly ListBufferAndMpb<Shapes.Arc> _arcs = new ListBufferAndMpb<Shapes.Arc>("arc_buffer");
		private bool _queuedDispose;
		private double _lastTime;
		private DrawRenderPassFeature _pass;

		static CommandBuilder() => Instance = new CommandBuilder();

		private CommandBuilder()
		{
			Camera.onPostRender += OnPostRender;
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
			RenderPipelineManager.endContextRendering += OnEndContextRendering;
			EditorApplication.update += OnUpdate;
		}
		
		private struct VertxDebuggingTick { }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitialiseRuntime()
		{
			// Queue RuntimeEarlyUpdate into the EarlyUpdate portion of the player loop.
			
			PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subsystems = playerLoop.subSystemList.ToArray();
			Type earlyUpdate = typeof(EarlyUpdate);
			for (int i = 0; i < subsystems.Length; i++)
			{
				if (subsystems[i].type != earlyUpdate)
					continue;
				
				var tick = new PlayerLoopSystem
				{
					type = typeof(VertxDebuggingTick),
					updateDelegate = Instance.RuntimeEarlyUpdate
				};

				var earlyUpdateSystem = subsystems[i];
				PlayerLoopSystem[] source = earlyUpdateSystem.subSystemList;
				PlayerLoopSystem[] dest = new PlayerLoopSystem[source.Length + 1];
				Array.Copy(source, 0, dest, 1, source.Length);
				dest[0] = tick;
				subsystems[i].subSystemList = dest;
			}
			
			playerLoop.subSystemList = subsystems;
			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		private void RuntimeEarlyUpdate()
		{
			double time = Time.timeSinceLevelLoadAsDouble;
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (_lastTime == time)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				_lines.Clear();
				_arcs.Clear();
				_colors.Clear();
				// TODO remove data that has a met duration
			}

			_lastTime = time;
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
					_pass = ScriptableObject.CreateInstance<DrawRenderPassFeature>();

				_pass.AddRenderPasses(renderer);
			}
#endif
		}

		private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
		{
			
		}

		private void OnUpdate() { }

		private void OnPostRender(Camera camera)
		{
			if (SharedRenderingDetails(camera))
				Graphics.ExecuteCommandBuffer(_commandBuffer);
		}

		public void ExecuteDrawRenderPass(ScriptableRenderContext context, Camera camera)
		{
			if (SharedRenderingDetails(camera))
				context.ExecuteCommandBuffer(_commandBuffer);
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
			// TODO this is handled per-camera, so there could probably be some culling done.

			RenderLines();
			RenderArcs();

			void RenderLines()
			{
				int lineCount = _lines.Count;
				if (lineCount <= 0)
					return;
				
				// Gather required assets.
				var lineMesh = AssetsUtility.Line.Value;
				var lineMaterial = AssetsUtility.LineMaterial.Value;
					
				// Synchronise the GraphicsBuffer with the data in the line buffer.
				_lines.SetGraphicsBufferDataIfDirty(commandBuffer);
				_colors.SetGraphicsBufferDataIfDirty(commandBuffer);
				_modifications.SetGraphicsBufferDataIfDirty(commandBuffer);
					
				// Set the buffers to be used by the property block
				MaterialPropertyBlock propertyBlock = _lines.PropertyBlock;
				propertyBlock.SetBuffer(_lines.BufferId, _lines.Buffer);
				propertyBlock.SetBuffer(_colors.BufferId, _colors.Buffer);
				propertyBlock.SetBuffer(_modifications.BufferId, _modifications.Buffer);
				
				// Render lines
				commandBuffer.DrawMeshInstancedProcedural(lineMesh, 0, lineMaterial, 0, lineCount, propertyBlock);
			}

			void RenderArcs()
			{
				int arcCount = _arcs.Count;
				if (arcCount <= 0)
					return;
				
				// Gather required assets.
				var circleMesh = AssetsUtility.Circle.Value;
				var arcMaterial = AssetsUtility.ArcMaterial.Value;
					
				// Synchronise the GraphicsBuffer with the data in the line buffer.
				_arcs.SetGraphicsBufferDataIfDirty(commandBuffer);
				_colors.SetGraphicsBufferDataIfDirty(commandBuffer);
				_modifications.SetGraphicsBufferDataIfDirty(commandBuffer);
					
				// Set the buffers to be used by the property block
				MaterialPropertyBlock propertyBlock = _arcs.PropertyBlock;
				propertyBlock.SetBuffer(_arcs.BufferId, _arcs.Buffer);
				propertyBlock.SetBuffer(_colors.BufferId, _colors.Buffer);
				propertyBlock.SetBuffer(_modifications.BufferId, _modifications.Buffer);
				propertyBlock.SetInt(_colorBufferStartId, _lines.Count);
				
				// Render lines
				commandBuffer.DrawMeshInstancedProcedural(circleMesh, 0, arcMaterial, 0, arcCount, propertyBlock);
			}
		}

		public void AppendRay(in Shapes.Ray ray, in Color color, float duration) => AppendLine(new Shapes.Line(ray), color, duration);

		public void AppendLine(in Shapes.Line line, in Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			InitialiseIfRequired();
			_lines.EnsureCreated();
			_lines.Add(line);
			_colors.EnsureCreated();
			_colors.Add(color);
			_modifications.EnsureCreated();
			_modifications.Add(modifications);
		}

		public void AppendArc(in Shapes.Arc arc, in Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			InitialiseIfRequired();
			_arcs.EnsureCreated();
			_arcs.Add(arc);
			_colors.EnsureCreated();
			_colors.Add(color);
			_modifications.EnsureCreated();
			_modifications.Add(modifications);
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

			_commandBuffer.Dispose();
			if (_durations.IsCreated)
				_durations.Dispose();
			_modifications.Dispose();
			_colors.Dispose();
			_lines.Dispose();
			_arcs.Dispose();

			if (_pass != null)
				Object.DestroyImmediate(_pass, true);
		}
	}
}
#endif