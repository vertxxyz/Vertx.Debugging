using Unity.Collections;
using UnityEngine;
#if !UNITY_2021_1_OR_NEWER
using Vertx.Debugging.Internal;
#else
using UnityEngine.Pool;
#endif
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		public readonly struct MeshNormals : IDrawable
		{
			public readonly Mesh Mesh;
			public readonly Matrix4x4 Matrix;
			public readonly float ArrowLength;

			public MeshNormals(Mesh mesh, Matrix4x4 matrix4X4, float arrowLength)
			{
				Mesh = mesh;
				Matrix = matrix4X4;
				ArrowLength = arrowLength;
			}

			public MeshNormals(Mesh mesh, Transform transform, float arrowLength)
				: this(mesh, transform.localToWorldMatrix, arrowLength) { }

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (Mesh == null)
					return;

				Matrix4x4 matrix = Matrix;
				var unscaledMatrix = Matrix4x4.TRS(matrix.MultiplyPoint(Vector3.zero), matrix.rotation, Vector3.one);

				var array = Mesh.AcquireReadOnlyMeshData(Mesh);

				for (int i = 0; i < array.Length; i++)
				{
					var meshData = array[i];
					var vertices = new NativeArray<Vector3>(meshData.vertexCount, Allocator.Temp);
					var normals = new NativeArray<Vector3>(meshData.vertexCount, Allocator.Temp);
					meshData.GetVertices(vertices);
					meshData.GetNormals(normals);
					for (int j = 0; j < meshData.vertexCount; j++)
						new Arrow(matrix.MultiplyPoint(vertices[j]), unscaledMatrix.MultiplyVector(normals[j]) * ArrowLength).Draw(ref commandBuilder, color, duration);
				}
			}
#endif
		}
	}
}