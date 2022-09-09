using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertx.Debugging
{
	public sealed partial class CommandBuilder
	{
		private class ListAndBuffer<T> : IDisposable where T : unmanaged
		{
			private const int InitialListCapacity = 32;

			public int BufferId { get; }
			private NativeList<T> _list;
			private GraphicsBuffer _buffer;
			private bool _dirty = true;

			public int Count => _list.IsCreated ? _list.Length : 0;
			
			public void Add(in T line)
			{
				_list.Add(line);
				_dirty = true;
			}
			
			public T this[int i]
			{
				get => _list[i];
				set
				{
					_list[i] = value;
					_dirty = true;
				}
			}
			
			public void SetValueWithoutNotify(int i, T value) => _list[i] = value;

			public ListAndBuffer(string bufferName) => BufferId = Shader.PropertyToID(bufferName);

			public GraphicsBuffer Buffer
			{
				get
				{
					if (_buffer == null)
					{
						_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _list.Capacity, UnsafeUtility.SizeOf<T>());
						_dirty = true;
					}
					else if (_buffer.count < _list.Capacity)
					{
						_buffer.Dispose();
						_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _list.Capacity, UnsafeUtility.SizeOf<T>());
						_dirty = true;
					}

					return _buffer;
				}
			}

			public GraphicsBuffer BufferUnchecked => _buffer;

			public void EnsureCreated()
			{
				if (!_list.IsCreated)
				{
					_list = new NativeList<T>(InitialListCapacity, Allocator.Persistent);
					_dirty = true;
				}
			}

			public void Dispose()
			{
				if (_list.IsCreated)
					_list.Dispose();
				_buffer?.Dispose();
				_dirty = true;
			}

			public void SetGraphicsBufferDataIfDirty(CommandBuffer commandBuffer)
			{
				if (!_dirty)
					return;
				commandBuffer.SetBufferData(Buffer, _list.AsArray(), 0, 0, _list.Length);
				_dirty = false;
			}

			public void Clear()
			{
				if (_list.IsCreated)
					_list.Clear();
				_dirty = true;
			}
		}

		private class ListBufferAndMpb<T> : ListAndBuffer<T> where T : unmanaged
		{
			private MaterialPropertyBlock _propertyBlock;

			public MaterialPropertyBlock PropertyBlock
			{
				get
				{
					if (_propertyBlock == null)
						_propertyBlock = new MaterialPropertyBlock();
					return _propertyBlock;
				}
			}

			public ListBufferAndMpb(string bufferName) : base(bufferName) { }
		}
	}
}