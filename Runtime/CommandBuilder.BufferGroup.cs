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
			public readonly ShapeBuffersWithData<Shapes.Line> Lines;
			public readonly ShapeBuffersWithData<Shapes.Arc> Arcs;
			public readonly ShapeBuffersWithData<Shapes.Box> Boxes;
			public readonly ShapeBuffersWithData<Shapes.Box2D> Box2Ds;
			public readonly ShapeBuffersWithData<Shapes.Outline> Outlines;
			public readonly ShapeBuffersWithData<Shapes.Cast> Casts;
			public readonly TextDataLists Texts = new TextDataLists();
			public readonly ScreenTextDataLists ScreenTexts = new ScreenTextDataLists();

			// Command buffer only used by the Built-in render pipeline.
			private CommandBuffer _commandBuffer;

			public BufferGroup(bool usesDurations, string commandBufferName)
			{
				_commandBufferName = commandBufferName;
				Lines = new ShapeBuffersWithData<Shapes.Line>("line_buffer", usesDurations);
				Arcs = new ShapeBuffersWithData<Shapes.Arc>("arc_buffer", usesDurations);
				Boxes = new ShapeBuffersWithData<Shapes.Box>("box_buffer", usesDurations);
				Box2Ds = new ShapeBuffersWithData<Shapes.Box2D>("mesh_buffer", usesDurations);
				Outlines = new ShapeBuffersWithData<Shapes.Outline>("outline_buffer", usesDurations);
				Casts = new ShapeBuffersWithData<Shapes.Cast>("cast_buffer", usesDurations);
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
				Box2Ds.Clear();
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
				Box2Ds.Dispose();
				Outlines.Dispose();
				Casts.Dispose();
				_commandBuffer?.Dispose();
			}

			public void RemoveByDeltaTime(float deltaTime, JobHandle? dependency)
			{
				Texts.RemoveByDeltaTime(deltaTime);
				ScreenTexts.RemoveByDeltaTime(deltaTime);

				JobHandle? coreHandle = null;
				int oldLineCount = QueueRemovalJob<Shapes.Line, RemovalJob<Shapes.Line>>(Lines, dependency, ref coreHandle);
				int oldArcCount = QueueRemovalJob<Shapes.Arc, RemovalJob<Shapes.Arc>>(Arcs, dependency, ref coreHandle);
				int oldBoxCount = QueueRemovalJob<Shapes.Box, RemovalJob<Shapes.Box>>(Boxes, dependency, ref coreHandle);
				int oldBox2DCount = QueueRemovalJob<Shapes.Box2D, RemovalJob<Shapes.Box2D>>(Box2Ds, dependency, ref coreHandle);
				int oldOutlineCount = QueueRemovalJob<Shapes.Outline, RemovalJob<Shapes.Outline>>(Outlines, dependency, ref coreHandle);
				int oldMatrixAndVectorsCount = QueueRemovalJob<Shapes.Cast, RemovalJob<Shapes.Cast>>(Casts, dependency, ref coreHandle);

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

					if (Box2Ds.Count != oldBox2DCount)
						Box2Ds.SetDirty();

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

					var removalJob = new TJob();
					removalJob.Configure(
						data.InternalList,
						data.DurationsInternalList,
						data.ModificationsInternalList,
						data.ColorsInternalList,
						deltaTime
					);
					handleOut = !handleOut.HasValue 
						? removalJob.Schedule(handleIn ?? default)
						: JobHandle.CombineDependencies(handleOut.Value, removalJob.Schedule(handleIn ?? default));
					return length;
				}
			}
		}
	}
}
#endif