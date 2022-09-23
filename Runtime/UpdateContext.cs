using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging
{
	internal static class UpdateContext
	{
		public enum UpdateState
		{
			Default,
			GizmosPreImageEffects,
			Update,
			GizmosPostImageEffects
		}

		internal static UpdateState State { get; private set; }
		internal static bool InGizmoState => State == UpdateState.GizmosPreImageEffects || State == UpdateState.GizmosPostImageEffects;
		private static Camera s_CurrentGizmoCamera;

		[DrawGizmo((GizmoType)int.MaxValue)]
		private static void OnDrawGizmos(Transform transform, GizmoType type)
		{
			if (State == UpdateState.GizmosPreImageEffects || State == UpdateState.GizmosPostImageEffects)
				return;
			s_CurrentGizmoCamera = Camera.current;
			State = State == UpdateState.Default ? UpdateState.GizmosPreImageEffects : UpdateState.GizmosPostImageEffects;
		}

		[InitializeOnLoadMethod]
		private static void Initialise()
		{
			SceneView.beforeSceneGui -= OnBeforeSceneGUI;
			SceneView.beforeSceneGui += OnBeforeSceneGUI;
		}

		public static void EarlyUpdate() => State = UpdateState.Update;

		private static void OnBeforeSceneGUI(SceneView obj) => Draw();

		public static void Draw()
		{
			if (Event.current.type != EventType.Layout)
				return;

			if (State == UpdateState.Default)
				return;

			State = UpdateState.Default;
			CommandBuilder.Instance.RenderGizmosGroup(s_CurrentGizmoCamera);
		}
	}
}