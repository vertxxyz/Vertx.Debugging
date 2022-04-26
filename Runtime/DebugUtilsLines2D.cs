#if VERTX_PHYSICS_2D
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawLine2D(Vector2 a, Vector2 b, Color color, float duration = 0) =>
			lineDelegate(a, b, color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void DrawLine2D(IEnumerable<Vector2> points, Color color, float duration = 0)
		{
			Vector3? previous = null;
			foreach (Vector2 point in points)
			{
				Vector3 point3 = point;
				if(previous.HasValue)
					lineDelegate(previous.Value, point3, color, duration);
				previous = point3;
			}
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawLine2D(IList<Vector2> points, Color color, float duration = 0)
		{
			int count = points.Count;
			if (count <= 1)
				return;
			
			Vector2 previous = points[0];
			for (int i = 1; i < count; i++)
			{
				Vector2 point = points[i];
				lineDelegate(previous, point, color, duration);
				previous = point;
			}
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrow2D(Vector2 point, float angle, Color color, float duration = 0)
		{
			//Draw rays
			GetRotationCoefficients(angle, out float s, out float c);
			Vector2 dir = RotateFast(Vector2.right, s, c);
			DrawArrow2D(point, dir, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawArrow2D(Vector2 point, Vector2 direction, Color color, float duration = 0)
		{
			rayDelegate(point, direction, color, duration);
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(point, direction, cross, color, duration);
		}
		
		private static void DrawArrowHead(Vector2 point, Vector2 dir, Vector2 cross, Color color, float duration = 0, float scale = 1)
		{
			const float arrowLength = 0.075f;
			const float arrowWidth = 0.05f;
			Vector2 arrowPoint = point + dir;
			cross.EnsureNormalized();
			Vector2 arrowCross = cross * (arrowWidth * scale);
			Vector2 a = arrowPoint + arrowCross;
			Vector2 b = arrowPoint - arrowCross;
			dir.EnsureNormalized();
			Vector2 arrowEnd = arrowPoint + dir * (arrowLength * scale);
			lineDelegate(a, b, color, duration);
			lineDelegate(a, arrowEnd, color, duration);
			lineDelegate(b, arrowEnd, color, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine2D(Vector2 origin, Vector2 destination, Color color, float duration = 0, float arrowheadScale = 1)
		{
			lineDelegate(origin, destination, color, duration);
			Vector2 direction = destination - origin;
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(origin, direction, cross, color, duration, arrowheadScale);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine2D(IEnumerable<Vector2> points, Color color, float duration = 0, float arrowheadScale = 1)
		{
			Vector3? previous = null;
			Vector3? origin = null;
			foreach (Vector2 point in points)
			{
				Vector3 point3 = point;
				if(previous.HasValue)
					lineDelegate(previous.Value, point3, color, duration);
				origin = previous;
				previous = point3;
			}

			if (!origin.HasValue)
				return;
			Vector2 direction = previous.Value - origin.Value;
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(origin.Value, direction, cross, color, duration, arrowheadScale);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawArrowLine2D(IList<Vector2> points, Color color, float duration = 0, float arrowheadScale = 1)
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
			Vector2 direction = previous - origin;
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(origin, direction, cross, color, duration, arrowheadScale);
		}
	}
}
#endif