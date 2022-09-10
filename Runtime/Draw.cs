using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Vertx.Debugging
{
	public sealed class SettingsScope : IDisposable
	{
		public Color Color { get; internal set; }
		public Matrix4x4 Matrix { get; internal set; }
		public float Duration { get; internal set; }

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public SettingsScope WithColor(Color color)
		{
			Color = color;
			return this;
		}

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public SettingsScope WithMatrix(Matrix4x4 matrix)
		{
			Matrix = matrix;
			return this;
		}

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public SettingsScope WithLocalSpace(Transform transform)
		{
			Matrix = transform.localToWorldMatrix;
			return this;
		}

		// ReSharper disable once MemberHidesStaticFromOuterClass
		public SettingsScope WithDuration(float duration)
		{
			Duration = duration;
			return this;
		}

		public void Dispose() { }
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public static class D
	{
#if UNITY_EDITOR
		private static readonly CommandBuilder _builder = CommandBuilder.Instance;
#endif
		
		private static readonly Color _hitColor = new Color(1, 0.1f, 0.2f);
		private static readonly Color _noHitColor = new Color(0.4f, 1f, 0.3f);

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(_builder, Color.white, 0);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, Color color, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(_builder, color, duration);
#endif
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void raw<T>(T shape, bool hit, float duration = 0) where T : struct, IDrawable
		{
#if UNITY_EDITOR
			shape.Draw(_builder, hit ? _hitColor : _noHitColor, duration);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void raw(Ray ray) => raw(new Shapes.Ray(ray.origin, ray.direction), Color.white);
	}

	public interface IDrawable
	{
#if UNITY_EDITOR
		void Draw(CommandBuilder commandBuilder, Color color, float duration);
#endif
	}
}