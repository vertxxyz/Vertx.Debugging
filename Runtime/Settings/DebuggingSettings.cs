#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging
{
	[FilePath(Path, FilePathAttribute.Location.ProjectFolder)]
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
		
		public enum Location : byte
		{
			None = 0,
			SceneView = 1,
			GameView = 1 << 1,
			All = SceneView | GameView
		}

		[Tooltip("Whether lines write to the depth buffer for a certain window. Depth writing will produce correct depth sorting.\n" +
		         "Under specific versions of some render pipelines you may find depth writing causes artifacts against other gizmos.")]
		public Location DepthWrite = Location.None;
		[Tooltip("Whether lines are depth tested for a certain window. Depth testing will produce faded lines behind solid objects.\n" +
		         "Under specific versions of some render pipelines you may find depth testing is resolved upside-down in the game view.")]
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