using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		public delegate void LineDelegateSimple(Vector3 a, Vector3 b);

		public delegate void LineDelegate(Vector3 a, Vector3 b, float t);

		public static Color StartColor => new Color(1f, 0.4f, 0.3f);
		public static Color EndColor => new Color(0.4f, 1f, 0.3f);

		public static Color HitColor => new Color(1, 0.1f, 0.2f);
		public static Color RayColor => new Color(0.4f, 1f, 0.3f);
		
		public static Color ColorX => new Color(1, 0.1f, 0.2f);
		public static Color ColorY => new Color(0.3f, 1, 0.1f);
		public static Color ColorZ => new Color(0.1f, 0.4f, 1);

		private static void EnsureNormalized(this ref Vector3 vector3)
		{
			float sqrMag = vector3.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector3 /= Mathf.Sqrt(sqrMag);
		}

		private static void EnsureNormalized(this ref Vector2 vector2)
		{
			float sqrMag = vector2.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector2 /= Mathf.Sqrt(sqrMag);
		}

		private static Vector3 GetAxisAlignedAlternateWhereRequired(Vector3 normal, Vector3 alternate)
		{
			if (Mathf.Abs(Vector3.Dot(normal, alternate)) > 0.999f)
				alternate = GetAxisAlignedAlternate(normal);
			return alternate;
		}

		private static Vector3 GetAxisAlignedAlternate(Vector3 normal)
		{
			Vector3 alternate = new Vector3(0, 0, 1);
			if (Mathf.Abs(Vector3.Dot(normal, alternate)) > 0.707f)
				alternate = new Vector3(0, 1, 0);
			return alternate;
		}

		public static Vector3 GetAxisAlignedPerpendicular(Vector3 normal)
		{
			Vector3 cross = Vector3.Cross(normal, GetAxisAlignedAlternate(normal));
			cross.EnsureNormalized();
			return cross;
		}
		
		public static Vector2 PerpendicularClockwise(Vector2 vector2) => new Vector2(vector2.y, -vector2.x);

		public static Vector2 PerpendicularCounterClockwise(Vector2 vector2) => new Vector2(-vector2.y, vector2.x);

		#region Shapes

		#region Circles And Arcs

		internal static void DrawCircle(Vector3 center, Vector3 normal, float radius, LineDelegate lineDelegate, int segmentCount = 100)
		{
			Vector3 cross = GetAxisAlignedPerpendicular(normal);
			Vector3 direction = cross * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float) segmentCount * 360, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				lineDelegate(lastPos, nextPos, (i - 1) / (float) segmentCount);
				currentRotation = rotation * currentRotation;
				lastPos = nextPos;
			}
		}

		internal static void DrawCircleFast(Vector3 center, Vector3 normal, Vector3 cross, float radius, LineDelegate lineDelegate, int segmentCount = 100)
		{
			Vector3 direction = cross * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float) segmentCount * 360, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				lineDelegate(lastPos, nextPos, (i - 1) / (float) segmentCount);
				currentRotation = rotation * currentRotation;
				lastPos = nextPos;
			}
		}

		internal static void DrawArc(Vector3 center, Vector3 normal, Vector3 startDirection, float radius, float totalAngle, LineDelegate lineDelegate, int segmentCount = 50)
		{
			Vector3 direction = startDirection * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float) segmentCount * totalAngle, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				lineDelegate(lastPos, nextPos, (i - 1) / (float) segmentCount);
				currentRotation = rotation * currentRotation;
				lastPos = nextPos;
			}
		}

		#endregion

		#region Boxes

		#region 3D

		internal readonly struct DrawBoxStructure
		{
			public readonly Vector3 UFL, UFR, UBL, UBR, DFL, DFR, DBL, DBR;

			public DrawBoxStructure(
				Vector3 halfExtents,
				Quaternion orientation)
			{
				Vector3
					up = orientation * new Vector3(0, halfExtents.y, 0),
					right = orientation * new Vector3(halfExtents.x, 0, 0),
					forward = orientation * new Vector3(0, 0, halfExtents.z);
				UFL = up + forward - right;
				UFR = up + forward + right;
				UBL = up - forward - right;
				UBR = up - forward + right;
				DFL = -up + forward - right;
				DFR = -up + forward + right;
				DBL = -up - forward - right;
				DBR = -up - forward + right;
			}
		}

		internal static void DrawBox(
			Vector3 center,
			Vector3 halfExtents,
			Quaternion orientation,
			LineDelegateSimple lineDelegate)
		{
			DrawBoxStructure box = new DrawBoxStructure(halfExtents, orientation);
			DrawBox(center, box, lineDelegate);
		}

		internal static void DrawBox(Vector3 center, DrawBoxStructure structure, LineDelegateSimple lineDelegate)
		{
			Vector3
				posUFL = structure.UFL + center,
				posUFR = structure.UFR + center,
				posUBL = structure.UBL + center,
				posUBR = structure.UBR + center,
				posDFL = structure.DFL + center,
				posDFR = structure.DFR + center,
				posDBL = structure.DBL + center,
				posDBR = structure.DBR + center;

			//up
			lineDelegate(posUFL, posUFR);
			lineDelegate(posUFR, posUBR);
			lineDelegate(posUBR, posUBL);
			lineDelegate(posUBL, posUFL);
			//down
			lineDelegate(posDFL, posDFR);
			lineDelegate(posDFR, posDBR);
			lineDelegate(posDBR, posDBL);
			lineDelegate(posDBL, posDFL);
			//down to up
			lineDelegate(posDFL, posUFL);
			lineDelegate(posDFR, posUFR);
			lineDelegate(posDBR, posUBR);
			lineDelegate(posDBL, posUBL);
		}

		#endregion

		#region 2D

		internal readonly struct DrawBoxStructure2D
		{
			public readonly Vector2 UR, UL, BR, BL;
			public readonly Vector2 UROrigin, ULOrigin;

			public DrawBoxStructure2D(
				Vector2 size,
				float angle,
				Vector2 offset = default)
			{
				size *= 0.5f;
				GetRotationCoefficients(angle, out var s, out var c);

				UROrigin = RotateFast(size, s, c);
				ULOrigin = RotateFast(new Vector2(-size.x, size.y), s, c);
				UR = offset + UROrigin;
				UL = offset + ULOrigin;
				BR = offset - ULOrigin;
				BL = offset - UROrigin;
			}
		}

		private static void GetRotationCoefficients(float angle, out float s, out float c)
		{
			float a = angle * Mathf.Deg2Rad;
			s = Mathf.Sin(a);
			c = Mathf.Cos(a);
		}

		private static Vector2 RotateFast(Vector2 vector, float s, float c)
		{
			float u = vector.x * c - vector.y * s;
			float v = vector.x * s + vector.y * c;
			return new Vector2(u, v);
		}

		internal static void DrawBox2DFast(
			Vector2 offset,
			DrawBoxStructure2D boxStructure2D,
			LineDelegateSimple lineDelegate)
		{
			Vector2 uRPosition = offset + boxStructure2D.UR;
			Vector2 uLPosition = offset + boxStructure2D.UL;
			Vector2 bRPosition = offset + boxStructure2D.BR;
			Vector2 bLPosition = offset + boxStructure2D.BL;
			lineDelegate(uLPosition, uRPosition);
			lineDelegate(uRPosition, bRPosition);
			lineDelegate(bRPosition, bLPosition);
			lineDelegate(bLPosition, uLPosition);
		}
		
		internal readonly struct DrawCapsuleStructure2D
		{
			public readonly float Radius;
			public readonly Vector3 Normal;
			public readonly Vector2 VerticalOffset;
			public readonly Vector2 Left, ScaledLeft, ScaledRight;

			public DrawCapsuleStructure2D(
				Vector2 size,
				CapsuleDirection2D capsuleDirection, 
				float angle)
			{
				if (capsuleDirection == CapsuleDirection2D.Horizontal)
				{
					float temp = size.y;
					size.y = size.x;
					size.x = temp;
					angle += 180;
				}
				
				Normal = Vector3.back;
				Radius = size.x * 0.5f;
				float vertical = Mathf.Max(0, size.y - size.x) * 0.5f;
				GetRotationCoefficients(angle, out var s, out var c);
				VerticalOffset = RotateFast(new Vector2(0, vertical), s, c);
			
				Left = new Vector2(c, s);
				ScaledLeft = Left * Radius;
				ScaledRight = -ScaledLeft;
			}
		}

		internal static void DrawCapsule2DFast(Vector2 offset, DrawCapsuleStructure2D capsuleStructure2D, LineDelegate lineDelegate)
		{
			Vector2 r1 = offset + capsuleStructure2D.VerticalOffset;
			Vector2 r2 = offset - capsuleStructure2D.VerticalOffset;
			DrawArc(r1, capsuleStructure2D.Normal, capsuleStructure2D.Left, capsuleStructure2D.Radius, -180, lineDelegate);
			DrawArc(r2, capsuleStructure2D.Normal, capsuleStructure2D.Left, capsuleStructure2D.Radius, 180, lineDelegate);
			lineDelegate(r1 + capsuleStructure2D.ScaledLeft, r2 + capsuleStructure2D.ScaledLeft, 0);
			lineDelegate(r1 + capsuleStructure2D.ScaledRight, r2 + capsuleStructure2D.ScaledRight, 0);
		}

		#endregion

		#endregion

		#region Capsule

		internal static void DrawCapsuleFast(Vector3 point1, Vector3 point2, float radius, Vector3 axis, Vector3 crossA, Vector3 crossB, LineDelegate lineDelegate)
		{
			//Circles
			DrawCircleFast(point1, axis, crossB, radius, lineDelegate);
			DrawCircleFast(point2, axis, crossB, radius, lineDelegate);

			//Caps
			DrawArc(point1, crossB, crossA, radius, 180, lineDelegate, 25);
			DrawArc(point1, crossA, crossB, radius, -180, lineDelegate, 25);

			DrawArc(point2, crossB, crossA, radius, -180, lineDelegate, 25);
			DrawArc(point2, crossA, crossB, radius, 180, lineDelegate, 25);

			//Joining Lines
			Vector3 a = crossA * radius;
			Vector3 b = crossB * radius;
			lineDelegate.Invoke(point1 + a, point2 + a, 0);
			lineDelegate.Invoke(point1 - a, point2 - a, 0);
			lineDelegate.Invoke(point1 + b, point2 + b, 0);
			lineDelegate.Invoke(point1 - b, point2 - b, 0);
		}

		#endregion

		#endregion
	}
}