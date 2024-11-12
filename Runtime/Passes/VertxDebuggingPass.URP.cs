#if UNITY_EDITOR
#if VERTX_URP
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if VERTX_CORERP_17_0_1_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace Vertx.Debugging
{
	internal sealed class VertxDebuggingRenderPass : ScriptableRenderPass
	{
#if VERTX_URP_9_0_OR_NEWER
		private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(
			BuiltinRenderTextureType.CurrentActive
		);
#else
		private static readonly RenderTargetIdentifier k_CurrentActive = new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive);
#endif

		private readonly ProfilingSampler _defaultProfilingSampler = new ProfilingSampler(CommandBuilder.Name);

#if VERTX_CORERP_17_0_1_OR_NEWER
		private class PassData
		{
			public Camera Camera;

			public Stack<UnsafeCommandBufferWrapper> Wrappers;
			public TextureHandle Color;
			public TextureHandle Depth;
		}

		private static readonly Stack<UnsafeCommandBufferWrapper> s_wrappers = new Stack<UnsafeCommandBufferWrapper>();

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			var commandBuilder = CommandBuilder.Instance;
			BufferGroup bufferGroup = commandBuilder.GetDefaultBufferGroup();
			BufferHandle linesHandle = renderGraph.ImportBuffer(bufferGroup.Lines.Buffer);
			BufferHandle dashedLinesHandle = renderGraph.ImportBuffer(bufferGroup.DashedLines.Buffer);
			BufferHandle arcsHandle = renderGraph.ImportBuffer(bufferGroup.Arcs.Buffer);
			BufferHandle boxesHandle = renderGraph.ImportBuffer(bufferGroup.Boxes.Buffer);
			BufferHandle outlinesHandle = renderGraph.ImportBuffer(bufferGroup.Outlines.Buffer);
			BufferHandle castsHandle = renderGraph.ImportBuffer(bufferGroup.Casts.Buffer);

			using (var builder = renderGraph.AddUnsafePass(CommandBuilder.Name, out PassData passData))
			{
				var cameraData = frameData.Get<UniversalCameraData>();
				var resourceData = frameData.Get<UniversalResourceData>();
				passData.Camera = cameraData.camera;
				passData.Wrappers = s_wrappers;
				passData.Color = resourceData.activeColorTexture;
				passData.Depth = resourceData.activeDepthTexture;

				builder.UseBuffer(linesHandle, AccessFlags.ReadWrite);
				builder.UseBuffer(dashedLinesHandle, AccessFlags.ReadWrite);
				builder.UseBuffer(arcsHandle, AccessFlags.ReadWrite);
				builder.UseBuffer(boxesHandle, AccessFlags.ReadWrite);
				builder.UseBuffer(outlinesHandle, AccessFlags.ReadWrite);
				builder.UseBuffer(castsHandle, AccessFlags.ReadWrite);

				builder.UseTexture(passData.Color, AccessFlags.Write);
				builder.UseTexture(passData.Depth, AccessFlags.ReadWrite);

				builder.AllowPassCulling(false);

				builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
				{
					if (!passData.Wrappers.TryPop(out UnsafeCommandBufferWrapper wrapper))
						wrapper = new UnsafeCommandBufferWrapper(context.cmd);
					else
						wrapper.OverrideCommandBuffer(context.cmd);

					// context.cmd.SetRenderTarget(data.Color, data.Depth, 0);
					CommandBuilder.Instance.ExecuteDrawRenderPass(wrapper, data.Camera);
					passData.Wrappers.Push(wrapper);
				});
			}
		}
#endif

		/// <summary>
		/// This method is called by the renderer before executing the render pass.
		/// Override this method if you need to to configure render targets and their clear state, and to create temporary render target textures.
		/// If a render pass doesn't override this method, this render pass renders to the active Camera's render target.
		/// You should never call CommandBuffer.SetRenderTarget. Instead call ConfigureTarget and ConfigureClear.
		/// </summary>
#if VERTX_CORERP_17_0_1_OR_NEWER
		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
#endif
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			=> ConfigureTarget(k_CurrentActive);

		/// <summary>
		/// Execute the pass. This is where custom rendering occurs. Specific details are left to the implementation
		/// </summary>
#if VERTX_CORERP_17_0_1_OR_NEWER
		[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
#endif
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get(CommandBuilder.Name);
			using (new ProfilingScope(commandBuffer, _defaultProfilingSampler))
			{
				commandBuffer.Clear();
				CommandBuilder.Instance.ExecuteDrawRenderPass(commandBuffer, renderingData.cameraData.camera);
				context.ExecuteCommandBuffer(commandBuffer);
			}

			CommandBufferPool.Release(commandBuffer);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
		}
	}
}
#endif
#endif