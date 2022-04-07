using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		[Conditional("UNITY_EDITOR")]
		public static void DrawMeshNormals(Mesh mesh, Transform transform, float rayLength, Color color) =>
			DrawMeshNormals(mesh, transform.localToWorldMatrix, rayLength, color);

		[Conditional("UNITY_EDITOR")]
		public static void DrawMeshNormals(Mesh mesh, Matrix4x4 localToWorld, float rayLength, Color color)
		{
			if (mesh == null)
				return;

			if (!mesh.isReadable)
			{
				Debug.LogWarning($"{mesh} must be marked as Read/Write to use {nameof(DrawMeshNormals)}.");
				return;
			}

			var unscaledMatrix = Matrix4x4.TRS(localToWorld.GetPosition(), localToWorld.rotation, Vector3.one);

			using (ListPool<Vector3>.Get(out List<Vector3> vertices))
			using (ListPool<Vector3>.Get(out List<Vector3> normals))
			{
				mesh.GetVertices(vertices);
				mesh.GetNormals(normals);
				for (int i = 0; i < vertices.Count; i++)
					DrawArrow(localToWorld.MultiplyPoint(vertices[i]), unscaledMatrix.MultiplyVector(normals[i]) * rayLength,
						arrowheadScale: rayLength, color: color);
			}
		}

		private static Vector3 Vector3Abs(Vector3 vector3) =>
			new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
	}
}