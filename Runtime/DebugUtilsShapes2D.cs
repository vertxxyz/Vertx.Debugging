using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
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

		public static void DrawBox2D(Vector2 origin, Vector2 size, float angle, Color color)
		{
			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);
			DrawBox2DFast(Vector2.zero, boxStructure2D, DrawLine);
			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		public static void DrawCircle2D(Vector2 origin, float radius, Color color)
		{
			DrawArc2D(origin, Vector2.up, radius, 360, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}

		public static void DrawCapsule2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Color color)
		{
			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);
			DrawCapsule2DFast(origin, capsuleStructure2D, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}
		
		public static void DrawPoint2D(Vector2 point, Color color, float rayLength = 0.3f, float highlightRadius = 0.05f)
		{
			//Draw rays
			Vector2 up = new Vector2(0, rayLength);
			Debug.DrawRay(point, up, color);
			GetRotationCoefficients(120, out float s, out float c);
			var dir1 = RotateFast(up, s, c);
			Debug.DrawRay(point, dir1, color);
			var dir2 = RotateFast(dir1, s, c);
			Debug.DrawRay(point, dir2, color);

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
			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p1, color);
		}
		
		public static void DrawArrow2D(Vector2 point, float angle, Color color)
		{
			//Draw rays
			GetRotationCoefficients(angle, out float s, out float c);
			Vector2 dir = RotateFast(Vector2.right, s, c);
			DrawArrow2D(point, dir,color );
		}

		public static void DrawArrow2D(Vector2 point, Vector2 direction, Color color)
		{
			Debug.DrawRay(point, direction, color);
			Vector2 cross = PerpendicularClockwise(direction);
			DrawArrowHead(point, direction, cross, color);
		}
		
		public static void DrawAxis2D(Vector2 point, float angle = 0, bool arrowHeads = false)
		{
			//Draw rays
			GetRotationCoefficients(angle, out float s, out float c);
			Vector2 r = RotateFast(Vector2.right, s, c);
			Vector2 u = RotateFast(Vector2.up, s, c);
			Debug.DrawRay(point, r, ColorX);
			Debug.DrawRay(point, u, ColorY);

			if (!arrowHeads)
				return;
			
			DrawArrowHead(point, r, u, ColorX);
			DrawArrowHead(point, u, r, ColorY);
		}
		
		private static void DrawArrowHead(Vector2 point, Vector2 dir, Vector2 cross, Color color)
		{
			const float arrowLength = 0.075f;
			const float arrowWidth = 0.05f;
			Vector2 arrowPoint = point + dir;
			Vector2 a = arrowPoint + cross * arrowWidth;
			Vector2 b = arrowPoint - cross * arrowWidth;
			Vector2 arrowEnd = arrowPoint + dir * arrowLength;
			Debug.DrawLine(a, b, color);
			Debug.DrawLine(a, arrowEnd, color);
			Debug.DrawLine(b, arrowEnd, color);
		}
	}
}