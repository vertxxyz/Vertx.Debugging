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

		public int Count => _values.IsCreated ? _values.Length : 0;
		
		// Avoids redundantly setting internal GraphicsBuffer data.
		public bool Dirty { get; private set; }
		
		// Optimises removal calls, avoiding running the removal job if not necessary.
		public bool HasNonZeroDuration { get; private set; }

		public void Initialise()
		{
			if (_initialised)
				return;
			_values = new UnsafeList<T>(InitialListCapacity, Allocator.Persistent);
			_durations = new UnsafeList<float>(InitialListCapacity, Allocator.Persistent);
			Dirty = true;
		}

		public void Add(T value, float duration)
		{
			_values.Add(value);
			_durations.Add(duration);
			Dirty = true;
			
			if (duration > 0)
				HasNonZeroDuration = true;
		}

		public void Dispose()
		{
			if (_values.IsCreated)
				_values.Dispose();
			if (_durations.IsCreated)
				_durations.Dispose();
			_initialised = false;
			Dirty = true;
		}

		public void Clear()
		{
			if (!_initialised)
				return;
			_values.Clear();
			_durations.Clear();
			Dirty = true;
			HasNonZeroDuration = false;
		}
	}

	internal struct UnmanagedCommandGroup
	{
		public UnmanagedCommandContainer<LineGroup> Line;
		public UnmanagedCommandContainer<DashedLineGroup> DashedLine;
		public UnmanagedCommandContainer<ArcGroup> Arc;
		public UnmanagedCommandContainer<BoxGroup> Box;
		public UnmanagedCommandContainer<OutlineGroup> Outline;
		public UnmanagedCommandContainer<CastGroup> Cast;

		public void Clear()
		{
			Line.Clear();
			DashedLine.Clear();
			Arc.Clear();
			Box.Clear();
			Outline.Clear();
			Cast.Clear();
		}

		public void Dispose()
		{
			Line.Dispose();
			DashedLine.Dispose();
			Arc.Dispose();
			Box.Dispose();
			Outline.Dispose();
			Cast.Dispose();
		}
	}

	internal struct UnmanagedCommandBuilder
	{
		internal static readonly SharedStatic<UnmanagedCommandBuilder> Instance
			= SharedStatic<UnmanagedCommandBuilder>.GetOrCreate<UnmanagedCommandBuilder>();

		public enum UpdateState
		{
			Update,
			Gizmos,
			Ignore
		}

		public UpdateState State { get; internal set; }

		public UnmanagedCommandGroup Standard;
		public UnmanagedCommandGroup Gizmos;

		private readonly bool TryGetGroup(out UnmanagedCommandGroup group)
		{
			switch (State)
			{
				case UpdateState.Update:
					group = Standard;
					return true;
				case UpdateState.Gizmos:
					group = Gizmos;
					return true;
				case UpdateState.Ignore:
				default:
					group = default;
					return false;
			}
		}

		public readonly void AppendLine(in Shape.Line line, Color color, float duration)
		{
			if (!TryGetGroup(out var group))
				return;
			group.Line.Add(new LineGroup(line, color.ToFloat4()), duration);
		}

		public readonly void AppendRay(in Shape.Ray ray, Color color, float duration) => AppendLine(new Shape.Line(ray), color, duration);

		public readonly void AppendArc(in Shape.Arc arc, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!TryGetGroup(out var group))
				return;
			group.Arc.Add(new ArcGroup(arc, color.ToFloat4(), modifications), duration);
		}

		public readonly void AppendDashedLine(in Shape.DashedLine dashedLine, Color color, float duration)
		{
			if (!TryGetGroup(out var group))
				return;
			group.DashedLine.Add(new DashedLineGroup(dashedLine, color.ToFloat4()), duration);
		}

		public readonly void AppendOutline(in Shape.Outline outline, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!TryGetGroup(out var group))
				return;
			group.Outline.Add(new OutlineGroup(outline, color.ToFloat4(), modifications), duration);
		}

		public readonly void AppendBox(in Shape.Box box, Color color, float duration, Shape.DrawModifications modifications = Shape.DrawModifications.None)
		{
			if (!TryGetGroup(out var group))
				return;
			group.Box.Add(new BoxGroup(box, color.ToFloat4(), modifications), duration);
		}

		public readonly void AppendCast(in Shape.Cast cast, Color color, float duration)
		{
			if (!TryGetGroup(out var group))
				return;
			group.Cast.Add(new CastGroup(cast, color.ToFloat4()), duration);
		}
        
		public void Dispose()
		{
			Standard.Dispose();
			Gizmos.Dispose();
		}
	}
}