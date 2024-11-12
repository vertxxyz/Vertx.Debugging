using System;
using Unity.Mathematics;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		public readonly struct Angle : IEquatable<Angle>
		{
			/// <summary>
			/// Angle in turns.
			/// </summary>
			public float Turns { get; }

			public float Radians => Turns * math.PI * 2;

			public float Degrees => Turns * 360;
			
			private Angle(float turns) => Turns = turns;

			public static Angle FromRadians(float value) => new Angle(value / (math.PI * 2));
			
			public static Angle FromDegrees(float value) => FromRadians(value * math.TORADIANS);
			
			public static Angle FromTurns(float value) => new Angle(value);

			public static Angle FromArcLength(float length, float radius)
			{
				if (radius == 0 || length == 0)
					return new Angle();
				return FromRadians(length / radius);
			}

			internal Angle Abs() => FromTurns(math.abs(Turns));
			
			public static implicit operator float(Angle value) => value.Turns;

			public bool Equals(Angle other) => Turns.Equals(other.Turns);

			public override bool Equals(object obj) => obj is Angle other && Equals(other);

			public override int GetHashCode() => Turns.GetHashCode();
		}

		/// <summary>
		/// Modifications made to the shape when drawing.<br/>
		/// Each modification may not supported by all shapes.
		/// </summary>
		[Flags]
		public enum DrawModifications
		{
			None = 0,
			/// <summary>
			/// Unimplemented!<br/>
			/// Fades the shape out to the ends of the defined range.<br/>
			/// Internally, this will use uv.v as the fade coordinate.
			/// </summary>
			AlphaFade = 1,
			/// <summary>
            /// Fades the shape out when the normals are not pointing towards the camera.
            /// </summary>
			NormalFade = 1 << 1,
			/// <summary>
			/// Unimplemented!<br/>
			/// Turns the shape so it faces the camera.
			/// </summary>
			FaceCamera = 1 << 2,
			/// <summary>
			/// A custom modification that can be implemented by a shape.<br/>
			/// - Arc: Will align the circle as if it's the outline of a sphere.
			/// - Outline: Swaps sides appropriately for a capsulecast.
			/// </summary>
			Custom = 1 << 3,
			/// <summary>
			/// A custom modification that can be implemented by a shape.<br/>
			/// - Arc: Will create a full circle, but it spirals inwards using the turns parameter as an inner radius multiplier.
			/// - Outline: Corrects sides appropriately for a boxcast.
			/// </summary>
			Custom2 = 1 << 4,
			/// <summary>
			/// A custom modification that can be implemented by a shape.<br/>
			/// - Outline: Standard perpendicular outline, but with two radii, one in C.x and another in C.y.
			/// </summary>
			Custom3 = 1 << 5
		}
	}
}