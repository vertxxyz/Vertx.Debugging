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
		private static bool s_Initialised;
		private static CurrentPipeline s_CurrentPipeline;

		public static CurrentPipeline PipelineCached => s_Initialised ? s_CurrentPipeline : Pipeline;

		public static CurrentPipeline Pipeline
		{
			get
			{
				s_Initialised = true;
				switch (GraphicsSettings.currentRenderPipeline)
				{
#if VERTX_URP
					case UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset:
						return s_CurrentPipeline = CurrentPipeline.URP;
#endif
#if VERTX_HDRP
					case UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset:
						return s_CurrentPipeline = CurrentPipeline.HDRP;
#endif
					default:
						return s_CurrentPipeline = CurrentPipeline.BuiltIn;
				}
			}
		}
	}
}