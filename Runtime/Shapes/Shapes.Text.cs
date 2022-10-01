using UnityEngine;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		/// <summary>
		/// Text drawn at a 3D position in the scene.
		/// </summary>
		public readonly struct Text : IDrawableCast
		{
			public readonly Vector3 Position;
			public readonly object Value;
			public readonly Camera Camera;

			public Text(Vector3 position, object value, Camera camera = null)
			{
				Camera = camera;
				Position = position;
				Value = value;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> Draw(commandBuilder, color, color == Color.white ? Color.black : Color.white, duration);

			public void Draw(CommandBuilder commandBuilder, Color backgroundColor, Color textColor, float duration)
				=> commandBuilder.AppendText(this, backgroundColor, textColor, duration);
#endif
		}
		
		/// <summary>
		/// Text drawn in the top left.<br/>
		/// Order is not maintained when mixing durations, including durations added with <see cref="Text"/>.
		/// </summary>
		public readonly struct ScreenText : IDrawableCast
		{
			public readonly object Value;

			public ScreenText(object value) => Value = value;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, Color.white, duration);
			public void Draw(CommandBuilder commandBuilder, Color backgroundColor, Color textColor, float duration)
				=> commandBuilder.AppendText(new Text(default, Value), backgroundColor, textColor, duration, DrawModifications.Custom);
#endif
		}

		internal sealed class TextData
		{
			public Vector3 Position;
			public object Value;
			public Camera Camera;
			public Color BackgroundColor;
			public Color TextColor;
			public DrawModifications Modifications;
		}
	}
}