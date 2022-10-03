#if UNITY_EDITOR
#if VERTX_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Vertx.Debugging
{
	internal class VertxDebuggingRenderPass : ScriptableRenderPass
	{
		private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive);
		// private static readonly RTHandle k_Depth = RTHandles.Alloc(BuiltinRenderTextureType.Depth);
			
		/// <summary>
		/// This method is called by the renderer before executing the render pass.
		/// Override this method if you need to to configure render targets and their clear state, and to create temporary render target textures.
		/// If a render pass doesn't override this method, this render pass renders to the active Camera's render target.
		/// You should never call CommandBuffer.SetRenderTarget. Instead call ConfigureTarget and ConfigureClear.
		/// </summary>
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureInput(ScriptableRenderPassInput.Depth);
			ConfigureTarget(k_CurrentActive);
		}
			
		/*/// <summary>
		/// Gets called by the renderer before executing the pass.
		/// Can be used to configure render targets and their clearing state.
		/// Can be user to create temporary render target textures.
		/// If this method is not overriden, the render pass will render to the active camera render target.
		/// </summary>
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ConfigureInput(ScriptableRenderPassInput.Depth);
		}*/

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			=> CommandBuilder.Instance.ExecuteDrawRenderPass(context, renderingData.cameraData.camera);

		public override void FrameCleanup(CommandBuffer cmd) { }
	}
}
#endif
#endif