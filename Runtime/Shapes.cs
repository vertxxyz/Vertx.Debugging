using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
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

			public static implicit operator Ray(UnityEngine.Ray ray) => new Ray(ray.origin, ray.direction);

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

		public struct Circle : IDrawable
		{
			private Arc _arc;

			public Matrix4x4 Matrix
			{
				get => _arc.Matrix;
				set => _arc = new Arc(value);
			}
			
			public Circle(Matrix4x4 matrix) => _arc = new Arc(matrix);

			public Circle(Vector3 origin, Quaternion rotation, float radius)
				=> _arc = new Arc(Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius)));
			
			public Circle(Vector3 origin, Vector3 normal, Vector3 direction, float radius)
				: this(
					origin,
					Quaternion.LookRotation(direction, normal) * Arc.s_Base3DRotation,
					radius
				) { }
			
			/// <summary>
			/// It's cheaper to use the <see cref="Circle(Vector3, Vector3, Vector3, float)"/> constructor if you already have a perpendicular facing direction for the circle.
			/// </summary>
			public Circle(Vector3 origin, Vector3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius) { }
			
#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => _arc.Draw(commandBuilder, color, duration);
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

			internal static readonly Quaternion s_Base3DRotation = Quaternion.Euler(90, -90, 0);

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

			public Sphere(Matrix4x4 matrix) => Matrix = matrix;
			
			public Sphere(Vector3 origin) => Matrix = Matrix4x4.Translate(origin);

			public Sphere(Vector3 origin, float radius)
				=> Matrix = Matrix4x4.Translate(origin) * Matrix4x4.Scale(new Vector3(radius, radius, radius));

			public Sphere(Vector3 origin, Quaternion rotation, float radius)
				=> Matrix = Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius));

			public Sphere(Transform transform, float radius) : this(transform.position, transform.rotation, radius) { }

			public Sphere(Transform transform) => Matrix = transform.localToWorldMatrix;
			
			public Sphere GetTranslated(Vector3 translation) => new Sphere(Matrix4x4.Translate(translation) * Matrix);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				var coreArc = new Arc(Matrix);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.up))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.right))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
			}

			public void Draw(CommandBuilder commandBuilder, Color color, float duration, Axes visibleAxes)
			{
				var coreArc = new Arc(Matrix);
				if ((visibleAxes & Axes.Y) != 0)
					commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				if ((visibleAxes & Axes.X) != 0)
					commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.up))), color, duration, DrawModifications.NormalFade);
				if ((visibleAxes & Axes.Z) != 0)
					commandBuilder.AppendArc(new Arc(Matrix * Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.right))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
			}
#endif
		}

		public struct Box : IDrawable
		{
			public Matrix4x4 Matrix;
			
			private Box(Matrix4x4 matrix) => Matrix = matrix;

			public Box(Vector3 position, Vector3 halfExtents, Quaternion orientation) : this(Matrix4x4.TRS(position, orientation, halfExtents)) { }

			public Box(Transform transform) => Matrix = transform.localToWorldMatrix;

			public Box(Bounds bounds) : this(bounds.center, bounds.extents, Quaternion.identity) { }
			
			public Box GetTranslated(Vector3 translation) => new Box(Matrix4x4.Translate(translation) * Matrix);

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

			public Capsule GetTranslated(Vector3 translation) => new Capsule(SpherePosition1 + translation, SpherePosition2 + translation, Radius);

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