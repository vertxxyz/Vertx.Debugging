using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		private static Matrix4x4 Translate(this Matrix4x4 matrix4X4, Vector3 translation)
			=> new Matrix4x4
			{
				m00 = matrix4X4.m00,
				m01 = matrix4X4.m01,
				m02 = matrix4X4.m02,
				m03 = matrix4X4.m03 + translation.x,
				m10 = matrix4X4.m10,
				m11 = matrix4X4.m11,
				m12 = matrix4X4.m12,
				m13 = matrix4X4.m13 + translation.y,
				m20 = matrix4X4.m20,
				m21 = matrix4X4.m21,
				m22 = matrix4X4.m22,
				m23 = matrix4X4.m23 + translation.z,
				m30 = matrix4X4.m30,
				m31 = matrix4X4.m31,
				m32 = matrix4X4.m32,
				m33 = matrix4X4.m33
			};

		private static Matrix4x4 TranslateAndSetZ(this Matrix4x4 matrix4X4, Vector2 translation, float z)
			=> new Matrix4x4
			{
				m00 = matrix4X4.m00,
				m01 = matrix4X4.m01,
				m02 = matrix4X4.m02,
				m03 = matrix4X4.m03 + translation.x,
				m10 = matrix4X4.m10,
				m11 = matrix4X4.m11,
				m12 = matrix4X4.m12,
				m13 = matrix4X4.m13 + translation.y,
				m20 = matrix4X4.m20,
				m21 = matrix4X4.m21,
				m22 = matrix4X4.m22,
				m23 = z,
				m30 = matrix4X4.m30,
				m31 = matrix4X4.m31,
				m32 = matrix4X4.m32,
				m33 = matrix4X4.m33
			};

		private static Matrix4x4 SetZ(this Matrix4x4 matrix4X4, float z)
			=> new Matrix4x4
			{
				m00 = matrix4X4.m00,
				m01 = matrix4X4.m01,
				m02 = matrix4X4.m02,
				m03 = matrix4X4.m03,
				m10 = matrix4X4.m10,
				m11 = matrix4X4.m11,
				m12 = matrix4X4.m12,
				m13 = matrix4X4.m13,
				m20 = matrix4X4.m20,
				m21 = matrix4X4.m21,
				m22 = matrix4X4.m22,
				m23 = z,
				m30 = matrix4X4.m30,
				m31 = matrix4X4.m31,
				m32 = matrix4X4.m32,
				m33 = matrix4X4.m33
			};

		private static Line GetWithZ(this in Line line, float z)
			=> new Line(new Vector3(line.A.x, line.A.y, z), new Vector3(line.B.x, line.B.y, z));
		
		private static Box2D GetTranslated(this in Box2D box2D, Vector3 translation)
			=> new Box2D(box2D.Matrix.Translate(translation));

		private static Box2D GetTranslatedWithZ(this in Box2D box2D, Vector2 translation, float z)
			=> new Box2D(box2D.Matrix.TranslateAndSetZ(translation, z));

		private static Box2D GetWithZ(this in Box2D box2D, float z)
			=> new Box2D(box2D.Matrix.SetZ(z));
		
		private static Arc GetWithZ(this in Arc arc, float z)
			=> new Arc(arc.Matrix.SetZ(z));

		private static Capsule2D GetTranslated(this in Capsule2D capsule2D, Vector2 translation)
			=> new Capsule2D(
				new Vector3(capsule2D._pointA.x + translation.x, capsule2D._pointA.y + translation.y, capsule2D._pointA.z),
				new Vector3(capsule2D._pointB.x + translation.x, capsule2D._pointB.y + translation.y, capsule2D._pointB.z),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);

		private static Capsule2D GetTranslatedWithZ(this in Capsule2D capsule2D, Vector2 translation, float zOverride)
			=> new Capsule2D(
				new Vector3(capsule2D._pointA.x + translation.x, capsule2D._pointA.y + translation.y, zOverride),
				new Vector3(capsule2D._pointB.x + translation.x, capsule2D._pointB.y + translation.y, zOverride),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);

		private static Capsule2D GetWithZ(this in Capsule2D capsule2D, float zOverride)
			=> new Capsule2D(
				new Vector3(capsule2D._pointA.x, capsule2D._pointA.y, zOverride),
				new Vector3(capsule2D._pointB.x, capsule2D._pointB.y, zOverride),
				capsule2D._radius,
				capsule2D._verticalDirection,
				capsule2D._scaledLeft
			);
	}
}