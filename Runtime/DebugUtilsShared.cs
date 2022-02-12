using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		public static Color StartColor => new Color(1f, 0.4f, 0.3f);
		public static Color EndColor => new Color(0.4f, 1f, 0.3f);

		public static Color HitColor => new Color(1, 0.1f, 0.2f);
		public static Color RayColor => new Color(0.4f, 1f, 0.3f);

		public static Color ColorX => new Color(1, 0.1f, 0.2f);
		public static Color ColorY => new Color(0.3f, 1, 0.1f);
		public static Color ColorZ => new Color(0.1f, 0.4f, 1);

		#region Gizmos

		internal delegate void ColouredLineDelegate(Vector3 a, Vector3 b, Color c, float duration = 0);

		public static GizmosScope DrawGizmosScope() => new GizmosScope();

		public readonly struct GizmosScope : IDisposable
		{
			private readonly Color gizmosColor;
			private readonly ColouredLineDelegate colouredLineDelegate;

			public GizmosScope(bool useGizmos = true)
			{
				colouredLineDelegate = lineDelegate;
				gizmosColor = Gizmos.color;
				if (useGizmos)
					lineDelegate = GizmosLine;
				else
					lineDelegate = DebugLine;
			}

			public void Dispose()
			{
				Gizmos.color = gizmosColor;
				lineDelegate = colouredLineDelegate;
			}
		}

		public static GizmosScopeWithMatrix DrawGizmosScope(Matrix4x4 matrix) => new GizmosScopeWithMatrix(matrix);

		public readonly struct GizmosScopeWithMatrix : IDisposable
		{
			private readonly Color gizmosColor;
			private readonly Matrix4x4 gizmosMatrix;
			private readonly ColouredLineDelegate colouredLineDelegate;

			public GizmosScopeWithMatrix(Matrix4x4 matrix, bool useGizmos = true)
			{
				colouredLineDelegate = lineDelegate;
				gizmosColor = Gizmos.color;
				gizmosMatrix = Gizmos.matrix;
				Gizmos.matrix = matrix;
				if (useGizmos)
					lineDelegate = GizmosLine;
				else
					lineDelegate = DebugLine;
			}

			public void Dispose()
			{
				Gizmos.color = gizmosColor;
				Gizmos.matrix = gizmosMatrix;
				lineDelegate = colouredLineDelegate;
			}
		}

		private static ColouredLineDelegate lineDelegate = DebugLine;
		private static readonly ColouredLineDelegate rayDelegate = (a, b, c, d) => lineDelegate(a, a + b, c, d);

		private static void DebugLine(Vector3 a, Vector3 b, Color c, float duration = 0) => Debug.DrawLine(a, b, c, duration);

		private static void GizmosLine(Vector3 a, Vector3 b, Color c, float duration = 0)
		{
			Gizmos.color = c;
			Gizmos.DrawLine(a, b);
		}

		#endregion

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

		[Conditional("UNITY_EDITOR")]
		public static void DrawCircle
		(
			Vector3 center,
			Vector3 normal,
			float radius,
			Color color,
			float duration = 0,
			int segmentCount = 100
		)
		{
			Vector3 cross = GetAxisAlignedPerpendicular(normal);
			Vector3 direction = cross * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float)segmentCount * 360, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				lineDelegate(lastPos, nextPos, color, duration);
				currentRotation = rotation * currentRotation;
				lastPos = nextPos;
			}
		}

		internal enum AlphaMode
		{
			Solid,
			AlphaEdges
		}

		internal static void DrawCircleFast
		(
			Vector3 center,
			Vector3 normal,
			Vector3 cross,
			float radius,
			Color color,
			float duration,
			AlphaMode alphaMode = AlphaMode.Solid,
			int segmentCount = 100
		)
		{
			Vector3 direction = cross * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float)segmentCount * 360, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				switch (alphaMode)
				{
					case AlphaMode.AlphaEdges:
						float f = (i - 1) / (float)segmentCount;
						lineDelegate(lastPos, nextPos, new Color(color.r, color.g, color.b, Mathf.Pow(1 - Mathf.Abs(f - 0.5f) * 2, 2) * color.a), duration);
						break;
					default:
					case AlphaMode.Solid:
						lineDelegate(lastPos, nextPos, color, duration);
						break;
				}

				currentRotation = rotation * currentRotation;
				lastPos = nextPos;
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void DrawArc
		(
			Vector3 center,
			Vector3 normal,
			Vector3 startDirection,
			float radius,
			float totalAngle,
			Color color,
			float duration = 0,
			int segmentCount = 50
		)
		{
			Vector3 direction = startDirection * radius;
			Vector3 lastPos = center + direction;
			Quaternion rotation = Quaternion.AngleAxis(1 / (float)segmentCount * totalAngle, normal);
			Quaternion currentRotation = rotation;
			for (int i = 1; i <= segmentCount; i++)
			{
				Vector3 nextPos = center + currentRotation * direction;
				lineDelegate(lastPos, nextPos, color, duration);
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

		internal static void DrawBox(Vector3 center, DrawBoxStructure structure, Color color, float duration)
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
			lineDelegate(posUFL, posUFR, color, duration);
			lineDelegate(posUFR, posUBR, color, duration);
			lineDelegate(posUBR, posUBL, color, duration);
			lineDelegate(posUBL, posUFL, color, duration);
			//down
			lineDelegate(posDFL, posDFR, color, duration);
			lineDelegate(posDFR, posDBR, color, duration);
			lineDelegate(posDBR, posDBL, color, duration);
			lineDelegate(posDBL, posDFL, color, duration);
			//down to up
			lineDelegate(posDFL, posUFL, color, duration);
			lineDelegate(posDFR, posUFR, color, duration);
			lineDelegate(posDBR, posUBR, color, duration);
			lineDelegate(posDBL, posUBL, color, duration);
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

		internal static void DrawBox2DFast
		(
			Vector2 offset,
			DrawBoxStructure2D boxStructure2D,
			Color color,
			float duration
		)
		{
			Vector2 uRPosition = offset + boxStructure2D.UR;
			Vector2 uLPosition = offset + boxStructure2D.UL;
			Vector2 bRPosition = offset + boxStructure2D.BR;
			Vector2 bLPosition = offset + boxStructure2D.BL;
			lineDelegate(uLPosition, uRPosition, color, duration);
			lineDelegate(uRPosition, bRPosition, color, duration);
			lineDelegate(bRPosition, bLPosition, color, duration);
			lineDelegate(bLPosition, uLPosition, color, duration);
		}

		internal readonly struct DrawCapsuleStructure2D
		{
			public readonly float Radius;
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

				Radius = size.x * 0.5f;
				float vertical = Mathf.Max(0, size.y - size.x) * 0.5f;
				GetRotationCoefficients(angle, out var s, out var c);
				VerticalOffset = RotateFast(new Vector2(0, vertical), s, c);

				Left = new Vector2(c, s);
				ScaledLeft = Left * Radius;
				ScaledRight = -ScaledLeft;
			}
		}

		internal static void DrawArc2D(Vector2 center, Vector2 startDirection, float radius, float totalAngle, Color color, float duration, int segmentCount = 50)
		{
			Vector2 direction = startDirection * radius;
			Vector2 lastPos = center + direction;
			GetRotationCoefficients(1 / (float)segmentCount * totalAngle, out var s, out var c);

			Vector2 currentDirection = direction;
			for (int i = 1; i <= segmentCount; i++)
			{
				currentDirection = RotateFast(currentDirection, s, c);
				Vector2 nextPos = center + currentDirection;
				lineDelegate(lastPos, nextPos, color, duration); // (i - 1) / (float)segmentCount
				lastPos = nextPos;
			}
		}

		internal static void DrawCapsule2DFast(Vector2 offset, DrawCapsuleStructure2D capsuleStructure2D, Color color, float duration)
		{
			Vector2 r1 = offset + capsuleStructure2D.VerticalOffset;
			Vector2 r2 = offset - capsuleStructure2D.VerticalOffset;
			DrawArc2D(r1, capsuleStructure2D.Left, capsuleStructure2D.Radius, 180, color, duration);
			DrawArc2D(r2, capsuleStructure2D.Left, capsuleStructure2D.Radius, -180, color, duration);
			lineDelegate(r1 + capsuleStructure2D.ScaledLeft, r2 + capsuleStructure2D.ScaledLeft, color, duration);
			lineDelegate(r1 + capsuleStructure2D.ScaledRight, r2 + capsuleStructure2D.ScaledRight, color, duration);
		}

		#endregion

		#endregion

		#region Capsule

		internal static void DrawCapsuleFast
		(
			Vector3 point1,
			Vector3 point2,
			float radius,
			Vector3 axis,
			Vector3 crossA,
			Vector3 crossB,
			Color color,
			float duration
		)
		{
			//Circles
			DrawCircleFast(point1, axis, crossB, radius, color, duration);
			DrawCircleFast(point2, axis, crossB, radius, color, duration);

			//Caps
			DrawArc(point1, crossB, crossA, radius, 180, color, duration, 25);
			DrawArc(point1, crossA, crossB, radius, -180, color, duration, 25);

			DrawArc(point2, crossB, crossA, radius, -180, color, duration, 25);
			DrawArc(point2, crossA, crossB, radius, 180, color, duration, 25);

			//Joining Lines
			Vector3 a = crossA * radius;
			Vector3 b = crossB * radius;
			lineDelegate.Invoke(point1 + a, point2 + a, color, duration);
			lineDelegate.Invoke(point1 - a, point2 - a, color, duration);
			lineDelegate.Invoke(point1 + b, point2 + b, color, duration);
			lineDelegate.Invoke(point1 - b, point2 - b, color, duration);
		}

		#endregion

		#endregion
	}
}