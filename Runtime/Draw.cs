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
		public static void raw<T>(T shape) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(s_Builder, Color.white, 0);
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
			shape.Draw(s_Builder, hit ? Shapes.HitColor : Shapes.CastColor, duration);
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
		public static void raw(Ray ray, Color color, float duration = 0) => raw(new Shapes.Ray(ray.origin, ray.direction), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? Shapes.HitColor : Shapes.CastColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0) => raw(new Shapes.Point(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);
		
		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, Color color, float duration = 0) => raw(new Shapes.Point2D(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector2 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0) => raw(new Shapes.Box(bounds), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0) => raw(new Shapes.SurfacePoint(hit.point, hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, float duration = 0) => raw(hit, Shapes.HitColor, duration);
		
#if VERTX_PHYSICS_2D
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, Color color, float duration = 0) => raw(new Shapes.Ray(hit.point, hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, float duration = 0) => raw(hit, Shapes.HitColor, duration);
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