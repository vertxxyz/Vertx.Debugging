#if UNITY_EDITOR
#if VERTX_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

		/// <summary>
		/// This method is called by the renderer before executing the render pass.
		/// Override this method if you need to to configure render targets and their clear state, and to create temporary render target textures.
		/// If a render pass doesn't override this method, this render pass renders to the active Camera's render target.
		/// You should never call CommandBuffer.SetRenderTarget. Instead call ConfigureTarget and ConfigureClear.
		/// </summary>
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			=> ConfigureTarget(k_CurrentActive);

		/*/// <summary>
		/// Gets called by the renderer before executing the pass.
		/// Can be used to configure render targets and their clearing state.
		/// Can be user to create temporary render target textures.
		/// If this method is not overriden, the render pass will render to the active camera render target.
		/// </summary>
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			
		}*/

		/// <summary>
		/// Execute the pass. This is where custom rendering occurs. Specific details are left to the implementation
		/// </summary>
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get(CommandBuilder.Name);
			using (new ProfilingScope(commandBuffer, _defaultProfilingSampler))
			{
				commandBuffer.Clear();
				CommandBuilder.Instance.ExecuteDrawRenderPass(context, commandBuffer, renderingData.cameraData.camera);
			}

			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}

		public override void FrameCleanup(CommandBuffer cmd) { }
	}
}
#endif
#endif