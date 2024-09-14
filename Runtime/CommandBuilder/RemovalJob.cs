#if UNITY_EDITOR
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Vertx.Debugging
{
	[BurstCompile]
	internal struct RemovalJob<T> : IJob where T : unmanaged
	{
		[NativeDisableUnsafePtrRestriction] private readonly UnsafeArray<float> _durations;
		[NativeDisableUnsafePtrRestriction] private readonly UnsafeArray<T> _elements;
		[ReadOnly] private readonly float _deltaTime;
		private NativeReference<int> _length;

		public RemovalJob(UnmanagedCommandContainer<T> group, float deltaTime)
		{
			_durations = group.Durations;
			_elements = group.Values;
			_length = group.LengthForJob;
			_deltaTime = deltaTime;
		}

		/// <summary>
		/// Removes indices in an unordered fashion,
		/// But the removals happen identically across the buffers, so this is not a concern.
		/// </summary>
		public void Execute()
		{
			int endIndex = _length.Value;
			for (int index = _length.Value - 1; index >= 0; index--)
			{
				float newDuration = _durations[index] - _deltaTime;
				if (newDuration > 0)
				{
					_durations[index] = newDuration;
					// ! Remember to change this when swapping between IJob and IJobFor
					continue;
				}

				// RemoveUnorderedAt, shared logic:
				endIndex--;
				_durations[index] = _durations[endIndex];
				_elements[index] = _elements[endIndex];
			}

			_length.Value = endIndex;
		}
	}
}
#endif