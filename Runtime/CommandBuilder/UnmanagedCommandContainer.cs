#if UNITY_EDITOR
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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

		public void Add(T value, float duration = 0)
		{
			_values.Add(value);
			if (UsesDurations)
			{
				_durations.Add(duration);
				if (duration > 0)
					HasNonZeroDuration = true;
			}
			Dirty = true;
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
}
#endif