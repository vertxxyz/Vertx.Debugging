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
		private static bool s_initialised;
		private static CurrentPipeline s_currentPipeline;

		public static CurrentPipeline PipelineCached => s_initialised ? s_currentPipeline : Pipeline;

		public static CurrentPipeline Pipeline
		{
			get
			{
				s_initialised = true;
				switch (GraphicsSettings.currentRenderPipeline)
				{
#if VERTX_URP
					case UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset:
						return s_currentPipeline = CurrentPipeline.URP;
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

						return s_currentPipeline = CurrentPipeline.HDRP;
#endif
					default:
						return s_currentPipeline = CurrentPipeline.BuiltIn;
				}
			}
		}
	}
}
#endif