#if UNITY_EDITOR
using UnityEngine;

namespace Vertx.Debugging.Internal
{
	[DefaultExecutionOrder(int.MinValue), AddComponentMenu("")]
	public sealed class DrawRuntimeBehaviour : MonoBehaviour
	{
		private static DrawRuntimeBehaviour s_Instance;

		public static DrawRuntimeBehaviour Instance
		{
			get
			{
				if (s_Instance != null)
					return s_Instance;
				GameObject gameObject = new GameObject(nameof(DrawRuntimeBehaviour))
				{
					hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave
				};
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
				s_Instance = gameObject.AddComponent<DrawRuntimeBehaviour>();
				return s_Instance;
			}
		}
		
		private void OnGUI() => DrawText.OnGUI();
	}
}
#endif