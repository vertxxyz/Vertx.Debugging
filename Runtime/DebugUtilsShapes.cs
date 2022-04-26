using System;
using System.Diagnostics;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawSphere(Vector3 position, float radius, Color color, float duration = 0)
		{
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			Vector3 forward = Vector3.forward;
			DrawCircleFast(position, up, right, radius, color, duration);
			DrawCircleFast(position, right, up, radius, color, duration);
			DrawCircleFast(position, forward, right, radius, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color, float duration = 0)
		{
			DrawBoxStructure box = new DrawBoxStructure(halfExtents, orientation);
			DrawBox(center, box, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBox(Vector3 center, Vector3 halfExtents, Color color, float duration = 0) => DrawBox(center, halfExtents, Quaternion.identity, color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color, float duration = 0)
		{
			Vector3 alignment = (start - end).normalized;
			Vector3 crossA = GetAxisAlignedPerpendicular(alignment);
			Vector3 crossB = Vector3.Cross(crossA, alignment);
			DrawCapsuleFast(start, end, radius, alignment, crossA, crossB, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawSurfacePoint(Vector3 point, Vector3 normal, Color color, float duration = 0)
		{
			rayDelegate(point, normal, color, duration);
			DrawCircle(point, normal, 0.05f, color, duration, 50);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawPoint
		(
			Vector3 point,
			Color color,
			float duration = 0,
			float rayLength = 0.3f,
			float highlightRadius = 0.05f
		)
		{
			Vector3 up = new Vector3(0, rayLength, 0);
			Quaternion rot = Quaternion.AngleAxis(120, Vector3.right);
			Vector3 d1 = rot * up;
			Quaternion rot1 = Quaternion.AngleAxis(120, up);
			Vector3 d2 = rot1 * d1;
			Vector3 d3 = rot1 * d2;

			rayDelegate(point, up, color, duration);
			rayDelegate(point, d1, color, duration);
			rayDelegate(point, d2, color, duration);
			rayDelegate(point, d3, color, duration);

			Vector3 down = new Vector3(0, -highlightRadius, 0);
			Vector3 p1 = rot * down;
			Vector3 p2 = rot1 * p1;
			Vector3 p3 = rot1 * p2;

			down += point;
			p1 += point;
			p2 += point;
			p3 += point;

			lineDelegate(down, p1, color, duration);
			lineDelegate(down, p2, color, duration);
			lineDelegate(down, p3, color, duration);
			lineDelegate(p1, p2, color, duration);
			lineDelegate(p2, p3, color, duration);
			lineDelegate(p3, p1, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawAxis(Vector3 point, bool arrowHeads = false, Axes axes = Axes.All, float scale = 1)
			=> DrawAxis(point, Quaternion.identity, arrowHeads, axes, scale);

		[Flags]
		public enum Axes
		{
			None = 0,
			X = 1,
			Y = 1 << 1,
			Z = 1 << 2,
			TwoDimensional = X | Y,
			All = X | Y | Z
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawAxis(Vector3 point, Quaternion rotation, bool arrowHeads = false, Axes axes = Axes.All, float scale = 1)
		{
			Vector3 right = rotation * new Vector3(scale, 0, 0);
			Vector3 up = rotation * new Vector3(0, scale, 0);
			Vector3 forward = rotation * new Vector3(0, 0, scale);
			bool drawX = (axes & Axes.X) != 0;
			bool drawY = (axes & Axes.Y) != 0;
			bool drawZ = (axes & Axes.Z) != 0;
			Color colorRight = ColorX;
			Color colorUp = ColorY;
			Color colorForward = ColorZ;
			if (drawX)
				rayDelegate(point, right, colorRight);
			if (drawY)
				rayDelegate(point, up, colorUp);
			if (drawZ)
				rayDelegate(point, forward, colorForward);

			if (!arrowHeads)
				return;

			if (drawX)
				DrawArrowHead(point, right, colorRight, scale: scale);
			if (drawY)
				DrawArrowHead(point, up, colorUp, scale: scale);
			if (drawZ)
				DrawArrowHead(point, forward, colorForward, scale: scale);
		}

		private static void DrawArrowHead(Vector3 point, Vector3 dir, Color color, float duration = 0, float scale = 1)
		{
			const float arrowLength = 0.075f;
			const float arrowWidth = 0.05f;
			const int segments = 3;

			Vector3 arrowPoint = point + dir;
			dir.EnsureNormalized();

			// Logic from DrawCircle
			void DoDrawArrowHead(Vector3 center, Vector3 normal, float radius)
			{
				Vector2[] circle = _circleCache.GetCircle(segments);
				Vector3 tangent = GetAxisAlignedPerpendicular(normal);
				Vector3 bitangent = Vector3.Cross(normal, tangent);
				tangent *= radius;
				bitangent *= radius;
				Vector3 lastPos = center + tangent;
				for (int i = 1; i <= segments; i++)
				{
					Vector2 c = circle[i];
					Vector3 nextPos = center + tangent * c.x + bitangent * c.y;
					lineDelegate(lastPos, nextPos, color, duration);
					lineDelegate(lastPos, arrowPoint, color, duration);
					lastPos = nextPos;
				}
			}

			DoDrawArrowHead(arrowPoint - dir * (arrowLength * scale), dir, arrowWidth * scale);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBounds(Bounds bounds, Color color, float duration = 0)
		{
			var lbf = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
			var ltb = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
			var rbb = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
			lineDelegate(bounds.min, lbf, color, duration);
			lineDelegate(bounds.min, ltb, color, duration);
			lineDelegate(bounds.min, rbb, color, duration);

			var rtb = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
			var rbf = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
			var ltf = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
			lineDelegate(bounds.max, rtb, color, duration);
			lineDelegate(bounds.max, rbf, color, duration);
			lineDelegate(bounds.max, ltf, color, duration);

			lineDelegate(rbb, rbf, color, duration);
			lineDelegate(rbb, rtb, color, duration);

			lineDelegate(lbf, rbf, color, duration);
			lineDelegate(lbf, ltf, color, duration);

			lineDelegate(ltb, rtb, color, duration);
			lineDelegate(ltb, ltf, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBounds(BoundsInt bounds, Color color, float duration = 0) => DrawBounds(new Bounds(bounds.center, bounds.size), color, duration);
	}
}