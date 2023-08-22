using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Vertx.Debugging
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class D
	{
#if UNITY_EDITOR
		private static UnmanagedCommandBuilder s_Builder => UnmanagedCommandBuilder.Instance.Data;
#endif

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, Color.white, duration);
#endif
		}

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color color, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, color, duration);
#endif
		}
		
#if UNITY_EDITOR
		internal static void raw<T>(UnmanagedCommandBuilder commandBuilder, T shape, Color color, float duration = 0) where T : struct, IDrawable
			=> shape.Draw(commandBuilder, color, duration);
#endif

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, bool hit, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, hit ? Shape.HitColor : Shape.CastColor, duration);
#endif
		}

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color castColor, Color hitColor, float duration = 0) where T : struct, IDrawableCast
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, castColor, hitColor, duration);
#endif
		}
        
#if UNITY_EDITOR
		internal static void raw<T>(UnmanagedCommandBuilder commandBuilder, T shape, Color castColor, Color hitColor, float duration = 0) where T : struct, IDrawableCast 
			=> shape.Draw(commandBuilder, castColor, hitColor, duration);
#endif

		// ------ Conversion for Unity types ------

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, Color color, float duration = 0) => raw(new Shape.Ray(ray.origin, ray.direction), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, Color color, float duration = 0) => raw(new Shape.Ray(new float3(ray.origin.x, ray.origin.y, 0), new float3(ray.direction.x, ray.direction.y, 0)), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, float duration = 0) => raw(ray, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0) => raw(new Shape.Point(position), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, Color color, float duration = 0) => raw(new Shape.Point2D(position), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, float duration = 0) => raw(position, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0) => raw(new Shape.Box(bounds), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, Color color, float duration = 0) => raw(new Shape.Box(bounds), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, Color color, float duration = 0) => raw(new Shape.Box2D(rect.center, rect.size), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, float duration = 0) => raw(rect, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, Color color, float duration = 0) => raw(new Shape.Box2D(rect.center, rect.size.xy()), color, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, float duration = 0) => raw(rect, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

#if VERTX_PHYSICS
		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0) => raw(new Shape.SurfacePoint(hit.point, hit.normal), color, duration);

		[BurstDiscard]
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
			switch (collider)
			{
				case BoxCollider boxCollider:
					new Shape.Box(boxCollider).Draw(s_Builder, color, duration);
					break;
				case SphereCollider sphereCollider:
					new Shape.Sphere(sphereCollider).Draw(s_Builder, color, duration);
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
		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, Color color, float duration = 0)
		{
			if (!hit)
				return;
			raw(new Shape.Ray(new float3(hit.point.x, hit.point.y, hit.transform.position.z), new float3(hit.normal.x, hit.normal.y, 0)), color, duration);
		}

		[BurstDiscard]
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
			switch (collider)
			{
				case BoxCollider2D boxCollider:
					var box = new Shape.Box2D(boxCollider);
					box.Draw(s_Builder, color, duration);
					if (boxCollider.edgeRadius > 0)
					{
						float edgeRadius = boxCollider.edgeRadius;
						Matrix4x4 matrix = box.Matrix;
						Quaternion rotation = matrix.rotation;
						Vector3 topRight = matrix.MultiplyPoint3x4(Shape.Box2D.s_Vertices[(int)Shape.Box2D.VertexCorner.TopRight]),
							topLeft = matrix.MultiplyPoint3x4(Shape.Box2D.s_Vertices[(int)Shape.Box2D.VertexCorner.TopLeft]),
							bottomLeft = matrix.MultiplyPoint3x4(Shape.Box2D.s_Vertices[(int)Shape.Box2D.VertexCorner.BottomLeft]),
							bottomRight = matrix.MultiplyPoint3x4(Shape.Box2D.s_Vertices[(int)Shape.Box2D.VertexCorner.BottomRight]);
						var angle = Shape.Angle.FromTurns(0.25f);
						Quaternion topRightRot = Quaternion.AngleAxis(45, Vector3.forward) * rotation,
							topLeftRot = Quaternion.AngleAxis(135, Vector3.forward) * rotation,
							bottomLeftRot = Quaternion.AngleAxis(225, Vector3.forward) * rotation,
							bottomRightRot = Quaternion.AngleAxis(315, Vector3.forward) * rotation;
						new Shape.Arc(topRight, topRightRot, edgeRadius, angle).Draw(s_Builder, color, duration);
						new Shape.Arc(topLeft, topLeftRot, edgeRadius, angle).Draw(s_Builder, color, duration);
						new Shape.Arc(bottomLeft, bottomLeftRot, edgeRadius, angle).Draw(s_Builder, color, duration);
						new Shape.Arc(bottomRight, bottomRightRot, edgeRadius, angle).Draw(s_Builder, color, duration);
						float h = math.sqrt(edgeRadius * edgeRadius * 0.5f);
						Vector3 a = new Vector3(h, h, 0),
							b = new Vector3(h, -h, 0);
						new Shape.Line(topLeft + topLeftRot * b, topRight + topRightRot * a).Draw(s_Builder, color, duration);
						new Shape.Line(topRight + topRightRot * b, bottomRight + bottomRightRot * a).Draw(s_Builder, color, duration);
						new Shape.Line(bottomRight + bottomRightRot * b, bottomLeft + bottomLeftRot * a).Draw(s_Builder, color, duration);
						new Shape.Line(bottomLeft + bottomLeftRot * b, topLeft + topLeftRot * a).Draw(s_Builder, color, duration);
					}

					break;
				case CircleCollider2D circleCollider2D:
					new Shape.Circle2D(circleCollider2D).Draw(s_Builder, color, duration);
					break;
				case CapsuleCollider2D capsuleCollider:
					new Shape.Capsule2D(capsuleCollider).Draw(s_Builder, color, duration);
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
		internal void Draw(UnmanagedCommandBuilder commandBuilder, Color color, float duration);
#endif
	}

	public interface IDrawableCast : IDrawable
	{
#if UNITY_EDITOR
		internal void Draw(UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration);
#endif
	}
}