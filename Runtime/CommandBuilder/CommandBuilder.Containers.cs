#if UNITY_EDITOR
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
	internal sealed partial class CommandBuilder
	{
		private static readonly int s_InstanceCountKey = Shader.PropertyToID("_InstanceCount");
		
		private sealed class BufferWrapper<T> where T : unmanaged
		{
			private readonly int _bufferId;
			private GraphicsBuffer _buffer;

			private BufferWrapper() { }

			public BufferWrapper(string bufferName) => _bufferId = Shader.PropertyToID(bufferName);

			public void SetBufferData(CommandBuffer commandBuffer, UnsafeList<T> data)
			{
				if (_buffer == null || _buffer.count < data.Capacity)
				{
					// Expand graphics buffer to encompass the capacity of the list.
					_buffer?.Dispose();
					_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Capacity, UnsafeUtility.SizeOf<T>());
				}

#if HAS_SET_BUFFER_DATA
				commandBuffer.SetBufferData(_buffer, AsArray(data), 0, 0, data.Length);
#else
				_buffer.SetData(AsArray(data), 0, 0, data.Length);
#endif
			}
			
			public unsafe NativeArray<T> AsArray(UnsafeList<T> list)
			{
				NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
				return array;
			}

			public void SetBufferToPropertyBlock(MaterialPropertyBlock propertyBlock) => propertyBlock.SetBuffer(_bufferId, _buffer);

			public void Dispose()
			{
				_buffer?.Dispose();
				_buffer = null;
			}
		}

		internal sealed class TextDataLists : TextDataLists<Shape.TextData>
		{
			public void Add(in Shape.Text text, Color backgroundColor, Color textColor, float duration)
			{
				Shape.TextData element = s_TextDataPool.Get();
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
		
		internal sealed class ScreenTextDataLists : TextDataLists<Shape.ScreenTextData>
		{
			public void Add(in Shape.ScreenText text, Color backgroundColor, Color textColor, float duration)
			{
				Shape.ScreenTextData element = s_TextDataPool.Get();
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

		internal abstract class TextDataLists<T> where T : class, Shape.IText, new()
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
		
		private sealed class ShapeBuffer<T> : IDisposable where T : unmanaged
		{
			private readonly BufferWrapper<T> _elements;
			private MaterialPropertyBlock _propertyBlock;
			public MaterialPropertyBlock PropertyBlock => _propertyBlock ?? (_propertyBlock = new MaterialPropertyBlock());
			public ShapeBuffer(string bufferName) => _elements = new BufferWrapper<T>(bufferName);

			public void Set(CommandBuffer commandBuffer, MaterialPropertyBlock propertyBlock, UnsafeList<T> elements, bool elementsDirty)
			{
				if (elementsDirty)
					_elements.SetBufferData(commandBuffer, elements);

				_elements.SetBufferToPropertyBlock(propertyBlock);
				propertyBlock.SetInt(s_InstanceCountKey, elements.Length);
			}

			public void Dispose() => _elements.Dispose();
		}
	}
}
#endif