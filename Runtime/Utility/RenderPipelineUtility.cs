using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
					case UniversalRenderPipelineAsset:
						return s_CurrentPipeline = CurrentPipeline.URP;
#endif
#if VERTX_HDRP
					case HDRenderPipelineAsset:
						return s_CurrentPipeline = CurrentPipeline.URP;
#endif
					default:
						return s_CurrentPipeline = CurrentPipeline.BuiltIn;
				}
			}
		}
	}
}