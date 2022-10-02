#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if VERTX_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Vertx.Debugging.Internal
{
	[DefaultExecutionOrder(int.MinValue), AddComponentMenu(""), ExecuteAlways]
	public sealed class DrawRuntimeBehaviour : MonoBehaviour
	{
		private static DrawRuntimeBehaviour s_Instance;

		public static DrawRuntimeBehaviour Instance
		{
			get
			{
				if (s_Instance != null)
					return s_Instance;
				GameObject gameObject = new GameObject(nameof(DrawRuntimeBehaviour), typeof(DrawRuntimeBehaviour))
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
				s_Instance = gameObject.GetComponent<DrawRuntimeBehaviour>();
				return s_Instance;
			}
		}

		private CurrentPipeline _currentConfiguration;

		public void InitialiseRenderPipelineSetup()
		{
#if VERTX_HDRP
			CurrentPipeline currentPipeline = RenderPipelineUtility.Pipeline;
			if (_currentConfiguration == currentPipeline)
				return;

			if (_currentConfiguration == CurrentPipeline.HDRP && TryGetComponent(out CustomPassVolume volume))
				DestroyImmediate(volume);
			_currentConfiguration = currentPipeline;
			if (currentPipeline == CurrentPipeline.HDRP)
			{
				if (!TryGetComponent(out volume))
					volume = gameObject.AddComponent<CustomPassVolume>();
				volume.isGlobal = true;
				volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
				volume.customPasses.Add(new VertxDebuggingCustomPass());
			}
#endif
		}

		private void OnGUI()
		{
			if (s_Instance != this)
			{
				DestroyImmediate(gameObject);
				return;
			}

			UpdateContext.OnGUI();
			
			if (Handles.ShouldRenderGizmos())
				DrawText.OnGUI();
		}
	}
}
#endif