#if UNITY_EDITOR
using System;
using Unity.Jobs;
using UnityEngine.Rendering;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		private sealed class BufferGroup : IDisposable
		{
			private readonly string _commandBufferName;
			public readonly ShapeBuffersWithData<LineGroup> Lines;
			public readonly ShapeBuffersWithData<ArcGroup> Arcs;
			public readonly ShapeBuffersWithData<BoxGroup> Boxes;
			public readonly ShapeBuffersWithData<OutlineGroup> Outlines;
			public readonly ShapeBuffersWithData<CastGroup> Casts;
			public readonly TextDataLists Texts = new TextDataLists();
			public readonly ScreenTextDataLists ScreenTexts = new ScreenTextDataLists();

			// Command buffer only used by the Built-in render pipeline.
			private CommandBuffer _commandBuffer;

			public BufferGroup(bool usesDurations, string commandBufferName)
			{
				_commandBufferName = commandBufferName;
				Lines = new ShapeBuffersWithData<LineGroup>("line_buffer", usesDurations);
				Arcs = new ShapeBuffersWithData<ArcGroup>("arc_buffer", usesDurations);
				Boxes = new ShapeBuffersWithData<BoxGroup>("box_buffer", usesDurations);
				Outlines = new ShapeBuffersWithData<OutlineGroup>("outline_buffer", usesDurations);
				Casts = new ShapeBuffersWithData<CastGroup>("cast_buffer", usesDurations);
			}

			/// <summary>
			/// Creates and caches a command buffer if none was passed into the function.<br/>
			/// Buffer is cleared if it was previously cached.
			/// </summary>
			public void ReadyResources(ref CommandBuffer commandBuffer)
			{
				if (commandBuffer != null)
					return;
				if (_commandBuffer == null)
					_commandBuffer = new CommandBuffer { name = _commandBufferName };
				else
					_commandBuffer.Clear();
				commandBuffer = _commandBuffer;
			}

			public void Clear()
			{
				Lines.Clear();
				Arcs.Clear();
				Boxes.Clear();
				Outlines.Clear();
				Casts.Clear();
				Texts.Clear();
				ScreenTexts.Clear();
			}

			public void Dispose()
			{
				Lines.Dispose();
				Arcs.Dispose();
				Boxes.Dispose();
				Outlines.Dispose();
				Casts.Dispose();
				_commandBuffer?.Dispose();
			}

			public void RemoveByDeltaTime(float deltaTime, JobHandle? dependency)
			{
				Texts.RemoveByDeltaTime(deltaTime);
				ScreenTexts.RemoveByDeltaTime(deltaTime);

				JobHandle? coreHandle = null;
				int oldLineCount = QueueRemovalJob<LineGroup, RemovalJob<LineGroup>>(Lines, dependency, ref coreHandle);
				int oldArcCount = QueueRemovalJob<ArcGroup, RemovalJob<ArcGroup>>(Arcs, dependency, ref coreHandle);
				int oldBoxCount = QueueRemovalJob<BoxGroup, RemovalJob<BoxGroup>>(Boxes, dependency, ref coreHandle);
				int oldOutlineCount = QueueRemovalJob<OutlineGroup, RemovalJob<OutlineGroup>>(Outlines, dependency, ref coreHandle);
				int oldMatrixAndVectorsCount = QueueRemovalJob<CastGroup, RemovalJob<CastGroup>>(Casts, dependency, ref coreHandle);

				if (!coreHandle.HasValue)
					coreHandle = dependency;

				if (coreHandle.HasValue)
				{
					coreHandle.Value.Complete();

					if (Lines.Count != oldLineCount)
						Lines.SetDirty();

					if (Arcs.Count != oldArcCount)
						Arcs.SetDirty();

					if (Boxes.Count != oldBoxCount)
						Boxes.SetDirty();
					if (Outlines.Count != oldOutlineCount)
						Outlines.SetDirty();

					if (Casts.Count != oldMatrixAndVectorsCount)
						Casts.SetDirty();
				}

				int QueueRemovalJob<T, TJob>(ShapeBuffersWithData<T> data, JobHandle? handleIn, ref JobHandle? handleOut)
					where T : unmanaged
					where TJob : struct, IRemovalJob<T>
				{
					int length = data.Count;
					if (length == 0)
						return 0;

					if (data.HasNonZeroDuration)
					{
						data.HasNonZeroDuration = false;
						
						var removalJob = new TJob();
						removalJob.Configure(
							data.InternalList,
							data.DurationsInternalList,
							deltaTime
						);
						handleOut = !handleOut.HasValue
							? removalJob.Schedule(handleIn ?? default)
							: JobHandle.CombineDependencies(handleOut.Value, removalJob.Schedule(handleIn ?? default));
						return length;
					}

					data.Clear();
					return 0;
				}
			}
		}
	}
}
#endif