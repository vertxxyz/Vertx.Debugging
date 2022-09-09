using Unity.Mathematics;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public readonly struct Line : IDrawable
		{
			public readonly float3 A;
			public readonly float3 B;

			public Line(Vector3 a, Vector3 b)
			{
				A = a;
				B = b;
			}

			public Line(Ray ray)
			{
				A = ray.Origin;
				B = ray.Origin + ray.Direction;
			}

			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendLine(this, color, duration);
		}

		public struct Ray : IDrawable
		{
			public float3 Origin, Direction;

			public Ray(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendRay(this, color, duration);
		}

		public struct Arc : IDrawable
		{
			public Matrix4x4 Matrix;
			public float Turns;

			public Angle Angle
			{
				set => Turns = value;
			}

			public Arc(Matrix4x4 matrix, Angle angle)
			{
				Matrix = matrix;
				Turns = angle;
			}
			
			public Arc(Matrix4x4 matrix) : this(matrix, Angle.FromTurns(1)) { }

			public Arc(Vector3 origin, Quaternion rotation, float radius, Angle angle)
			{
				Turns = angle;
				Matrix = Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius));
			}

			public Arc(Vector3 origin, Quaternion rotation, float radius) : this(origin, rotation, radius, Angle.FromTurns(1)) { }

			public Arc(Vector2 origin, float rotationDegrees, float radius, Angle angle) : this(origin, Quaternion.AngleAxis(rotationDegrees, Vector3.forward), radius, angle) { }

			public Arc(Vector2 origin, float rotationDegrees, float radius) : this(origin, rotationDegrees, radius, Angle.FromTurns(1)) { }

			public Arc(Vector3 origin, Vector3 normal, Vector3 centerDirection, float radius, Angle angle) : this(origin, Quaternion.LookRotation(normal, centerDirection), radius, angle) { }

			public Arc(Vector3 origin, Vector3 normal, Vector3 centerDirection, float radius) : this(origin, Quaternion.LookRotation(normal, centerDirection), radius, Angle.FromTurns(1)) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendArc(this, color, duration);
#endif
		}

		public struct Sphere : IDrawable
		{
			public Matrix4x4 Matrix;

			public Sphere(Vector3 origin)
			{
				Matrix = Matrix4x4.Translate(origin);
			}

			public Sphere(Vector3 origin, float radius)
			{
				Matrix = Matrix4x4.Translate(origin) * Matrix4x4.Scale(new Vector3(radius, radius, radius));
			}
			
			public Sphere(Vector3 origin, Quaternion rotation, float radius)
			{
				Matrix = Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius));
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				var coreArc = new Arc(Matrix);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				// Vector3 forward = Matrix.MultiplyPoint3x4(Vector3.forward);
				// Vector3 right = Matrix.MultiplyPoint3x4(Vector3.right);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.up))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.right))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.FaceCamera);
			}
#endif
		}

		public struct SphereCast : IDrawable
		{
			public Vector3 Origin, Direction;
			public float Radius, MaxDistance;

			public SphereCast(Vector3 origin, Vector3 direction, float radius, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
				Direction = direction;
				Radius = radius;
				MaxDistance = maxDistance;
			}

			public SphereCast(in UnityEngine.Ray ray)
			{
				Origin = ray.origin;
				Direction = ray.direction;
				Radius = 0;
				MaxDistance = 0;
			}

			public SphereCast(in Ray ray)
			{
				Origin = ray.Origin;
				Direction = ray.Direction;
				Radius = 0;
				MaxDistance = 0;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) { }
#endif
		}

		public struct Capsule : IDrawable
		{
			public Vector3 SpherePosition1, SpherePosition2;
			public float Radius;

			public Capsule(Vector3 center, Quaternion rotation, float height, float radius)
			{
				float pointHeight = height * 0.5f - radius;
				Vector3 direction = rotation * new Vector3(0, pointHeight, 0);
				SpherePosition1 = center + direction;
				SpherePosition2 = center - direction;
				Radius = radius;
			}

			public Capsule(Vector3 spherePosition1, Vector3 spherePosition2, float radius)
			{
				SpherePosition1 = spherePosition1;
				SpherePosition2 = spherePosition2;
				Radius = radius;
			}

			public Capsule(Vector3 lowestPosition, Vector3 direction, float height, float radius)
			{
				SpherePosition1 = lowestPosition + direction * radius;
				SpherePosition2 = SpherePosition1 + direction * (height - radius * 2);
				Radius = radius;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) { }
#endif
		}
	}
}