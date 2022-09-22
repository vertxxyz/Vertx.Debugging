using System;
using UnityEngine;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		private static CircleCache CircleCache => s_circleCache ?? (s_circleCache = new CircleCache());
		private static CircleCache s_circleCache;
		
		// Axis
		public static readonly Color XColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color YColor = new Color(0.3f, 1, 0.1f);
		public static readonly Color ZColor = new Color(0.1f, 0.4f, 1);
		// Casts
		public static readonly Color HitColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color CastColor = new Color(0.4f, 1f, 0.3f);
		// Physics Events
		public static readonly Color EnterColor = new Color(1, 0.1f, 0.2f);
		public static readonly Color StayColor = new Color(1f, 0.4f, 0.3f);
		public static readonly Color ExitColor = new Color(0.4f, 1f, 0.3f);

		[Flags]
		public enum Axes : byte
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

		private static float ToAngleDegrees(this Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

		private static Vector2 PerpendicularClockwise(Vector2 vector2) => new Vector2(vector2.y, -vector2.x);

		private static Vector2 PerpendicularCounterClockwise(Vector2 vector2) => new Vector2(-vector2.y, vector2.x);

		private static Vector3 PerpendicularClockwise(Vector3 vector3) => new Vector3(vector3.y, -vector3.x, vector3.z);

		private static Vector3 PerpendicularCounterClockwise(Vector3 vector3) => new Vector3(-vector3.y, vector3.x, vector3.z);

		private static Vector3 Add(this Vector3 a, Vector2 b) => new Vector3(a.x + b.x, a.y + b.y, a.z);

		public static class BoxUtility
		{
			[Flags]
			public enum Direction : byte
			{
				None = 0,
				Top = 1,
				Right = 1 << 1,
				Forward = 1 << 2,
				Bottom = 1 << 3,
				Left = 1 << 4,
				Back = 1 << 5
			}

			public static readonly Point[] Points =
			{
				new Point(new Vector3(-1, -1, -1), Direction.Left | Direction.Bottom | Direction.Back),
				new Point(new Vector3(1, -1, -1), Direction.Right | Direction.Bottom | Direction.Back),
				new Point(new Vector3(-1, 1, -1), Direction.Left | Direction.Top | Direction.Back),
				new Point(new Vector3(1, 1, -1), Direction.Right | Direction.Top | Direction.Back),
				new Point(new Vector3(-1, -1, 1), Direction.Left | Direction.Bottom | Direction.Forward),
				new Point(new Vector3(1, -1, 1), Direction.Right | Direction.Bottom | Direction.Forward),
				new Point(new Vector3(-1, 1, 1), Direction.Left | Direction.Top | Direction.Forward),
				new Point(new Vector3(1, 1, 1), Direction.Right | Direction.Top | Direction.Forward)
			};

			public readonly struct Point
			{
				public readonly Vector3 Coordinate;
				public readonly Direction Direction;

				public Point(Vector3 coordinate, Direction direction)
				{
					Coordinate = coordinate;
					Direction = direction;
				}
			}

			public static readonly Edge[] Edges =
			{
				new Edge(new Vector3(-1, -1, -1), new Vector3(1, -1, -1), Direction.Bottom | Direction.Back),
				new Edge(new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), Direction.Left | Direction.Back),
				new Edge(new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), Direction.Left | Direction.Bottom),
				//
				new Edge(new Vector3(-1, -1, 1), new Vector3(-1, 1, 1), Direction.Left | Direction.Forward),
				new Edge(new Vector3(-1, -1, 1), new Vector3(1, -1, 1), Direction.Bottom | Direction.Forward),
				//
				new Edge(new Vector3(-1, 1, -1), new Vector3(1, 1, -1), Direction.Top | Direction.Back),
				new Edge(new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), Direction.Left | Direction.Top),
				//
				new Edge(new Vector3(1, -1, -1), new Vector3(1, 1, -1), Direction.Right | Direction.Back),
				new Edge(new Vector3(1, -1, -1), new Vector3(1, -1, 1), Direction.Right | Direction.Bottom),
				//
				new Edge(new Vector3(1, 1, 1), new Vector3(1, 1, -1), Direction.Right | Direction.Top),
				new Edge(new Vector3(1, 1, 1), new Vector3(1, -1, 1), Direction.Right | Direction.Forward),
				new Edge(new Vector3(1, 1, 1), new Vector3(-1, 1, 1), Direction.Top | Direction.Forward)
			};

			public readonly struct Edge
			{
				public readonly Vector3 A, B;
				public readonly Direction Direction;

				public Edge(Vector3 a, Vector3 b, Direction direction)
				{
					A = a;
					B = b;
					Direction = direction;
				}
			}

			public static Direction ConstructDirection(float dotRight, float dotUp, float dotForward)
			{
				const float epsilon = 0.00001f;

				Direction direction = Direction.None;
				if (dotRight < -epsilon)
					direction |= Direction.Left;
				else if (dotRight > epsilon)
					direction |= Direction.Right;

				if (dotUp < -epsilon)
					direction |= Direction.Bottom;
				else if (dotUp > epsilon)
					direction |= Direction.Top;

				if (dotForward < -epsilon)
					direction |= Direction.Back;
				else if (dotForward > epsilon)
					direction |= Direction.Forward;

				return direction;
			}

			public static int CountDirections(Direction direction)
			{
				int counter = 0;
				for (int i = 1; i <= (int)Direction.Back; i++)
				{
					if ((direction & (Direction)(1 << i)) != 0)
						counter++;
				}

				return counter;
			}
		}
	}
}