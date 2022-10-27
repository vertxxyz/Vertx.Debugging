#if UNITY_EDITOR
using UnityEngine;
using System;
#if !UNITY_2020_1_OR_NEWER
using Object = UnityEngine.Object;
#else
using UnityEditor;
#endif

namespace Vertx.Debugging
{
#if UNITY_2020_1_OR_NEWER
	[FilePath(Path, FilePathAttribute.Location.ProjectFolder)]
#endif
	internal class DebuggingSettings : ScriptableSingleton<DebuggingSettings>
	{
		public const string Path = "ProjectSettings/VertxDebuggingSettings.asset";

		[Serializable]
		public class ColorGroup
		{
			[Header("Casts")]
			public Color HitColor = Constants.HitColor;
			public Color CastColor = Constants.CastColor;

			[Header("Physics Events")]
			public Color EnterColor = Constants.EnterColor;
			public Color StayColor = Constants.StayColor;
			public Color ExitColor = Constants.ExitColor;

			[Header("Axes")]
			public Color XColor = Constants.XColor;
			public Color YColor = Constants.YColor;
			public Color ZColor = Constants.ZColor;
		}

		public ColorGroup Colors;

		/// <summary>
		/// This must match <see cref="CommandBuilder.RenderingType"/> Scene and Game
		/// </summary>
		[Flags]
		public enum Location : byte
		{
			None = 0,
			SceneView = 1,
			GameView = 1 << 1,
			All = SceneView | GameView
		}

		[Tooltip("Whether lines write to the depth buffer for a certain window. Depth writing will produce correct depth sorting.\n" +
		         "Under specific versions of some render pipelines you may find depth writing causes artifacts in the game view against other gizmos.")]
		public Location DepthWrite = Location.None;
		[Tooltip("Whether lines are depth tested for a certain window. Depth testing will produce faded lines behind solid objects.\n" +
		         "Under specific versions of some render pipelines you may find depth testing is resolved upside-down in the game view.\nSome do not depth test properly at all.")]
		public Location DepthTest = Location.All;

		public void Save() => Save(true);
	}
	
#if !UNITY_2020_1_OR_NEWER
	// 2019 doesn't have the FilePath attribute, so we just override *everything* manually
	public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
	{
		private static T _instance;
		
		public static T instance
		{
			get
			{
				if (_instance == null)
					CreateAndLoad();
				return _instance;
			}
		}

		private static void CreateAndLoad()
		{
			System.Diagnostics.Debug.Assert(_instance == null);

			// Load
			if (!string.IsNullOrEmpty(DebuggingSettings.Path))
			{
				// If a file exists
				UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(DebuggingSettings.Path);
			}

			if (_instance == null)
			{
				// Create
				T t = CreateInstance<T>();
				t.hideFlags = HideFlags.HideAndDontSave;
				_instance = t;
			}

			System.Diagnostics.Debug.Assert(_instance != null);
		}
		

		protected void Save(bool saveAsText)
		{
			string folderPath = System.IO.Path.GetDirectoryName(DebuggingSettings.Path);
			if (!System.IO.Directory.Exists(folderPath))
				System.IO.Directory.CreateDirectory(folderPath);

			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _instance }, DebuggingSettings.Path, saveAsText);
		}
	}
#endif
}
#endif