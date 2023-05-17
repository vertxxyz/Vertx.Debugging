using System.Collections.Generic;
using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
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

			public Line GetShortened(float shortenBy, float minShorteningNormalised = 0)
			{
				Vector3 dir = B - A;
				dir.EnsureNormalized(out float length);
				float totalLength = length;
				length = Mathf.Max(length - shortenBy, length * minShorteningNormalised);
				length = totalLength - length;
				return new Line(A + dir * length, B - dir * length);
			}
		}

		public readonly struct DashedLine : IDrawable
		{
			public readonly Vector3 A, B;

			public DashedLine(Vector3 a, Vector3 b)
			{
				A = a;
				B = b;
			}

			public DashedLine(Ray ray)
			{
				A = ray.Origin;
				B = ray.Origin + ray.Direction;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendDashedLine(this, color, duration);
#endif

			public DashedLine GetShortened(float shortenBy, float minShorteningNormalised = 0)
			{
				Vector3 dir = B - A;
				dir.EnsureNormalized(out float length);
				float totalLength = length;
				length = Mathf.Max(length - shortenBy, length * minShorteningNormalised);
				length = totalLength - length;
				return new DashedLine(A + dir * length, B - dir * length);
			}
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

			public Ray(UnityEngine.Ray ray, float distance = Mathf.Infinity) : this(ray.origin, ray.direction * GetClampedMaxDistance(distance)) { }

			public Ray(UnityEngine.Ray2D ray, float distance = Mathf.Infinity) : this(ray.origin, ray.direction * GetClampedMaxDistance(distance)) { }

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
			internal const float HeadLength = 0.075f;
			internal const float HeadWidth = 0.05f;

			public Arrow(Vector3 origin, Vector3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Arrow(Vector3 origin, Quaternion rotation, float length = 1) : this(origin, rotation * Vector3.forward * length) { }
			public Arrow(Line line) : this(line.A, line.B - line.A) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				DrawArrowHead(commandBuilder, Origin, Direction, color, duration);
			}

			public static void DrawArrowHead(CommandBuilder commandBuilder, Vector3 point, Vector3 dir, Color color, float duration = 0)
			{
				const int segments = 3;

				Vector3 arrowPoint = point + dir;
				dir.EnsureNormalized(out float length);

				float headLength = HeadLength;
				float headWidth = HeadWidth;

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

		/// <summary>
		/// An arrow with only one side of its head.<br/>
		/// Commonly used to represent the HalfEdge data structure.
		/// </summary>
		public readonly struct HalfArrow : IDrawable
		{
			public readonly Line Line;
			public readonly Vector3 Perpendicular;

			public HalfArrow(Line line, Vector3 perpendicular)
			{
				Line = line;
				perpendicular.EnsureNormalized();
				Perpendicular = perpendicular;
			}

			public HalfArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular) : this(new Line(origin, origin + direction), perpendicular) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendLine(Line, color, duration);
				DrawHalfArrowHead(commandBuilder, color, duration);
			}

			private void DrawHalfArrowHead(CommandBuilder commandBuilder, Color color, float duration = 0)
			{
				Vector3 end = Line.B;
				Vector3 dir = Line.A - Line.B;

				dir.EnsureNormalized(out float length);

				float headLength = Arrow.HeadLength;
				float headWidth = Arrow.HeadWidth;

				if (headLength > length * 0.5f)
				{
					headLength *= length;
					headWidth *= length;
				}

				Vector3 cross = Vector3.Cross(dir, Perpendicular);
				Vector3 a = end + dir * headLength;
				Vector3 b = a + cross * headWidth;
				commandBuilder.AppendLine(new Line(end, b), color, duration);
				commandBuilder.AppendLine(new Line(a, b), color, duration);
			}
#endif
		}

		/// <summary>
		/// An arrow that is curved between two points.<br/>
		/// Useful when straight lines can overlap with other shapes.
		/// </summary>
		public readonly struct CurvedArrow : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly Vector3 Direction;
			public readonly Vector3 Perpendicular;
			public readonly Angle Angle;

			public CurvedArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular) : this(origin, direction, perpendicular, Angle.FromTurns(0.1f)) { }

			public CurvedArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular, Angle angle)
			{
				Origin = origin;
				Direction = direction;
				perpendicular.EnsureNormalized();
				if (!Mathf.Approximately(Vector3.Dot(direction, perpendicular), 0))
				{
					direction.EnsureNormalized();
					perpendicular = Vector3.Cross(direction, Vector3.Cross(direction, perpendicular).normalized);
				}

				Perpendicular = perpendicular;
				Angle = angle.Turns > 0.5f ? Angle.FromTurns(0.5f) : angle;
			}

			public CurvedArrow(in Line line, Vector3 perpendicular) : this(line.A, line.B - line.A, perpendicular) { }

			public CurvedArrow(in Line line, Vector3 perpendicular, Angle angle) : this(line.A, line.B - line.A, perpendicular, angle) { }

#if UNITY_EDITOR
			private const float ArrowHeadAngle = 30f;
			private static readonly Quaternion ArrowheadRotation = Quaternion.AngleAxis(-ArrowHeadAngle * 2, Vector3.up);

			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				// All this could be improved, reducing complex and redundant calculations.
				Vector3 end = Origin + Direction;
				Vector3 center = Origin + Direction * 0.5f;
				Vector3 dir = Direction;
				dir.EnsureNormalized(out float length);

				float headLength = Arrow.HeadLength;
				if (headLength > length * 0.5f)
					headLength *= length;

				Vector3 cross = Vector3.Cross(dir, Perpendicular);
				float radius = Arc.GetRadius(Angle, length);
				float offset = radius * Mathf.Cos(0.5f * Angle.Radians);
				commandBuilder.AppendArc(new Arc(center - cross * offset, Perpendicular, cross, radius, Angle), color, duration);

				Quaternion arrowheadRotation = Quaternion.LookRotation(cross, Perpendicular) * Quaternion.AngleAxis(Angle.Degrees * 0.5f - 90 + ArrowHeadAngle, Vector3.up);
				Vector3 arrowRay = new Vector3(0, 0, headLength);
				commandBuilder.AppendLine(new Line(end, end + arrowheadRotation * arrowRay), color, duration);
				commandBuilder.AppendLine(new Line(end, end + arrowheadRotation * ArrowheadRotation * arrowRay), color, duration);
			}
#endif
		}

		public readonly struct Arrow2DFromNormal : IDrawable
		{
			public readonly Vector3 Origin, Direction, Normal;
			internal const float HeadLength = 0.075f;
			internal const float HeadWidth = 0.05f;

			public Arrow2DFromNormal(Vector3 origin, Vector3 direction, Vector3 normal)
			{
				Origin = origin;
				Direction = direction;
				Normal = normal;
				Normal.EnsureNormalized();
			}

			public Arrow2DFromNormal(Vector3 origin, Quaternion rotation, float length = 1)
			{
				Origin = origin;
				Direction = rotation * Vector3.forward * length;
				Normal = rotation * Vector3.right;
				Normal.EnsureNormalized();
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				Arrow2D.DrawArrowHead(commandBuilder, Origin + Direction, Direction.normalized, Normal, color, duration);
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

			public Axis(Transform transform, bool showArrowHeads = true, Axes visibleAxes = Axes.All, float scale = 1)
			{
				Origin = transform.position;
				Rotation = transform.rotation;
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

			/// <summary>
			/// A circle oriented along XY
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="rotation">The rotation of the circle, the circle will be oriented along XY</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(Vector3 origin, Quaternion rotation, float radius)
				=> _arc = new Arc(Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius)));

			/// <summary>
			/// If <see cref="normal"/> or <see cref="direction"/> are zero, this will spam logs to the console. Please validate your own inputs if you expect them to be incorrect.
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="normal">The normal facing outwards from the circle.</param>
			/// <param name="direction">Any direction on the plane the circle lies.</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(Vector3 origin, Vector3 normal, Vector3 direction, float radius)
				: this(
					origin,
					Quaternion.LookRotation(Mathf.Abs(Vector3.Dot(direction, normal)) > 0.999f ? GetValidPerpendicular(normal) : direction, normal) * Arc.s_Base3DRotation,
					radius
				) { }

			/// <summary>
			/// It's cheaper to use the <see cref="Circle(Vector3, Vector3, Vector3, float)"/> constructor if you already have a perpendicular facing direction for the circle.<br/>
			/// If <see cref="normal"/> is zero, this will spam logs to the console. Please validate your own inputs if you expect them to be incorrect.
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="normal">The normal facing outwards from the circle.</param>
			/// <param name="radius">The radius of the circle.</param>
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

			/// <summary>
			/// Creates an Arc
			/// </summary>
			internal Arc(Matrix4x4 matrix) : this(matrix, Angle.FromTurns(1)) { }

			public Arc(Vector3 origin, Quaternion rotation, float radius, Angle angle)
			{
				Angle = angle;
				Matrix = Matrix4x4.TRS(origin, rotation, new Vector3(radius, radius, radius));
			}

			public Arc(Vector3 origin, Quaternion rotation, float radius) : this(origin, rotation, radius, Angle.FromTurns(1)) { }

			public Arc(Vector2 origin, float rotationDegrees, float radius, Angle angle)
				// If NaN is introduced externally by the user this protects against that silently.
				: this(origin, float.IsNaN(rotationDegrees) ? Quaternion.identity : Quaternion.AngleAxis(rotationDegrees, Vector3.forward), radius, angle) { }

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

			/// <param name="chord">A line that makes up the chord of the arc (two positions at the ends of the arc)</param>
			/// <param name="aim">The direction for the arc to bend towards.</param>
			/// <param name="arcLength">The length of the arc. If <see cref="arcLength"/> is less than the length of the chord, nothing will draw.</param>
			public Arc(Line chord, Vector3 aim, float arcLength)
			{
				Vector3 tangent = chord.A - chord.B;
				float chordLength = tangent.magnitude;
				if (chordLength < 0.00001f || arcLength <= chordLength)
				{
					Angle = default;
					Matrix = Matrix4x4.identity;
					// ideally this would draw a line if the arc length was less than or equal to the chord length.
					return;
				}

				(float radius, float height, Angle angle) = GetSegmentDetails(chordLength, arcLength);

				tangent /= chordLength;
				aim.EnsureNormalized();
				Vector3 normal = Vector3.Cross(aim, tangent).normalized;
				// Ensure direction is tangent to the chord:
				Vector3 direction = Vector3.Cross(tangent, normal);

				Matrix = Matrix4x4.TRS(
					(chord.A + chord.B) / 2 - direction * height,
					Quaternion.LookRotation(direction, normal) * s_Base3DRotation,
					new Vector3(radius, radius, radius)
				);
				Angle = angle;
			}

			/// <summary>
			/// Gets other details about a circular segment/arc from the input parameters.
			/// </summary>
			/// <param name="chordLength">The length of the arc's chord.</param>
			/// <param name="arcLength">The length of the arc.</param>
			/// <returns>The radius of the arc, sagitta (height), and angle.</returns>
			private static (float radius, float height, Angle angle) GetSegmentDetails(float chordLength, float arcLength)
			{
				// Thanks to @FreyaHolmer's community <3
				// Taylor expansion to find first approximation
				float x = Mathf.Sqrt(48f * ((arcLength - chordLength) / (2f * arcLength)));

				// Newton method to find root within acceptable error.
				float error;
				int iterations = 0;
				do
				{
					float sR = Mathf.Sin(x * 0.5f);
					error = sR / x - chordLength / (2 * arcLength);
					float firstDerivative = (x * Mathf.Cos(x * 0.5f) - 2f * sR) / (2f * x * x);
					x -= error / firstDerivative;
				} while (error > 0.001f && ++iterations < 10);

				float angleRad = x;
				float radius = arcLength / angleRad;
				float height = radius * Mathf.Cos(0.5f * angleRad);
				return (radius, height, Angle.FromRadians(angleRad));
			}


			public static float GetRadius(in Angle angle, float chordLength) => chordLength / (2 * Mathf.Sin(0.5f * angle.Radians));
			public static Angle GetAngle(float radius, float chordLength) => Angle.FromRadians(Mathf.Asin(chordLength / (2f * radius)) * 2);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (Angle.Turns == 0)
					return;
				commandBuilder.AppendArc(this, color, duration);
			}
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

#if VERTX_PHYSICS
			public Sphere(SphereCollider sphereCollider)
			{
				Transform transform = sphereCollider.transform;
				Vector3 scale = transform.lossyScale;
				float radiusScaled = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * sphereCollider.radius;
				Matrix = Matrix4x4.TRS(transform.TransformPoint(sphereCollider.center), transform.rotation, new Vector3(radiusScaled, radiusScaled, radiusScaled));
			}
#endif

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
			public readonly bool Shade3D;

			internal Box(Matrix4x4 matrix, bool shade3D = true)
			{
				Matrix = matrix;
				Shade3D = shade3D;
			}

			public Box(Vector3 position, Vector3 halfExtents, Quaternion orientation, bool shade3D = true) : this(Matrix4x4.TRS(position, orientation, halfExtents), shade3D) { }

			public Box(Vector3 position, Vector3 halfExtents, bool shade3D = true) : this(Matrix4x4.TRS(position, Quaternion.identity, halfExtents), shade3D) { }

			public Box(Transform transform, bool shade3D = true)
			{
				Shade3D = shade3D;
				Matrix = transform.localToWorldMatrix;
			}

			public Box(Bounds bounds, bool shade3D = true) : this(bounds.center, bounds.extents, Quaternion.identity, shade3D) { }

			public Box(BoundsInt bounds, bool shade3D = true) : this(bounds.center, (Vector3)bounds.size * 2f, Quaternion.identity, shade3D) { }

#if VERTX_PHYSICS
			public Box(BoxCollider boxCollider)
			{
				Shade3D = true;
				Transform transform = boxCollider.transform;
				Matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale));
			}
#endif

			public Box GetTranslated(Vector3 translation) => new Box(Matrix4x4.Translate(translation) * Matrix);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendBox(this, color, duration, Shade3D ? DrawModifications.NormalFade : DrawModifications.None);
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

#if VERTX_PHYSICS
			public Capsule(CharacterController characterController)
			{
				Transform transform = characterController.transform;
				Vector3 scale = transform.lossyScale;
				float radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
				Radius = characterController.radius * radiusScale;
				float offset = Mathf.Max(characterController.height * 0.5f * Mathf.Abs(scale.y), Radius) - Radius;
				Vector3 center = transform.TransformPoint(characterController.center);
				SpherePosition1 = new Vector3(center.x, center.y - offset, center.z);
				SpherePosition2 = new Vector3(center.x, center.y + offset, center.z);
			}

			public Capsule(CapsuleCollider collider)
			{
				Transform transform = collider.transform;
				Vector3 scale = transform.lossyScale;
				float radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
				Radius = collider.radius * radiusScale;
				Vector3 center = transform.TransformPoint(collider.center);
				float offsetY = Mathf.Max(collider.height * 0.5f * Mathf.Abs(scale.y), Radius) - Radius;
				Vector3 offset = transform.TransformDirection(new Vector3(0, offsetY, 0));
				SpherePosition1 = center - offset;
				SpherePosition2 = center + offset;
			}
#endif

			public Capsule GetTranslated(Vector3 translation) => new Capsule(SpherePosition1 + translation, SpherePosition2 + translation, Radius);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 up = SpherePosition2 - SpherePosition1;
				up.EnsureNormalized(out float length);
				if (length == 0)
				{
					new Sphere(SpherePosition1, Radius).Draw(commandBuilder, color, duration);
					return;
				}

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

		public readonly struct Cylinder : IDrawable
		{
			public readonly Vector3 Center;
			public readonly Quaternion Rotation;
			public readonly float HalfHeight;
			public readonly float Radius;

			public Cylinder(Vector3 center, Quaternion rotation, float height, float radius)
			{
				Center = center;
				Rotation = rotation;
				HalfHeight = height * 0.5f;
				Radius = radius;
			}

			public Cylinder(Vector3 point1, Vector3 point2, float radius)
			{
				Vector3 up = point2 - point1;
				up.EnsureNormalized(out HalfHeight);
				HalfHeight *= 0.5f;
				Center = point1 + up * HalfHeight;
				Radius = radius;
				if (HalfHeight == 0)
					Rotation = Quaternion.identity;
				else
				{
					Vector3 perpendicular = GetValidPerpendicular(up);
					Rotation = Quaternion.LookRotation(perpendicular, up);
				}
			}

			public Cylinder(Vector3 lowestPosition, Vector3 direction, float height, float radius)
			{
				direction.EnsureNormalized();
				HalfHeight = height * 0.5f;
				Center = lowestPosition + direction * HalfHeight;
				Radius = radius;
				if (HalfHeight == 0)
					Rotation = Quaternion.identity;
				else
				{
					Vector3 perpendicular = GetValidPerpendicular(direction);
					Rotation = Quaternion.LookRotation(perpendicular, direction);
				}
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Quaternion circleRotation = Quaternion.LookRotation(Rotation * Vector3.up, Rotation * Vector3.right);
				if (HalfHeight == 0)
				{
					new Circle(Center, circleRotation, Radius).Draw(commandBuilder, color, duration);
					return;
				}

				Vector3 up = Rotation * new Vector3(0, HalfHeight, 0);
				Vector3 point1 = Center + up;
				Vector3 point2 = Center - up;

				if (Radius == 0)
				{
					commandBuilder.AppendLine(new Line(point1, point2), color, duration);
					return;
				}

				Vector3 perpendicular = Rotation * new Vector3(Radius, 0, 0);
				Vector3 perpendicular2 = Rotation * new Vector3(0, 0, Radius);


				new Circle(point1, circleRotation, Radius).Draw(commandBuilder, color, duration);
				new Circle(point2, circleRotation, Radius).Draw(commandBuilder, color, duration);

				commandBuilder.AppendOutline(new Outline(point1 + perpendicular, point2 + perpendicular, perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 - perpendicular, point2 - perpendicular, -perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 + perpendicular2, point2 + perpendicular2, perpendicular2), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 - perpendicular2, point2 - perpendicular2, -perpendicular2), color, duration, DrawModifications.NormalFade);

				commandBuilder.AppendOutline(new Outline(point1, point2, Radius), color, duration);
				commandBuilder.AppendOutline(new Outline(point2, point1, Radius), color, duration);
			}
#endif
		}

		public readonly struct Plane : IDrawable
		{
			public readonly UnityEngine.Plane Value;
			public readonly Vector3 PointOnPlane;
			public readonly Vector2 DisplaySize;

			/// <summary>
			/// Constructs a Plane
			/// </summary>
			/// <param name="plane">The backing plane</param>
			/// <param name="pointOnPlane">A point to center the debug visual on</param>
			/// <param name="displaySize">The size of the debugging plane</param>
			public Plane(UnityEngine.Plane plane, Vector3 pointOnPlane, Vector2 displaySize)
			{
				Value = plane;
				PointOnPlane = plane.ClosestPointOnPlane(pointOnPlane);
				DisplaySize = displaySize;
			}

			/// <summary>
			/// Constructs a Plane
			/// </summary>
			/// <param name="plane">The backing plane</param>
			/// <param name="pointOnPlane">A point to center the debug visual on, defaults to 2x2m in size.</param>
			public Plane(UnityEngine.Plane plane, Vector3 pointOnPlane)
			{
				Value = plane;
				PointOnPlane = plane.ClosestPointOnPlane(pointOnPlane);
				DisplaySize = new Vector2(2, 2);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 normal = Value.normal;
				Vector3 perpendicular = GetValidPerpendicular(normal);
				Quaternion rotation = Quaternion.LookRotation(normal, perpendicular);
				new Box2D(PointOnPlane, rotation, DisplaySize).Draw(commandBuilder, color, duration);
				new Arrow(PointOnPlane, rotation).Draw(commandBuilder, color, duration);
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