using UnityEngine;

namespace Vertx.Debugging
{
	[AddComponentMenu("Debugging/Debug Collider Bounds")]
	public sealed class DebugColliderBounds : DebugComponentBase
	{
		[SerializeField] private Color _color = Shapes.CastColor;
		[SerializeField] private Collider _collider;
#if VERTX_PHYSICS_2D
		[SerializeField] private Collider2D _collider2D;
#endif

		private void Reset()
		{
			_collider = GetComponent<Collider>();
#if VERTX_PHYSICS_2D
			_collider2D = GetComponent<Collider2D>();
#endif
		}

		protected override bool ShouldDraw()
		{
#if VERTX_PHYSICS_2D
			return _collider != null || _collider2D != null;
#else
			return _collider != null;
#endif
		}

		protected override void Draw()
		{
#if VERTX_PHYSICS_2D
			if (_collider != null)
				D.raw(_collider.bounds, _color);
			if (_collider2D != null)
				D.raw(_collider2D.bounds, _color);
#else
			D.raw(_collider.bounds, _color);
#endif
		}
	}
}