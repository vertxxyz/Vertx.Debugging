#if UNITY_EDITOR
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertx.Debugging
{
	internal sealed class BufferWrapper<T> where T : unmanaged
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
}
#endif