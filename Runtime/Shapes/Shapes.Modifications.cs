using Unity.Mathematics;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		public static float2 xy(this Vector3 value) => new float2(value.x, value.y);
		public static float3 xy0(this float2 value) => new float3(value.x, value.y, 0);
		public static float3 xy0(this Vector2 value) => new float3(value.x, value.y, 0);
		public static float3 xyz(this Vector3Int value) => new float3(value.x, value.y, value.z);
		public static float2 xy(this Vector2Int value) => new float2(value.x, value.y);

		public static float4 GetRow(this float4x4 matrix, int row) => new float4(matrix.c0[row], matrix.c1[row], matrix.c2[row], matrix.c3[row]);
		public static void SetRow(this float4x4 matrix, int row, float4 value)
		{
			matrix.c0[row] = value[0];
            matrix.c1[row] = value[1];
            matrix.c2[row] = value[2];
            matrix.c3[row] = value[3];
        }

		public static float3 MultiplyPoint3x4(this float4x4 matrix, float3 point)
		{
			float3 res;
			float4 c0 = matrix.c0;
			float4 c1 = matrix.c1;
			float4 c2 = matrix.c2;
			float4 c3 = matrix.c3;
			res.x = c0[0] * point.x + c1[0] * point.y + c2[0] * point.z + c3[0];
			res.y = c0[1] * point.x + c1[1] * point.y + c2[1] * point.z + c3[1];
			res.z = c0[2] * point.x + c1[2] * point.y + c2[2] * point.z + c3[2];
			return res;
		}
        
		public static float3 MultiplyVector(this float4x4 matrix, float3 vector)
		{
			float3 res;
			float4 c0 = matrix.c0;
			float4 c1 = matrix.c1;
			float4 c2 = matrix.c2;
			float4 c3 = matrix.c3;
			res.x = c0[0] * vector.x + c1[0] * vector.y + c2[0] * vector.z;
			res.y = c0[1] * vector.x + c1[1] * vector.y + c2[1] * vector.z;
			res.z = c0[2] * vector.x + c1[2] * vector.y + c2[2] * vector.z;
			return res;
		}
		
		private static void Translate(this ref float4x4 matrix, float3 translation)
		{
			float4 c3 = matrix.c3;
			c3.x += translation.x;
			c3.y += translation.y;
			c3.z += translation.z;
			matrix.c3 = c3;
		}

		private static void TranslateAndSetZ(this ref float4x4 matrix, float2 translation, float z)
		{
			float4 c3 = matrix.c3;
			c3.x += translation.x;
			c3.y += translation.y;
			c3.z = z;
			matrix.c3 = c3;
		}

		private static void SetZ(this ref float4x4 matrix, float z)
		{
			float4 c3 = matrix.c3;
			c3.z = z;
			matrix.c3 = c3;
		}

		private static Line GetWithZ(this in Line line, float z)
			=> new Line(new float3(line.A.x, line.A.y, z), new float3(line.B.x, line.B.y, z));
		
		private static Box2D GetTranslated(this in Box2D box2D, float3 translation)
		{
			float4x4 matrix = box2D.Matrix;
			matrix.Translate(translation);
			return new Box2D(matrix);
		}

		private static Box2D GetTranslatedWithZ(this in Box2D box2D, float2 translation, float z)
		{
			float4x4 matrix = box2D.Matrix;
			matrix.TranslateAndSetZ(translation, z);
			return new Box2D(matrix);
		}

		private static Box2D GetWithZ(this in Box2D box2D, float z)
		{
			float4x4 matrix = box2D.Matrix;
			matrix.SetZ(z);
			return new Box2D(matrix);
		}

		private static Arc GetWithZ(this in Arc arc, float z)
		{
			float4x4 matrix = arc.Matrix;
			matrix.SetZ(z);
			return new Arc(matrix);
		}

		private static Capsule2D GetTranslated(this in Capsule2D capsule2D, float2 translation)
			=> new Capsule2D(
				new float3(capsule2D._pointA.x + translation.x, capsule2D._pointA.y + translation.y, capsule2D._pointA.z),
				new float3(capsule2D._pointB.x + translation.x, capsule2D._pointB.y + translation.y, capsule2D._pointB.z),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);

		private static Capsule2D GetTranslatedWithZ(this in Capsule2D capsule2D, float2 translation, float zOverride)
			=> new Capsule2D(
				new float3(capsule2D._pointA.x + translation.x, capsule2D._pointA.y + translation.y, zOverride),
				new float3(capsule2D._pointB.x + translation.x, capsule2D._pointB.y + translation.y, zOverride),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);

		private static Capsule2D GetWithZ(this in Capsule2D capsule2D, float zOverride)
			=> new Capsule2D(
				new float3(capsule2D._pointA.x, capsule2D._pointA.y, zOverride),
				new float3(capsule2D._pointB.x, capsule2D._pointB.y, zOverride),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);
	}
}