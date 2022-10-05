using UnityEngine;

namespace Vertx.Debugging
{
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Debugging/Debug Mesh Normals")]
	public sealed class DebugMeshNormals : DebugComponentBase
	{
		[SerializeField] private Color _color = Shapes.CastColor;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private float _rayLength = 0.1f;

		private void Reset() => _meshFilter = GetComponent<MeshFilter>();

		protected override bool ShouldDraw()
		{
			if (_meshFilter == null)
				return false;
			Mesh mesh = _meshFilter.sharedMesh;
			return mesh != null && mesh.isReadable;
		}

		protected override void Draw()
			=> D.raw(new Shapes.MeshNormals(_meshFilter.sharedMesh, transform, _rayLength), _color);
	}
}