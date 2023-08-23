#if UNITY_EDITOR
using JetBrains.Annotations;
using Unity.Mathematics;

namespace Vertx.Debugging
{
	internal readonly struct LineGroup
	{
		[UsedImplicitly] public readonly Shape.Line Line;
		[UsedImplicitly] public readonly float4 Color;

		public LineGroup(in Shape.Line line, float4 color)
		{
			Line = line;
			Color = color;
		}
	}

	internal readonly struct DashedLineGroup
	{
		[UsedImplicitly] public readonly Shape.DashedLine Line;
		[UsedImplicitly] public readonly float4 Color;

		public DashedLineGroup(in Shape.DashedLine line, float4 color)
		{
			Line = line;
			Color = color;
		}
	}

	internal readonly struct ArcGroup
	{
		[UsedImplicitly] public readonly Shape.Arc Arc;
		[UsedImplicitly] public readonly float4 Color;
		[UsedImplicitly] public readonly Shape.DrawModifications Modifications;

		public ArcGroup(in Shape.Arc arc, float4 color, Shape.DrawModifications modifications)
		{
			Arc = arc;
			Color = color;
			Modifications = modifications;
		}
	}

	internal readonly struct BoxGroup
	{
		[UsedImplicitly] public readonly float4x4 Box;
		[UsedImplicitly] public readonly float4 Color;
		[UsedImplicitly] public readonly Shape.DrawModifications Modifications;

		public BoxGroup(in Shape.Box box, float4 color, Shape.DrawModifications modifications)
		{
			Box = box.Matrix;
			Color = color;
			Modifications = modifications;
		}
	}

	internal readonly struct OutlineGroup
	{
		[UsedImplicitly] public readonly Shape.Outline Outline;
		[UsedImplicitly] public readonly float4 Color;
		[UsedImplicitly] public readonly Shape.DrawModifications Modifications;

		public OutlineGroup(in Shape.Outline outline, float4 color, Shape.DrawModifications modifications)
		{
			Outline = outline;
			Color = color;
			Modifications = modifications;
		}
	}

	internal readonly struct CastGroup
	{
		[UsedImplicitly] public readonly Shape.Cast Cast;
		[UsedImplicitly] public readonly float4 Color;

		public CastGroup(in Shape.Cast cast, float4 color)
		{
			Cast = cast;
			Color = color;
		}
	}
}
#endif