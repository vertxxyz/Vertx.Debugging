#if UNITY_EDITOR
#if VERTX_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

namespace Vertx.Debugging
{
	internal class VertxDebuggingCustomPass : CustomPass
	{
		protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd) { }

#if VERTX_HDRP_9_0_OR_NEWER
		protected override void Execute(CustomPassContext context)
			=> CommandBuilder.Instance.ExecuteDrawRenderPass(context.renderContext, context.hdCamera.camera);
#else
		protected override void Execute(ScriptableRenderContext context, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
			=> CommandBuilder.Instance.ExecuteDrawRenderPass(context, camera.camera);
#endif

		protected override void Cleanup() { }
	}
}
#endif
#endif