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

			public Asset(string name, string extension = "asset") => _path = $"Packages/com.vertx.debugging/Runtime/Assets/{name}.{extension}";
		}

		public static readonly Asset<Mesh> Line = new Asset<Mesh>("Line");
		public static readonly Asset<Mesh> Circle = new Asset<Mesh>("Circle");
		public static readonly Asset<Mesh> Box = new Asset<Mesh>("Box");
		public static readonly Asset<Mesh> Box2D = new Asset<Mesh>("Box2D");
		public static readonly Asset<Material> DefaultMaterial = new Asset<Material>("Default", "mat");
		public static readonly Asset<Material> LineMaterial = new Asset<Material>("Line", "mat");
		public static readonly Asset<Material> ArcMaterial = new Asset<Material>("Arc", "mat");
		public static readonly Asset<Material> BoxMaterial = new Asset<Material>("Box", "mat");
		public static readonly Asset<Material> OutlineMaterial = new Asset<Material>("Outline", "mat");
	}
}