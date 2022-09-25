using System;
using UnityEngine;
using Vertx.Debugging.Internal;

#if UNITY_EDITOR

namespace Vertx.Debugging
{
	// ReSharper disable once ClassCannotBeInstantiated
	public sealed partial class CommandBuilder
	{
		private bool InitialiseAndGetGroup(float duration, out BufferGroup group)
		{
			switch (UpdateContext.State)
			{
				case UpdateContext.UpdateState.Update:
					// Don't append while we're paused.
					if (Application.isPlaying && Time.deltaTime == 0)
					{
						group = null;
						return false;
					}

					duration = GetDuration(duration);
					if (duration < 0)
					{
						group = null;
						return false;
					}

					group = _defaultGroup;
					break;
				case UpdateContext.UpdateState.CapturingGizmos:
					// Force the runtime object to exist
					_ = DrawRuntimeBehaviour.Instance;
					group = _gizmosGroup;
					break;
				case UpdateContext.UpdateState.Ignore:
					group = null;
					return false;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		public void AppendRay(Shapes.Ray ray, Color color, float duration) => AppendLine(new Shapes.Line(ray), color, duration);

		public void AppendLine(Shapes.Line line, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(duration, out var group)) return;
			group.Lines.Add(line, color, modifications, duration);
		}

		public void AppendArc(Shapes.Arc arc, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(duration, out var group)) return;
			group.Arcs.Add(arc, color, modifications, duration);
		}

		public void AppendBox(Shapes.Box box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(duration, out var group)) return;
			group.Boxes.Add(box, color, modifications, duration);
		}

		public void AppendBox2D(Shapes.Box2D box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(duration, out var group)) return;
			group.Box2Ds.Add(box, color, modifications, duration);
		}

		internal void AppendOutline(Shapes.Outline outline, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(duration, out var group)) return;
			group.Outlines.Add(outline, color, modifications, duration);
		}

		public void AppendText(Shapes.Text text, Color backgroundColor, Color textColor, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos)
			{
				Debug.LogWarning("Drawing Text is unsupported from a Gizmos context.");
				return;
			}

			if (!InitialiseAndGetGroup(duration, out _)) return;
			_texts.Add(text, backgroundColor, textColor, modifications, duration);
			// Force the runtime object to exist
			_ = DrawRuntimeBehaviour.Instance;
		}
	}
}
#endif