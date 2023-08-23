#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging
{
	/// <summary>
	/// Tracks whether Gizmos are being captured.<br/>
	/// (we're expecting a call from OnDrawGizmos, OnDrawGizmosSelected, or DrawGizmo)
	/// </summary>
	[DefaultExecutionOrder(int.MinValue + 1)]
	internal static class UpdateContext
	{
		public enum UpdateState
		{
			Update,
			CapturingGizmos
		}

		public static UpdateState State { get; private set; }

		[DrawGizmo((GizmoType)int.MaxValue)]
		private static void OnDrawGizmos(Transform transform, GizmoType type)
		{
			if (State == UpdateState.CapturingGizmos)
				return;

			State = UpdateState.CapturingGizmos;
			
			ref UnmanagedCommandBuilder builder = ref UnmanagedCommandBuilder.Instance.Data;
			builder.State = UnmanagedCommandBuilder.UpdateState.Gizmos;
			builder.Gizmos.Clear();
			
			CommandBuilder.Instance.ClearGizmoGroup();
		}

		[InitializeOnLoadMethod]
		private static void Initialise()
		{
			SceneView.duringSceneGui -= OnDuringSceneGUI;
			SceneView.duringSceneGui += OnDuringSceneGUI;
		}

		public static void ForceStateToUpdate()
		{
			ref UnmanagedCommandBuilder builder = ref UnmanagedCommandBuilder.Instance.Data;
			builder.State = UnmanagedCommandBuilder.UpdateState.Update;
			State = UpdateState.Update;
		}

		private static void OnDuringSceneGUI(SceneView obj)
		{
			if (RenderPipelineUtility.PipelineCached != CurrentPipeline.BuiltIn)
				return;
			
			if (Event.current.type != EventType.Repaint)
				return;
			if (State == UpdateState.CapturingGizmos)
				CommandBuilder.Instance.RenderGizmosGroup(true);

			ForceStateToUpdate();
		}

		public static void OnGUI()
		{
			// This is handled better in render pipelines, so we only end up handling state and gizmo drawing in OnGUI from Built-in.
			if (RenderPipelineUtility.PipelineCached != CurrentPipeline.BuiltIn)
				return;
			
			if (Event.current.type != EventType.Repaint)
				return;
			
			if (State == UpdateState.CapturingGizmos)
				CommandBuilder.Instance.RenderGizmosGroup(false);

			ForceStateToUpdate();
		}
	}
}
#endif