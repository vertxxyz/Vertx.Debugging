using System.Collections.Generic;
using UnityEngine;
#if !UNITY_2021_1_OR_NEWER
using Vertx.Debugging.Internal;
#else
using UnityEngine.Pool;
#endif

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public struct MeshNormals : IDrawable
		{
			public Mesh Mesh;
			public Matrix4x4 Matrix;
			public float ArrowLength;

			public MeshNormals(Mesh mesh, Matrix4x4 matrix4X4, float arrowLength)
			{
				Mesh = mesh;
				Matrix = matrix4X4;
				ArrowLength = arrowLength;
			}

			public MeshNormals(Mesh mesh, Transform transform, float arrowLength)
				: this(mesh, transform.localToWorldMatrix, arrowLength) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (Mesh == null)
					return;

				if (!Mesh.isReadable)
				{
					Debug.LogWarning($"{Mesh} must be marked as Read/Write to use {nameof(MeshNormals)}.");
					return;
				}

				var unscaledMatrix = Matrix4x4.TRS(Matrix.MultiplyPoint(Vector3.zero), Matrix.rotation, Vector3.one);

				using (ListPool<Vector3>.Get(out List<Vector3> vertices))
				using (ListPool<Vector3>.Get(out List<Vector3> normals))
				{
					Mesh.GetVertices(vertices);
					Mesh.GetNormals(normals);
					for (int i = 0; i < vertices.Count; i++)
						new Arrow(Matrix.MultiplyPoint(vertices[i]), unscaledMatrix.MultiplyVector(normals[i]) * ArrowLength).Draw(commandBuilder, color, duration);
				}
			}
#endif
		}
	}
}