#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Vertx.Debugging
{
	internal sealed class ShapeBuffer<T> : IDisposable where T : unmanaged
	{
		internal GraphicsBuffer Buffer => _elements.Buffer;
		private readonly BufferWrapper<T> _elements;
		private MaterialPropertyBlock _propertyBlock;
		// ReSharper disable once ConvertToNullCoalescingCompoundAssignment
		public MaterialPropertyBlock PropertyBlock => _propertyBlock ?? (_propertyBlock = new MaterialPropertyBlock());
		public ShapeBuffer(string bufferName, UnmanagedCommandContainer<T> unmanagedData) => _elements = new BufferWrapper<T>(bufferName, unmanagedData);

		public void Set(ICommandBuffer commandBuffer, MaterialPropertyBlock propertyBlock, UnsafeArray<T> elements, int length, bool elementsDirty)
		{
			if (elementsDirty)
				_elements.SetBufferData(commandBuffer, elements);

			_elements.SetBufferToPropertyBlock(propertyBlock);
			propertyBlock.SetInt(CommandBuilder.s_InstanceCountKey, length);
		}

		public void Dispose() => _elements.Dispose();
	}
}
#endif