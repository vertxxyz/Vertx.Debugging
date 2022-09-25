#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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