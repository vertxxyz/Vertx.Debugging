#if UNITY_EDITOR
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Vertx.Debugging
{
	internal sealed class BufferWrapper<T> where T : unmanaged
	{
		internal GraphicsBuffer Buffer { get; set; }

		private readonly int _bufferId;

		public BufferWrapper(string bufferName, UnmanagedCommandContainer<T> unmanagedData)
		{
			_bufferId = Shader.PropertyToID(bufferName);
			Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, unmanagedData.Capacity, UnsafeUtility.SizeOf<T>());
		}

		public void SetBufferData(ICommandBuffer commandBuffer, UnsafeArray<T> data)
		{
			if (Buffer == null || Buffer.count != data.Length)
			{
				Debug.LogWarning("Buffer changed length, this value should remain constant.");
				// Expand graphics buffer to encompass the capacity of the list.
				Buffer?.Dispose();
				Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Length, UnsafeUtility.SizeOf<T>());
			}

			commandBuffer.SetBufferData(Buffer, data.AsNativeArray(), 0, 0, data.Length);
		}

		public void SetBufferToPropertyBlock(MaterialPropertyBlock propertyBlock) => propertyBlock.SetBuffer(_bufferId, Buffer);

		public void Dispose()
		{
			Buffer?.Dispose();
			Buffer = null;
		}
	}
}
#endif