using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		#region Casts

		#region CircleCast2D

		public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance)
			=> DrawCircleCast2D(origin, radius, direction, distance, StartColor, EndColor);

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
			
			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region BoxCast2D

		public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
			=> BoxCast2D(origin, size, angle, direction, distance, StartColor, EndColor);

		public static void BoxCast2D(
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

				DrawLineV(currentA, nextA, t);
				DrawLineV(currentB, nextB, t);

				currentA = nextA;
				currentB = nextB;
			}

			color = colorEnd;
			DrawBox2DFast(scaledDirection, boxStructure2D, DrawLine);

			void DrawLineV(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region CapsuleCast2D

		public static void DrawCapsuleCast2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
			=> DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance, StartColor, EndColor);

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

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#endregion
		
		#region RaycastHits

		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, int maxCount = -1)
			=> DrawBoxCast2DHits(hits, origin, size, angle, direction, HitColor, maxCount);

		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;
			
			direction.EnsureNormalized();
			
			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);
			
			for (int i = 0; i < maxCount; i++)
				DrawBox2DFast(direction * hits[i].distance, boxStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}
		
		public static void DrawCircleCast2DHits(RaycastHit2D[] hits, Vector2 origin, float radius, Vector2 direction, int maxCount = -1)
			=> DrawCircleCast2DHits(hits, origin, radius, direction, HitColor, maxCount);

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

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}
		
		public static void DrawCapsuleCast2DHits(RaycastHit2D[] hits, Vector2 origin, 
			Vector2 size,
			CapsuleDirection2D capsuleDirection, 
			float angle,
			Vector2 direction, int maxCount = -1)
			=> DrawCapsuleCast2DHits(hits, origin, size, capsuleDirection, angle, direction, HitColor, maxCount);

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

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region Both

		public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] hits, float distance, int count)
		{
			DrawCircleCast2D(origin, radius, direction, distance);
			DrawCircleCast2DHits(hits, origin, radius, direction, count);
		}
		
		public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] hits, float distance, int count)
		{
			DrawBoxCast2D(origin, size, angle, direction, distance);
			DrawBoxCast2DHits(hits, origin, size, angle, direction, count);
		}
		
		public static void DrawCapsuleCast2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] hits, float distance, int count)
		{
			DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance);
			DrawCapsuleCast2DHits(hits, origin, size, capsuleDirection, angle, direction, count);
		}

		#endregion
	}
}