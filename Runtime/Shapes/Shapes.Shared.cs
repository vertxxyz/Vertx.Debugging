using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static partial class Shape
	{
		static Shape()
		{
#if UNITY_EDITOR
			// Called after a delay to avoid errors when called during serialization:
			// > UnityEngine.UnityException: LoadSerializedFileAndForget is not allowed to be called from a MonoBehaviour constructor (or instance field initializer),
			// > call it in Awake or Start instead. Called from MonoBehaviour 'DebugCollisionEvents' on game object 'Prefab'.
			// > See "Script Serialization" page in the Unity Manual for further details.
			// The first time this is used, colors may resolve as constants, but the typical case will mean these values are overridden by that point.
			EditorApplication.delayCall += SyncColors;
#endif
		}

#if UNITY_EDITOR
		// Axis
		public static Color XColor { get; private set; } = Constants.XColor;
		public static Color YColor { get; private set; } = Constants.YColor;
		public static Color ZColor { get; private set; } = Constants.ZColor;

		// Casts
		public static Color HitColor { get; private set; } = Constants.HitColor;
		public static Color CastColor { get; private set; } = Constants.CastColor;

		// Physics Events
		public static Color EnterColor { get; private set; } = Constants.EnterColor;
		public static Color StayColor { get; private set; } = Constants.StayColor;
		public static Color ExitColor { get; private set; } = Constants.ExitColor;

		internal static void SyncColors()
		{
			var settings = DebuggingPreferences.instance;
			DebuggingPreferences.ColorGroup colors = settings.Colors;
			if (colors == null)
			{
				colors = new DebuggingPreferences.ColorGroup();
				settings.Colors = colors;
			}

			XColor = colors.XColor;
			YColor = colors.YColor;
			ZColor = colors.ZColor;
			HitColor = colors.HitColor;
			CastColor = colors.CastColor;
			EnterColor = colors.EnterColor;
			StayColor = colors.StayColor;
			ExitColor = colors.ExitColor;
		}
#else
		// Axis
		public static readonly Color XColor = Constants.XColor;
		public static readonly Color YColor = Constants.YColor;
		public static readonly Color ZColor = Constants.ZColor;
		// Casts
		public static readonly Color HitColor = Constants.HitColor;
		public static readonly Color CastColor = Constants.CastColor;
		// Physics Events
		public static readonly Color EnterColor = Constants.EnterColor;
		public static readonly Color StayColor = Constants.StayColor;
		public static readonly Color ExitColor = Constants.ExitColor;
#endif

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

		private static float3 GetValidPerpendicular(float3 input)
		{
			float3 alt = math.right();
			if (math.abs(math.dot(input, alt)) > 0.95)
				alt = math.up();
			return math.normalizesafe(math.cross(input, alt));
		}

		private static float3 GetValidAxisAligned(float3 normal)
		{
			float3 alternate = new float3(0, 0, 1);
			if (math.abs(math.dot(normal, alternate)) > 0.707f)
				alternate = new float3(0, 1, 0);
			return alternate;
		}

		private static void EnsureNormalized(this ref float3 vector3, out float length)
		{
			float sqrMag = math.lengthsq(vector3);
			if (Mathf.Approximately(sqrMag, 1))
			{
				length = 1;
				return;
			}

			length = math.sqrt(sqrMag);
			vector3 /= length;
		}

		private static void EnsureNormalized(this ref float2 vector2)
		{
			float sqrMag = math.lengthsq(vector2);
			if (Mathf.Approximately(sqrMag, 1))
				return;
			if (Mathf.Approximately(sqrMag, 0))
				return;

			vector2 /= math.sqrt(sqrMag);
		}

		private static void EnsureNormalized(this ref float2 vector2, out float length)
		{
			float sqrMag = math.lengthsq(vector2);
			if (Mathf.Approximately(sqrMag, 1))
			{
				length = 1;
				return;
			}
			if (Mathf.Approximately(sqrMag, 0))
			{
				length = 0;
				return;
			}

			length = math.sqrt(sqrMag);
			vector2 /= length;
		}

		private static void EnsureNormalized(this ref float3 vector3)
		{
			float sqrMag = math.lengthsq(vector3);
			if (Mathf.Approximately(sqrMag, 1))
				return;
			if (Mathf.Approximately(sqrMag, 0))
				return;
			vector3 /= math.sqrt(sqrMag);
		}

		private static float GetClampedMaxDistance(float distance)
		{
			if (float.IsInfinity(distance))
				return MaxDrawDistance;
			return math.min(distance, MaxDrawDistance);
		}

		private static void GetRotationCoefficients(Angle angle, out float s, out float c)
		{
			float a = angle.Radians;
			s = math.sin(a);
			c = math.cos(a);
		}

		private static float2 RotateUsingCoefficients(float2 vector, float s, float c)
		{
			float u = vector.x * c - vector.y * s;
			float v = vector.x * s + vector.y * c;
			return new float2(u, v);
		}

		private static float2 Rotate(float2 vector, Angle angle)
		{
			GetRotationCoefficients(angle, out float s, out float c);
			return RotateUsingCoefficients(vector, s, c);
		}

		private static float3 RotateUsingCoefficients(float3 vector, float s, float c)
		{
			float u = vector.x * c - vector.y * s;
			float v = vector.x * s + vector.y * c;
			return new float3(u, v, vector.z);
		}

		private static float3 Rotate(float3 vector, Angle angle)
		{
			GetRotationCoefficients(angle, out float s, out float c);
			return RotateUsingCoefficients(vector, s, c);
		}

		private static float2 GetDirectionFromAngle(Angle angle) => angle == 0 ? new float2(1, 0) : Rotate(new float2(1, 0), angle);

		private static float ToAngleDegrees(this float2 v) => math.atan2(v.y, v.x) * math.TODEGREES;

		private static float2 PerpendicularClockwise(float2 vector2) => new float2(vector2.y, -vector2.x);

		private static float2 PerpendicularCounterClockwise(float2 vector2) => new float2(-vector2.y, vector2.x);

		private static float3 PerpendicularClockwise(float3 vector3) => new float3(vector3.y, -vector3.x, vector3.z);

		private static float3 PerpendicularCounterClockwise(float3 vector3) => new float3(-vector3.y, vector3.x, vector3.z);

		private static float3 Add(this float3 a, float2 b) => new float3(a.x + b.x, a.y + b.y, a.z);

		private static bool HasZeroDistanceHit(RaycastHit[] results, int resultCount)
		{
			for (int i = 0; i < resultCount; i++)
			{
				if (results[i].distance == 0)
					return true;
			}

			return false;
		}

		// ReSharper disable CompareOfFloatsByEqualityOperator
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsWhite(Color c) => c.r == 1 && c.g == 1 && c.b == 1 && c.a == 1;
		// ReSharper restore CompareOfFloatsByEqualityOperator

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
				new Point(new float3(-1, -1, -1), Direction.Left | Direction.Bottom | Direction.Back),
				new Point(new float3(1, -1, -1), Direction.Right | Direction.Bottom | Direction.Back),
				new Point(new float3(-1, 1, -1), Direction.Left | Direction.Top | Direction.Back),
				new Point(new float3(1, 1, -1), Direction.Right | Direction.Top | Direction.Back),
				new Point(new float3(-1, -1, 1), Direction.Left | Direction.Bottom | Direction.Forward),
				new Point(new float3(1, -1, 1), Direction.Right | Direction.Bottom | Direction.Forward),
				new Point(new float3(-1, 1, 1), Direction.Left | Direction.Top | Direction.Forward),
				new Point(new float3(1, 1, 1), Direction.Right | Direction.Top | Direction.Forward)
			};

			public readonly struct Point
			{
				public readonly float3 Coordinate;
				public readonly Direction Direction;

				public Point(float3 coordinate, Direction direction)
				{
					Coordinate = coordinate;
					Direction = direction;
				}
			}

			public static readonly Edge[] Edges =
			{
				new Edge(new float3(-1, -1, -1), new float3(1, -1, -1), Direction.Bottom | Direction.Back),
				new Edge(new float3(-1, -1, -1), new float3(-1, 1, -1), Direction.Left | Direction.Back),
				new Edge(new float3(-1, -1, -1), new float3(-1, -1, 1), Direction.Left | Direction.Bottom),
				//
				new Edge(new float3(-1, -1, 1), new float3(-1, 1, 1), Direction.Left | Direction.Forward),
				new Edge(new float3(-1, -1, 1), new float3(1, -1, 1), Direction.Bottom | Direction.Forward),
				//
				new Edge(new float3(-1, 1, -1), new float3(1, 1, -1), Direction.Top | Direction.Back),
				new Edge(new float3(-1, 1, -1), new float3(-1, 1, 1), Direction.Left | Direction.Top),
				//
				new Edge(new float3(1, -1, -1), new float3(1, 1, -1), Direction.Right | Direction.Back),
				new Edge(new float3(1, -1, -1), new float3(1, -1, 1), Direction.Right | Direction.Bottom),
				//
				new Edge(new float3(1, 1, 1), new float3(1, 1, -1), Direction.Right | Direction.Top),
				new Edge(new float3(1, 1, 1), new float3(1, -1, 1), Direction.Right | Direction.Forward),
				new Edge(new float3(1, 1, 1), new float3(-1, 1, 1), Direction.Top | Direction.Forward)
			};

			public readonly struct Edge
			{
				public readonly float3 A, B;
				public readonly Direction Direction;

				public Edge(float3 a, float3 b, Direction direction)
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