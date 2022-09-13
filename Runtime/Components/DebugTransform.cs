using UnityEngine;

namespace Vertx.Debugging
{
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