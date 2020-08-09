using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertx.Debugging
{
	[DefaultExecutionOrder(-999999999)]
	public class DebugUtilsRuntimeObject : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics() => instance = null;

		private static DebugUtilsRuntimeObject instance;

		public static DebugUtilsRuntimeObject Instance
		{
			get
			{
				if (instance != null) return instance;
				GameObject gameObject = new GameObject("DebugUtils") {hideFlags = HideFlags.HideInHierarchy};
				DontDestroyOnLoad(gameObject);
				instance = gameObject.AddComponent<DebugUtilsRuntimeObject>();
				return instance;
			}
		}

		#region Update

		private readonly List<Action> updateActions = new List<Action>();

		void Update()
		{
			foreach (Action updateAction in updateActions)
				updateAction?.Invoke();
			updateActions.Clear();
		}

		public void RegisterUpdateAction(Action action) => updateActions.Add(action);

		#endregion

		#region Fixed Update

		private readonly List<Action> fixedUpdateActions = new List<Action>();
		
		void FixedUpdate()
		{
			foreach (Action fixedUpdateAction in fixedUpdateActions)
				fixedUpdateAction?.Invoke();
			fixedUpdateActions.Clear();
		}
		
		public void RegisterFixedUpdateAction(Action action) => fixedUpdateActions.Add(action);

		#endregion
	}
}