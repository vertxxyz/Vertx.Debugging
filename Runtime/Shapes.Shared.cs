using System;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		private static CircleCache CircleCache => s_circleCache ?? (s_circleCache = new CircleCache());
		private static CircleCache s_circleCache;
		public static Color ColorX => new Color(1, 0.1f, 0.2f);
		public static Color ColorY => new Color(0.3f, 1, 0.1f);
		public static Color ColorZ => new Color(0.1f, 0.4f, 1);
		public static readonly Color HitColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color CastColor = new Color(0.4f, 1f, 0.3f);

		[Flags]
		public enum Axes
		{
			None = 0,
			X = 1,
			Y = 1 << 1,
			Z = 1 << 2,
			TwoDimensional = X | Y,
			All = X | Y | Z
		}

		private const int MaxDrawDistance = 1_000_000;

		private static Vector3 GetValidPerpendicular(Vector3 input)
		{
			Vector3 alt = Vector3.right;
			if (Mathf.Abs(Vector3.Dot(input, alt)) > 0.95)
				alt = Vector3.up;
			return Vector3.Cross(input, alt).normalized;
		}

		private static Vector3 GetValidAxisAligned(Vector3 normal)
		{
			Vector3 alternate = new Vector3(0, 0, 1);
			if (Mathf.Abs(Vector3.Dot(normal, alternate)) > 0.707f)
				alternate = new Vector3(0, 1, 0);
			return alternate;
		}

		private static void EnsureNormalized(this ref Vector3 vector3, out float length)
		{
			float sqrMag = vector3.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
			{
				length = 1;
				return;
			}

			length = Mathf.Sqrt(sqrMag);
			vector3 /= length;
		}

		private static void EnsureNormalized(this ref Vector2 vector2)
		{
			float sqrMag = vector2.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector2 /= Mathf.Sqrt(sqrMag);
		}

		private static void EnsureNormalized(this ref Vector3 vector3)
		{
			float sqrMag = vector3.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector3 /= Mathf.Sqrt(sqrMag);
		}

		private static float GetClampedMaxDistance(float distance)
		{
			if (float.IsInfinity(distance))
				return MaxDrawDistance;
			return Mathf.Min(distance, MaxDrawDistance);
		}

		private static void GetRotationCoefficients(float angle, out float s, out float c)
		{
			float a = angle * Mathf.Deg2Rad;
			s = Mathf.Sin(a);
			c = Mathf.Cos(a);
		}

		private static Vector2 RotateUsingCoefficients(Vector2 vector, float s, float c)
		{
			float u = vector.x * c - vector.y * s;
			float v = vector.x * s + vector.y * c;
			return new Vector2(u, v);
		}

		private static Vector2 Rotate(Vector2 vector, float angle)
		{
			GetRotationCoefficients(angle, out float s, out float c);
			return RotateUsingCoefficients(vector, s, c);
		}
		
		private static Vector3 RotateUsingCoefficients(Vector3 vector, float s, float c)
		{
			float u = vector.x * c - vector.y * s;
			float v = vector.x * s + vector.y * c;
			return new Vector3(u, v, vector.z);
		}

		private static Vector3 Rotate(Vector3 vector, float angle)
		{
			GetRotationCoefficients(angle, out float s, out float c);
			return RotateUsingCoefficients(vector, s, c);
		}

		private static Vector2 GetDirectionFromAngle(float angle)
		{
			if (angle == 0)
				return Vector2.right;
			return Rotate(Vector2.right, angle);
		}

		private static Vector2 PerpendicularClockwise(Vector2 vector2) => new Vector2(vector2.y, -vector2.x);

		private static Vector2 PerpendicularCounterClockwise(Vector2 vector2) => new Vector2(-vector2.y, vector2.x);
	}
}