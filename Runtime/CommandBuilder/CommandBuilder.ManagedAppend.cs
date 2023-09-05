#if UNITY_EDITOR
using System;
using UnityEngine;
using Vertx.Debugging.Internal;

namespace Vertx.Debugging
{
	// ReSharper disable once ClassCannotBeInstantiated
	internal sealed partial class CommandBuilder
	{
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
			group = UpdateContext.State switch
			{
				UpdateContext.UpdateState.Update => _defaultGroup,
				UpdateContext.UpdateState.CapturingGizmos => _gizmosGroup,
				_ => throw new ArgumentOutOfRangeException()
			};
			return TryGetAdjustedDuration(ref duration);
		}

		public bool TryGetAdjustedDuration(ref float duration)
		{
			switch (UpdateContext.State)
			{
				case UpdateContext.UpdateState.Update:
					// Don't append while we're paused.
					if (_isPlaying && _isPaused && _pauseCapture.IsSamePausedFrame(s_TimeThisFrame))
						return false;

					// Calls from FixedUpdate should hang around until the next FixedUpdate, at minimum.
					if (Time.inFixedTimeStep)
					{
						float fixedDeltaTime = Time.fixedDeltaTime;
						if (duration < fixedDeltaTime)
						{
							// Time from the last 
							// ReSharper disable once ArrangeRedundantParentheses
							duration += (Time.fixedTime + fixedDeltaTime) - s_TimeThisFrame;
						}
					}
					return duration > 0;
				case UpdateContext.UpdateState.CapturingGizmos:
					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
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