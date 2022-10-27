#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Vertx.Debugging
{
	internal sealed class CircleCache
	{
		private readonly Dictionary<int, Vector2[]> _circles = new Dictionary<int, Vector2[]>();

		public Vector2[] GetCircle(int segments)
		{
			if (_circles.TryGetValue(segments, out Vector2[] circle))
				return circle;

			circle = new Vector2[segments + 1];

			const float max = Mathf.PI * 2;
			float division = 1 / (float)segments * max;
			int i = 0;
			for (float angle = 0; angle < max; angle += division)
			{
				circle[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				i++;
			}

			circle[segments] = circle[0];

			_circles.Add(segments, circle);
			return circle;
		}
	}
}
#endif