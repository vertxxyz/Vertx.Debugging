using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawLine(Vector3 a, Vector3 b, Color color, float duration = 0) =>
			lineDelegate(a, b, color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void DrawLine(IEnumerable<Vector3> points, Color color, float duration = 0)
		{
			Vector3? previous = null;
			foreach (Vector3 point in points)
			{
				if(previous.HasValue)
					lineDelegate(previous.Value, point, color, duration);
				previous = point;
			}
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawLine(IList<Vector3> points, Color color, float duration = 0)
		{
			int count = points.Count;
			if (count <= 1)
				return;
			
			Vector3 previous = points[0];
			for (int i = 1; i < count; i++)
			{
				Vector3 point = points[i];
				lineDelegate(previous, point, color, duration);
				previous = point;
			}
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrow(Vector3 position, Vector3 direction, Color color, float duration = 0, float arrowheadScale = 1)
		{
			rayDelegate(position, direction, color, duration);
			DrawArrowHead(position, direction, color, duration, arrowheadScale);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine(Vector3 origin, Vector3 destination, Color color, float duration = 0, float arrowheadScale = 1)
		{
			lineDelegate(origin, destination, color, duration);
			Vector3 direction = destination - origin;
			DrawArrowHead(origin, direction, color, duration, arrowheadScale);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine(IEnumerable<Vector3> points, Color color, float duration = 0, float arrowheadScale = 1)
		{
			Vector3? previous = null;
			Vector3? origin = null;
			foreach (Vector3 point in points)
			{
				if(previous.HasValue)
					lineDelegate(previous.Value, point, color, duration);
				origin = previous;
				previous = point;
			}

			if (!origin.HasValue)
				return;
			Vector3 direction = previous.Value - origin.Value;
			DrawArrowHead(origin.Value, direction, color, duration, arrowheadScale);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine(IList<Vector3> points, Color color, float duration = 0, float arrowheadScale = 1)
		{
			int count = points.Count;
			if (count <= 1)
				return;
			
			Vector3 previous = points[0];
			Vector3 origin = previous;
			for (int i = 1; i < count; i++)
			{
				Vector3 point = points[i];
				lineDelegate(previous, point, color, duration);
				origin = previous;
				previous = point;
			}
			Vector3 direction = previous - origin;
			DrawArrowHead(origin, direction, color, duration, arrowheadScale);
		}
	}
}