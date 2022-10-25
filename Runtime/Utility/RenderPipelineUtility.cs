#if UNITY_EDITOR
using UnityEngine;
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
					case UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset _:
						return s_CurrentPipeline = CurrentPipeline.URP;
#endif
#if VERTX_HDRP
					case UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset asset:
						if (!asset.currentPlatformRenderPipelineSettings.supportCustomPass)
						{
							Debug.LogWarning(
								$"{CommandBuilder.Name} does not support drawing without Custom Pass enabled.\nYou can find this setting in Frame Settings/Camera/Rendering on your HD Render Pipeline Global Settings asset.",
								asset
							);
						}

						return s_CurrentPipeline = CurrentPipeline.HDRP;
#endif
					default:
						return s_CurrentPipeline = CurrentPipeline.BuiltIn;
				}
			}
		}
	}
}
#endif