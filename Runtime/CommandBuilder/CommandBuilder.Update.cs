#if UNITY_EDITOR
using System;
using System.Linq;
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
	internal sealed partial class CommandBuilder
	{
		private const string RemoveShapesByDurationProfilerName = Name + " " + nameof(RemoveShapesByDuration);

		private static float s_TimeThisFrame;
		private static float s_DeltaTimeThisFrame;

		/// <summary>
		/// Queues <see cref="FrameInitialization"/> into the <see cref="Initialization"/> portion of the player loop.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitialiseUpdate()
		{
			PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subsystems = playerLoop.subSystemList.ToArray();
			InjectFirstIn(typeof(Initialization), typeof(VertxDebuggingInitialization), FrameInitialization);
			InjectFirstIn(typeof(FixedUpdate), typeof(VertxDebuggingFixedUpdate), StartFixedUpdate);
			playerLoop.subSystemList = subsystems;
			UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
			return;

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

		private static void FrameInitialization()
		{
			UpdateContext.ForceStateToUpdate();
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			s_DeltaTimeThisFrame = Time.deltaTime;
			if (s_DeltaTimeThisFrame == 0)
			{
				// The game is paused, we don't need to clean up or transfer any data.
			}
			else
			{
				Instance.RemoveShapesByDuration(s_DeltaTimeThisFrame);
			}

			s_TimeThisFrame = Time.time;
			ref var builder = ref UnmanagedCommandBuilder.Instance.Data;
			builder.Time = s_TimeThisFrame;
		}

		private static void StartFixedUpdate()
		{
			ref var builder = ref UnmanagedCommandBuilder.Instance.Data;
			builder.FixedTime = Time.fixedTime;
			builder.FixedTimeStep = Time.fixedDeltaTime;
		}

		/// <summary>
		/// Remove data where the duration has been met.
		/// </summary>
		private void RemoveShapesByDuration(float deltaTime)
		{
			Profiler.BeginSample(RemoveShapesByDurationProfilerName);
			ref var unmanagedCommandBuilder = ref UnmanagedCommandBuilder.Instance.Data;
			_defaultGroup.RemoveByDeltaTime(deltaTime, ref unmanagedCommandBuilder.Standard);
			Profiler.EndSample();
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