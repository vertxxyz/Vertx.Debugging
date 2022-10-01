#if UNITY_EDITOR
using System;
using UnityEngine;
using Vertx.Debugging.Internal;

namespace Vertx.Debugging
{
	// ReSharper disable once ClassCannotBeInstantiated
	public sealed partial class CommandBuilder
	{
		private bool InitialiseAndGetGroup(ref float duration, out BufferGroup group)
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

		public void AppendRay(in Shapes.Ray ray, Color color, float duration) => AppendLine(new Shapes.Line(ray), color, duration);

		public void AppendLine(in Shapes.Line line, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Lines.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos
					? new Shapes.Line(Gizmos.matrix.MultiplyPoint3x4(line.A), Gizmos.matrix.MultiplyPoint3x4(line.B))
					: line,
				color,
				modifications,
				duration
			);
		}

		public void AppendArc(in Shapes.Arc arc, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Arcs.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shapes.Arc(Gizmos.matrix * arc.Matrix, arc.Angle) : arc,
				color,
				modifications,
				duration
			);
		}

		public void AppendBox(in Shapes.Box box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Boxes.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shapes.Box(Gizmos.matrix * box.Matrix) : box,
				color,
				modifications,
				duration
			);
		}

		public void AppendBox2D(in Shapes.Box2D box, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Box2Ds.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shapes.Box2D(Gizmos.matrix * box.Matrix) : box,
				color,
				modifications,
				duration
			);
		}

		internal void AppendOutline(in Shapes.Outline outline, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Outlines.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos
					? new Shapes.Outline(Gizmos.matrix.MultiplyPoint3x4(outline.A), Gizmos.matrix.MultiplyPoint3x4(outline.B), Gizmos.matrix.MultiplyPoint3x4(outline.C))
					: outline,
				color,
				modifications,
				duration
			);
		}

		internal void AppendCast(in Shapes.Cast cast, Color color, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Casts.Add(
				UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shapes.Cast(Gizmos.matrix * cast.Matrix, Gizmos.matrix.MultiplyPoint3x4(cast.Vector)) : cast,
				color,
				modifications,
				duration
			);
		}

		public void AppendText(in Shapes.Text text, Color backgroundColor, Color textColor, float duration, Shapes.DrawModifications modifications = Shapes.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Texts.Add(text, backgroundColor, textColor, modifications, duration);
			// Force the runtime object to exist
			_ = DrawRuntimeBehaviour.Instance;
		}
	}
}
#endif