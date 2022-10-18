#if UNITY_EDITOR
using System;
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
				if (!ReferenceEquals(s_Instance, null))
					return s_Instance;
				GameObject gameObject = new GameObject(nameof(DrawRuntimeBehaviour), typeof(DrawRuntimeBehaviour))
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
				s_Instance = gameObject.GetComponent<DrawRuntimeBehaviour>();
				AssemblyReloadEvents.beforeAssemblyReload += SetNull;
				return s_Instance;
			}
		}

		private static void SetNull()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= SetNull;
			s_Instance = null;
		}

#if VERTX_HDRP
		private CurrentPipeline _currentConfiguration;

		public void InitialiseRenderPipelineSetup()
		{
			CurrentPipeline currentPipeline = RenderPipelineUtility.Pipeline;
			if (_currentConfiguration == currentPipeline)
				return;
			
			_currentConfiguration = currentPipeline;
			if (currentPipeline == CurrentPipeline.HDRP)
			{
				if (!TryGetComponent(out CustomPassVolume volume))
				{
					volume = gameObject.AddComponent<CustomPassVolume>();
					volume.isGlobal = true;
					volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
					volume.customPasses.Add(new VertxDebuggingCustomPass { name = CommandBuilder.Name });
				}
			}
			else if(TryGetComponent(out CustomPassVolume volume))
			{
				DestroyImmediate(volume);
			}
		}
#endif

		private void Update()
		{
			if (DestroyedIfInvalid())
				return;
		}

		private void OnGUI()
		{
			if (DestroyedIfInvalid())
				return;

			UpdateContext.OnGUI();

			if (Handles.ShouldRenderGizmos())
				DrawText.OnGUI();
		}

		private bool DestroyedIfInvalid()
		{
			if (s_Instance == this)
				return false;
			DestroyImmediate(gameObject);
			return true;
		}

		private void OnDestroy()
		{
			if (s_Instance == this)
				s_Instance = null;
		}
	}
}
#endif