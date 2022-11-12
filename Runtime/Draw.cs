using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Vertx.Debugging
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class D
	{
#if UNITY_EDITOR
		private static readonly CommandBuilder s_Builder = CommandBuilder.Instance;
#endif

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, Color.white, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color color, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, color, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, bool hit, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color castColor, Color hitColor, float duration = 0) where T : struct, IDrawableCast
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, castColor, hitColor, duration);
#endif
		}

		// ------ Conversion for Unity types ------

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, Color color, float duration = 0) => raw(new Shape.Ray(ray.origin, ray.direction), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0) => raw(new Shape.Point(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, Color color, float duration = 0) => raw(new Shape.Point2D(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, float duration = 0) => raw(position, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0) => raw(new Shape.Box(bounds), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, Color color, float duration = 0) => raw(new Shape.Box2D(rect.center, rect.size), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, float duration = 0) => raw(rect, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

#if VERTX_PHYSICS
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0) => raw(new Shape.SurfacePoint(hit.point, hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, float duration = 0) => raw(hit, Shape.HitColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			switch (collider)
			{
				case BoxCollider boxCollider:
					new Shape.Box(boxCollider).Draw(s_Builder, color, duration);
					break;
				case CapsuleCollider capsuleCollider:
					new Shape.Capsule(capsuleCollider).Draw(s_Builder, color, duration);
					break;
				case CharacterController characterController:
					new Shape.Capsule(characterController).Draw(s_Builder, color, duration);
					break;
				case MeshCollider meshCollider:
					raw(meshCollider.bounds);
					break;
				case SphereCollider sphereCollider:
					new Shape.Sphere(sphereCollider).Draw(s_Builder, color, duration);
					break;
				default:
					// Could be null
					return;
			}
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, float duration = 0) => raw(collider, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider collider, bool hit, float duration = 0) => raw(collider, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
		
#if VERTX_PHYSICS_2D
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, Color color, float duration = 0) => raw(new Shape.Ray(new Vector3(hit.point.x, hit.point.y, hit.transform.position.z), hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, float duration = 0) => raw(hit, Shape.HitColor, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			switch (collider)
			{
				case BoxCollider2D boxCollider:
					new Shape.Box2D(boxCollider).Draw(s_Builder, color, duration);
					break;
				case CapsuleCollider2D capsuleCollider:
					new Shape.Capsule2D(capsuleCollider).Draw(s_Builder, color, duration);
					break;
				case CircleCollider2D circleCollider2D:
					new Shape.Circle2D(circleCollider2D).Draw(s_Builder, color, duration);
					break;
				default:
					// Could be null
					return;
			}
#endif
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, float duration = 0) => raw(collider, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, bool hit, float duration = 0) => raw(collider, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
	}

	public interface IDrawable
	{
#if UNITY_EDITOR
		void Draw(CommandBuilder commandBuilder, Color color, float duration);
#endif
	}
	
	public interface IDrawableCast : IDrawable
	{
#if UNITY_EDITOR
		void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration);
#endif
	}
}