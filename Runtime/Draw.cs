using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
#if !UNITY_2021_1_OR_NEWER
using Vertx.Debugging.Internal;
#else
using UnityEngine.Pool;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Vertx.Debugging
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class D
	{
#if UNITY_EDITOR
		private static ref UnmanagedCommandBuilder s_Builder => ref UnmanagedCommandBuilder.Instance.Data;

		internal static void AdjustDuration(ref float duration)
		{
			ref UnmanagedCommandBuilder builder = ref s_Builder;
			if (builder.State != UnmanagedCommandBuilder.UpdateState.Update)
				return;

			if (JobsUtility.IsExecutingJob)
			{
				// TODO handle durations within jobs.
				// Is there a mechanism for detecting FixedUpdate jobs?
				return;
			}

			if (!Time.inFixedTimeStep)
			{
				// Only FixedUpdate calls need their times adjusted.
				return;
			}

			// Adjust the duration of calls from FixedUpdate so that they are displayed for the full duration of this fixed step, and won't be cleared
			// by an Update occurring until that fixed step has actually passed.
			float fixedDeltaTime = builder.FixedTimeStep;
			if (duration < fixedDeltaTime)
			{
				duration +=
					// From the current time, to the next fixed time.
					builder.FixedTime + fixedDeltaTime
					- builder.Time;
			}
		}
#endif

		/// <summary>
		/// Draw a shape for <see cref="duration"/>.
		/// </summary>
		/// <param name="shape">The shape to draw.</param>
		/// <param name="duration">The length of time to draw for. 0 will be one frame (FixedUpdate is handled correctly).</param>
		/// <typeparam name="T">The type of shape.</typeparam>
		/// <remarks>Don't call this function recursively within <see cref="IDrawable.Draw"/>, as duration may be adjusted twice.</remarks>
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, float duration = 0) where T : struct, IDrawable
			=> raw(shape, Color.white, duration);

		/// <summary>
		/// Draw a shape for <see cref="duration"/> with <see cref="color"/>.
		/// </summary>
		/// <param name="shape">The shape to draw.</param>
		/// <param name="color">The color used to draw the shape.</param>
		/// <param name="duration">The length of time to draw for. 0 will be one frame (FixedUpdate is handled correctly).</param>
		/// <typeparam name="T">The type of shape.</typeparam>
		/// <remarks>Don't call this function recursively within <see cref="IDrawable.Draw"/>, as duration may be adjusted twice.</remarks>
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color color, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			AdjustDuration(ref duration);
			shape.Draw(ref s_Builder, color, duration);
#endif
		}

		/// <summary>
		/// Draw a shape for <see cref="duration"/> with <see cref="hit"/> defining the color.<br/>
		/// The colors used for hit/not hit can be adjusted in the preferences as Hit Color and Cast Color.
		/// </summary>
		/// <param name="shape">The shape to draw.</param>
		/// <param name="hit">Whether to draw using the hit color or the cast color.</param>
		/// <param name="duration">The length of time to draw for. 0 will be one frame (FixedUpdate is handled correctly).</param>
		/// <typeparam name="T">The type of shape.</typeparam>
		/// <remarks>Don't call this function recursively within <see cref="IDrawable.Draw"/>, as duration may be adjusted twice.</remarks>
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, bool hit, float duration = 0) where T : struct, IDrawable
			=> raw(shape, hit ? Shape.HitColor : Shape.CastColor, duration);

		/// <summary>
		/// Draw a shape for <see cref="duration"/> with <see cref="castColor"/> and <see cref="hitColor"/>.
		/// </summary>
		/// <param name="shape">The shape to draw.</param>
		/// <param name="castColor">The color used to draw the cast.</param>
		/// <param name="hitColor">The color used to draw the hits associated with the cast.</param>
		/// <param name="duration">The length of time to draw for. 0 will be one frame (FixedUpdate is handled correctly).</param>
		/// <typeparam name="T">The type of shape.</typeparam>
		/// <remarks>Don't call this function recursively within <see cref="IDrawable.Draw"/>, as duration may be adjusted twice.</remarks>
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color castColor, Color hitColor, float duration = 0) where T : struct, IDrawableCast
		{
#if UNITY_EDITOR
			AdjustDuration(ref duration);
			shape.Draw(ref s_Builder, castColor, hitColor, duration);
#endif
		}

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(IDrawableManaged shape, float duration = 0)
			=> raw(shape, Color.white, duration);

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(IDrawableManaged shape, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			// Managed shapes adjust time later.
			shape.Draw(CommandBuilder.Instance, color, duration);
#endif
		}

		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(IDrawableManaged shape, bool hit, float duration = 0)
			=> raw(shape, hit ? Shape.HitColor : Shape.CastColor, duration);

		/// <summary>
		/// Draw a shape for <see cref="duration"/> with <see cref="castColor"/> and <see cref="hitColor"/>.<br/>
		/// </summary>
		/// <param name="shape">The shape to draw.</param>
		/// <param name="castColor">The color used to draw the cast. Background color for text.</param>
		/// <param name="hitColor">The color used to draw the hits associated with the cast. Color for text.</param>
		/// <param name="duration">The length of time to draw for. 0 will be one frame (FixedUpdate is handled correctly).</param>
		[BurstDiscard]
		[Conditional("UNITY_EDITOR")]
		public static void raw(IDrawableCastManaged shape, Color castColor, Color hitColor, float duration = 0)
		{
#if UNITY_EDITOR
			// Managed shapes adjust time later.
			shape.Draw(CommandBuilder.Instance, castColor, hitColor, duration);
#endif
		}

		// ------ Conversion for Unity types ------

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, Color color, float duration = 0)
			=> raw(new Shape.Ray(ray.origin, ray.direction), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, Color color, float duration = 0)
			=> raw(new Shape.Ray(new float3(ray.origin.x, ray.origin.y, 0), new float3(ray.direction.x, ray.direction.y, 0)), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, bool hit, float duration = 0) => raw(ray, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray2D ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0)
			=> raw(new Shape.Point(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, Color color, float duration = 0)
			=> raw(new Shape.Point2D(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, bool hit, float duration = 0) => raw(position, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0)
			=> raw(new Shape.Box(bounds), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, Color color, float duration = 0)
			=> raw(new Shape.Box(bounds), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(BoundsInt bounds, bool hit, float duration = 0) => raw(bounds, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, Color color, float duration = 0)
			=> raw(new Shape.Box2D(rect.center, rect.size), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, float duration = 0) => raw(rect, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Rect rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, Color color, float duration = 0)
			=> raw(new Shape.Box2D(rect.center, rect.size.xy()), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, float duration = 0) => raw(rect, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RectInt rect, bool hit, float duration = 0) => raw(rect, hit ? Shape.HitColor : Shape.CastColor, duration);

#if VERTX_PHYSICS
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0)
			=> raw(new Shape.SurfacePoint(hit.point, hit.normal), color, duration);

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
					raw(new Shape.Box(boxCollider), color, duration);
					break;
				case SphereCollider sphereCollider:
					raw(new Shape.Sphere(sphereCollider), color, duration);
					break;
				case CapsuleCollider capsuleCollider:
					raw(new Shape.Capsule(capsuleCollider), color, duration);
					break;
				case CharacterController characterController:
					raw(new Shape.Capsule(characterController), color, duration);
					break;
				case MeshCollider meshCollider:
					raw(meshCollider.bounds, color, duration);
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
			raw(new Shape.Ray(new float3(hit.point.x, hit.point.y, hit.transform.position.z), new float3(hit.normal.x, hit.normal.y, 0)), color, duration);
#endif
		}


		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, float duration = 0) => raw(hit, Shape.HitColor, duration);

		/// <summary>
		/// Draws a <see cref="Collider2D"/>.<br/>
		/// Currently supported colliders are <see cref="BoxCollider2D"/>, <see cref="CircleCollider2D"/>, <see cref="CapsuleCollider2D"/>, and <see cref="PolygonCollider2D"/>.<br/>
		/// <see cref="BoxCollider2D.edgeRadius"/> does not support skew.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void raw(Collider2D collider, Color color, float duration = 0)
		{
#if UNITY_EDITOR
			switch (collider)
			{
				case BoxCollider2D boxCollider:
					raw(new Shape.Box2DWithEdgeRadius(boxCollider), color, duration);
					break;
				case CircleCollider2D circleCollider2D:
					raw(new Shape.Circle2D(circleCollider2D), color, duration);
					break;
				case CapsuleCollider2D capsuleCollider:
					raw(new Shape.Capsule2D(capsuleCollider), color, duration);
					break;
				case PolygonCollider2D polygonCollider:
					Transform transform = polygonCollider.transform;
					using (ListPool<Vector3>.Get(out var points))
					using (ListPool<Vector2>.Get(out var points2d))
					{
						for (var i = 0; i < polygonCollider.pathCount; i++)
						{
							polygonCollider.GetPath(i, points2d);
							if (points2d.Count == 0) continue;
							points.Clear();
							foreach (var p in points2d)
								points.Add(transform.TransformPoint(p));
							points.Add(points[0]);
							new Shape.LineStrip(points).Draw(s_Builder, color, duration);
						}
					}
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

	public interface IDrawableManaged
	{
#if UNITY_EDITOR
		internal void Draw(CommandBuilder commandBuilder, Color color, float duration);
#endif
	}

	public interface IDrawableCastManaged : IDrawableManaged
	{
#if UNITY_EDITOR
		internal void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration);
#endif
	}
}