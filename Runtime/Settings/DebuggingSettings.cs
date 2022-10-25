#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

#if !UNITY_2020_1_OR_NEWER
		// 2019 doesn't have the FilePath attribute, so we just override *everything* manually
		private static DebuggingSettings _instance;

		public new static DebuggingSettings instance
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
			if (!string.IsNullOrEmpty(Path))
			{
				// If a file exists
				ForceInternalInstanceToNull();
				UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(Path);
			}

			if (_instance == null)
			{
				// Create
				ForceInternalInstanceToNull();
				DebuggingSettings t = CreateInstance<DebuggingSettings>();
				t.hideFlags = HideFlags.HideAndDontSave;
				_instance = t;
			}

			System.Diagnostics.Debug.Assert(_instance != null);
		}
		
		private static void ForceInternalInstanceToNull() => typeof(ScriptableSingleton<DebuggingSettings>).GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

		protected override void Save(bool saveAsText)
		{
			string folderPath = System.IO.Path.GetDirectoryName(Path);
			if (!System.IO.Directory.Exists(folderPath))
				System.IO.Directory.CreateDirectory(folderPath);

			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _instance }, Path, saveAsText);
		}
#endif

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
}
#endif

namespace Vertx.Debugging
{
	internal static class Constants
	{
		public static readonly Color HitColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color CastColor = new Color(0.4f, 1f, 0.3f);

		public static readonly Color EnterColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color StayColor = new Color(1f, 0.4f, 0.3f);
		public static readonly Color ExitColor = new Color(0.4f, 1f, 0.3f);

		public static readonly Color XColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color YColor = new Color(0.3f, 1, 0.1f);
		public static readonly Color ZColor = new Color(0.1f, 0.4f, 1);
	}
}