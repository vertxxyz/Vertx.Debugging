#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;

namespace Vertx.Debugging
{
	internal sealed class CircleCache
	{
		private readonly Dictionary<int, float2[]> _circles = new Dictionary<int, float2[]>();

		public float2[] GetCircle(int segments)
		{
			if (_circles.TryGetValue(segments, out float2[] circle))
				return circle;

			circle = new float2[segments + 1];

			const float max = math.PI * 2;
			float division = 1 / (float)segments * max;
			int i = 0;
			for (float angle = 0; angle < max; angle += division)
			{
				circle[i] = new float2(math.cos(angle), math.sin(angle));
				i++;
			}

			circle[segments] = circle[0];

			_circles.Add(segments, circle);
			return circle;
		}
	}
}
#endif