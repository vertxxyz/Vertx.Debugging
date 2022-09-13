#if VERTX_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Vertx.Debugging
{
	internal class VertxDebuggingRendererFeature : ScriptableRendererFeature
	{
		private class VertxDebuggingRenderPass : ScriptableRenderPass
		{
			private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive);
			// private static readonly RTHandle k_Depth = RTHandles.Alloc(BuiltinRenderTextureType.Depth);
			
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			{
				ConfigureTarget(k_CurrentActive);
			}
			
			/*public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				ScriptableRenderer renderer = renderingData.cameraData.renderer;
				ConfigureTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
			}*/

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
				=> CommandBuilder.Instance.ExecuteDrawRenderPass(context, renderingData.cameraData.camera);

			public override void FrameCleanup(CommandBuffer cmd) { }
		}

		private VertxDebuggingRenderPass _pass;

		public override void Create() =>
			_pass = new VertxDebuggingRenderPass
			{
				renderPassEvent = RenderPassEvent.AfterRendering + 10
			};

		public void AddRenderPasses(ScriptableRenderer renderer) => renderer.EnqueuePass(_pass);

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => AddRenderPasses(renderer);
	}
}
#endif