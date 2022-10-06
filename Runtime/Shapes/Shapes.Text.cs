using System;
using UnityEngine;
using Object = UnityEngine.Object;

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
			
			/// <param name="position">Position to anchor the text to.</param>
			/// <param name="value">The value to be converted to a string.</param>
			/// <param name="camera">If a camera is not provided, the text will only draw in the Scene view.</param>
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

		[Flags]
		public enum View : byte
		{
			None = 0,
			Game = 1,
			Scene = 1 << 1,
			All = Game | Scene
		}
		
		/// <summary>
		/// Text drawn in the top left.<br/>
		/// Drawn in the Scene view using an Overlay for versions that support it.
		/// </summary>
		public readonly struct ScreenText : IDrawableCast
		{
			public readonly object Value;
			public readonly Object Context;
			public readonly View ActiveViews;

			/// <param name="value">The value to be converted to a string.</param>
			/// <param name="context">If provided, this object is pinged when the text is clicked.</param>
			/// <param name="activeViews">The views this text is displayed in.</param>
			public ScreenText(object value, Object context = null, View activeViews = View.All)
			{
				ActiveViews = activeViews;
				Value = value;
				Context = context;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> Draw(commandBuilder, color, color == Color.white ? Color.black : Color.white, duration);
			
			public void Draw(CommandBuilder commandBuilder, Color backgroundColor, Color textColor, float duration)
			{
				if (ActiveViews == View.None) return;
				commandBuilder.AppendScreenText(this, backgroundColor, textColor, duration);
			}
#endif
		}

#if UNITY_EDITOR
		internal interface IText
		{
			void Reset();
		}
		
		internal sealed class TextData : IText
		{
			public object Value;
			public Color BackgroundColor;
			public Color TextColor;

			public Vector3 Position;
			public Camera Camera;
			// Only used after 3D text is calculated.
			public Vector2 ScreenPosition;
			public float Distance;
			public float Alpha;
			
			public void Reset()
			{
				Value = null;
				Camera = null;
			}
		}
		
		internal sealed class ScreenTextData : IText
		{
			public object Value;
			public Object Context;
			public Color BackgroundColor;
			public Color TextColor;
			public View ActiveViews;
			public void Reset()
			{
				Value = null;
				Context = null;
			}
		}
#endif
	}
}