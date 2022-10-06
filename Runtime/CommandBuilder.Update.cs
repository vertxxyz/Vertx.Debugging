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
			if (Time.deltaTime == 0)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				RemoveShapesByDuration(Time.deltaTime, null);
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
				NativeList<Shapes.DrawModifications> modifications,
				NativeList<Color> colors,
				float deltaTime
			);
		}

#if VERTX_BURST
		[Unity.Burst.BurstCompile]
#endif
		private struct RemovalJob<T> : IRemovalJob<T> where T : unmanaged
		{
			public NativeList<T> Elements;
			public NativeList<float> Durations;
			public NativeList<Shapes.DrawModifications> Modifications;
			public NativeList<Color> Colors;
			public float DeltaTime;

			public void Configure(NativeList<T> elements, NativeList<float> durations, NativeList<Shapes.DrawModifications> modifications, NativeList<Color> colors, float deltaTime)
			{
				Elements = elements;
				Durations = durations;
				Modifications = modifications;
				Colors = colors;
				DeltaTime = deltaTime;
			}

			/// <summary>
			/// Removes indices in an unordered fashion,
			/// But the removals happen identically across the buffers, so this is not a concern.
			/// </summary>
			public void Execute()
			{
				for (int index = Elements.Length - 1; index >= 0; index--)
				{
					float oldDuration = Durations[index];
					float newDuration = oldDuration - DeltaTime;
					if (newDuration > 0)
					{
						Durations[index] = newDuration;
						// ! Remember to change this when swapping between IJob and IJobFor
						continue;
					}

					// RemoveUnorderedAt, shared logic:
					int endIndex = Durations.Length - 1;

					Durations[index] = Durations[endIndex];
					Durations.RemoveAt(endIndex);

					Elements[index] = Elements[endIndex];
					Elements.RemoveAt(endIndex);

					Modifications[index] = Modifications[endIndex];
					Modifications.RemoveAt(endIndex);

					Colors[index] = Colors[endIndex];
					Colors.RemoveAt(endIndex);
				}
			}
		}

		private void OnUpdate()
		{
#if VERTX_HDRP
			DrawRuntimeBehaviour.Instance.InitialiseRenderPipelineSetup();
#endif
			// TODO cleanup if things aren't running and stuff is getting out of hand...
		}

		private static bool IsInFixedUpdate()
#if UNITY_2020_3_OR_NEWER
			=> Time.inFixedTimeStep;
#else
			=> Time.deltaTime == Time.fixedDeltaTime;
#endif

		private float GetDuration(float duration)
		{
			// Calls from FixedUpdate should hang around until the next FixedUpdate, at minimum.
			if (IsInFixedUpdate() && duration < Time.fixedDeltaTime)
			{
				// Time from the last 
				// ReSharper disable once ArrangeRedundantParentheses
				duration += (Time.fixedTime + Time.fixedDeltaTime) - _timeThisFrame;
			}

			return duration;
		}
	}
}
#endif