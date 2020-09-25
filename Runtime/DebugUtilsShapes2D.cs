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
			DrawCircle(origin, Vector3.forward, radius, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}

		public static void DrawCapsule2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Color color)
		{
			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);
			DrawCapsule2DFast(origin, capsuleStructure2D, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float t) => Debug.DrawLine(a, b, color);
		}
	}
}