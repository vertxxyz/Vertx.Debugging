#if UNITY_EDITOR
#if UNITY_2021_1_OR_NEWER
#define HAS_SET_BUFFER_DATA
#endif
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

		public void SetBufferData(CommandBuffer commandBuffer, UnsafeArray<T> data)
		{
			if (_buffer == null || _buffer.count < data.Length)
			{
				// Expand graphics buffer to encompass the capacity of the list.
				_buffer?.Dispose();
				_buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Length, UnsafeUtility.SizeOf<T>());
			}

#if HAS_SET_BUFFER_DATA
			commandBuffer.SetBufferData(_buffer, data.AsNativeArray(), 0, 0, data.Length);
#else
			_buffer.SetData(data.AsNativeArray(), 0, 0, data.Length);
#endif
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