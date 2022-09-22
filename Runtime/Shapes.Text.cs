using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public readonly struct Text : IDrawable
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
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendText(this, color, duration);
#endif
		}

		internal sealed class TextData
		{
			public Vector3 Position;
			public object Value;
			public Camera Camera;

			public TextData() { }

			public TextData(Vector3 position, object value, Camera camera)
			{
				Position = position;
				Value = value;
				Camera = camera;
			}
		}

		internal sealed class TextDataPool : ObjectPool<TextData>
		{
			public TextDataPool(int defaultCapacity = 10, int maxSize = 10000)
				: base(() => new TextData(), null, data =>
				{
					data.Camera = null;
					data.Value = null;
				}, null, false, defaultCapacity, maxSize) { }
		}
	}
}