using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawSphere(Vector3 position, float radius, Color color)
		{
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			Vector3 forward = Vector3.forward;
			DrawCircleFast(position, up, right, radius, DrawLine);
			DrawCircleFast(position, right, up, radius, DrawLine);
			DrawCircleFast(position, forward, right, radius, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color)
		{
			DrawBox(center, halfExtents, orientation, DrawLine);
			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBox(Vector3 center, Vector3 halfExtents, Color color) => DrawBox(center, halfExtents, Quaternion.identity, color);

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color)
		{
			Vector3 alignment = (start - end).normalized;
			Vector3 crossA = GetAxisAlignedPerpendicular(alignment);
			Vector3 crossB = Vector3.Cross(crossA, alignment);
			DrawCapsuleFast(start, end, radius, alignment, crossA, crossB, DrawLine);
			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawSurfacePoint(Vector3 point, Vector3 normal, Color color, float duration = 0)
		{
			Debug.DrawRay(point, normal, color, duration);
			DrawCircle(point, normal, 0.05f, DrawLine, 50);
			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawPoint(
			Vector3 point,
			Color color,
			float duration = 0,
			float rayLength = 0.3f,
			float highlightRadius = 0.05f)
		{
			Vector3 up = new Vector3(0, rayLength, 0);
			Quaternion rot = Quaternion.AngleAxis(120, Vector3.right);
			Vector3 d1 = rot * up;
			Quaternion rot1 = Quaternion.AngleAxis(120, up);
			Vector3 d2 = rot1 * d1;
			Vector3 d3 = rot1 * d2;

			Debug.DrawRay(point, up, color, duration);
			Debug.DrawRay(point, d1, color, duration);
			Debug.DrawRay(point, d2, color, duration);
			Debug.DrawRay(point, d3, color, duration);

			Vector3 down = new Vector3(0, -highlightRadius, 0);
			Vector3 p1 = rot * down;
			Vector3 p2 = rot1 * p1;
			Vector3 p3 = rot1 * p2;

			down += point;
			p1 += point;
			p2 += point;
			p3 += point;

			Debug.DrawLine(down, p1, color, duration);
			Debug.DrawLine(down, p2, color, duration);
			Debug.DrawLine(down, p3, color, duration);
			Debug.DrawLine(p1, p2, color, duration);
			Debug.DrawLine(p2, p3, color, duration);
			Debug.DrawLine(p3, p1, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawArrow(Vector3 position, Vector3 direction, Color color, float duration = 0, float arrowheadScale = 1)
		{
			Debug.DrawRay(position, direction, color);
			DrawArrowHead(position, direction, color, duration, arrowheadScale);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawAxis(Vector3 point, bool arrowHeads = false, float scale = 1)
			=> DrawAxis(point, Quaternion.identity, arrowHeads, scale);

		[Conditional("UNITY_EDITOR")]
		public static void DrawAxis(Vector3 point, Quaternion rotation, bool arrowHeads = false, float scale = 1)
		{
			Vector3 right = rotation * new Vector3(scale, 0, 0);
			Vector3 up = rotation * new Vector3(0, scale, 0);
			Vector3 forward = rotation * new Vector3(0, 0, scale);
			Color colorRight = ColorX;
			Debug.DrawRay(point, right, colorRight);
			Color colorUp = ColorY;
			Debug.DrawRay(point, up, colorUp);
			Color colorForward = ColorZ;
			Debug.DrawRay(point, forward, colorForward);

			if (!arrowHeads)
				return;

			DrawArrowHead(point, right, colorRight, scale: scale);
			DrawArrowHead(point, up, colorUp, scale: scale);
			DrawArrowHead(point, forward, colorForward, scale: scale);
		}
		
		private static void DrawArrowHead(Vector3 point, Vector3 dir, Color color, float duration = 0, float scale = 1)
		{
			const float arrowLength = 0.075f;
			const float arrowWidth = 0.05f;
			const int segments = 3;

			Vector3 arrowPoint = point + dir;
			dir.EnsureNormalized();
			DrawCircle(arrowPoint - dir * (arrowLength * scale), dir, arrowWidth * scale, (a, b, f) =>
			{
				Debug.DrawLine(a, b, color, duration);
				Debug.DrawLine(a, arrowPoint, color, duration);
			}, segments);
		}
	}
}