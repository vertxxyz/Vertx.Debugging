using UnityEngine;

namespace Vertx.Debugging
{
#if UNITY_EDITOR
	/// <summary>
	/// This only exists to avoid an issue with enum fields of an underlying type: byte.
	/// https://issuetracker.unity3d.com/issues/none-or-mixed-option-gets-set-to-enumflagsfield-when-selecting-the-everything-option
	/// Which has also regressed in later versions of 2022.
	/// </summary>
	[UnityEditor.CustomEditor(typeof(DebugTransform))]
	internal sealed class DebugTransformEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI() => DrawDefaultInspector();
	}
#endif
	
	[AddComponentMenu("Debugging/Debug Transform")]
	public sealed class DebugTransform : DebugComponentBase
	{
		[SerializeField] private float _scale = 1;
		[SerializeField] private Shapes.Axes _axes = Shapes.Axes.All;

		protected override bool ShouldDraw() => true;

		protected override void Draw()
		{
			Transform t = transform;
			D.raw(new Shapes.Axis(t.position, t.rotation, true, _axes, _scale));
		}
	}
}