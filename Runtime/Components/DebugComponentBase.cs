using UnityEngine;

namespace Vertx.Debugging
{
	public abstract class DebugComponentBase : MonoBehaviour
	{
		[SerializeField] private bool _drawOnlyWhenSelected = true;
		[SerializeField] protected Color _color = DebugUtils.RayColor;

		[System.Serializable]
		public struct ColorDurationPair
		{
			public Color Color;
			public float Duration;

			public ColorDurationPair(Color color, float duration)
			{
				Color = color;
				Duration = duration;
			}

			public ColorDurationPair(Color color)
			{
				Color = color;
				Duration = 0;
			}
		}

		private void OnDrawGizmos()
		{
			if (_drawOnlyWhenSelected || !enabled || !ShouldDraw())
				return;

			using (DebugUtils.DrawGizmosScope())
				Draw();
		}

		private void OnDrawGizmosSelected()
		{
			if (!_drawOnlyWhenSelected || !enabled || !ShouldDraw())
				return;

			using (DebugUtils.DrawGizmosScope())
				Draw();
		}

		protected abstract bool ShouldDraw();

		protected abstract void Draw();
		
		// ReSharper disable once Unity.RedundantEventFunction
		protected virtual void OnDisable()
		{
			// Only here to get the enabled tick-box.
		}
	}
}