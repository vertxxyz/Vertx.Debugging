#if UNITY_EDITOR
using System;
using Unity.Collections;
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
		public CommandBufferWrapper BuiltInCommandBuffer { get; private set; }

		private BufferGroup() { }

		public BufferGroup(string commandBufferName, UnmanagedCommandGroup unmanagedGroup)
		{
			_commandBufferName = commandBufferName;
			Lines = new ShapeBuffer<LineGroup>("line_buffer", unmanagedGroup.Lines);
			DashedLines = new ShapeBuffer<DashedLineGroup>("dashed_line_buffer", unmanagedGroup.DashedLines);
			Arcs = new ShapeBuffer<ArcGroup>("arc_buffer", unmanagedGroup.Arcs);
			Boxes = new ShapeBuffer<BoxGroup>("box_buffer", unmanagedGroup.Boxes);
			Outlines = new ShapeBuffer<OutlineGroup>("outline_buffer", unmanagedGroup.Outlines);
			Casts = new ShapeBuffer<CastGroup>("cast_buffer", unmanagedGroup.Casts);
			_counters = new int[(int)ShapeIndex.Length];
		}

		/// <summary>
		/// Creates and caches a command buffer if none was passed into the function.<br/>
		/// Buffer is cleared if it was previously cached.
		/// </summary>
		public void ReadyResources(ref ICommandBuffer commandBuffer)
		{
			if (commandBuffer != null)
				return;
			if (BuiltInCommandBuffer == null)
				BuiltInCommandBuffer = new CommandBufferWrapper(new CommandBuffer { name = _commandBufferName });
			else
				BuiltInCommandBuffer.CommandBuffer.Clear();
			commandBuffer = BuiltInCommandBuffer;
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
			BuiltInCommandBuffer?.Dispose();
		}

		public void RemoveByDeltaTime(float deltaTime, ref UnmanagedCommandGroup group)
		{
			Texts.RemoveByDeltaTime(deltaTime);
			ScreenTexts.RemoveByDeltaTime(deltaTime);

			JobHandle? coreHandle = null;
			_counters[(int)ShapeIndex.Line] = QueueRemovalJob(ref group.Lines, new RemovalJob<LineGroup>(group.Lines, deltaTime), ref coreHandle);
			_counters[(int)ShapeIndex.DashedLine] = QueueRemovalJob(ref group.DashedLines, new RemovalJob<DashedLineGroup>(group.DashedLines, deltaTime), ref coreHandle);
			_counters[(int)ShapeIndex.Arc] = QueueRemovalJob(ref group.Arcs, new RemovalJob<ArcGroup>(group.Arcs, deltaTime), ref coreHandle);
			_counters[(int)ShapeIndex.Box] = QueueRemovalJob(ref group.Boxes, new RemovalJob<BoxGroup>(group.Boxes, deltaTime), ref coreHandle);
			_counters[(int)ShapeIndex.Outline] = QueueRemovalJob(ref group.Outlines, new RemovalJob<OutlineGroup>(group.Outlines, deltaTime), ref coreHandle);
			_counters[(int)ShapeIndex.Cast] = QueueRemovalJob(ref group.Casts, new RemovalJob<CastGroup>(group.Casts, deltaTime), ref coreHandle);

			if (!coreHandle.HasValue)
				return;

			coreHandle.Value.Complete();

			for (ShapeIndex i = 0; i < ShapeIndex.Length; i++)
			{
				int oldCount = _counters[(int)i];
				// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
				switch (i)
				{
					case ShapeIndex.Line:
					{
						ref var g = ref group.Lines;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
					case ShapeIndex.DashedLine:
					{
						ref var g = ref group.DashedLines;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
					case ShapeIndex.Arc:
					{
						ref var g = ref group.Arcs;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
					case ShapeIndex.Box:
					{
						ref var g = ref group.Boxes;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
					case ShapeIndex.Outline:
					{
						ref var g = ref group.Outlines;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
					case ShapeIndex.Cast:
					{
						ref var g = ref group.Casts;
						if (g.LengthForJob.Value == oldCount) continue;
						g.ChangedAfterRemoval();
						continue;
					}
				}
			}

			return;

			int QueueRemovalJob<T>(ref UnmanagedCommandContainer<T> data, RemovalJob<T> job, ref JobHandle? handleOut)
				where T : unmanaged
			{
				int length = data.Count;
				if (length == 0)
					return 0;

				if (data.HasNonZeroDuration)
				{
					NativeReference<int> lengthForJob = data.LengthForJob;
					lengthForJob.Value = data.Count;
					handleOut = !handleOut.HasValue
						? job.Schedule()
						: JobHandle.CombineDependencies(handleOut.Value, job.Schedule());
					return length;
				}

				data.Clear();
				return 0;
			}
		}
	}
}
#endif