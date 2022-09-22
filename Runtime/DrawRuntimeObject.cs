using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertx.Debugging
{
	[DefaultExecutionOrder(int.MinValue), AddComponentMenu("")]
	public sealed class DrawRuntimeObject : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics() => instance = null;

		private static DrawRuntimeObject instance;

		public static DrawRuntimeObject Instance
		{
			get
			{
				if (instance != null) 
					return instance;
				GameObject gameObject = new GameObject(nameof(DrawRuntimeObject))
				{
					hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave
				};
				DontDestroyOnLoad(gameObject);
				instance = gameObject.AddComponent<DrawRuntimeObject>();
				return instance;
			}
		}

		#region OnGUI

		private readonly List<Action> _guiActions = new List<Action>();
		
		private void OnGUI()
		{
			DrawText.OnGUI();
			
			foreach (Action guiAction in _guiActions)
				guiAction?.Invoke();
			if (Event.current.type == EventType.Repaint)
				_guiActions.Clear();
		}

		public void RegisterOnGUIAction(Action action) => _guiActions.Add(action);

		#endregion
	}
}