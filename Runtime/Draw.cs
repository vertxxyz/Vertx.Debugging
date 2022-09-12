using System.Collections.Generic;
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

		private static readonly Color s_HitColor = new Color(1, 0.1f, 0.2f);
		private static readonly Color s_NoHitColor = new Color(0.4f, 1f, 0.3f);

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
			shape.Draw(s_Builder, hit ? s_HitColor : s_NoHitColor, duration);
#endif
		}

		// ------ Conversion for Unity types ------

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, Color color, float duration = 0) => raw(new Shapes.Ray(ray.origin, ray.direction), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, bool hit, float duration = 0) => raw(ray, hit ? s_HitColor : s_NoHitColor, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray, float duration = 0) => raw(ray, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, Color color, float duration = 0) => raw(new Shapes.Point(position), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Vector3 position, float duration = 0) => raw(position, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, Color color, float duration = 0) => raw(new Shapes.Box(bounds), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(Bounds bounds, float duration = 0) => raw(bounds, Color.white, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, Color color, float duration = 0) => raw(new Shapes.Ray(hit.point, hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit hit, float duration = 0) => raw(hit, s_HitColor, duration);
		
#if VERTX_PHYSICS_2D
		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, Color color, float duration = 0) => raw(new Shapes.Ray(hit.point, hit.normal), color, duration);

		[Conditional("UNITY_EDITOR")]
		public static void raw(RaycastHit2D hit, float duration = 0) => raw(hit, s_HitColor, duration);
#endif
	}

	public interface IDrawable
	{
#if UNITY_EDITOR
		void Draw(CommandBuilder commandBuilder, Color color, float duration);
#endif
	}
}