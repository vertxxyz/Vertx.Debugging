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
			DrawCircleFast(origin, Vector3.forward, Vector3.up, radius, DrawLine);
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
			Vector3 up = new Vector3(0, rayLength, 0);
			Vector3 perpendicular = Vector3.back;
			Debug.DrawRay(point, up, color);
			Quaternion rot = Quaternion.AngleAxis(120, perpendicular);
			var dir1 = rot * up;
			Debug.DrawRay(point, dir1, color);
			var dir2 = rot * dir1;
			Debug.DrawRay(point, dir2, color);

			if (Mathf.Approximately(highlightRadius, 0))
				return;
			
			//Draw triangle
			Vector3 p1 = new Vector3(0, -highlightRadius, 0);
			Vector3 p2 = rot * p1;
			Vector3 p3 = rot * p2;
			Vector3 o = point;
			p1 += o;
			p2 += o;
			p3 += o;
			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p1, color);
		}
	}
}