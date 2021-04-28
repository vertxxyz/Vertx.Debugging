using System.Diagnostics;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		#region Casts

		#region CircleCast2D

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2D(Vector2 origin,
			float radius,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;

			Color color = colorStart;
			DrawCircleFast(origin, back, up, radius, DrawLine);

			Vector2 perpendicularClockwise = PerpendicularClockwise(direction);
			Vector2 perpendicularCounterClockwise = PerpendicularCounterClockwise(direction);

			Vector2 scaledDirection = direction * distance;

			Vector2 originA = origin + perpendicularClockwise * radius;
			Vector2 originB = origin + perpendicularCounterClockwise * radius;
			Vector2 currentA = originA;
			Vector2 currentB = originB;

			for (int i = 1; i <= 10; i++)
			{
				float t = i / (float) 10;
				color = Color.Lerp(colorStart, colorEnd, t);
				Vector2 nextA = originA + scaledDirection * t;
				Vector2 nextB = originB + scaledDirection * t;

				DrawLine(currentA, nextA, t);
				DrawLine(currentB, nextB, t);

				currentA = nextA;
				currentB = nextB;
			}


			color = colorEnd;
			DrawCircleFast(origin + scaledDirection, back, up, radius, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => lineDelegate(a, b, color);
		}

		#endregion

		#region BoxCast2D

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2D(
			Vector2 origin,
			Vector2 size,
			float angle,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			Color color = colorStart;
			DrawBox2DFast(Vector2.zero, boxStructure2D, DrawLine);

			Vector2 scaledDirection = direction * distance;
			float dotUR = Vector2.Dot(direction, boxStructure2D.UROrigin);
			float dotBL = Vector2.Dot(direction, boxStructure2D.ULOrigin);

			Vector2 p1;
			Vector2 p2;
			if (Mathf.Abs(dotUR) < Mathf.Abs(dotBL))
			{
				p1 = boxStructure2D.UR;
				p2 = boxStructure2D.BL;
			}
			else
			{
				p1 = boxStructure2D.UL;
				p2 = boxStructure2D.BR;
			}

			Vector2 originA = p1;
			Vector2 originB = p2;
			Vector2 currentA = originA;
			Vector2 currentB = originB;

			for (int i = 1; i <= 10; i++)
			{
				float t = i / (float) 10;
				color = Color.Lerp(colorStart, colorEnd, t);
				Vector2 nextA = originA + scaledDirection * t;
				Vector2 nextB = originB + scaledDirection * t;

				DrawLine(currentA, nextA);
				DrawLine(currentB, nextB);

				currentA = nextA;
				currentB = nextB;
			}

			color = colorEnd;
			DrawBox2DFast(scaledDirection, boxStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b) => lineDelegate(a, b, color);
		}

		#endregion

		#region CapsuleCast2D

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2D(
			Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();

			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			Color color = colorStart;
			DrawCapsule2DFast(origin, capsuleStructure2D, DrawLine);

			var scaledDirection = direction * distance;
			Vector2 destination = origin + scaledDirection;


			float dot = Vector2.Dot(PerpendicularClockwise(capsuleStructure2D.VerticalOffset), direction);
			float sign = Mathf.Sign(dot);
			float scaledRadius = capsuleStructure2D.Radius * sign;

			Vector2 o1 = PerpendicularCounterClockwise(direction) * scaledRadius;
			Vector2 o2 = PerpendicularClockwise(direction) * scaledRadius;
			Vector2 originA = origin + capsuleStructure2D.VerticalOffset + o1;
			Vector2 originB = origin - capsuleStructure2D.VerticalOffset + o2;
			Vector2 currentA = originA;
			Vector2 currentB = originB;

			for (int i = 1; i <= 10; i++)
			{
				float t = i / (float) 10;
				color = Color.Lerp(colorStart, colorEnd, t);
				Vector2 nextA = originA + scaledDirection * t;
				Vector2 nextB = originB + scaledDirection * t;

				DrawLine(currentA, nextA, t);
				DrawLine(currentB, nextB, t);

				currentA = nextA;
				currentB = nextB;
			}

			color = colorEnd;
			DrawCapsule2DFast(destination, capsuleStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => lineDelegate(a, b, color);
		}

		#endregion

		#endregion

		#region RaycastHits

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2DHits(RaycastHit2D[] hits, Color color, int maxCount = -1, float rayLength = 1, float duration = 0)
		{
			if (maxCount < 0)
				maxCount = hits.Length;
			for (int i = 0; i < maxCount; i++)
				rayDelegate(hits[i].point, hits[i].normal * rayLength, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;

			direction.EnsureNormalized();

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			for (int i = 0; i < maxCount; i++)
				DrawBox2DFast(direction * hits[i].distance, boxStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b) => lineDelegate(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2DHits(RaycastHit2D[] hits, Vector2 origin, float radius, Vector2 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;

			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;

			for (int i = 0; i < maxCount; i++)
				DrawCircleFast(origin + direction * hits[i].distance, back, up, radius, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => lineDelegate(a, b, color);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2DHits(RaycastHit2D[] hits,
			Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			Color color,
			int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;

			direction.EnsureNormalized();

			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			for (int i = 0; i < maxCount; i++)
				DrawCapsule2DFast(origin + direction * hits[i].distance, capsuleStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => lineDelegate(a, b, color);
		}

		#endregion

		#region Both

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Ray ray,
			RaycastHit2D[] hits,
			float distance,
			Color rayColor,
			Color hitColor,
			int maxCount = -1,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(ray.origin, ray.direction * distance, rayColor, duration);
			DrawRaycast2DHits(hits, hitColor, maxCount, hitRayLength, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Vector2 origin,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			Color rayColor,
			Color hitColor,
			int maxCount = -1,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(origin, direction * distance, rayColor, duration);
			DrawRaycast2DHits(hits, hitColor, maxCount, hitRayLength, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2D(
			Vector2 origin,
			float radius,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int count,
			Color startColor,
			Color endColor,
			Color hitColor)
		{
			DrawCircleCast2D(origin, radius, direction, distance, startColor, endColor);
			DrawCircleCast2DHits(hits, origin, radius, direction, hitColor, count);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2D(Vector2 origin,
			Vector2 size,
			float angle,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int count,
			Color startColor,
			Color endColor,
			Color hitColor)
		{
			DrawBoxCast2D(origin, size, angle, direction, distance, startColor, endColor);
			DrawBoxCast2DHits(hits, origin, size, angle, direction, hitColor, count);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2D(Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int count,
			Color startColor,
			Color endColor,
			Color hitColor)
		{
			DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance, startColor, endColor);
			DrawCapsuleCast2DHits(hits, origin, size, capsuleDirection, angle, direction, hitColor, count);
		}

		#endregion
	}
}