#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging
{
	internal static class AssetsUtility
	{
		public sealed class Asset<T> where T : Object
		{
			private readonly string _path;
			private bool _initialised;
			private T _value;

			public T Value
			{
				get
				{
					if (_initialised)
						return _value;
					_initialised = true;
					_value = AssetDatabase.LoadAssetAtPath<T>(_path);
					return _value;
				}
			}

			public static implicit operator T(Asset<T> asset) => asset.Value;

			public Asset(string name, string extension = "asset") => _path = $"Packages/com.vertx.debugging/Editor/Assets/{name}.{extension}";
		}
		
		public sealed class MaterialAsset
		{
			private readonly string _path;
			private readonly bool _enableInstancing;
			private bool _initialised;
			private Material _value;

			public Material Value
			{
				get
				{
					if (_initialised)
						return _value;
					_initialised = true;
					_value = new Material(AssetDatabase.LoadAssetAtPath<Shader>(_path))
					{
						enableInstancing = _enableInstancing,
						hideFlags = HideFlags.HideAndDontSave
					};
					AssemblyReloadEvents.beforeAssemblyReload += Dispose;
					return _value;
				}
			}

			private void Dispose()
			{
				AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
				Object.DestroyImmediate(_value, true);
			}

			public static implicit operator Material(MaterialAsset asset) => asset.Value;

			public MaterialAsset(string name, bool enableInstancing = true)
			{
				_enableInstancing = enableInstancing;
				_path = $"Packages/com.vertx.debugging/Editor/Assets/{name}.shader";
			}
		}

		public static readonly Asset<Mesh> Line = new Asset<Mesh>("VertxLine");
		public static readonly Asset<Mesh> Circle = new Asset<Mesh>("VertxCircle");
		public static readonly Asset<Mesh> Box = new Asset<Mesh>("VertxBox");
		public static readonly Asset<Mesh> Box2D = new Asset<Mesh>("VertxBox2D");
		public static readonly MaterialAsset DefaultMaterial = new MaterialAsset("VertxMesh");
		public static readonly MaterialAsset LineMaterial = new MaterialAsset("VertxLine");
		public static readonly MaterialAsset ArcMaterial = new MaterialAsset("VertxArc");
		public static readonly MaterialAsset BoxMaterial = new MaterialAsset("VertxBox");
		public static readonly MaterialAsset OutlineMaterial = new MaterialAsset("VertxOutline");
		public static readonly MaterialAsset CastMaterial = new MaterialAsset("VertxCast");
		public static readonly Asset<Font> JetBrainsMono = new Asset<Font>("JetbrainsMono-Regular", "ttf");
	}
}
#endif