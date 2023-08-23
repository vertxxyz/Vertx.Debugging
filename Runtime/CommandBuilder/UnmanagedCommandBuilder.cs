#if UNITY_EDITOR
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Vertx.Debugging
{
	internal struct UnmanagedCommandContainer<T> : IDisposable where T : unmanaged
	{
		private const int InitialListCapacity = 32;

		private bool _initialised;
		private UnsafeList<T> _values; // Includes colors
		private UnsafeList<float> _durations;

		public UnsafeList<T> Values => _values;
		public UnsafeList<float> Durations => _durations;

		public int Count => _values.IsCreated ? _values.Length : 0;

		// Avoids redundantly setting internal GraphicsBuffer data.
		public bool Dirty { get; private set; }

		// Optimises removal calls, avoiding running the removal job if not necessary.
		public bool HasNonZeroDuration { get; private set; }

		public bool UsesDurations { get; private set; }

		public void Initialise(bool usesDurations)
		{
			if (_initialised)
				return;
			UsesDurations = usesDurations;
			_values = new UnsafeList<T>(InitialListCapacity, Allocator.Persistent);
			_durations = usesDurations ? new UnsafeList<float>(InitialListCapacity, Allocator.Persistent) : default;
			Dirty = true;
			_initialised = true;
		}

		public void Add(T value, float duration)
		{
			_values.Add(value);
			if (UsesDurations)
				_durations.Add(duration);
			Dirty = true;

			if (duration > 0)
				HasNonZeroDuration = true;
		}

		public void Dispose()
		{
			if (_values.IsCreated)
				_values.Dispose();
			if (UsesDurations && _durations.IsCreated)
				_durations.Dispose();
			_initialised = false;
			Dirty = true;
		}

		public void Clear()
		{
			if (!_initialised)
				return;
			_values.Clear();
			if (UsesDurations)
				_durations.Clear();
			Dirty = true;
			HasNonZeroDuration = false;
		}

		public void ChangedAfterRemoval()
		{
			Dirty = true;
			if (Count == 0)
				HasNonZeroDuration = false;
		}
	}

	internal struct UnmanagedCommandGroup
	{
		public UnmanagedCommandContainer<LineGroup> Lines;
		public UnmanagedCommandContainer<DashedLineGroup> DashedLines;
		public UnmanagedCommandContainer<ArcGroup> Arcs;
		public UnmanagedCommandContainer<BoxGroup> Boxes;
		public UnmanagedCommandContainer<OutlineGroup> Outlines;
		public UnmanagedCommandContainer<CastGroup> Casts;

		public void Clear()
		{
			Lines.Clear();
			DashedLines.Clear();
			Arcs.Clear();
			Boxes.Clear();
			Outlines.Clear();
			Casts.Clear();
		}

		public void Dispose()
		{
			Lines.Dispose();
			DashedLines.Dispose();
			Arcs.Dispose();
			Boxes.Dispose();
			Outlines.Dispose();
			Casts.Dispose();
		}

		public void Initialise(bool usesDurations)
		{
			Lines.Initialise(usesDurations);
			DashedLines.Initialise(usesDurations);
			Arcs.Initialise(usesDurations);
			Boxes.Initialise(usesDurations);
			Outlines.Initialise(usesDurations);
			Casts.Initialise(usesDurations);
		}
	}

	internal struct UnmanagedCommandBuilder
	{
		internal static readonly SharedStatic<UnmanagedCommandBuilder> Instance
			= SharedStatic<UnmanagedCommandBuilder>.GetOrCreate<UnmanagedCommandBuilder>();

		public enum UpdateState
		{
			Update,
			Gizmos
		}

		public UpdateState State { get; internal set; }

		public UnmanagedCommandGroup Standard;
		public UnmanagedCommandGroup Gizmos;

		public void Initialise()
		{
			Standard.Initialise(true);
			Gizmos.Initialise(false);
		}

		public void AppendLine(in Shape.Line line, Color color, float duration)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.Lines.Add(new LineGroup(line, color.ToFloat4()), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.Lines.Add(new LineGroup(line, color.ToFloat4()), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AppendRay(in Shape.Ray ray, Color color, float duration) => AppendLine(new Shape.Line(ray), color, duration);

		public void AppendArc(in Shape.Arc arc, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.Arcs.Add(new ArcGroup(arc, color.ToFloat4(), modifications), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.Arcs.Add(new ArcGroup(arc, color.ToFloat4(), modifications), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
        }

		public void AppendDashedLine(in Shape.DashedLine dashedLine, Color color, float duration)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.DashedLines.Add(new DashedLineGroup(dashedLine, color.ToFloat4()), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.DashedLines.Add(new DashedLineGroup(dashedLine, color.ToFloat4()), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AppendOutline(in Shape.Outline outline, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.Outlines.Add(new OutlineGroup(outline, color.ToFloat4(), modifications), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.Outlines.Add(new OutlineGroup(outline, color.ToFloat4(), modifications), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AppendBox(in Shape.Box box, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.Boxes.Add(new BoxGroup(box, color.ToFloat4(), modifications), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.Boxes.Add(new BoxGroup(box, color.ToFloat4(), modifications), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AppendCast(in Shape.Cast cast, Color color, float duration)
		{
			switch (State)
			{
				case UpdateState.Update:
					Standard.Casts.Add(new CastGroup(cast, color.ToFloat4()), duration);
					break;
				case UpdateState.Gizmos:
					Gizmos.Casts.Add(new CastGroup(cast, color.ToFloat4()), duration);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
			Standard.Dispose();
			Gizmos.Dispose();
		}

		public void Clear()
		{
			Standard.Clear();
			Gizmos.Clear();
		}
	}
}
#endif