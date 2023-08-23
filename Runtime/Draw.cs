using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Vertx.Debugging
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class D
	{
#if UNITY_EDITOR
		private static ref UnmanagedCommandBuilder s_Builder => ref UnmanagedCommandBuilder.Instance.Data;
#endif

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			shape.Draw(ref s_Builder, Color.white, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color color, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			shape.Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, bool hit, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			shape.Draw(ref s_Builder, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color castColor, Color hitColor, float duration = 0) where T : struct, IDrawableCast
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			shape.Draw(ref s_Builder, castColor, hitColor, duration);
#endif
		}
		// ------ Conversion for Unity types ------

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Ray(ray.origin, ray.direction).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Ray(new float3(ray.origin.x, ray.origin.y, 0), new float3(ray.direction.x, ray.direction.y, 0)).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Point(position).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Point2D(position).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Box(bounds).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Box(bounds).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Box2D(rect.center, rect.size).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, float duration = 0) => raw(rect, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Box2D(rect.center, rect.size.xy()).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, float duration = 0) => raw(rect, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

#if VERTX_PHYSICS
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.SurfacePoint(hit.point, hit.normal).Draw(ref s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, float duration = 0) => raw(hit, Shape.HitColor, duration);

		/// <summary>
		/// Draws a <see cref="Collider"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider"/>, <see cref="SphereCollider"/>, <see cref="CapsuleCollider"/>, and <see cref="CharacterController"/>. <see cref="MeshCollider"/> will draw as bounds.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			switch (collider)
			{
				case BoxCollider boxCollider:
					new Shape.Box(boxCollider).Draw(ref s_Builder, color, duration);
					break;
				case SphereCollider sphereCollider:
					new Shape.Sphere(sphereCollider).Draw(ref s_Builder, color, duration);
					break;
				case CapsuleCollider capsuleCollider:
					new Shape.Capsule(capsuleCollider).Draw(ref s_Builder, color, duration);
					break;
				case CharacterController characterController:
					new Shape.Capsule(characterController).Draw(ref s_Builder, color, duration);
					break;
				case MeshCollider meshCollider:
					raw(meshCollider.bounds);
					break;
				default:
					// Could be null
					return;
			}
#endif
		}

		/// <summary>
		/// Draws a <see cref="Collider"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider"/>, <see cref="SphereCollider"/>, <see cref="CapsuleCollider"/>, and <see cref="CharacterController"/>. <see cref="MeshCollider"/> will draw as bounds.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, float duration = 0) => raw(collider, Color.white, duration);

		/// <summary>
		/// Draws a <see cref="Collider"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider"/>, <see cref="SphereCollider"/>, <see cref="CapsuleCollider"/>, and <see cref="CharacterController"/>. <see cref="MeshCollider"/> will draw as bounds.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, bool hit, float duration = 0) => raw(collider, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif

#if VERTX_PHYSICS_2D

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!hit)
				return;
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			new Shape.Ray(new float3(hit.point.x, hit.point.y, hit.transform.position.z), new float3(hit.normal.x, hit.normal.y, 0)).Draw(ref s_Builder, color, duration);
#endif
		}


		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, float duration = 0) => raw(hit, Shape.HitColor, duration);

		/// <summary>
		/// Draws a <see cref="Collider2D"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider2D"/>, <see cref="CircleCollider2D"/>, and <see cref="CapsuleCollider2D"/>.<br/>
		/// <see cref="BoxCollider2D.edgeRadius"/> does not support skew.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			if (!CommandBuilder.Instance.TryGetAdjustedDuration(ref duration))
				return;
			
			switch (collider)
			{
				case BoxCollider2D boxCollider:
					new Shape.Box2DWithEdgeRadius(boxCollider).Draw(ref s_Builder, color, duration);
					break;
				case CircleCollider2D circleCollider2D:
					new Shape.Circle2D(circleCollider2D).Draw(ref s_Builder, color, duration);
					break;
				case CapsuleCollider2D capsuleCollider:
					new Shape.Capsule2D(capsuleCollider).Draw(ref s_Builder, color, duration);
					break;
				default:
					// Could be null
					return;
			}
#endif
		}

		/// <summary>
		/// Draws a <see cref="Collider2D"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider2D"/>, <see cref="CircleCollider2D"/>, and <see cref="CapsuleCollider2D"/>.<br/>
		/// <see cref="BoxCollider2D.edgeRadius"/> does not support skew.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, float duration = 0) => raw(collider, Color.white, duration);

		/// <summary>
		/// Draws a <see cref="Collider2D"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider2D"/>, <see cref="CircleCollider2D"/>, and <see cref="CapsuleCollider2D"/>.<br/>
		/// <see cref="BoxCollider2D.edgeRadius"/> does not support skew.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, bool hit, float duration = 0) => raw(collider, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
	}

	public interface IDrawable
	{
#if UNITY_EDITOR
		internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration);
#endif
	}

	public interface IDrawableCast : IDrawable
	{
#if UNITY_EDITOR
		internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration);
#endif
	}
}