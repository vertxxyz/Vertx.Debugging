using UnityEngine;

namespace Vertx.Debugging
{
	[AddComponentMenu("Debugging/Debug Renderer Bounds")]
	public class DebugRendererBounds : DebugComponentBase
	{
		[SerializeField] private Renderer _renderer;

		private void Reset() => _renderer = GetComponent<Renderer>();

		protected override bool ShouldDraw() => _renderer != null;

		protected override void Draw() => DebugUtils.DrawBounds(_renderer.bounds, _color);
	}
}