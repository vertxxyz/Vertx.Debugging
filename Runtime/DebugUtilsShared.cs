using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		public delegate void LineDelegateSimple(Vector3 a, Vector3 b);

		public delegate void LineDelegate(Vector3 a, Vector3 b, float t);

		public static Color StartColor => new Color(1f, 0.4f, 0.3f);
		public static Color EndColor => new Color(0.4f, 1f, 0.3f);

		private static void EnsureNormalized(this ref Vector3 vector3)
		{
			float sqrMag = vector3.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector3 /= Mathf.Sqrt(sqrMag);
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
			if (Mathf.Abs(Vector3.Dot(normal, alternate)) > 0.9f)
				alternate = new Vector3(0, 1, 0);
			return alternate;
		}

		public static Vector3 GetAxisAlignedPerpendicular(Vector3 normal)
		{
			Vector3 cross = Vector3.Cross(normal, GetAxisAlignedAlternate(normal));
			cross.EnsureNormalized();
			return cross;
		}

		#region Shapes

		#region Circles And Arcs

		public static void DrawCircle(Vector3 center, Vector3 normal, float radius, LineDelegate lineDelegate, int segmentCount = 100)
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

		public static void DrawCircleFast(Vector3 center, Vector3 normal, Vector3 cross, float radius, LineDelegate lineDelegate, int segmentCount = 100)
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

		public static void DrawArc(Vector3 center, Vector3 normal, Vector3 startDirection, float radius, float totalAngle, LineDelegate lineDelegate, int segmentCount = 50)
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

		public struct DrawBoxStructure
		{
			public Vector3 UFL, UFR, UBL, UBR, DFL, DFR, DBL, DBR;

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

		public static void DrawBox(
			Vector3 center,
			Vector3 halfExtents,
			Quaternion orientation,
			LineDelegateSimple lineDelegate)
		{
			DrawBoxStructure box = new DrawBoxStructure(halfExtents, orientation);
			DrawBox(center, box, lineDelegate);
		}

		public static void DrawBox(Vector3 center, DrawBoxStructure structure, LineDelegateSimple lineDelegate)
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

		#region Capsule

		public static void DrawCapsuleFast(Vector3 point1, Vector3 point2, float radius, Vector3 axis, Vector3 crossA, Vector3 crossB, LineDelegate lineDelegate)
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