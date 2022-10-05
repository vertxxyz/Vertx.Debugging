using System;
using UnityEngine;

namespace Vertx.Debugging
{
#if UNITY_EDITOR
	/// <summary>
	/// This only exists to avoid an issue with enum fields of an underlying type: byte.
	/// https://issuetracker.unity3d.com/issues/none-or-mixed-option-gets-set-to-enumflagsfield-when-selecting-the-everything-option
	/// Which has also regressed in later versions of 2022.
	/// </summary>
	[UnityEditor.CustomEditor(typeof(DebugTriggerEvents))]
	internal sealed class DebugTriggerEventsEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI() => DrawDefaultInspector();
	}
#endif
	
	[AddComponentMenu("Debugging/Debug Trigger Events")]
	public sealed class DebugTriggerEvents : MonoBehaviour
	{
		[SerializeField] private Type _type = Type.Enter;
		[PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Enter)]
		[SerializeField]
		private DebugComponentBase.ColorDurationPair _enter =
			new DebugComponentBase.ColorDurationPair(Shapes.EnterColor, 0.1f);
		[PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Stay)]
		[SerializeField]
		private DebugComponentBase.ColorDurationPair _stay =
			new DebugComponentBase.ColorDurationPair(Shapes.StayColor);
		[PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Exit)]
		[SerializeField]
		private DebugComponentBase.ColorDurationPair _exit =
			new DebugComponentBase.ColorDurationPair(Shapes.ExitColor, 0.1f);

		[Flags]
		private enum Type : byte
		{
			None,
			Enter = 1,
			Stay = 1 << 1,
			Exit = 1 << 2
		}

#if UNITY_EDITOR
		private void OnTriggerEnter(Collider collider)
		{
			if ((_type & Type.Enter) == 0) return;
			D.raw(collider.bounds, _enter.Color, _enter.Duration);
		}

		private void OnTriggerStay(Collider collider)
		{
			if ((_type & Type.Stay) == 0) return;
			D.raw(collider.bounds, _stay.Color, _stay.Duration);
		}

		private void OnTriggerExit(Collider collider)
		{
			if ((_type & Type.Exit) == 0) return;
			D.raw(collider.bounds, _exit.Color, _exit.Duration);
		}


#if VERTX_PHYSICS_2D
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if ((_type & Type.Enter) == 0) return;
			D.raw(collider.bounds, _enter.Color, _enter.Duration);
		}

		private void OnTriggerStay2D(Collider2D collider)
		{
			if ((_type & Type.Stay) == 0) return;
			D.raw(collider.bounds, _stay.Color, _stay.Duration);
		}

		private void OnTriggerExit2D(Collider2D collider)
		{
			if ((_type & Type.Exit) == 0) return;
			D.raw(collider.bounds, _exit.Color, _exit.Duration);
		}
#endif
#endif
	}
}