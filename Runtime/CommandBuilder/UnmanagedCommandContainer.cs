#if UNITY_EDITOR
using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Vertx.Debugging
{
	internal struct Unit
	{
		internal int _begin;
		internal int _next;
		internal int _end;

		internal Unit AllocateAtomic(int count)
		{
			int begin = _next;
			while (true)
			{
				int end = math.min(begin + count, _end);
				if (begin == end)
					return default;
				int found = Interlocked.CompareExchange(ref _next, end, begin);
				if (found == begin)
					return new Unit { _begin = begin, _next = begin, _end = end };
				begin = found;
			}
		}

		internal int AllocateAtomic() => AllocateAtomic(1)._begin;

		internal Unit(int count)
		{
			_begin = _next = 0;
			_end = count;
		}

		internal Unit(int me, int writers, int writableBegin, int writableEnd)
		{
			int writables = writableEnd - writableBegin;
			_begin = writableBegin + (me * writables) / writers;
			_end = writableBegin + ((me + 1) * writables) / writers;
			if (_begin > writableEnd)
				_begin = writableEnd;
			if (_end > writableEnd)
				_end = writableEnd;
			_next = _begin;
		}

		internal void Fill() => _next = _end;

		internal int Length => _end - _begin;
		internal int Filled => _next - _begin;
		internal int Remaining => _end - _next;

		internal void SetFilled(int newCount) => _next = math.min(_end, _begin + newCount);
	}


	internal readonly unsafe struct UnsafeArray<T> : IDisposable where T : unmanaged
	{
		readonly T* _pointer;
		readonly int _length;
		internal T* GetUnsafePtr() => _pointer;

		internal UnsafeArray(int length)
		{
			int size = UnsafeUtility.SizeOf<T>() * length;
			int alignment = UnsafeUtility.AlignOf<T>();
			_pointer = (T*)UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);
			_length = length;
		}

		public void Dispose() => UnsafeUtility.Free(_pointer, Allocator.Persistent);

		internal int Length => _length;
		internal ref T this[int index] => ref UnsafeUtility.AsRef<T>(_pointer + index);

		internal NativeArray<T> AsNativeArray()
		{
			var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(_pointer, _length, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
			return array;
		}
	}

	internal struct UnmanagedCommandContainer<T> : IDisposable where T : unmanaged
	{
		private bool _initialised;
		private UnsafeArray<T> _values; // Includes colors
		private UnsafeArray<float> _durations;
		private Unit _allocations;
		private NativeReference<int> _lengthForJob;

		public UnsafeArray<T> Values => _values;
		public UnsafeArray<float> Durations => _durations;

		public NativeReference<int> LengthForJob => _lengthForJob;

		public int Count => _allocations.Filled;
		public int Capacity => _allocations.Length;

		// Avoids redundantly setting internal GraphicsBuffer data.
		public bool Dirty { get; private set; }

		// Optimises removal calls, avoiding running the removal job if not necessary.
		public bool HasNonZeroDuration { get; private set; }

		public bool UsesDurations { get; private set; }

		public void Initialise(int allocations, bool usesDurations)
		{
			if (_initialised)
				return;
			UsesDurations = usesDurations;
			_values = new UnsafeArray<T>(allocations);
			_durations = usesDurations ? new UnsafeArray<float>(allocations) : default;
			_allocations = new Unit(allocations);
			_lengthForJob = new NativeReference<int>(0, Allocator.Persistent);
			Dirty = true;
			_initialised = true;
		}

		public void Add(T value, float duration = 0)
		{
			int index = _allocations.AllocateAtomic(1)._next;
			SetIndex(value, duration, index);
		}

		private void SetIndex(T value, float duration, int index)
		{
			_values[index] = value;
			if (UsesDurations)
			{
				_durations[index] = duration;
				if (duration > 0)
					HasNonZeroDuration = true;
			}

			Dirty = true;
		}

		public Unit AllocateRange(int count) => _allocations.AllocateAtomic(count);

		public void AddUsingRange(ref Unit range, T value, float duration = 0)
		{
			if (range._next < range._end)
				SetIndex(value, duration, range._next++);
			else
				throw new ArgumentException($"Allocation was out of range. Allocate more space using {nameof(AllocateRange)} before attempting to {nameof(AddUsingRange)}");
		}

		public void Dispose()
		{
			_values.Dispose();
			if (UsesDurations)
				_durations.Dispose();
			if (_lengthForJob.IsCreated)
				_lengthForJob.Dispose();
			_initialised = false;
			Dirty = true;
		}

		public void Clear()
		{
			if (!_initialised)
				return;
			_allocations = new Unit(_values.Length);
			Dirty = true;
			HasNonZeroDuration = false;
		}

		public void ChangedAfterRemoval()
		{
			Dirty = true;
			_allocations.SetFilled(math.min(_allocations.Filled, _lengthForJob.Value));
			if (Count == 0)
				HasNonZeroDuration = false;
		}
	}
}
#endif