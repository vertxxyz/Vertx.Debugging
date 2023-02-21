#if VERTX_PHYSICS_2D
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Vertx.Debugging.Shape;

// ReSharper disable RedundantCast
// ReSharper disable Unity.PreferNonAllocApi

namespace Vertx.Debugging
{
	public static class DrawPhysics2D
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ColliderDistance2D Distance(Collider2D colliderA, Collider2D colliderB)
		{
			ColliderDistance2D distance = Physics2D.Distance(colliderA, colliderB);
			D.raw(new Text(distance.pointB + distance.normal * (distance.distance * 0.5f), distance.distance), DrawPhysicsSettings.Duration);
			D.raw(new Line(distance.pointA, distance.pointB), CastColor, DrawPhysicsSettings.Duration);
			return distance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ClosestPoint(Vector2 position, Collider2D collider)
		{
			Vector2 result = Physics2D.ClosestPoint(position, collider);
			D.raw(new Point2D(result), HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ClosestPoint(Vector2 position, Rigidbody2D rigidbody)
		{
			Vector2 result = Physics2D.ClosestPoint(position, rigidbody);
			D.raw(new Point2D(result), HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Linecast(Vector2 start, Vector2 end)
		{
			RaycastHit2D result = Physics2D.Linecast(start, end);
			D.raw(new Linecast2D(start, end, result), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask)
		{
			RaycastHit2D result = Physics2D.Linecast(start, end, layerMask);
			D.raw(new Linecast2D(start, end, result), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask, float minDepth)
		{
			RaycastHit2D result = Physics2D.Linecast(start, end, layerMask, minDepth);
			D.raw(new Linecast2D(start, end, result, minDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D result = Physics2D.Linecast(start, end, layerMask, minDepth, maxDepth);
			D.raw(new Linecast2D(start, end, result, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Linecast(Vector2 start, Vector2 end, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			int count = Physics2D.Linecast(start, end, contactFilter, results);
			D.raw(new LinecastAll2D(start, end, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Linecast(Vector2 start, Vector2 end, ContactFilter2D contactFilter, List<RaycastHit2D> results)
		{
			int count = Physics2D.Linecast(start, end, contactFilter, results);
			D.raw(new LinecastAll2D(start, end, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end)
		{
			RaycastHit2D[] results = Physics2D.LinecastAll(start, end);
			D.raw(new LinecastAll2D(start, end, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.LinecastAll(start, end, layerMask);
			D.raw(new LinecastAll2D(start, end, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask, float minDepth)
		{
			RaycastHit2D[] results = Physics2D.LinecastAll(start, end, layerMask, minDepth);
			D.raw(new LinecastAll2D(start, end, results, minDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D[] results = Physics2D.LinecastAll(start, end, layerMask, minDepth, maxDepth);
			D.raw(new LinecastAll2D(start, end, results, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results)
		{
			int count = Physics2D.LinecastNonAlloc(start, end, results);
			D.raw(new LinecastAll2D(start, end, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask)
		{
			int count = Physics2D.LinecastNonAlloc(start, end, results, layerMask);
			D.raw(new LinecastAll2D(start, end, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.LinecastNonAlloc(start, end, results, layerMask, minDepth);
			D.raw(new LinecastAll2D(start, end, results, count, minDepth), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.LinecastNonAlloc(start, end, results, layerMask, minDepth, maxDepth);
			D.raw(new LinecastAll2D(start, end, results, count, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction)
		{
			RaycastHit2D result = Physics2D.Raycast(origin, direction);
			D.raw(new Raycast2D(origin, direction, result), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance)
		{
			RaycastHit2D result = Physics2D.Raycast(origin, direction, distance);
			D.raw(new Raycast2D(origin, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D result = Physics2D.Raycast(origin, direction, distance, layerMask);
			D.raw(new Raycast2D(origin, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D result = Physics2D.Raycast(origin, direction, distance, layerMask, minDepth);
			D.raw(new Raycast2D(origin, direction, result, distance, minDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D result = Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new Raycast2D(origin, direction, result, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			int count = Physics2D.Raycast(origin, direction, contactFilter, results);
			D.raw(new RaycastAll2D(origin, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.Raycast(origin, direction, contactFilter, results, distance);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance = Mathf.Infinity)
		{
			int count = Physics2D.Raycast(origin, direction, contactFilter, results, distance);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results)
		{
			int count = Physics2D.RaycastNonAlloc(origin, direction, results);
			D.raw(new RaycastAll2D(origin, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.RaycastNonAlloc(origin, direction, results, distance);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
		{
			int count = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
		{
			int count = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask, minDepth);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance, minDepth), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask, minDepth, maxDepth);
			D.raw(new RaycastAll2D(origin, direction, results, count, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction)
		{
			RaycastHit2D[] results = Physics2D.RaycastAll(origin, direction);
			D.raw(new RaycastAll2D(origin, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance)
		{
			RaycastHit2D[] results = Physics2D.RaycastAll(origin, direction, distance);
			D.raw(new RaycastAll2D(origin, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.RaycastAll(origin, direction, distance, layerMask);
			D.raw(new RaycastAll2D(origin, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D[] results = Physics2D.RaycastAll(origin, direction, distance, layerMask, minDepth);
			D.raw(new RaycastAll2D(origin, direction, results, distance, minDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D[] results = Physics2D.RaycastAll(origin, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new RaycastAll2D(origin, direction, results, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction)
		{
			RaycastHit2D result = Physics2D.CircleCast(origin, radius, direction);
			D.raw(new CircleCast(origin, radius, direction, (RaycastHit2D?)null), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance)
		{
			RaycastHit2D result = Physics2D.CircleCast(origin, radius, direction, distance);
			D.raw(new CircleCast(origin, radius, direction, (RaycastHit2D?)null, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D result = Physics2D.CircleCast(origin, radius, direction, distance, layerMask);
			D.raw(new CircleCast(origin, radius, direction, (RaycastHit2D?)null, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D result = Physics2D.CircleCast(origin, radius, direction, distance, layerMask, minDepth);
			D.raw(new CircleCast(origin, radius, direction, (RaycastHit2D?)null, distance, minDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D result = Physics2D.CircleCast(origin, radius, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new CircleCast(origin, radius, direction, (RaycastHit2D?)null, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			int count = Physics2D.CircleCast(origin, radius, direction, contactFilter, results);
			D.raw(new CircleCastAll(origin, radius, direction, results), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.CircleCast(origin, radius, direction, contactFilter, results, distance);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance = Mathf.Infinity)
		{
			int count = Physics2D.CircleCast(origin, radius, direction, contactFilter, results, distance);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction)
		{
			RaycastHit2D[] results = Physics2D.CircleCastAll(origin, radius, direction);
			D.raw(new CircleCastAll(origin, radius, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance)
		{
			RaycastHit2D[] results = Physics2D.CircleCastAll(origin, radius, direction, distance);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.CircleCastAll(origin, radius, direction, distance, layerMask);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D[] results = Physics2D.CircleCastAll(origin, radius, direction, distance, layerMask, minDepth);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance, minDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D[] results = Physics2D.CircleCastAll(origin, radius, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new CircleCastAll(origin, radius, direction, results, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results)
		{
			int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results);
			D.raw(new CircleCastAll(origin, radius, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance);
			D.raw(new CircleCastAll(origin, radius, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
		{
			int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask);
			D.raw(new CircleCastAll(origin, radius, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
		{
			int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, minDepth);
			D.raw(new CircleCastAll(origin, radius, direction, results, count, distance, minDepth), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, minDepth, maxDepth);
			D.raw(new CircleCastAll(origin, radius, direction, results, count, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction)
		{
			RaycastHit2D result = Physics2D.BoxCast(origin, size, angle, direction);
			D.raw(new BoxCast2D(origin, size, angle, direction, result), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
		{
			RaycastHit2D result = Physics2D.BoxCast(origin, size, angle, direction, distance);
			D.raw(new BoxCast2D(origin, size, angle, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D result = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask);
			D.raw(new BoxCast2D(origin, size, angle, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D result = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask, minDepth);
			D.raw(new BoxCast2D(origin, size, angle, direction, result, distance, minDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D result = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new BoxCast2D(origin, size, angle, direction, result, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			int count = Physics2D.BoxCast(origin, size, angle, direction, contactFilter, results);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.BoxCast(origin, size, angle, direction, contactFilter, results, distance);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance = Mathf.Infinity)
		{
			int count = Physics2D.BoxCast(origin, size, angle, direction, contactFilter, results, distance);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction)
		{
			RaycastHit2D[] results = Physics2D.BoxCastAll(origin, size, angle, direction);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
		{
			RaycastHit2D[] results = Physics2D.BoxCastAll(origin, size, angle, direction, distance);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D[] results = Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, distance, minDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D[] results = Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results)
		{
			int count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
		{
			int count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
		{
			int count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, minDepth);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance, minDepth), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, minDepth, maxDepth);
			D.raw(new BoxCast2DAll(origin, size, angle, direction, results, count, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction)
		{
			RaycastHit2D result = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction);
			D.raw(new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, result), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
		{
			RaycastHit2D result = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance);
			D.raw(new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D result = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask);
			D.raw(new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, result, distance), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D result = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth);
			D.raw(new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, result, distance, minDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D result = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, result, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			int count = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results, distance);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance = Mathf.Infinity)
		{
			int count = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction)
		{
			RaycastHit2D[] results = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
		{
			RaycastHit2D[] results = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, distance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
		{
			RaycastHit2D[] results = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, distance, minDepth), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
		{
			RaycastHit2D[] results = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, maxDepth);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results)
		{
			int count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
		{
			int count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count, distance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
		{
			int count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask, minDepth);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count, distance, minDepth), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask, minDepth, maxDepth);
			D.raw(new CapsuleCast2DAll(origin, size, capsuleDirection, angle, direction, results, count, distance, minDepth, maxDepth), DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D GetRayIntersection(UnityEngine.Ray ray)
		{
			RaycastHit2D result = Physics2D.GetRayIntersection(ray);
			D.raw(ray, DrawPhysicsSettings.Duration);
			if (result.collider)
				D.raw(result, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D GetRayIntersection(UnityEngine.Ray ray, float distance)
		{
			RaycastHit2D result = Physics2D.GetRayIntersection(ray, distance);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			if (result.collider)
				D.raw(result, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D GetRayIntersection(UnityEngine.Ray ray, float distance, int layerMask)
		{
			RaycastHit2D result = Physics2D.GetRayIntersection(ray, distance, layerMask);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			if (result.collider)
				D.raw(result, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] GetRayIntersectionAll(UnityEngine.Ray ray)
		{
			RaycastHit2D[] results = Physics2D.GetRayIntersectionAll(ray);
			D.raw(ray, DrawPhysicsSettings.Duration);
			foreach (RaycastHit2D result in results)
				D.raw(result, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] GetRayIntersectionAll(UnityEngine.Ray ray, float distance)
		{
			RaycastHit2D[] results = Physics2D.GetRayIntersectionAll(ray, distance);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			foreach (RaycastHit2D result in results)
				D.raw(result, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit2D[] GetRayIntersectionAll(UnityEngine.Ray ray, float distance, int layerMask)
		{
			RaycastHit2D[] results = Physics2D.GetRayIntersectionAll(ray, distance, layerMask);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			foreach (RaycastHit2D result in results)
				D.raw(result, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRayIntersectionNonAlloc(UnityEngine.Ray ray, RaycastHit2D[] results)
		{
			int count = Physics2D.GetRayIntersectionNonAlloc(ray, results);
			D.raw(ray, DrawPhysicsSettings.Duration);
			for (var i = 0; i < count; i++)
				D.raw(results[i], DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRayIntersectionNonAlloc(UnityEngine.Ray ray, RaycastHit2D[] results, float distance)
		{
			int count = Physics2D.GetRayIntersectionNonAlloc(ray, results, distance);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			for (var i = 0; i < count; i++)
				D.raw(results[i], DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRayIntersectionNonAlloc(UnityEngine.Ray ray, RaycastHit2D[] results, float distance, int layerMask)
		{
			int count = Physics2D.GetRayIntersectionNonAlloc(ray, results, distance, layerMask);
			D.raw(new Raycast(ray, null, distance), DrawPhysicsSettings.Duration);
			for (var i = 0; i < count; i++)
				D.raw(results[i], DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapPoint(Vector2 point)
		{
			Collider2D result = Physics2D.OverlapPoint(point);
			D.raw(new Point2D(point), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapPoint(Vector2 point, int layerMask)
		{
			Collider2D result = Physics2D.OverlapPoint(point, layerMask);
			D.raw(new Point2D(point), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapPoint(Vector2 point, int layerMask, float minDepth)
		{
			Collider2D result = Physics2D.OverlapPoint(point, layerMask, minDepth);
			D.raw(new Point2D(point, minDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapPoint(Vector2 point, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D result = Physics2D.OverlapPoint(point, layerMask, minDepth, maxDepth);
			D.raw(new Point2D(point, minDepth), result, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Point2D(point, maxDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPoint(Vector2 point, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapPoint(point, contactFilter, results);
			D.raw(new Point2D(point), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPoint(Vector2 point, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapPoint(point, contactFilter, results);
			D.raw(new Point2D(point), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapPointAll(Vector2 point)
		{
			Collider2D[] results = Physics2D.OverlapPointAll(point);
			D.raw(new Point2D(point), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask)
		{
			Collider2D[] results = Physics2D.OverlapPointAll(point, layerMask);
			D.raw(new Point2D(point), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask, float minDepth)
		{
			Collider2D[] results = Physics2D.OverlapPointAll(point, layerMask, minDepth);
			D.raw(new Point2D(point, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D[] results = Physics2D.OverlapPointAll(point, layerMask, minDepth, maxDepth);
			D.raw(new Point2D(point, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Point2D(point, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results)
		{
			int count = Physics2D.OverlapPointNonAlloc(point, results);
			D.raw(new Point2D(point), results.Length > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = results[i];
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			}

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask)
		{
			int count = Physics2D.OverlapPointNonAlloc(point, results, layerMask);
			D.raw(new Point2D(point), results.Length > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = results[i];
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			}

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.OverlapPointNonAlloc(point, results, layerMask, minDepth);
			D.raw(new Point2D(point, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = results[i];
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			}

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.OverlapPointNonAlloc(point, results, layerMask, minDepth, maxDepth);
			D.raw(new Point2D(point, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Point2D(point, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = results[i];
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			}

			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCircle(Vector2 point, float radius)
		{
			Collider2D result = Physics2D.OverlapCircle(point, radius);
			D.raw(new Circle2D(point, radius), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask)
		{
			Collider2D result = Physics2D.OverlapCircle(point, radius, layerMask);
			D.raw(new Circle2D(point, radius), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask, float minDepth)
		{
			Collider2D result = Physics2D.OverlapCircle(point, radius, layerMask, minDepth);
			D.raw(new Circle2D(point, radius, minDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D result = Physics2D.OverlapCircle(point, radius, layerMask, minDepth, maxDepth);
			D.raw(new Circle2D(point, radius, minDepth), result, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Circle2D(point, radius, maxDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircle(Vector2 point, float radius, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapCircle(point, radius, contactFilter, results);
			D.raw(new Circle2D(point, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircle(Vector2 point, float radius, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapCircle(point, radius, contactFilter, results);
			D.raw(new Circle2D(point, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCircleAll(Vector2 point, float radius)
		{
			Collider2D[] results = Physics2D.OverlapCircleAll(point, radius);
			D.raw(new Circle2D(point, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask)
		{
			Collider2D[] results = Physics2D.OverlapCircleAll(point, radius, layerMask);
			D.raw(new Circle2D(point, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask, float minDepth)
		{
			Collider2D[] results = Physics2D.OverlapCircleAll(point, radius, layerMask, minDepth);
			D.raw(new Circle2D(point, radius, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D[] results = Physics2D.OverlapCircleAll(point, radius, layerMask, minDepth, maxDepth);
			D.raw(new Circle2D(point, radius, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Circle2D(point, radius, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results)
		{
			int count = Physics2D.OverlapCircleNonAlloc(point, radius, results);
			D.raw(new Circle2D(point, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask)
		{
			int count = Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask);
			D.raw(new Circle2D(point, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask, minDepth);
			D.raw(new Circle2D(point, radius, minDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask, minDepth, maxDepth);
			D.raw(new Circle2D(point, radius, minDepth), count > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Circle2D(point, radius, minDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle)
		{
			Collider2D result = Physics2D.OverlapBox(point, size, angle);
			D.raw(new Box2D(point, size, angle), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask)
		{
			Collider2D result = Physics2D.OverlapBox(point, size, angle, layerMask);
			D.raw(new Box2D(point, size, angle), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth)
		{
			Collider2D result = Physics2D.OverlapBox(point, size, angle, layerMask, minDepth);
			D.raw(new Box2D(point, size, angle, minDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D result = Physics2D.OverlapBox(point, size, angle, layerMask, minDepth, maxDepth);
			D.raw(new Box2D(point, size, angle, minDepth), result, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Box2D(point, size, angle, maxDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBox(Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapBox(point, size, angle, contactFilter, results);
			D.raw(new Box2D(point, size, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBox(Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapBox(point, size, angle, contactFilter, results);
			D.raw(new Box2D(point, size, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle)
		{
			Collider2D[] results = Physics2D.OverlapBoxAll(point, size, angle);
			D.raw(new Box2D(point, size, angle), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask)
		{
			Collider2D[] results = Physics2D.OverlapBoxAll(point, size, angle, layerMask);
			D.raw(new Box2D(point, size, angle), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth)
		{
			Collider2D[] results = Physics2D.OverlapBoxAll(point, size, angle, layerMask, minDepth);
			D.raw(new Box2D(point, size, angle, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D[] results = Physics2D.OverlapBoxAll(point, size, angle, layerMask, minDepth, maxDepth);
			D.raw(new Box2D(point, size, angle, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Box2D(point, size, angle, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results)
		{
			int count = Physics2D.OverlapBoxNonAlloc(point, size, angle, results);
			D.raw(new Box2D(point, size, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask)
		{
			int count = Physics2D.OverlapBoxNonAlloc(point, size, angle, results, layerMask);
			D.raw(new Box2D(point, size, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.OverlapBoxNonAlloc(point, size, angle, results, layerMask, minDepth);
			D.raw(new Box2D(point, size, angle, minDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.OverlapBoxNonAlloc(point, size, angle, results, layerMask, minDepth, maxDepth);
			D.raw(new Box2D(point, size, angle, minDepth), count > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Box2D(point, size, angle, maxDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB)
		{
			Collider2D result = Physics2D.OverlapArea(pointA, pointB);
			D.raw(new Area2D(pointA, pointB), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask)
		{
			Collider2D result = Physics2D.OverlapArea(pointA, pointB, layerMask);
			D.raw(new Area2D(pointA, pointB), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth)
		{
			Collider2D result = Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D result = Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth, maxDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), result, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Area2D(pointA, pointB, maxDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapArea(Vector2 pointA, Vector2 pointB, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapArea(pointA, pointB, contactFilter, results);
			D.raw(new Box2D(pointA, pointB), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapArea(Vector2 pointA, Vector2 pointB, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapArea(pointA, pointB, contactFilter, results);
			D.raw(new Box2D(pointA, pointB), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB)
		{
			Collider2D[] results = Physics2D.OverlapAreaAll(pointA, pointB);
			D.raw(new Area2D(pointA, pointB), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask)
		{
			Collider2D[] results = Physics2D.OverlapAreaAll(pointA, pointB, layerMask);
			D.raw(new Area2D(pointA, pointB), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth)
		{
			Collider2D[] results = Physics2D.OverlapAreaAll(pointA, pointB, layerMask, minDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D[] results = Physics2D.OverlapAreaAll(pointA, pointB, layerMask, minDepth, maxDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Area2D(pointA, pointB, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results)
		{
			int count = Physics2D.OverlapAreaNonAlloc(pointA, pointB, results);
			D.raw(new Area2D(pointA, pointB), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask)
		{
			int count = Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask);
			D.raw(new Area2D(pointA, pointB), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, maxDepth);
			D.raw(new Area2D(pointA, pointB, minDepth), count > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Area2D(pointA, pointB, maxDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle)
		{
			Collider2D result = Physics2D.OverlapCapsule(point, size, direction, angle);
			D.raw(new Capsule2D(point, size, direction, angle), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask)
		{
			Collider2D result = Physics2D.OverlapCapsule(point, size, direction, angle, layerMask);
			D.raw(new Capsule2D(point, size, direction, angle), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth)
		{
			Collider2D result = Physics2D.OverlapCapsule(point, size, direction, angle, layerMask, minDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D result = Physics2D.OverlapCapsule(point, size, direction, angle, layerMask, minDepth, maxDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), result, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Capsule2D(point, size, direction, angle, maxDepth), result, DrawPhysicsSettings.Duration);
			D.raw(result, HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapCapsule(point, size, direction, angle, contactFilter, results);
			D.raw(new Capsule2D(point, size, direction, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapCapsule(point, size, direction, angle, contactFilter, results);
			D.raw(new Capsule2D(point, size, direction, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle)
		{
			Collider2D[] results = Physics2D.OverlapCapsuleAll(point, size, direction, angle);
			D.raw(new Capsule2D(point, size, direction, angle), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask)
		{
			Collider2D[] results = Physics2D.OverlapCapsuleAll(point, size, direction, angle, layerMask);
			D.raw(new Capsule2D(point, size, direction, angle), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth)
		{
			Collider2D[] results = Physics2D.OverlapCapsuleAll(point, size, direction, angle, layerMask, minDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth, float maxDepth)
		{
			Collider2D[] results = Physics2D.OverlapCapsuleAll(point, size, direction, angle, layerMask, minDepth, maxDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Capsule2D(point, size, direction, angle, maxDepth), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider2D collider in results)
				D.raw(collider, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

#if !UNITY_2023_1_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results)
		{
			int count = Physics2D.OverlapCapsuleNonAlloc(point, size, direction, angle, results);
			D.raw(new Capsule2D(point, size, direction, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask)
		{
			int count = Physics2D.OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask);
			D.raw(new Capsule2D(point, size, direction, angle), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask, float minDepth)
		{
			int count = Physics2D.OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask, minDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
		{
			int count = Physics2D.OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask, minDepth, maxDepth);
			D.raw(new Capsule2D(point, size, direction, angle, minDepth), count > 0, DrawPhysicsSettings.Duration);
			if (maxDepth - minDepth > 0.001f)
				D.raw(new Capsule2D(point, size, direction, angle, maxDepth), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCollider(Collider2D collider, ContactFilter2D contactFilter, Collider2D[] results)
		{
			int count = Physics2D.OverlapCollider(collider, contactFilter, results);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCollider(Collider2D collider, ContactFilter2D contactFilter, List<Collider2D> results)
		{
			int count = Physics2D.OverlapCollider(collider, contactFilter, results);
			for (int i = 0; i < count; i++)
				D.raw(results[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider1, Collider2D collider2, ContactFilter2D contactFilter, ContactPoint2D[] contacts)
		{
			int count = Physics2D.GetContacts(collider1, collider2, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, ContactPoint2D[] contacts)
		{
			int count = Physics2D.GetContacts(collider, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, ContactFilter2D contactFilter, ContactPoint2D[] contacts)
		{
			int count = Physics2D.GetContacts(collider, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, Collider2D[] colliders)
		{
			int count = Physics2D.GetContacts(collider, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, ContactFilter2D contactFilter, Collider2D[] colliders)
		{
			int count = Physics2D.GetContacts(collider, contactFilter, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, ContactPoint2D[] contacts)
		{
			int count = Physics2D.GetContacts(rigidbody, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, ContactFilter2D contactFilter, ContactPoint2D[] contacts)
		{
			int count = Physics2D.GetContacts(rigidbody, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, Collider2D[] colliders)
		{
			int count = Physics2D.GetContacts(rigidbody, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, ContactFilter2D contactFilter, Collider2D[] colliders)
		{
			int count = Physics2D.GetContacts(rigidbody, contactFilter, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider1, Collider2D collider2, ContactFilter2D contactFilter, List<ContactPoint2D> contacts)
		{
			int count = Physics2D.GetContacts(collider1, collider2, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, List<ContactPoint2D> contacts)
		{
			int count = Physics2D.GetContacts(collider, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, ContactFilter2D contactFilter, List<ContactPoint2D> contacts)
		{
			int count = Physics2D.GetContacts(collider, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, List<Collider2D> colliders)
		{
			int count = Physics2D.GetContacts(collider, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Collider2D collider, ContactFilter2D contactFilter, List<Collider2D> colliders)
		{
			int count = Physics2D.GetContacts(collider, contactFilter, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, List<ContactPoint2D> contacts)
		{
			int count = Physics2D.GetContacts(rigidbody, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, ContactFilter2D contactFilter, List<ContactPoint2D> contacts)
		{
			int count = Physics2D.GetContacts(rigidbody, contactFilter, contacts);
			for (int i = 0; i < count; i++)
				D.raw(new Shape.Ray2D(contacts[i].point, contacts[i].normal, contacts[i].collider.transform.position.z), HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, List<Collider2D> colliders)
		{
			int count = Physics2D.GetContacts(rigidbody, colliders);
			for (int i = 0; i < count; i++)
				D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetContacts(Rigidbody2D rigidbody, ContactFilter2D contactFilter, List<Collider2D> colliders)
		{
        	int count = Physics2D.GetContacts(rigidbody, contactFilter, colliders);
        	for (int i = 0; i < count; i++)
        		D.raw(colliders[i], HitColor, DrawPhysicsSettings.Duration);
        	return count;
        }
	}
}
#endif