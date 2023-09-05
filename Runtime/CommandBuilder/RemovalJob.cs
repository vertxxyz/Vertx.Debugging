#if UNITY_EDITOR
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Vertx.Debugging
{
	internal interface IRemovalJob<T> : IJob where T : unmanaged
	{
		void Configure(
			UnsafeList<T> elements,
			UnsafeList<float> durations,
			float deltaTime
		);
	}
	
	[BurstCompile]
	internal struct RemovalJob<T> : IRemovalJob<T> where T : unmanaged
	{
		public UnsafeList<float> Durations;
		public UnsafeList<T> Elements;
		[ReadOnly]
		public float DeltaTime;

		public void Configure(UnsafeList<T> elements, UnsafeList<float> durations, float deltaTime)
		{
			Elements = elements;
			Durations = durations;
			DeltaTime = deltaTime;
		}

		/// <summary>
		/// Removes indices in an unordered fashion,
		/// But the removals happen identically across the buffers, so this is not a concern.
		/// </summary>
		public void Execute()
		{
			int endIndex = Durations.Length;
			for (int index = Elements.Length - 1; index >= 0; index--)
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

#if VERTX_COLLECTIONS_1_0_0_PRE_4_OR_NEWER
			int totalRemoved = Durations.Length - endIndex;
				
			// This check is required for earlier versions of collections.
			if (totalRemoved == 0)
				return;
				
			Durations.RemoveRange(endIndex, totalRemoved);
			Elements.RemoveRange(endIndex, totalRemoved);
#else
				if (Durations.Length == endIndex)
					return;

				int last = Durations.Length;
				Durations.RemoveRange(endIndex, last);
				Elements.RemoveRange(endIndex, last);
#endif
		}
	}
}
#endif