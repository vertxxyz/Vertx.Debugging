using UnityEngine.Rendering;

namespace Vertx.Debugging
{
	internal enum CurrentPipeline
	{
		BuiltIn,
		URP,
		HDRP
	}

	internal static class RenderPipelineUtility
	{
		private static bool _initialised;

		public static CurrentPipeline Pipeline
		{
			get
			{
				var pipelineAsset = GraphicsSettings.currentRenderPipeline;
				if (pipelineAsset == null)
					return CurrentPipeline.BuiltIn;
#if VERTX_URP
				var type = pipelineAsset.GetType();
				if (type.Name == "UniversalRenderPipelineAsset")
					return CurrentPipeline.URP;
#endif
#if VERTX_HDRP
				var type = pipelineAsset.GetType();
				if (type.Name == "HDRenderPipelineAsset")
					return CurrentPipeline.HDRP;
#endif
				return CurrentPipeline.BuiltIn;
			}
		}
	}
}