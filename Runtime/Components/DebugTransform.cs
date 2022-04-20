using UnityEngine;

namespace Vertx.Debugging
{
	[AddComponentMenu("Debugging/Debug Transform")]
	public class DebugTransform : DebugComponentBase
	{
		[SerializeField] private float _scale = 1;
		[SerializeField] private DebugUtils.Axes _axes = DebugUtils.Axes.All;

		protected override bool ShouldDraw() => true;

		protected override void Draw()
		{
			Transform t = transform;
			DebugUtils.DrawAxis(t.position, t.rotation, true, _axes, _scale);
		}
	}
}