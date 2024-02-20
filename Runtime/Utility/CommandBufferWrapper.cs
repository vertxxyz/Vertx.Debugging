using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertx.Debugging
{
	internal interface ICommandBuffer
	{
		void SetGlobalMatrix(int key, Matrix4x4 matrix);
		
		void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties);
		
		void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : unmanaged;
	}
	
	internal sealed class CommandBufferWrapper : ICommandBuffer
	{
		public CommandBuffer CommandBuffer { get; private set; }

		public CommandBufferWrapper(CommandBuffer commandBuffer) => CommandBuffer = commandBuffer;

		public void OverrideCommandBuffer(CommandBuffer commandBuffer) => CommandBuffer = commandBuffer;

		public void SetGlobalMatrix(int key, Matrix4x4 matrix) => CommandBuffer.SetGlobalMatrix(key, matrix);

		public void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : unmanaged
			=> CommandBuffer.SetBufferData(buffer, data, nativeBufferStartIndex, graphicsBufferStartIndex, count);

		public void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties)
			=> CommandBuffer.DrawMeshInstancedProcedural(mesh, submeshIndex, material, shaderPass, count, properties);

		public void Dispose() => CommandBuffer?.Dispose();
	}
	
#if VERTX_URP
	internal sealed class UnsafeCommandBufferWrapper : ICommandBuffer
	{
		public UnsafeCommandBuffer CommandBuffer { get; private set; }

		public UnsafeCommandBufferWrapper(UnsafeCommandBuffer commandBuffer) => 
			CommandBuffer = commandBuffer;

		public void OverrideCommandBuffer(UnsafeCommandBuffer commandBuffer) => CommandBuffer = commandBuffer;

		public void SetGlobalMatrix(int key, Matrix4x4 matrix) => CommandBuffer.SetGlobalMatrix(key, matrix);

		public void SetBufferData<T>(GraphicsBuffer buffer, NativeArray<T> data, int nativeBufferStartIndex, int graphicsBufferStartIndex, int count) where T : unmanaged
			=> CommandBuffer.SetBufferData(buffer, data, nativeBufferStartIndex, graphicsBufferStartIndex, count);

		public void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties)
			=> CommandBuffer.DrawMeshInstancedProcedural(mesh, submeshIndex, material, shaderPass, count, properties);
	}
#endif
}