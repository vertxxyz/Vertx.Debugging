using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		private static CircleCache CircleCache => s_circleCache ?? (s_circleCache = new CircleCache());
		private static CircleCache s_circleCache;

		public static Color ColorX => new Color(1, 0.1f, 0.2f);
		public static Color ColorY => new Color(0.3f, 1, 0.1f);
		public static Color ColorZ => new Color(0.1f, 0.4f, 1);

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

		private static void EnsureNormalized(this ref Vector3 vector3)
		{
			float sqrMag = vector3.sqrMagnitude;
			if (Mathf.Approximately(sqrMag, 1))
				return;
			vector3 /= Mathf.Sqrt(sqrMag);
		}

		public struct Line : IDrawable
		{
			public Vector3 A, B;

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

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendLine(this, color, duration);
#endif
		}

		public struct LineStrip : IDrawable
		{
			public IEnumerable<Vector3> Points;

			public LineStrip(IEnumerable<Vector3> points) => Points = points;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3? previous = null;
				foreach (Vector3 point in Points)
				{
					if (previous.HasValue)
						commandBuilder.AppendLine(new Line(previous.Value, point), color, duration);
					previous = point;
				}
			}
#endif
		}

		public struct Ray : IDrawable
		{
			public Vector3 Origin, Direction;

			public Ray(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendRay(this, color, duration);
#endif
		}

		public struct Point : IDrawable
		{
			public Vector3 Position;
			public float Scale;

			public Point(Vector3 position, float scale = 0.3f)
			{
				Position = position;
				Scale = scale;
			}

			public Point(Vector2 position, float scale = 0.3f) : this((Vector3)position, scale) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float distance = Scale * 0.5f;
				commandBuilder.AppendLine(new Line(new Vector3(Position.x - distance, Position.y, Position.z), new Vector3(Position.x + distance, Position.y, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new Vector3(Position.x, Position.y - distance, Position.z), new Vector3(Position.x, Position.y + distance, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new Vector3(Position.x, Position.y, Position.z - distance), new Vector3(Position.x, Position.y, Position.z + distance)), color, duration);
			}
#endif
		}

		public struct Arrow : IDrawable
		{
			public Vector3 Origin, Direction;

			public Arrow(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Arrow(Vector3 origin, Quaternion rotation, float length = 1) : this(origin, rotation * Vector3.forward * length) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				DrawArrowHead(commandBuilder, Origin, Direction, color, duration);
			}

			public static void DrawArrowHead(CommandBuilder commandBuilder, Vector3 point, Vector3 dir, Color color, float duration = 0)
			{
				const float headLength = 0.075f;
				const float headWidth = 0.05f;
				const int segments = 3;

				Vector3 arrowPoint = point + dir;
				dir.EnsureNormalized();

				void DoDrawArrowHead(Vector3 center, Vector3 normal, float radius)
				{
					Vector2[] circle = CircleCache.GetCircle(segments);
					Vector3 tangent = GetValidPerpendicular(normal);
					Vector3 bitangent = Vector3.Cross(normal, tangent);
					tangent *= radius;
					bitangent *= radius;
					Vector3 lastPos = center + tangent;
					for (int i = 1; i <= segments; i++)
					{
						Vector2 c = circle[i];
						Vector3 nextPos = center + tangent * c.x + bitangent * c.y;
						commandBuilder.AppendLine(new Line(lastPos, nextPos), color, duration);
						commandBuilder.AppendLine(new Line(lastPos, arrowPoint), color, duration);
						lastPos = nextPos;
					}
				}

				DoDrawArrowHead(arrowPoint - dir * headLength, dir, headWidth);
			}
#endif
		}

		public struct ArrowStrip : IDrawable
		{
			public IEnumerable<Vector3> Points;

			public ArrowStrip(IEnumerable<Vector3> points) => Points = points;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3? previous = null;
				Vector3? origin = null;
				foreach (Vector3 point in Points)
				{
					if (previous.HasValue)
						commandBuilder.AppendLine(new Line(previous.Value, point), color, duration);
					origin = previous;
					previous = point;
				}

				if (!origin.HasValue)
					return;
				Vector3 direction = previous.Value - origin.Value;
				Arrow.DrawArrowHead(commandBuilder, origin.Value, direction, color, duration);
			}
#endif
		}

		public struct Axis : IDrawable
		{
			public Vector3 Origin;
			public Quaternion Rotation;
			public bool ShowArrowHeads;
			public Axes VisibleAxes;
			public float Scale;

			public Axis(Vector3 origin, Quaternion rotation, bool showArrowHeads = true, Axes visibleAxes = Axes.All, float scale = 1)
			{
				Origin = origin;
				Rotation = rotation;
				ShowArrowHeads = showArrowHeads;
				VisibleAxes = visibleAxes;
				Scale = scale;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (ShowArrowHeads)
				{
					if ((VisibleAxes & Axes.X) != 0)
						new Arrow(Origin, Rotation * new Vector3(Scale, 0, 0)).Draw(commandBuilder, ColorX, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Arrow(Origin, Rotation * new Vector3(0, Scale, 0)).Draw(commandBuilder, ColorY, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Arrow(Origin, Rotation * new Vector3(0, 0, Scale)).Draw(commandBuilder, ColorZ, duration);
				}
				else
				{
					if ((VisibleAxes & Axes.X) != 0)
						new Ray(Origin, Rotation * new Vector3(Scale, 0, 0)).Draw(commandBuilder, ColorX, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Ray(Origin, Rotation * new Vector3(0, Scale, 0)).Draw(commandBuilder, ColorY, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Ray(Origin, Rotation * new Vector3(0, 0, Scale)).Draw(commandBuilder, ColorZ, duration);
				}
			}
#endif
		}

		public struct SurfacePoint : IDrawable
		{
			public Vector3 Origin, Direction;
			public float Radius;

			public SurfacePoint(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				EnsureNormalized(ref direction, out float length);
				Direction = direction;
				Radius = length * 0.05f;
			}

			public SurfacePoint(Vector3 origin, Vector3 direction, float radius)
			{
				Origin = origin;
				EnsureNormalized(ref direction, out _);
				Direction = direction;
				Radius = radius;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				commandBuilder.AppendArc(new Arc(Origin, Direction, Radius), color, duration);
			}
#endif
		}

		/// <summary>
		/// This is 2D, the rotation or matrix used will align the arc facing right, aligned with XY.<br/>
		/// Use the helper constructors to create an Arc aligned how you require.
		/// </summary>
		public struct Arc : IDrawable
		{
			public Matrix4x4 Matrix;
			public float Turns;

			private static readonly Quaternion s_Base3DRotation = Quaternion.Euler(90, -90, 0);

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

			public Arc(Vector3 origin, Vector3 normal, Vector3 direction, float radius, Angle angle)
				: this(
					origin,
					Quaternion.LookRotation(direction, normal) * s_Base3DRotation,
					radius,
					angle
				) { }

			public Arc(Vector3 origin, Vector3 normal, Vector3 direction, float radius) : this(origin, normal, direction, radius, Angle.FromTurns(1)) { }

			/// <summary>
			/// It's cheaper to use the <see cref="Arc(Vector3, Vector3, Vector3, float)"/> constructor if you already have a perpendicular facing direction for the circle.
			/// </summary>
			public Arc(Vector3 origin, Vector3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius, Angle.FromTurns(1)) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendArc(this, color, duration);
#endif
		}

		public struct Sphere : IDrawable
		{
			public Matrix4x4 Matrix;

			public Sphere(Vector3 origin) => Matrix = Matrix4x4.Translate(origin);

			public Sphere(Vector3 origin, float radius)
				=> Matrix = Matrix4x4.Translate(origin) * Matrix4x4.Scale(new Vector3(radius, radius, radius));

			public Sphere(Vector3 origin, Quaternion rotation, float radius)
				=> Matrix = Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius));

			public Sphere(Transform transform, float radius) : this(transform.position, transform.rotation, radius) { }

			public Sphere(Transform transform) => Matrix = transform.localToWorldMatrix;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				var coreArc = new Arc(Matrix);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				// Vector3 forward = Matrix.MultiplyPoint3x4(Vector3.forward);
				// Vector3 right = Matrix.MultiplyPoint3x4(Vector3.right);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.up))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.right))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
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

		public struct Box : IDrawable
		{
			public Matrix4x4 Matrix;

			public Box(Vector3 position, Quaternion rotation, Vector3 scale) => Matrix = Matrix4x4.TRS(position, rotation, scale);

			public Box(Transform transform) => Matrix = transform.localToWorldMatrix;

			public Box(Bounds bounds) : this(bounds.center, Quaternion.identity, bounds.extents) { }

			public Box(Vector3 position, Vector3 halfExtents, Quaternion orientation) : this(position, orientation, halfExtents) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendBox(this, color, duration, DrawModifications.NormalFade);
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
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 up = (SpherePosition2 - SpherePosition1).normalized;
				Vector3 down = -up;

				Vector3 perpendicular = GetValidPerpendicular(up);
				Vector3 perpendicular2 = Vector3.Cross(up, perpendicular);

				Angle halfAngle = Angle.FromTurns(0.5f);
				commandBuilder.AppendArc(new Arc(SpherePosition2, perpendicular, up, Radius, halfAngle), color, duration);
				commandBuilder.AppendArc(new Arc(SpherePosition2, perpendicular2, up, Radius, halfAngle), color, duration);

				commandBuilder.AppendArc(new Arc(SpherePosition1, perpendicular, down, Radius, halfAngle), color, duration);
				commandBuilder.AppendArc(new Arc(SpherePosition1, perpendicular2, down, Radius, halfAngle), color, duration);

				commandBuilder.AppendArc(new Arc(SpherePosition1, up, perpendicular, Radius), color, duration);
				commandBuilder.AppendArc(new Arc(SpherePosition2, down, perpendicular, Radius), color, duration);

				commandBuilder.AppendArc(new Arc(SpherePosition2, perpendicular, up, Radius, halfAngle), color, duration, DrawModifications.Custom);
				commandBuilder.AppendArc(new Arc(SpherePosition1, perpendicular, down, Radius, halfAngle), color, duration, DrawModifications.Custom);

				perpendicular *= Radius;
				perpendicular2 *= Radius;

				commandBuilder.AppendLine(new Line(SpherePosition1 + perpendicular, SpherePosition2 + perpendicular), color, duration);
				commandBuilder.AppendLine(new Line(SpherePosition1 - perpendicular, SpherePosition2 - perpendicular), color, duration);
				commandBuilder.AppendLine(new Line(SpherePosition1 + perpendicular2, SpherePosition2 + perpendicular2), color, duration);
				commandBuilder.AppendLine(new Line(SpherePosition1 - perpendicular2, SpherePosition2 - perpendicular2), color, duration);
			}
#endif
		}
	}
}