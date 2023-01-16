#if VERTX_PHYSICS
using UnityEngine;
using System.Runtime.CompilerServices;
using static Vertx.Debugging.Shape;

// ReSharper disable RedundantCast
// ReSharper disable Unity.PreferNonAllocApi

namespace Vertx.Debugging
{
	public static class DrawPhysics
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Raycast(origin, direction, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new Shape.Ray(origin, direction, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
		{
			bool hit = Physics.Raycast(origin, direction, maxDistance, layerMask);
			D.raw(new Shape.Ray(origin, direction, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance)
		{
			bool hit = Physics.Raycast(origin, direction, maxDistance);
			D.raw(new Shape.Ray(origin, direction, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction)
		{
			bool hit = Physics.Raycast(origin, direction);
			D.raw(new Shape.Ray(origin, direction, Mathf.Infinity), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new Raycast(origin, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
			D.raw(new Raycast(origin, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
		{
			bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance);
			D.raw(new Raycast(origin, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo)
		{
			bool hit = Physics.Raycast(origin, direction, out hitInfo);
			D.raw(new Raycast(origin, direction, hitInfo), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Raycast(ray, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new Shape.Ray(ray, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, float maxDistance, int layerMask)
		{
			bool hit = Physics.Raycast(ray, maxDistance, layerMask);
			D.raw(new Shape.Ray(ray, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, float maxDistance)
		{
			bool hit = Physics.Raycast(ray, maxDistance);
			D.raw(new Shape.Ray(ray, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray)
		{
			bool hit = Physics.Raycast(ray);
			D.raw(new Shape.Ray(ray), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new Raycast(ray, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			bool hit = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
			D.raw(new Raycast(ray, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, out RaycastHit hitInfo, float maxDistance)
		{
			bool hit = Physics.Raycast(ray, out hitInfo, maxDistance);
			D.raw(new Raycast(ray, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Raycast(UnityEngine.Ray ray, out RaycastHit hitInfo)
		{
			bool hit = Physics.Raycast(ray, out hitInfo);
			D.raw(new Raycast(ray, hitInfo), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Linecast(start, end, layerMask, queryTriggerInteraction);
			D.raw(new Line(start, end), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end, int layerMask)
		{
			bool hit = Physics.Linecast(start, end, layerMask);
			D.raw(new Line(start, end), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end)
		{
			bool hit = Physics.Linecast(start, end);
			D.raw(new Line(start, end), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.Linecast(start, end, out hitInfo, layerMask, queryTriggerInteraction);
			D.raw(new Linecast(start, end, hitInfo), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask)
		{
			bool hit = Physics.Linecast(start, end, out hitInfo, layerMask);
			D.raw(new Linecast(start, end, hitInfo), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo)
		{
			bool hit = Physics.Linecast(start, end, out hitInfo);
			D.raw(new Linecast(start, end, hitInfo), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, maxDistance, layerMask);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, maxDistance);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, null), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo)
		{
			bool hit = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo);
			D.raw(new CapsuleCast(new Capsule(point1, point2, radius), direction, hitInfo), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCast(origin, radius, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask);
			D.raw(new SphereCast(origin, radius, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
		{
			bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance);
			D.raw(new SphereCast(origin, radius, direction, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo)
		{
			bool hit = Physics.SphereCast(origin, radius, direction, out hitInfo);
			D.raw(new SphereCast(origin, radius, direction, hitInfo), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.SphereCast(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCast(ray, radius, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, float maxDistance, int layerMask)
		{
			bool hit = Physics.SphereCast(ray, radius, maxDistance, layerMask);
			D.raw(new SphereCast(ray, radius, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, float maxDistance)
		{
			bool hit = Physics.SphereCast(ray, radius, maxDistance);
			D.raw(new SphereCast(ray, radius, null, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius)
		{
			bool hit = Physics.SphereCast(ray, radius);
			D.raw(new SphereCast(ray, radius, null), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCast(ray, radius, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			bool hit = Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask);
			D.raw(new SphereCast(ray, radius, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, out RaycastHit hitInfo, float maxDistance)
		{
			bool hit = Physics.SphereCast(ray, radius, out hitInfo, maxDistance);
			D.raw(new SphereCast(ray, radius, hit ? hitInfo : (RaycastHit?)null, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SphereCast(UnityEngine.Ray ray, float radius, out RaycastHit hitInfo)
		{
			bool hit = Physics.SphereCast(ray, radius, out hitInfo);
			D.raw(new SphereCast(ray, radius, hitInfo), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new BoxCast(center, halfExtents, direction, null, orientation, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance, layerMask);
			D.raw(new BoxCast(center, halfExtents, direction, null, orientation, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance);
			D.raw(new BoxCast(center, halfExtents, direction, null, orientation, maxDistance), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, orientation);
			D.raw(new BoxCast(center, halfExtents, direction, null, orientation), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction);
			D.raw(new BoxCast(center, halfExtents, direction, null), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new BoxCast(center, halfExtents, direction, hit ? hitInfo : (RaycastHit?)null, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation, float maxDistance, int layerMask)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask);
			D.raw(new BoxCast(center, halfExtents, direction, hit ? hitInfo : (RaycastHit?)null, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation, float maxDistance)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance);
			D.raw(new BoxCast(center, halfExtents, direction, hit ? hitInfo : (RaycastHit?)null, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, Quaternion orientation)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation);
			D.raw(new BoxCast(center, halfExtents, direction, hit ? hitInfo : (RaycastHit?)null, orientation), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo)
		{
			bool hit = Physics.BoxCast(center, halfExtents, direction, out hitInfo);
			D.raw(new BoxCast(center, halfExtents, direction, hit ? hitInfo : (RaycastHit?)null), DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.RaycastAll(origin, direction, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new RaycastAll(origin, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.RaycastAll(origin, direction, maxDistance, layerMask);
			D.raw(new RaycastAll(origin, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance)
		{
			RaycastHit[] results = Physics.RaycastAll(origin, direction, maxDistance);
			D.raw(new RaycastAll(origin, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction)
		{
			RaycastHit[] results = Physics.RaycastAll(origin, direction);
			D.raw(new RaycastAll(origin, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(UnityEngine.Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new RaycastAll(ray, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(UnityEngine.Ray ray, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.RaycastAll(ray, maxDistance, layerMask);
			D.raw(new RaycastAll(ray, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(UnityEngine.Ray ray, float maxDistance)
		{
			RaycastHit[] results = Physics.RaycastAll(ray, maxDistance);
			D.raw(new RaycastAll(ray, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] RaycastAll(UnityEngine.Ray ray)
		{
			RaycastHit[] results = Physics.RaycastAll(ray);
			D.raw(new RaycastAll(ray, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(UnityEngine.Ray ray, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new RaycastAll(ray, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(UnityEngine.Ray ray, RaycastHit[] results, float maxDistance, int layerMask)
		{
			int count = Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask);
			D.raw(new RaycastAll(ray, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(UnityEngine.Ray ray, RaycastHit[] results, float maxDistance)
		{
			int count = Physics.RaycastNonAlloc(ray, results, maxDistance);
			D.raw(new RaycastAll(ray, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(UnityEngine.Ray ray, RaycastHit[] results)
		{
			int count = Physics.RaycastNonAlloc(ray, results);
			D.raw(new RaycastAll(ray, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.RaycastNonAlloc(origin, direction, results, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new RaycastAll(origin, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
		{
			int count = Physics.RaycastNonAlloc(origin, direction, results, maxDistance, layerMask);
			D.raw(new RaycastAll(origin, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance)
		{
			int count = Physics.RaycastNonAlloc(origin, direction, results, maxDistance);
			D.raw(new RaycastAll(origin, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results)
		{
			int count = Physics.RaycastNonAlloc(origin, direction, results);
			D.raw(new RaycastAll(origin, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance, layerMask);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance)
		{
			RaycastHit[] results = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction)
		{
			RaycastHit[] results = Physics.CapsuleCastAll(point1, point2, radius, direction);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.SphereCastAll(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCastAll(origin, radius, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.SphereCastAll(origin, radius, direction, maxDistance, layerMask);
			D.raw(new SphereCastAll(origin, radius, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance)
		{
			RaycastHit[] results = Physics.SphereCastAll(origin, radius, direction, maxDistance);
			D.raw(new SphereCastAll(origin, radius, direction, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction)
		{
			RaycastHit[] results = Physics.SphereCastAll(origin, radius, direction);
			D.raw(new SphereCastAll(origin, radius, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(UnityEngine.Ray ray, float radius, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.SphereCastAll(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCastAll(ray, radius, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(UnityEngine.Ray ray, float radius, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.SphereCastAll(ray, radius, maxDistance, layerMask);
			D.raw(new SphereCastAll(ray, radius, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(UnityEngine.Ray ray, float radius, float maxDistance)
		{
			RaycastHit[] results = Physics.SphereCastAll(ray, radius, maxDistance);
			D.raw(new SphereCastAll(ray, radius, results, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] SphereCastAll(UnityEngine.Ray ray, float radius)
		{
			RaycastHit[] results = Physics.SphereCastAll(ray, radius);
			D.raw(new SphereCastAll(ray, radius, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			Collider[] results = Physics.OverlapCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
			D.raw(new Capsule(point0, point1, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask)
		{
			Collider[] results = Physics.OverlapCapsule(point0, point1, radius, layerMask);
			D.raw(new Capsule(point0, point1, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius)
		{
			Collider[] results = Physics.OverlapCapsule(point0, point1, radius);
			D.raw(new Capsule(point0, point1, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			Collider[] results = Physics.OverlapSphere(position, radius, layerMask, queryTriggerInteraction);
			D.raw(new Sphere(position, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask)
		{
			Collider[] results = Physics.OverlapSphere(position, radius, layerMask);
			D.raw(new Sphere(position, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapSphere(Vector3 position, float radius)
		{
			Collider[] results = Physics.OverlapSphere(position, radius);
			D.raw(new Sphere(position, radius), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ComputePenetration(Collider colliderA, Vector3 positionA, Quaternion rotationA, Collider colliderB, Vector3 positionB, Quaternion rotationB, out Vector3 direction, out float distance)
		{
#if UNITY_EDITOR
			bool result = Physics.ComputePenetration(colliderA, positionA, rotationA, colliderB, positionB, rotationB, out direction, out distance);

			if (!result)
				return false;

			Vector3 closestPoint = Physics.ClosestPoint(positionA, colliderB, positionB, rotationB);
			Vector3 offset = direction * distance;
			D.raw(new Arrow(closestPoint - offset, offset), HitColor, DrawPhysicsSettings.Duration);
			return true;
#else
			return Physics.ComputePenetration(colliderA, positionA, rotationA, colliderB, positionB, rotationB, out direction, out distance);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ClosestPoint(Vector3 point, Collider collider, Vector3 position, Quaternion rotation)
		{
			Vector3 result = Physics.ClosestPoint(point, collider, position, rotation);
			D.raw(new Arrow(new Line(point, result)), HitColor, DrawPhysicsSettings.Duration);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);
			D.raw(new Sphere(position, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask)
		{
			int count = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask);
			D.raw(new Sphere(position, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results)
		{
			int count = Physics.OverlapSphereNonAlloc(position, radius, results);
			D.raw(new Sphere(position, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.CheckSphere(position, radius, layerMask, queryTriggerInteraction);
			D.raw(new Sphere(position, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckSphere(Vector3 position, float radius, int layerMask)
		{
			bool hit = Physics.CheckSphere(position, radius, layerMask);
			D.raw(new Sphere(position, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckSphere(Vector3 position, float radius)
		{
			bool hit = Physics.CheckSphere(position, radius);
			D.raw(new Sphere(position, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
		{
			int count = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance)
		{
			int count = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results)
		{
			int count = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results);
			D.raw(new CapsuleCastAll(new Capsule(point1, point2, radius), direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCastAll(origin, radius, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
		{
			int count = Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask);
			D.raw(new SphereCastAll(origin, radius, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance)
		{
			int count = Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance);
			D.raw(new SphereCastAll(origin, radius, direction, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results)
		{
			int count = Physics.SphereCastNonAlloc(origin, radius, direction, results);
			D.raw(new SphereCastAll(origin, radius, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(UnityEngine.Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new SphereCastAll(ray, radius, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(UnityEngine.Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask)
		{
			int count = Physics.SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask);
			D.raw(new SphereCastAll(ray, radius, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(UnityEngine.Ray ray, float radius, RaycastHit[] results, float maxDistance)
		{
			int count = Physics.SphereCastNonAlloc(ray, radius, results, maxDistance);
			D.raw(new SphereCastAll(ray, radius, results, count, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SphereCastNonAlloc(UnityEngine.Ray ray, float radius, RaycastHit[] results)
		{
			int count = Physics.SphereCastNonAlloc(ray, radius, results);
			D.raw(new SphereCastAll(ray, radius, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.CheckCapsule(start, end, radius, layerMask, queryTriggerInteraction);
			D.raw(new Capsule(start, end, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask)
		{
			bool hit = Physics.CheckCapsule(start, end, radius, layerMask);
			D.raw(new Capsule(start, end, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckCapsule(Vector3 start, Vector3 end, float radius)
		{
			bool hit = Physics.CheckCapsule(start, end, radius);
			D.raw(new Capsule(start, end, radius), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layermask, QueryTriggerInteraction queryTriggerInteraction)
		{
			bool hit = Physics.CheckBox(center, halfExtents, orientation, layermask, queryTriggerInteraction);
			D.raw(new Box(center, halfExtents, orientation), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layermask)
		{
			bool hit = Physics.CheckBox(center, halfExtents, orientation, layermask);
			D.raw(new Box(center, halfExtents, orientation), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
		{
			bool hit = Physics.CheckBox(center, halfExtents, orientation);
			D.raw(new Box(center, halfExtents, orientation), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckBox(Vector3 center, Vector3 halfExtents)
		{
			bool hit = Physics.CheckBox(center, halfExtents);
			D.raw(new Box(center, halfExtents), hit, DrawPhysicsSettings.Duration);
			return hit;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			Collider[] results = Physics.OverlapBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
			D.raw(new Box(center, halfExtents, orientation), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask)
		{
			Collider[] results = Physics.OverlapBox(center, halfExtents, orientation, layerMask);
			D.raw(new Box(center, halfExtents, orientation), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
		{
			Collider[] results = Physics.OverlapBox(center, halfExtents, orientation);
			D.raw(new Box(center, halfExtents, orientation), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents)
		{
			Collider[] results = Physics.OverlapBox(center, halfExtents);
			D.raw(new Box(center, halfExtents), results.Length > 0, DrawPhysicsSettings.Duration);
			foreach (Collider c in results)
				D.raw(c.bounds, HitColor, DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, mask, queryTriggerInteraction);
			D.raw(new Box(center, halfExtents, orientation), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask)
		{
			int count = Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, mask);
			D.raw(new Box(center, halfExtents, orientation), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation)
		{
			int count = Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation);
			D.raw(new Box(center, halfExtents, orientation), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results)
		{
			int count = Physics.OverlapBoxNonAlloc(center, halfExtents, results);
			D.raw(new Box(center, halfExtents), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, count, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation)
		{
			int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, count, orientation), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance)
		{
			int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, count, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance, int layerMask)
		{
			int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, count, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results)
		{
			int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, results);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, count), DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			RaycastHit[] results = Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask)
		{
			RaycastHit[] results = Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance)
		{
			RaycastHit[] results = Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, orientation, maxDistance), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation)
		{
			RaycastHit[] results = Physics.BoxCastAll(center, halfExtents, direction, orientation);
			D.raw(new BoxCastAll(center, halfExtents, direction, results, orientation), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction)
		{
			RaycastHit[] results = Physics.BoxCastAll(center, halfExtents, direction);
			D.raw(new BoxCastAll(center, halfExtents, direction, results), DrawPhysicsSettings.Duration);
			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			int count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, queryTriggerInteraction);
			D.raw(new Capsule(point0, point1, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results, int layerMask)
		{
			int count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask);
			D.raw(new Capsule(point0, point1, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results)
		{
			int count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results);
			D.raw(new Capsule(point0, point1, radius), count > 0, DrawPhysicsSettings.Duration);
			for (int i = 0; i < count; i++)
				D.raw(results[i].bounds, HitColor, DrawPhysicsSettings.Duration);
			return count;
		}
	}
}
#endif