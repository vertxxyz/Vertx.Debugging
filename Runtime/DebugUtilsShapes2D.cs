using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawArea2D(
			Vector2 point1,
			Vector2 point2,
			Color color)
		{
			Vector2 point3 = new Vector2(point1.x, point2.y);
			Debug.DrawLine(point1, point3, color);
			Debug.DrawLine(point3, point2, color);
			Vector2 point4 = new Vector2(point2.x, point1.y);
			Debug.DrawLine(point2, point4, color);
			Debug.DrawLine(point4, point1, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBox2D(Vector2 origin, Vector2 size, float angle, Color color)
		{
			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);
			DrawBox2DFast(Vector2.zero, boxStructure2D, DrawLine);
			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircle2D(Vector2 origin, float radius, Color color)
		{
			DrawArc2D(origin, Vector2.up, radius, 360, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsule2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Color color)
		{
			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);
			DrawCapsule2DFast(origin, capsuleStructure2D, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawPoint2D(Vector2 point, Color color, float duration = 0, float rayLength = 0.3f, float highlightRadius = 0.05f)
		{
			//Draw rays
			Vector2 up = new Vector2(0, rayLength);
			Debug.DrawRay(point, up, color, duration);
			GetRotationCoefficients(120, out float s, out float c);
			var dir1 = RotateFast(up, s, c);
			Debug.DrawRay(point, dir1, color, duration);
			var dir2 = RotateFast(dir1, s, c);
			Debug.DrawRay(point, dir2, color, duration);

			if (Mathf.Approximately(highlightRadius, 0))
				return;

			//Draw triangle
			Vector3 p1 = new Vector3(0, -highlightRadius);
			Vector3 p2 = RotateFast(p1, s, c);
			Vector3 p3 = RotateFast(p2, s, c);
			Vector3 o = point;
			p1 += o;
			p2 += o;
			p3 += o;
			Debug.DrawLine(p1, p2, color, duration);
			Debug.DrawLine(p2, p3, color, duration);
			Debug.DrawLine(p3, p1, color, duration);
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
			Debug.DrawRay(point, direction, color, duration);
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(point, direction, cross, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawAxis2D(Vector2 point, float angle = 0, bool arrowHeads = false, float scale = 1)
		{
			//Draw rays
			GetRotationCoefficients(angle, out float s, out float c);
			Vector2 r = RotateFast(new Vector2(scale, 0), s, c);
			Vector2 u = RotateFast(new Vector2(0, scale), s, c);
			Debug.DrawRay(point, r, ColorX);
			Debug.DrawRay(point, u, ColorY);

			if (!arrowHeads)
				return;

			DrawArrowHead(point, r, u, ColorX, scale: scale);
			DrawArrowHead(point, u, r, ColorY, scale: scale);
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
			Debug.DrawLine(a, b, color, duration);
			Debug.DrawLine(a, arrowEnd, color, duration);
			Debug.DrawLine(b, arrowEnd, color, duration);
		}
	}
}