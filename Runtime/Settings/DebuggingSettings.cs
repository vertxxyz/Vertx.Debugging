#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

namespace Vertx.Debugging
{
	[FilePath(Path, FilePathAttribute.Location.ProjectFolder)]
	internal class DebuggingSettings : ScriptableSingleton<DebuggingSettings>
	{
		public const string Path = "ProjectSettings/VertxDebuggingSettings.asset";
		
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

		[Serializable]
		public class Allocations
		{
			[Min(0)]
			public int Lines = Constants.AllocatedLines;
			[Min(0)]
			public int DashedLines = Constants.AllocatedDashedLines;
			[Min(0)]
			public int Arcs = Constants.AllocatedArcs;
			[Min(0)]
			public int Boxes = Constants.AllocatedBoxes;
			[Min(0)]
			public int Outlines = Constants.AllocatedOutlines;
			[Min(0)]
			public int Casts = Constants.AllocatedCasts;
		}

		public Allocations 
			AllocationsForGizmos,
			AllocationsWithDurations;

		public void Save() => Save(true);
	}
}
#endif