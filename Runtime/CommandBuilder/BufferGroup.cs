#if UNITY_EDITOR
using System;
using Unity.Jobs;
using UnityEngine.Rendering;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace Vertx.Debugging
{
	internal sealed class BufferGroup : IDisposable
	{
		private readonly string _commandBufferName;
		public readonly ShapeBuffer<LineGroup> Lines;
		public readonly ShapeBuffer<DashedLineGroup> DashedLines;
		public readonly ShapeBuffer<ArcGroup> Arcs;
		public readonly ShapeBuffer<BoxGroup> Boxes;
		public readonly ShapeBuffer<OutlineGroup> Outlines;
		public readonly ShapeBuffer<CastGroup> Casts;
		public readonly CommandBuilder.TextDataLists Texts = new CommandBuilder.TextDataLists();
		public readonly CommandBuilder.ScreenTextDataLists ScreenTexts = new CommandBuilder.ScreenTextDataLists();
		private readonly int[] _counters;

		private enum ShapeIndex
		{
			Line = 0,
			DashedLine = 1,
			Arc = 2,
			Box = 3,
			Outline = 4,
			Cast = 5,
			// -------
			Length = 6
		}

		// Command buffer only used by the Built-in render pipeline.
		private CommandBuffer _commandBuffer;

		private BufferGroup() { }

		public BufferGroup(string commandBufferName)
		{
			_commandBufferName = commandBufferName;
			Lines = new ShapeBuffer<LineGroup>("line_buffer");
			DashedLines = new ShapeBuffer<DashedLineGroup>("dashed_line_buffer");
			Arcs = new ShapeBuffer<ArcGroup>("arc_buffer");
			Boxes = new ShapeBuffer<BoxGroup>("box_buffer");
			Outlines = new ShapeBuffer<OutlineGroup>("outline_buffer");
			Casts = new ShapeBuffer<CastGroup>("cast_buffer");
			_counters = new int[(int)ShapeIndex.Length];
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
			Texts.Clear();
			ScreenTexts.Clear();
		}

		public void Dispose()
		{
			Lines.Dispose();
			DashedLines.Dispose();
			Arcs.Dispose();
			Boxes.Dispose();
			Outlines.Dispose();
			Casts.Dispose();
			UnmanagedCommandBuilder.Instance.Data.Dispose();
			_commandBuffer?.Dispose();
		}

		public void RemoveByDeltaTime(float deltaTime, ref UnmanagedCommandGroup group)
		{
			Texts.RemoveByDeltaTime(deltaTime);
			ScreenTexts.RemoveByDeltaTime(deltaTime);

			JobHandle? coreHandle = null;
			_counters[(int)ShapeIndex.Line] = QueueRemovalJob<LineGroup, RemovalJob<LineGroup>>(ref group.Lines, ref coreHandle);
			_counters[(int)ShapeIndex.DashedLine] = QueueRemovalJob<DashedLineGroup, RemovalJob<DashedLineGroup>>(ref group.DashedLines, ref coreHandle);
			_counters[(int)ShapeIndex.Arc] = QueueRemovalJob<ArcGroup, RemovalJob<ArcGroup>>(ref group.Arcs, ref coreHandle);
			_counters[(int)ShapeIndex.Box] = QueueRemovalJob<BoxGroup, RemovalJob<BoxGroup>>(ref group.Boxes, ref coreHandle);
			_counters[(int)ShapeIndex.Outline] = QueueRemovalJob<OutlineGroup, RemovalJob<OutlineGroup>>(ref group.Outlines, ref coreHandle);
			_counters[(int)ShapeIndex.Cast] = QueueRemovalJob<CastGroup, RemovalJob<CastGroup>>(ref group.Casts, ref coreHandle);

			if (coreHandle.HasValue)
			{
				coreHandle.Value.Complete();

				for (ShapeIndex i = 0; i < ShapeIndex.Length; i++)
				{
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (i)
					{
						case ShapeIndex.Line:
						{
							ref var g = ref group.Lines;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
						case ShapeIndex.DashedLine:
						{
							ref var g = ref group.DashedLines;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
						case ShapeIndex.Arc:
						{
							ref var g = ref group.Arcs;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
						case ShapeIndex.Box:
						{
							ref var g = ref group.Boxes;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
						case ShapeIndex.Outline:
						{
							ref var g = ref group.Outlines;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
						case ShapeIndex.Cast:
						{
							ref var g = ref group.Casts;
							if (g.Count == _counters[(int)i]) continue;
							g.ChangedAfterRemoval();
							continue;
						}
					}
				}
			}

			int QueueRemovalJob<T, TJob>(ref UnmanagedCommandContainer<T> data, ref JobHandle? handleOut)
				where T : unmanaged
				where TJob : struct, IRemovalJob<T>
			{
				int length = data.Count;
				if (length == 0)
					return 0;

				if (data.HasNonZeroDuration)
				{
					var removalJob = new TJob();
					removalJob.Configure(
						data.Values,
						data.Durations,
						deltaTime
					);
					handleOut = !handleOut.HasValue
						? removalJob.Schedule()
						: JobHandle.CombineDependencies(handleOut.Value, removalJob.Schedule());
					return length;
				}

				data.Clear();
				return 0;
			}
		}
	}
}
#endif