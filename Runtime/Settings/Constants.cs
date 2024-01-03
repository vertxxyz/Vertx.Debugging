using UnityEngine;
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

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
		
		public const int AllocatedLines = 50000;
		public const int AllocatedDashedLines = 10000;
		public const int AllocatedArcs = 50000;
		public const int AllocatedBoxes = 30000;
		public const int AllocatedOutlines = 20000;
		public const int AllocatedCasts = 20000;
	}
}