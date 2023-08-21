using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Vertx.Debugging
{
	internal struct UnmanagedCommandContainer<T> : IDisposable where T : unmanaged
	{
		private const int InitialListCapacity = 32;

		private bool _initialised;
		private UnsafeList<T> _values;
		private UnsafeList<float> _durations;

		public int Count => _values.IsCreated ? _values.Length : 0;

		public void Initialise()
		{
			if (_initialised)
				return;
			_values = new UnsafeList<T>(InitialListCapacity, Allocator.Persistent);
			_durations = new UnsafeList<float>(InitialListCapacity, Allocator.Persistent);
		}

		public void Add(T value, float duration)
		{
			_values.Add(value);
			_durations.Add(duration);
		}

		public void Dispose()
		{
			if (_values.IsCreated)
				_values.Dispose();
			if (_durations.IsCreated)
				_durations.Dispose();
			_initialised = false;
		}
	}

	internal struct UnmanagedCommandBuilder
	{
		internal static readonly SharedStatic<UnmanagedCommandBuilder> Instance
			= SharedStatic<UnmanagedCommandBuilder>.GetOrCreate<UnmanagedCommandBuilder>();


		public UnmanagedCommandContainer<LineGroup> Line;
		public UnmanagedCommandContainer<DashedLineGroup> DashedLine;
		public UnmanagedCommandContainer<ArcGroup> Arc;
		public UnmanagedCommandContainer<BoxGroup> Box;
		public UnmanagedCommandContainer<OutlineGroup> Outline;
		public UnmanagedCommandContainer<CastGroup> Cast;
	}
}