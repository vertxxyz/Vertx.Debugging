using System.Collections.Generic;
using UnityEngine;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public readonly struct Line : IDrawable
		{
			public readonly Vector3 A, B;

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

		public readonly struct LineStrip : IDrawable
		{
			public readonly IEnumerable<Vector3> Points;

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

		public readonly struct Ray : IDrawable
		{
			public readonly Vector3 Origin, Direction;

			public Ray(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Ray(Vector3 origin, Vector3 direction, float distance)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction * GetClampedMaxDistance(distance);
			}

			public static implicit operator Ray(UnityEngine.Ray ray) => new Ray(ray.origin, ray.direction);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendRay(this, color, duration);
#endif
		}

		public readonly struct Point : IDrawable
		{
			public readonly Vector3 Position;
			public readonly float Scale;

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

		public readonly struct Arrow : IDrawable
		{
			public readonly Vector3 Origin, Direction;

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
				float headLength = 0.075f;
				float headWidth = 0.05f;
				const int segments = 3;

				Vector3 arrowPoint = point + dir;
				dir.EnsureNormalized(out float length);

				if (headLength > length * 0.5f)
				{
					headLength *= length;
					headWidth *= length;
				}

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

		public readonly struct ArrowStrip : IDrawable
		{
			public readonly IEnumerable<Vector3> Points;

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

		public readonly struct Axis : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly Quaternion Rotation;
			public readonly bool ShowArrowHeads;
			public readonly Axes VisibleAxes;
			public readonly float Scale;

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
						new Arrow(Origin, Rotation * new Vector3(Scale, 0, 0)).Draw(commandBuilder, XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Arrow(Origin, Rotation * new Vector3(0, Scale, 0)).Draw(commandBuilder, YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Arrow(Origin, Rotation * new Vector3(0, 0, Scale)).Draw(commandBuilder, ZColor, duration);
				}
				else
				{
					if ((VisibleAxes & Axes.X) != 0)
						new Ray(Origin, Rotation * new Vector3(Scale, 0, 0)).Draw(commandBuilder, XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Ray(Origin, Rotation * new Vector3(0, Scale, 0)).Draw(commandBuilder, YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Ray(Origin, Rotation * new Vector3(0, 0, Scale)).Draw(commandBuilder, ZColor, duration);
				}
			}
#endif
		}

		public readonly struct SurfacePoint : IDrawable
		{
			public readonly Vector3 Origin, Direction;
			public readonly float Radius;

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

		public readonly struct Circle : IDrawable
		{
			private readonly Arc _arc;

			public Matrix4x4 Matrix => _arc.Matrix;

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
		public readonly struct Arc : IDrawable
		{
			public readonly Matrix4x4 Matrix;
			public readonly Angle Angle;

			internal static readonly Quaternion s_Base3DRotation = Quaternion.Euler(90, -90, 0);
			

			public Arc(Matrix4x4 matrix, Angle angle)
			{
				Matrix = matrix;
				Angle = angle;
			}

			public Arc(Matrix4x4 matrix) : this(matrix, Angle.FromTurns(1)) { }

			public Arc(Vector3 origin, Quaternion rotation, float radius, Angle angle)
			{
				Angle = angle;
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

		public readonly struct Sphere : IDrawable
		{
			public readonly Matrix4x4 Matrix;

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

		public readonly struct Hemisphere : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly Quaternion Orientation;
			public readonly float Radius;

			public Hemisphere(Vector3 origin, Quaternion orientation, float radius)
			{
				Origin = origin;
				Orientation = orientation;
				Radius = radius;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 direction = Orientation * Vector3.forward;

				// Cap ----
				Vector3 perpendicular = Orientation * Vector3.up;
				Vector3 perpendicular2 = Orientation * Vector3.right;
				Draw(commandBuilder, Origin, direction, perpendicular, perpendicular2, Radius, color, duration);
			}

			public static void Draw(CommandBuilder commandBuilder, Vector3 origin, Vector3 direction, Vector3 tangent, Vector3 bitangent, float radius, Color color, float duration)
			{
				// Cap ----
				Vector3 endPos = origin;
				Angle halfAngle = Angle.FromTurns(0.5f);
				// 3 arcs
				commandBuilder.AppendArc(new Arc(endPos, tangent, direction, radius, halfAngle), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(endPos, bitangent, direction, radius, halfAngle), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(endPos, direction, tangent, radius), color, duration, DrawModifications.NormalFade);
				// cap outline
				commandBuilder.AppendArc(new Arc(endPos, tangent, direction, radius, halfAngle), color, duration, DrawModifications.Custom | DrawModifications.NormalFade);
				// --------
			}
#endif
		}

		public readonly struct Box : IDrawable
		{
			public readonly Matrix4x4 Matrix;

			internal Box(Matrix4x4 matrix) => Matrix = matrix;

			public Box(Vector3 position, Vector3 halfExtents, Quaternion orientation) : this(Matrix4x4.TRS(position, orientation, halfExtents)) { }

			public Box(Transform transform) => Matrix = transform.localToWorldMatrix;

			public Box(Bounds bounds) : this(bounds.center, bounds.extents, Quaternion.identity) { }

			public Box GetTranslated(Vector3 translation) => new Box(Matrix4x4.Translate(translation) * Matrix);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendBox(this, color, duration, DrawModifications.NormalFade);
#endif
		}

		public readonly struct Capsule : IDrawable
		{
			public readonly Vector3 SpherePosition1, SpherePosition2;
			public readonly float Radius;

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
				Hemisphere.Draw(commandBuilder, SpherePosition2, up, perpendicular, perpendicular2, Radius, color, duration);
				Hemisphere.Draw(commandBuilder, SpherePosition1, down, perpendicular, perpendicular2, Radius, color, duration);

				perpendicular *= Radius;
				perpendicular2 *= Radius;

				commandBuilder.AppendOutline(new Outline(SpherePosition1 + perpendicular, SpherePosition2 + perpendicular, perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(SpherePosition1 - perpendicular, SpherePosition2 - perpendicular, -perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(SpherePosition1 + perpendicular2, SpherePosition2 + perpendicular2, perpendicular2), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(SpherePosition1 - perpendicular2, SpherePosition2 - perpendicular2, -perpendicular2), color, duration, DrawModifications.NormalFade);

				commandBuilder.AppendOutline(new Outline(SpherePosition1, SpherePosition2, Radius), color, duration);
				commandBuilder.AppendOutline(new Outline(SpherePosition2, SpherePosition1, Radius), color, duration);
			}
#endif
		}

		/// <summary>
		/// An outline is a special structure:<br/>
		/// - Handles straight lines that are correctly bounded against a capsule, outlining it.<br/>
		/// - Creates single-sided lines for capsules;<br/>
		/// - Handles the lines between sphere and capsule casts.<br/>
		/// </summary>
		internal readonly struct Outline : IDrawable
		{
			public readonly Vector3 A, B, C;

			public Outline(Vector3 a, Vector3 b, float radius)
			{
				A = a;
				B = b;
				C = new Vector3(radius, 0, 0);
			}

			public Outline(Vector3 a, Vector3 b, Vector3 c)
			{
				A = a;
				B = b;
				C = c;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendOutline(this, color, duration);
#endif
		}
		
		/// <summary>
		/// Cast is a special structure:<br/>
		/// - Used in combination with a geometry shader for the outlines of a box cast.
		/// </summary>
		internal readonly struct Cast : IDrawable
		{
			public readonly Matrix4x4 Matrix;
			public readonly Vector3 Vector;

			public Cast(Matrix4x4 matrix, Vector3 vector)
			{
				Matrix = matrix;
				Vector = vector;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendCast(this, color, duration);
#endif
		}
	}
}