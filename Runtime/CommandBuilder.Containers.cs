#if UNITY_2020_1_OR_NEWER
#define HAS_GRAPHICS_BUFFER
#endif
#if UNITY_2021_1_OR_NEWER
#define HAS_SET_BUFFER_DATA
#endif
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	// ReSharper disable once ClassCannotBeInstantiated
	public sealed partial class CommandBuilder
	{
		private class ListWrapper<T> : IDisposable where T : unmanaged
		{
			private const int InitialListCapacity = 32;

			public NativeList<T> List;
			public int Count => List.IsCreated ? List.Length : 0;

			public void Create() => List = new NativeList<T>(InitialListCapacity, Allocator.Persistent);

			public virtual void Dispose()
			{
				if (List.IsCreated)
					List.Dispose();
			}
		}

		private sealed class ListAndBuffer<T> : ListWrapper<T> where T : unmanaged
		{
			private readonly int _bufferId;
#if HAS_GRAPHICS_BUFFER
			private GraphicsBuffer _buffer;

#else
			private ComputeBuffer _buffer;
#endif

			private ListAndBuffer() { }

			public ListAndBuffer(string bufferName) => _bufferId = Shader.PropertyToID(bufferName);

			public void SetBufferData(CommandBuffer commandBuffer)
			{
				if (_buffer == null || _buffer.count < List.Capacity)
				{
					// Expand graphics buffer to encompass the capacity of the list.
					_buffer?.Dispose();
#if HAS_GRAPHICS_BUFFER
					_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, List.Capacity, UnsafeUtility.SizeOf<T>());
#else
					_buffer = new ComputeBuffer(List.Capacity, UnsafeUtility.SizeOf<T>(), ComputeBufferType.Structured);
#endif
				}

#if HAS_SET_BUFFER_DATA
				commandBuffer.SetBufferData(_buffer, List.AsArray(), 0, 0, List.Length);
#else
				_buffer.SetData(List.AsArray(), 0, 0, List.Length);
#endif
			}

			public void SetBufferToPropertyBlock(MaterialPropertyBlock propertyBlock) => propertyBlock.SetBuffer(_bufferId, _buffer);

			public override void Dispose()
			{
				base.Dispose();
				_buffer?.Dispose();
				_buffer = null;
			}
		}

		internal sealed class TextDataLists
		{
			private sealed class TextDataPool :
#if !UNITY_2021_1_OR_NEWER
				Vertx.Debugging.Internal.ObjectPool<Shapes.TextData>
#else
				UnityEngine.Pool.ObjectPool<Shapes.TextData>
#endif
			{
				public TextDataPool(int defaultCapacity = 10, int maxSize = 10000)
					: base(() => new Shapes.TextData(), null, data =>
					{
						data.Camera = null;
						data.Value = null;
					}, null, false, defaultCapacity, maxSize) { }
			}

			private readonly List<float> _durations = new List<float>();
			private readonly TextDataPool _textDataPool = new TextDataPool();
			private readonly List<Shapes.TextData> _elements = new List<Shapes.TextData>();

			public int Count => _elements.Count;

			public List<Shapes.TextData> InternalList => _elements;

			public void Add(in Shapes.Text text, Color backgroundColor, Color textColor, Shapes.DrawModifications modifications, float duration)
			{
				Shapes.TextData element = _textDataPool.Get();
				element.Position = text.Position;
				element.Value = text.Value;
				element.Camera = text.Camera;
				element.BackgroundColor = backgroundColor;
				element.TextColor = textColor;
				element.Modifications = modifications;
				_elements.Add(element);
				_durations.Add(duration);
			}

			public void Clear()
			{
				if (_elements.Count == 0)
					return;
				_elements.Clear();
				_durations.Clear();
			}

			public void RemoveByDeltaTime(float deltaTime)
			{
				for (int index = _elements.Count - 1; index >= 0; index--)
				{
					float oldDuration = _durations[index];
					float newDuration = oldDuration - deltaTime;
					if (newDuration > 0)
					{
						_durations[index] = newDuration;
						// ! Remember to change this when swapping between IJob and IJobFor
						continue;
					}

					// RemoveUnorderedAt, shared logic:
					int endIndex = _durations.Count - 1;

					_durations[index] = _durations[endIndex];
					_durations.RemoveAt(endIndex);

					_textDataPool.Release(_elements[index]);
					_elements[index] = _elements[endIndex];
					_elements.RemoveAt(endIndex);
				}
			}
		}

		private sealed class ShapeBuffersWithData<T> : IDisposable where T : unmanaged
		{
			private bool _dirty = true;
			private readonly ListWrapper<float> _durations;
			private readonly ListAndBuffer<T> _elements;
			private readonly ListAndBuffer<Color> _colors = new ListAndBuffer<Color>("color_buffer");
			private readonly ListAndBuffer<Shapes.DrawModifications> _modifications = new ListAndBuffer<Shapes.DrawModifications>("modifications_buffer");

			private MaterialPropertyBlock _propertyBlock;

			public MaterialPropertyBlock PropertyBlock => _propertyBlock ?? (_propertyBlock = new MaterialPropertyBlock());

			public int Count => _elements.Count;

			public NativeList<T> InternalList => _elements.List;
			public NativeList<float> DurationsInternalList => _durations.List;
			public NativeList<Shapes.DrawModifications> ModificationsInternalList => _modifications.List;
			public NativeList<Color> ColorsInternalList => _colors.List;
			
			public ShapeBuffersWithData(string bufferName, bool usesDurations = true)
			{
				if (usesDurations)
					_durations = new ListWrapper<float>();
				_elements = new ListAndBuffer<T>(bufferName);
			}

			public void Set(CommandBuffer commandBuffer, MaterialPropertyBlock propertyBlock)
			{
				if (_dirty)
				{
					_elements.SetBufferData(commandBuffer);
					_colors.SetBufferData(commandBuffer);
					_modifications.SetBufferData(commandBuffer);
					_dirty = false;
				}

				_elements.SetBufferToPropertyBlock(propertyBlock);
				_colors.SetBufferToPropertyBlock(propertyBlock);
				_modifications.SetBufferToPropertyBlock(propertyBlock);
			}

			public void SetDirty() => _dirty = true;

			private void EnsureCreated()
			{
				if (_elements.List.IsCreated)
					return;
				_elements.Create();
				_colors.Create();
				_modifications.Create();
				_durations?.Create();
				_dirty = true;
			}

			public void Add(T element, Color color, Shapes.DrawModifications modifications, float duration)
			{
				EnsureCreated();
				_elements.List.Add(element);
				_colors.List.Add(color);
				_modifications.List.Add(modifications);
				_durations?.List.Add(duration);
				_dirty = true;
			}

			public void Clear()
			{
				if (_elements.Count == 0)
					return;
				_elements.List.Clear();
				_colors.List.Clear();
				_modifications.List.Clear();
				_durations?.List.Clear();
				_dirty = true;
			}

			public void Dispose()
			{
				if (!_elements.List.IsCreated)
					return;
				_elements.Dispose();
				_colors.Dispose();
				_modifications.Dispose();
				_durations?.Dispose();
			}
		}
	}
}