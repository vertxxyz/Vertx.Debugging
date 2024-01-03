#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

namespace Vertx.Debugging
{
	[FilePath(Path, FilePathAttribute.Location.PreferencesFolder)]
	internal class DebuggingPreferences : ScriptableSingleton<DebuggingPreferences>
	{
		public const string Path = "VertxDebuggingPreferences.asset";

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

		public void Save() => Save(true);
	}
}
#endif