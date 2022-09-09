using System;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public readonly struct Angle
		{
			/// <summary>
			/// Angle in turns
			/// </summary>
			public float Turns { get; }

			public float Radians => Turns * Mathf.PI * 2;

			public float Degrees => Turns * 360;
			
			private Angle(float turns) => Turns = turns;

			public static Angle FromRadians(float value) => new Angle(value / (Mathf.PI * 2));
			
			public static Angle FromDegrees(float value) => FromRadians(value * Mathf.Deg2Rad);
			
			public static Angle FromTurns(float value) => new Angle(value);
			
			public static implicit operator float(Angle value) => value.Turns;
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
			/// Fades the shape out to the ends of the defined range.<br/>
			/// Internally, this will use uv.v as the fade coordinate.
			/// </summary>
			AlphaFade = 1,
			/// <summary>
            /// Fades the shape out when the normals are not pointing towards the camera.
            /// </summary>
			NormalFade = 1 << 1,
			/// <summary>
			/// Turns the shape so it faces the camera.
			/// </summary>
			FaceCamera = 1 << 2,
			/// <summary>
			/// A custom modification that can be implemented by a shape.<br/>
			/// - Arc: Will align the circle as if it's the outline of a sphere.
			/// </summary>
			Custom = 1 << 3
		}
	}
}