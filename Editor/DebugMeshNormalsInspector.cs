using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging.Editor
{
	[CustomEditor(typeof(DebugMeshNormals))]
	internal sealed class DebugMeshNormalsInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			var meshFilter = (MeshFilter)serializedObject.FindProperty("_meshFilter").objectReferenceValue;
			if (meshFilter == null)
				EditorGUILayout.HelpBox("MeshFilter is not assigned.", MessageType.Warning);
			else if (meshFilter.sharedMesh == null)
				EditorGUILayout.HelpBox("MeshFilter has no assigned mesh.", MessageType.Warning);
		}
	}
}