#if UNITY_EDITOR
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

		internal sealed class TextDataLists : TextDataLists<Shapes.TextData>
		{
			public void Add(in Shapes.Text text, Color backgroundColor, Color textColor, float duration)
			{
				Shapes.TextData element = s_TextDataPool.Get();
				_elements.Add(element);
				_durations.Add(duration);
				_isDirty = true;
				element.Position = UpdateContext.State == UpdateContext.UpdateState.CapturingGizmos ? Gizmos.matrix.MultiplyPoint3x4(text.Position) : text.Position;
				element.Value = text.Value;
				element.Camera = text.Camera;
				element.BackgroundColor = backgroundColor;
				element.TextColor = textColor;
			}
		}
		
		internal sealed class ScreenTextDataLists : TextDataLists<Shapes.ScreenTextData>
		{
			public void Add(in Shapes.ScreenText text, Color backgroundColor, Color textColor, float duration)
			{
				Shapes.ScreenTextData element = s_TextDataPool.Get();
				_elements.Add(element);
				_durations.Add(duration);
				_isDirty = true;
				element.Value = text.Value;
				element.BackgroundColor = backgroundColor;
				element.TextColor = textColor;
				element.Context = text.Context;
				element.ActiveViews = text.ActiveViews;
			}

			public override void RemoveByDeltaTime(float deltaTime)
			{
				for (int index = _elements.Count - 1; index >= 0; index--)
				{
					float oldDuration = _durations[index];
					float newDuration = oldDuration - deltaTime;
					if (newDuration > 0)
					{
						_durations[index] = newDuration;
						continue;
					}

					// We care about order for on-screen text, so we are forced to do an unoptimised remove.
					_durations.RemoveAt(index);
					s_TextDataPool.Release(_elements[index]);
					_elements.RemoveAt(index);
					_isDirty = true;
				}
			}
		}

		internal abstract class TextDataLists<T> where T : class, Shapes.IText, new()
		{
			protected sealed class TextDataPool :
#if !UNITY_2021_1_OR_NEWER
				Vertx.Debugging.Internal.ObjectPool<T>
#else
				UnityEngine.Pool.ObjectPool<T>
#endif
			{
				public TextDataPool(int defaultCapacity = 10, int maxSize = 10000)
					: base(() => new T(), null, data => data.Reset(), null, false, defaultCapacity, maxSize) { }
			}

			protected static readonly TextDataPool s_TextDataPool = new TextDataPool();
			// ReSharper disable InconsistentNaming
			protected readonly List<float> _durations = new List<float>();
			protected readonly List<T> _elements = new List<T>();
			protected bool _isDirty = true;
			// ReSharper restore InconsistentNaming

			public int Count => _elements.Count;

			public IReadOnlyList<T> Elements => _elements;

			public bool ReadAndResetIsDirty()
			{
				bool isDirty = _isDirty;
				_isDirty = false;
				return isDirty;
			}

			public void Clear()
			{
				if (_elements.Count == 0)
					return;
				foreach (T element in _elements)
					s_TextDataPool.Release(element);
				_elements.Clear();
				_durations.Clear();
				_isDirty = true;
			}

			public virtual void RemoveByDeltaTime(float deltaTime)
			{
				for (int index = _elements.Count - 1; index >= 0; index--)
				{
					float oldDuration = _durations[index];
					float newDuration = oldDuration - deltaTime;
					if (newDuration > 0)
					{
						_durations[index] = newDuration;
						continue;
					}

					// RemoveUnorderedAt, shared logic:
					int endIndex = _durations.Count - 1;

					_durations[index] = _durations[endIndex];
					_durations.RemoveAt(endIndex);

					s_TextDataPool.Release(_elements[index]);
					_elements[index] = _elements[endIndex];
					_elements.RemoveAt(endIndex);
					_isDirty = true;
				}
			}
		}

		private sealed class ShapeBuffersWithData<T> : IDisposable where T : unmanaged
		{
			// Avoids redundantly setting internal GraphicsBuffer data.
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

			public void Add(in T element, Color color, Shapes.DrawModifications modifications, float duration)
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
#endif