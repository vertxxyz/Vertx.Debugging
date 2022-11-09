#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vertx.Debugging.Internal;

namespace Vertx.Debugging
{
	// ReSharper disable once ClassCannotBeInstantiated
	public sealed partial class CommandBuilder
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsInFixedUpdate()
#if UNITY_2020_3_OR_NEWER
			=> Time.inFixedTimeStep;
#else
			=> Time.deltaTime == Time.fixedDeltaTime;
#endif

		private class PauseCapture
		{
			private float _lastPausedTime;
			private float _lastCommittedPauseTime;

			public bool IsSamePausedFrame(float timeThisFrame)
			{
				if (_lastCommittedPauseTime == timeThisFrame)
					return true;
				_lastPausedTime = timeThisFrame;
				return false;
			}

			public void CommitCurrentPausedFrame() => _lastCommittedPauseTime = _lastPausedTime;
		}

		private readonly PauseCapture _pauseCapture = new PauseCapture();
		
		private bool InitialiseAndGetGroup(ref float duration, out BufferGroup group)
		{
			switch (UpdateContext.State)
			{
				case UpdateContext.UpdateState.Update:
					// Don't append while we're paused.
					if (_isPlaying && _isPaused && _pauseCapture.IsSamePausedFrame(_timeThisFrame))
					{
						group = null;
						return false;
					}

					// Calls from FixedUpdate should hang around until the next FixedUpdate, at minimum.
					if (IsInFixedUpdate())
					{
						float fixedDeltaTime = Time.fixedDeltaTime;
						if (duration < fixedDeltaTime)
						{
							// Time from the last 
							// ReSharper disable once ArrangeRedundantParentheses
							duration += (Time.fixedTime + fixedDeltaTime) - _timeThisFrame;
						}
					}
					
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

		public void AppendRay(in Shape.Ray ray, Color color, float duration) => AppendLine(new Shape.Line(ray), color, duration);

		public void AppendLine(in Shape.Line line, Color color, float duration)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Lines.Add(
				new LineGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos
						? new Shape.Line(Gizmos.matrix.MultiplyPoint3x4(line.A), Gizmos.matrix.MultiplyPoint3x4(line.B))
						: line,
					color
				),
				duration
			);
		}
		
		public void AppendDashedLine(in Shape.DashedLine line, Color color, float duration)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.DashedLines.Add(
				new DashedLineGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos
						? new Shape.DashedLine(Gizmos.matrix.MultiplyPoint3x4(line.A), Gizmos.matrix.MultiplyPoint3x4(line.B))
						: line,
					color
				),
				duration
			);
		}

		public void AppendArc(in Shape.Arc arc, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Arcs.Add(
				new ArcGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shape.Arc(Gizmos.matrix * arc.Matrix, arc.Angle) : arc,
					color,
					modifications
				),
				duration
			);
		}

		public void AppendBox(in Shape.Box box, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Boxes.Add(
				new BoxGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shape.Box(Gizmos.matrix * box.Matrix) : box,
					color,
					modifications
				),
				duration
			);
		}

		internal void AppendOutline(in Shape.Outline outline, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Outlines.Add(
				new OutlineGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos
						? new Shape.Outline(Gizmos.matrix.MultiplyPoint3x4(outline.A), Gizmos.matrix.MultiplyPoint3x4(outline.B), Gizmos.matrix.MultiplyPoint3x4(outline.C))
						: outline,
					color,
					modifications
				),
				duration
			);
		}

		internal void AppendCast(in Shape.Cast cast, Color color, float duration)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.Casts.Add(
				new CastGroup(
					UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? new Shape.Cast(Gizmos.matrix * cast.Matrix, Gizmos.matrix.MultiplyPoint3x4(cast.Vector)) : cast,
					color
				),
				duration
			);
		}

		public void AppendText(in Shape.Text text, Color backgroundColor, Color textColor, float duration)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			// Gizmo.matrix repositioning is handled in Add.
			group.Texts.Add(text, backgroundColor, textColor, duration);
			// Force the runtime object to exist
			_ = DrawRuntimeBehaviour.Instance;
		}

		public void AppendScreenText(in Shape.ScreenText text, Color backgroundColor, Color textColor, float duration)
		{
			if (!InitialiseAndGetGroup(ref duration, out var group)) return;
			group.ScreenTexts.Add(text, backgroundColor, textColor, duration);
			// Force the runtime object to exist
			_ = DrawRuntimeBehaviour.Instance;
		}
	}
}
#endif