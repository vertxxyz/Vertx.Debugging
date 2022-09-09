#if VERTX_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Vertx.Debugging
{
	public class DrawRenderPassFeature : ScriptableRendererFeature
	{
		private class DrawRenderPass : ScriptableRenderPass
		{
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) { }

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
				=> CommandBuilder.Instance.ExecuteDrawRenderPass(context, renderingData.cameraData.camera);

			public override void FrameCleanup(CommandBuffer cmd) { }
		}

		private DrawRenderPass _pass;

		public override void Create() =>
			_pass = new DrawRenderPass
			{
				renderPassEvent = RenderPassEvent.AfterRendering + 10
			};

		public void AddRenderPasses(ScriptableRenderer renderer) => renderer.EnqueuePass(_pass);

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => AddRenderPasses(renderer);
	}
}
#endif