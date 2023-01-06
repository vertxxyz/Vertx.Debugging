using UnityEngine;

namespace Vertx.Debugging
{
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Debugging/Debug Mesh Normals")]
	public sealed class DebugMeshNormals : DebugComponentBase
	{
		[SerializeField] private Color _color = Shape.CastColor;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private float _rayLength = 0.1f;

		private void Reset() => _meshFilter = GetComponent<MeshFilter>();

		protected override bool ShouldDraw()
		{
			if (_meshFilter == null)
				return false;
			Mesh mesh = _meshFilter.sharedMesh;
#if !UNITY_2020_1_OR_NEWER
			return mesh != null && mesh.isReadable;
#else
			return mesh != null;
#endif
		}

		protected override void Draw()
			=> D.raw(new Shape.MeshNormals(_meshFilter.sharedMesh, transform, _rayLength), _color);
	}
}