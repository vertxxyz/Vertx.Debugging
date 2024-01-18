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
		[NativeDisableUnsafePtrRestriction]
		public UnsafeArray<float> Durations;
		[NativeDisableUnsafePtrRestriction]
		public UnsafeArray<T> Elements;
		[ReadOnly]
		public float DeltaTime;

		public NativeReference<int> Length;

		public RemovalJob(UnmanagedCommandContainer<T> group, float deltaTime)
		{
			Durations = group.Durations;
			Elements = group.Values;
			Length = group.LengthForJob;
			DeltaTime = deltaTime;
		}

		/// <summary>
		/// Removes indices in an unordered fashion,
		/// But the removals happen identically across the buffers, so this is not a concern.
		/// </summary>
		public void Execute()
		{
			int endIndex = Length.Value;
			for (int index = Length.Value - 1; index >= 0; index--)
			{
				float newDuration = Durations[index] - DeltaTime;
				if (newDuration > 0)
				{
					Durations[index] = newDuration;
					// ! Remember to change this when swapping between IJob and IJobFor
					continue;
				}

				// RemoveUnorderedAt, shared logic:
				endIndex--;
				Durations[index] = Durations[endIndex];
				Elements[index] = Elements[endIndex];
			}

			Length.Value = endIndex;
		}
	}
}
#endif