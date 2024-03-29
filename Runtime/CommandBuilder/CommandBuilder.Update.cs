#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
#define HAS_CONTEXT_RENDERING
#endif
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using Vertx.Debugging.PlayerLoop;
#if VERTX_HDRP
using Vertx.Debugging.Internal;
#endif

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		private const string RemoveShapesByDurationProfilerName = Name + " " + nameof(RemoveShapesByDuration);

		private float _timeThisFrame;
		private float _deltaTimeThisFrame;

		/// <summary>
		/// Queues <see cref="EarlyUpdate"/> into the EarlyUpdate portion of the player loop.
		/// </summary>
		[InitializeOnLoadMethod, RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitialiseUpdate()
		{
			PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subsystems = playerLoop.subSystemList.ToArray();
			Type earlyUpdate = typeof(EarlyUpdate);
			InjectFirstIn(earlyUpdate, typeof(VertxDebugging), Instance.EarlyUpdate);

			playerLoop.subSystemList = subsystems;
			UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);

			void InjectFirstIn(Type type, Type actionType, PlayerLoopSystem.UpdateFunction action)
			{
				for (int i = 0; i < subsystems.Length; i++)
				{
					if (subsystems[i].type != type)
						continue;

					var earlyUpdateSystem = subsystems[i];
					PlayerLoopSystem[] source = earlyUpdateSystem.subSystemList;
					for (int j = 0; j < source.Length; j++)
					{
						// Already appended time callback.
						if (source[j].type == actionType)
							return;
					}

					PlayerLoopSystem[] dest = new PlayerLoopSystem[source.Length + 1];
					Array.Copy(source, 0, dest, 1, source.Length);
					dest[0] = new PlayerLoopSystem
					{
						type = actionType,
						updateDelegate = action
					};
					subsystems[i].subSystemList = dest;
				}
			}
		}

		private void EarlyUpdate()
		{
			UpdateContext.ForceStateToUpdate();
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			_deltaTimeThisFrame = Time.deltaTime;
			if (_deltaTimeThisFrame == 0)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				RemoveShapesByDuration(_deltaTimeThisFrame, null);
			}

			_timeThisFrame = Time.time;
		}

		/// <summary>
		/// Remove data where the duration has been met.
		/// </summary>
		private void RemoveShapesByDuration(float deltaTime, JobHandle? dependency)
		{
			Profiler.BeginSample(RemoveShapesByDurationProfilerName);
			_defaultGroup.RemoveByDeltaTime(deltaTime, dependency);
			Profiler.EndSample();
		}

		private interface IRemovalJob<T> : IJob where T : unmanaged
		{
			void Configure(
				NativeList<T> elements,
				NativeList<float> durations,
				float deltaTime
			);
		}

#if VERTX_BURST
		[Unity.Burst.BurstCompile]
#endif
		private struct RemovalJob<T> : IRemovalJob<T> where T : unmanaged
		{
			public NativeList<float> Durations;
			public NativeList<T> Elements;
			[ReadOnly]
			public float DeltaTime;

			public void Configure(NativeList<T> elements, NativeList<float> durations, float deltaTime)
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

		private void OnUpdate()
		{
#if VERTX_HDRP
			DrawRuntimeBehaviour.Instance.InitialiseRenderPipelineSetup();
#endif
			// TODO cleanup if things aren't running and stuff is getting out of hand...
		}
	}
}
#endif