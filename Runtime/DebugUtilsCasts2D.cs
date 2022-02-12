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
			Color colorEnd,
			float duration = 0)
		{
			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;

			Color color = colorStart;
			DrawCircleFast(origin, back, up, radius, color, duration);

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

				lineDelegate(currentA, nextA, color, duration);
				lineDelegate(currentB, nextB, color, duration);

				currentA = nextA;
				currentB = nextB;
			}


			color = colorEnd;
			DrawCircleFast(origin + scaledDirection, back, up, radius, color, duration);
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
			Color colorEnd,
			float duration = 0)
		{
			direction.EnsureNormalized();

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			Color color = colorStart;
			DrawBox2DFast(Vector2.zero, boxStructure2D, color, duration);

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

				lineDelegate(currentA, nextA, color, duration);
				lineDelegate(currentB, nextB, color, duration);

				currentA = nextA;
				currentB = nextB;
			}

			color = colorEnd;
			DrawBox2DFast(scaledDirection, boxStructure2D, color, duration);
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
			Color colorEnd,
			float duration = 0)
		{
			direction.EnsureNormalized();

			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			Color color = colorStart;
			DrawCapsule2DFast(origin, capsuleStructure2D, color, duration);

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

				lineDelegate(currentA, nextA, color, duration);
				lineDelegate(currentB, nextB, color, duration);

				currentA = nextA;
				currentB = nextB;
			}

			color = colorEnd;
			DrawCapsule2DFast(destination, capsuleStructure2D, color, duration);
		}

		#endregion

		#endregion

		#region RaycastHits
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2DHit(RaycastHit2D hit, Color color, float rayLength = 1, float duration = 0) 
			=> rayDelegate(hit.point, hit.normal * rayLength, color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2DHits(RaycastHit2D[] hits, Color color, int hitCount = -1, float rayLength = 1, float duration = 0)
		{
			if (hitCount < 0)
				hitCount = hits.Length;
			for (int i = 0; i < hitCount; i++)
				rayDelegate(hits[i].point, hits[i].normal * rayLength, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2DHit(RaycastHit2D hit, Vector2 origin, Vector2 size, float angle, Vector2 direction, Color color, float duration = 0)
		{
			if (hit.collider == null) return;
			direction.EnsureNormalized();

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			DrawBox2DFast(direction * hit.distance, boxStructure2D, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, Color color, int hitCount = -1, float duration = 0)
		{
			if (hitCount < 0)
				hitCount = hits.Length;

			if (hitCount == 0) return;

			direction.EnsureNormalized();

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			for (int i = 0; i < hitCount; i++)
				DrawBox2DFast(direction * hits[i].distance, boxStructure2D, color, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2DHit(RaycastHit2D hit, Vector2 origin, float radius, Vector2 direction, Color color, float duration = 0)
		{
			if (hit.collider == null) return;
			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;

			DrawCircleFast(origin + direction * hit.distance, back, up, radius, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2DHits(RaycastHit2D[] hits, Vector2 origin, float radius, Vector2 direction, Color color, int hitCount = -1, float duration = 0)
		{
			if (hitCount < 0)
				hitCount = hits.Length;

			if (hitCount == 0) return;

			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;

			for (int i = 0; i < hitCount; i++)
				DrawCircleFast(origin + direction * hits[i].distance, back, up, radius, color, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2DHit(RaycastHit2D hit,
			Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			Color color,
			float duration = 0)
		{
			if (hit.collider == null) return;

			direction.EnsureNormalized();

			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			DrawCapsule2DFast(origin + direction * hit.distance, capsuleStructure2D, color, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2DHits(RaycastHit2D[] hits,
			Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			Color color,
			int hitCount = -1,
			float duration = 0)
		{
			if (hitCount < 0)
				hitCount = hits.Length;

			if (hitCount == 0) return;

			direction.EnsureNormalized();

			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			for (int i = 0; i < hitCount; i++)
				DrawCapsule2DFast(origin + direction * hits[i].distance, capsuleStructure2D, color, duration);
		}

		#endregion

		#region Both
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Ray ray,
			RaycastHit2D hit,
			float distance,
			Color rayColor,
			Color hitColor,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(ray.origin, ray.direction * distance, rayColor, duration);
			DrawRaycast2DHit(hit, hitColor, hitRayLength, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Ray ray,
			RaycastHit2D[] hits,
			float distance,
			Color rayColor,
			Color hitColor,
			int hitCount = -1,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(ray.origin, ray.direction * distance, rayColor, duration);
			DrawRaycast2DHits(hits, hitColor, hitCount, hitRayLength, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Vector2 origin,
			Vector2 direction,
			RaycastHit2D hit,
			float distance,
			Color rayColor,
			Color hitColor,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(origin, direction * distance, rayColor, duration);
			DrawRaycast2DHit(hit, hitColor, hitRayLength, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawRaycast2D(
			Vector2 origin,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			Color rayColor,
			Color hitColor,
			int hitCount = -1,
			float hitRayLength = 1,
			float duration = 0)
		{
			if (float.IsInfinity(distance))
				distance = 10000000;
			rayDelegate(origin, direction * distance, rayColor, duration);
			DrawRaycast2DHits(hits, hitColor, hitCount, hitRayLength, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2D(
			Vector2 origin,
			float radius,
			Vector2 direction,
			RaycastHit2D hit,
			float distance,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawCircleCast2D(origin, radius, direction, distance, startColor, endColor, duration);
			DrawCircleCast2DHit(hit, origin, radius, direction, hitColor, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircleCast2D(
			Vector2 origin,
			float radius,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int hitCount,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawCircleCast2D(origin, radius, direction, distance, startColor, endColor, duration);
			DrawCircleCast2DHits(hits, origin, radius, direction, hitColor, hitCount, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2D(Vector2 origin,
			Vector2 size,
			float angle,
			Vector2 direction,
			RaycastHit2D hit,
			float distance,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawBoxCast2D(origin, size, angle, direction, distance, startColor, endColor, duration);
			DrawBoxCast2DHit(hit, origin, size, angle, direction, hitColor, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawBoxCast2D(Vector2 origin,
			Vector2 size,
			float angle,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int hitCount,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawBoxCast2D(origin, size, angle, direction, distance, startColor, endColor, duration);
			DrawBoxCast2DHits(hits, origin, size, angle, direction, hitColor, hitCount, duration);
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2D(Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			RaycastHit2D hit,
			float distance,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance, startColor, endColor, duration);
			DrawCapsuleCast2DHit(hit, origin, size, capsuleDirection, angle, direction, hitColor, duration);
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawCapsuleCast2D(Vector2 origin,
			Vector2 size,
			CapsuleDirection2D capsuleDirection,
			float angle,
			Vector2 direction,
			RaycastHit2D[] hits,
			float distance,
			int hitCount,
			Color startColor,
			Color endColor,
			Color hitColor,
			float duration = 0)
		{
			DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance, startColor, endColor, duration);
			DrawCapsuleCast2DHits(hits, origin, size, capsuleDirection, angle, direction, hitColor, hitCount, duration);
		}

		#endregion
	}
}