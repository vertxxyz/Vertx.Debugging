using System.Collections.Generic;
using Unity.Mathematics;
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
			public readonly float3 A, B;

			public Line(float3 a, float3 b)
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
				float3 dir = B - A;
				dir.EnsureNormalized(out float length);
				float totalLength = length;
				length = math.max(length - shortenBy, length * minShorteningNormalised);
				length = totalLength - length;
				return new Line(A + dir * length, B - dir * length);
			}
		}

		public readonly struct DashedLine : IDrawable
		{
			public readonly float3 A, B;

			public DashedLine(float3 a, float3 b)
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
				float3 dir = B - A;
				dir.EnsureNormalized(out float length);
				float totalLength = length;
				length = math.max(length - shortenBy, length * minShorteningNormalised);
				length = totalLength - length;
				return new DashedLine(A + dir * length, B - dir * length);
			}
		}

		public readonly struct LineStrip : IDrawable
		{
			public readonly IEnumerable<float3> Points;

			public LineStrip(IEnumerable<float3> points) => Points = points;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float3? previous = null;
				foreach (float3 point in Points)
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
			public readonly float3 Origin, Direction;

			public Ray(float3 origin, float3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Ray(float3 origin, float3 direction, float distance)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction * GetClampedMaxDistance(distance);
			}

			public Ray(UnityEngine.Ray ray, float distance = math.INFINITY) : this(ray.origin, ray.direction * GetClampedMaxDistance(distance)) { }

			public Ray(UnityEngine.Ray2D ray, float distance = math.INFINITY) : this(ray.origin.xy0(), (ray.direction * GetClampedMaxDistance(distance)).xy0()) { }

			public static implicit operator Ray(UnityEngine.Ray ray) => new Ray(ray.origin, ray.direction);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendRay(this, color, duration);
#endif
		}

		public readonly struct Point : IDrawable
		{
			public readonly float3 Position;
			public readonly float Scale;

			public Point(float3 position, float scale = 0.3f)
			{
				Position = position;
				Scale = scale;
			}

			public Point(float2 position, float scale = 0.3f) : this(new float3(position.x, position.y, 0), scale) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float distance = Scale * 0.5f;
				commandBuilder.AppendLine(new Line(new float3(Position.x - distance, Position.y, Position.z), new float3(Position.x + distance, Position.y, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new float3(Position.x, Position.y - distance, Position.z), new float3(Position.x, Position.y + distance, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new float3(Position.x, Position.y, Position.z - distance), new float3(Position.x, Position.y, Position.z + distance)), color, duration);
			}
#endif
		}

		public readonly struct Arrow : IDrawable
		{
			public readonly float3 Origin, Direction;
			internal const float HeadLength = 0.075f;
			internal const float HeadWidth = 0.05f;

			public Arrow(float3 origin, float3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Arrow(float3 origin, quaternion rotation, float length = 1) : this(origin, math.mul(rotation, math.forward()) * length) { }
			public Arrow(Line line) : this(line.A, line.B - line.A) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				DrawArrowHead(commandBuilder, Origin, Direction, color, duration);
			}

			public static void DrawArrowHead(CommandBuilder commandBuilder, float3 point, float3 dir, Color color, float duration = 0)
			{
				const int segments = 3;

				float3 arrowPoint = point + dir;
				dir.EnsureNormalized(out float length);

				float headLength = HeadLength;
				float headWidth = HeadWidth;

				if (headLength > length * 0.5f)
				{
					headLength *= length;
					headWidth *= length;
				}

				void DoDrawArrowHead(float3 center, float3 normal, float radius)
				{
					float2[] circle = CircleCache.GetCircle(segments);
					float3 tangent = GetValidPerpendicular(normal);
					float3 bitangent = math.cross(normal, tangent);
					tangent *= radius;
					bitangent *= radius;
					float3 lastPos = center + tangent;
					for (int i = 1; i <= segments; i++)
					{
						float2 c = circle[i];
						float3 nextPos = center + tangent * c.x + bitangent * c.y;
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
			public readonly IEnumerable<float3> Points;

			public ArrowStrip(IEnumerable<float3> points) => Points = points;

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float3? previous = null;
				float3? origin = null;
				foreach (float3 point in Points)
				{
					if (previous.HasValue)
						commandBuilder.AppendLine(new Line(previous.Value, point), color, duration);
					origin = previous;
					previous = point;
				}

				if (!origin.HasValue)
					return;
				float3 direction = previous.Value - origin.Value;
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
			public readonly float3 Perpendicular;

			public HalfArrow(Line line, float3 perpendicular)
			{
				Line = line;
				perpendicular.EnsureNormalized();
				Perpendicular = perpendicular;
			}

			public HalfArrow(float3 origin, float3 direction, float3 perpendicular) : this(new Line(origin, origin + direction), perpendicular) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendLine(Line, color, duration);
				DrawHalfArrowHead(commandBuilder, color, duration);
			}

			private void DrawHalfArrowHead(CommandBuilder commandBuilder, Color color, float duration = 0)
			{
				float3 end = Line.B;
				float3 dir = Line.A - Line.B;

				dir.EnsureNormalized(out float length);

				float headLength = Arrow.HeadLength;
				float headWidth = Arrow.HeadWidth;

				if (headLength > length * 0.5f)
				{
					headLength *= length;
					headWidth *= length;
				}

				float3 cross = math.cross(dir, Perpendicular);
				float3 a = end + dir * headLength;
				float3 b = a + cross * headWidth;
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
			public readonly float3 Origin;
			public readonly float3 Direction;
			public readonly float3 Perpendicular;
			public readonly Angle Angle;

			public CurvedArrow(float3 origin, float3 direction, float3 perpendicular) : this(origin, direction, perpendicular, Angle.FromTurns(0.1f)) { }

			public CurvedArrow(float3 origin, float3 direction, float3 perpendicular, Angle angle)
			{
				Origin = origin;
				Direction = direction;
				perpendicular.EnsureNormalized();
				if (!Mathf.Approximately(math.dot(direction, perpendicular), 0))
				{
					direction.EnsureNormalized();
					perpendicular = math.normalize(math.cross(direction, math.cross(direction, perpendicular)));
				}

				Perpendicular = perpendicular;
				Angle = angle.Turns > 0.5f ? Angle.FromTurns(0.5f) : angle;
			}

			public CurvedArrow(in Line line, float3 perpendicular) : this(line.A, line.B - line.A, perpendicular) { }

			public CurvedArrow(in Line line, float3 perpendicular, Angle angle) : this(line.A, line.B - line.A, perpendicular, angle) { }

#if UNITY_EDITOR
			private const float ArrowHeadAngle = 30f * math.TORADIANS;
			private static readonly quaternion ArrowheadRotation = quaternion.AxisAngle(math.up(), -ArrowHeadAngle * 2);

			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				// All this could be improved, reducing complex and redundant calculations.
				float3 end = Origin + Direction;
				float3 center = Origin + Direction * 0.5f;
				float3 dir = Direction;
				dir.EnsureNormalized(out float length);

				float headLength = Arrow.HeadLength;
				if (headLength > length * 0.5f)
					headLength *= length;

				float3 cross = math.cross(dir, Perpendicular);
				float radius = Arc.GetRadius(Angle, length);
				float offset = radius * math.cos(0.5f * Angle.Radians);
				commandBuilder.AppendArc(new Arc(center - cross * offset, Perpendicular, cross, radius, Angle), color, duration);

				quaternion arrowheadRotation = math.mul(quaternion.LookRotation(cross, Perpendicular), quaternion.AxisAngle(math.up(), Angle.Radians * 0.5f - math.PI * 2 + ArrowHeadAngle));
				float3 arrowRay = new float3(0, 0, headLength);
				commandBuilder.AppendLine(new Line(end, end + math.mul(arrowheadRotation, arrowRay)), color, duration);
				commandBuilder.AppendLine(new Line(end, end + math.mul(math.mul(arrowheadRotation, ArrowheadRotation), arrowRay)), color, duration);
			}
#endif
		}

		public readonly struct Arrow2DFromNormal : IDrawable
		{
			public readonly float3 Origin, Direction, Normal;
			internal const float HeadLength = 0.075f;
			internal const float HeadWidth = 0.05f;

			public Arrow2DFromNormal(float3 origin, float3 direction, float3 normal)
			{
				Origin = origin;
				Direction = direction;
				Normal = normal;
				Normal.EnsureNormalized();
			}

			public Arrow2DFromNormal(float3 origin, quaternion rotation, float length = 1)
			{
				Origin = origin;
				Direction = math.mul(rotation, math.forward()) * length;
				Normal = math.mul(rotation, math.right());
				Normal.EnsureNormalized();
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				Arrow2D.DrawArrowHead(commandBuilder, Origin + Direction, math.normalizesafe(Direction), Normal, color, duration);
			}
#endif
		}

		public readonly struct Axis : IDrawable
		{
			public readonly float3 Origin;
			public readonly quaternion Rotation;
			public readonly bool ShowArrowHeads;
			public readonly Axes VisibleAxes;
			public readonly float Scale;

			public Axis(float3 origin, quaternion rotation, bool showArrowHeads = true, Axes visibleAxes = Axes.All, float scale = 1)
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
						new Arrow(Origin, math.mul(Rotation, new float3(Scale, 0, 0))).Draw(commandBuilder, XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Arrow(Origin, math.mul(Rotation, new float3(0, Scale, 0))).Draw(commandBuilder, YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Arrow(Origin, math.mul(Rotation, new float3(0, 0, Scale))).Draw(commandBuilder, ZColor, duration);
				}
				else
				{
					if ((VisibleAxes & Axes.X) != 0)
						new Ray(Origin, math.mul(Rotation, new float3(Scale, 0, 0))).Draw(commandBuilder, XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Ray(Origin, math.mul(Rotation, new float3(0, Scale, 0))).Draw(commandBuilder, YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Ray(Origin, math.mul(Rotation, new float3(0, 0, Scale))).Draw(commandBuilder, ZColor, duration);
				}
			}
#endif
		}

		public readonly struct SurfacePoint : IDrawable
		{
			public readonly float3 Origin, Direction;
			public readonly float Radius;

			public SurfacePoint(float3 origin, float3 direction)
			{
				Origin = origin;
				EnsureNormalized(ref direction, out float length);
				Direction = direction;
				Radius = length * 0.05f;
			}

			public SurfacePoint(float3 origin, float3 direction, float radius)
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

			public float4x4 Matrix => _arc.Matrix;

			public Circle(float4x4 matrix) => _arc = new Arc(matrix);

			/// <summary>
			/// A circle oriented along XY
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="rotation">The rotation of the circle, the circle will be oriented along XY</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(float3 origin, quaternion rotation, float radius)
				=> _arc = new Arc(float4x4.TRS(origin, rotation, new float3(radius, radius, radius)));

			/// <summary>
			/// If <see cref="normal"/> or <see cref="direction"/> are zero, this will spam logs to the console. Please validate your own inputs if you expect them to be incorrect.
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="normal">The normal facing outwards from the circle.</param>
			/// <param name="direction">Any direction on the plane the circle lies.</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(float3 origin, float3 normal, float3 direction, float radius)
				: this(
					origin,
					math.mul(quaternion.LookRotation(math.abs(math.dot(direction, normal)) > 0.999f ? GetValidPerpendicular(normal) : direction, normal), Arc.s_Base3DRotation),
					radius
				) { }

			/// <summary>
			/// It's cheaper to use the <see cref="Circle(float3, float3, float3, float)"/> constructor if you already have a perpendicular facing direction for the circle.<br/>
			/// If <see cref="normal"/> is zero, this will spam logs to the console. Please validate your own inputs if you expect them to be incorrect.
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="normal">The normal facing outwards from the circle.</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(float3 origin, float3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius) { }

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
			public readonly float4x4 Matrix;
			public readonly Angle Angle;

			internal static readonly quaternion s_Base3DRotation = quaternion.Euler(90 * math.TORADIANS, -90 * math.TORADIANS, 0);


			public Arc(float4x4 matrix, Angle angle)
			{
				Matrix = matrix;
				Angle = angle;
			}

			/// <summary>
			/// Creates an Arc
			/// </summary>
			internal Arc(float4x4 matrix) : this(matrix, Angle.FromTurns(1)) { }

			public Arc(float3 origin, quaternion rotation, float radius, Angle angle)
			{
				Angle = angle;
				Matrix = float4x4.TRS(origin, rotation, new float3(radius, radius, radius));
			}

			public Arc(float3 origin, quaternion rotation, float radius) : this(origin, rotation, radius, Angle.FromTurns(1)) { }

			public Arc(float2 origin, float rotationDegrees, float radius, Angle angle)
				// If NaN is introduced externally by the user this protects against that silently.
				: this(origin.xy0(), float.IsNaN(rotationDegrees) ? quaternion.identity : quaternion.AxisAngle(math.forward(), rotationDegrees * math.TORADIANS), radius, angle) { }

			public Arc(float2 origin, float rotationDegrees, float radius) : this(origin, rotationDegrees, radius, Angle.FromTurns(1)) { }

			public Arc(float3 origin, float3 normal, float3 direction, float radius, Angle angle)
				: this(
					origin,
					math.mul(quaternion.LookRotation(direction, normal), s_Base3DRotation),
					radius,
					angle
				) { }

			public Arc(float3 origin, float3 normal, float3 direction, float radius) : this(origin, normal, direction, radius, Angle.FromTurns(1)) { }

			/// <summary>
			/// It's cheaper to use the <see cref="Arc(float3, float3, float3, float)"/> constructor if you already have a perpendicular facing direction for the circle.
			/// </summary>
			public Arc(float3 origin, float3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius, Angle.FromTurns(1)) { }

			/// <param name="chord">A line that makes up the chord of the arc (two positions at the ends of the arc)</param>
			/// <param name="aim">The direction for the arc to bend towards.</param>
			/// <param name="arcLength">The length of the arc. If <see cref="arcLength"/> is less than the length of the chord, nothing will draw.</param>
			public Arc(Line chord, float3 aim, float arcLength)
			{
				float3 tangent = chord.A - chord.B;
				float chordLength = math.length(tangent);
				if (chordLength < 0.00001f || arcLength <= chordLength)
				{
					Angle = default;
					Matrix = float4x4.identity;
					// ideally this would draw a line if the arc length was less than or equal to the chord length.
					return;
				}

				(float radius, float height, Angle angle) = GetSegmentDetails(chordLength, arcLength);

				tangent /= chordLength;
				aim.EnsureNormalized();
				float3 normal = math.normalizesafe(math.cross(aim, tangent));
				// Ensure direction is tangent to the chord:
				float3 direction = math.cross(tangent, normal);

				Matrix = float4x4.TRS(
					(chord.A + chord.B) / 2 - direction * height,
					math.mul(quaternion.LookRotation(direction, normal), s_Base3DRotation),
					new float3(radius, radius, radius)
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
				float x = math.sqrt(48f * ((arcLength - chordLength) / (2f * arcLength)));

				// Newton method to find root within acceptable error.
				float error;
				int iterations = 0;
				do
				{
					float sR = math.sin(x * 0.5f);
					error = sR / x - chordLength / (2 * arcLength);
					float firstDerivative = (x * math.cos(x * 0.5f) - 2f * sR) / (2f * x * x);
					x -= error / firstDerivative;
				} while (error > 0.001f && ++iterations < 10);

				float angleRad = x;
				float radius = arcLength / angleRad;
				float height = radius * math.cos(0.5f * angleRad);
				return (radius, height, Angle.FromRadians(angleRad));
			}


			public static float GetRadius(in Angle angle, float chordLength) => chordLength / (2 * math.sin(0.5f * angle.Radians));
			public static Angle GetAngle(float radius, float chordLength) => Angle.FromRadians(math.asin(chordLength / (2f * radius)) * 2);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (Angle.Turns == 0)
					return;
				commandBuilder.AppendArc(this, color, duration);
			}
#endif
		}

		/// <summary>
		/// This is 2D, by default the annulus faces right, aligned with XY.<br/>
		/// Use the helper constructors to create an Arc aligned how you require.
		/// </summary>
		public readonly struct Annulus : IDrawable
		{
			public readonly float3 Origin;
			public readonly quaternion Rotation;
			public readonly float InnerRadius, OuterRadius;
			public readonly Angle SectorWidth;

			/// <summary>
			/// Creates an annulus.
			/// </summary>
			public Annulus(float3 origin, quaternion rotation, float innerRadius, float outerRadius)
				: this(origin, rotation, innerRadius, outerRadius, Angle.FromTurns(1)) { }
			
			/// <summary>
			/// Creates an annulus.
			/// </summary>
			public Annulus(float3 origin, float3 normal, float3 direction, float innerRadius, float outerRadius)
				: this(origin, normal, direction, innerRadius, outerRadius, Angle.FromTurns(1)) { }

			/// <summary>
			/// Creates an annulus sector.
			/// </summary>
			public Annulus(float3 origin, quaternion rotation, float innerRadius, float outerRadius, Angle sectorWidth)
			{
				SectorWidth = sectorWidth;
				Origin = origin;
				Rotation = rotation;
				InnerRadius = innerRadius;
				OuterRadius = outerRadius;
			}
			
			/// <summary>
			/// Creates an annulus sector.
			/// </summary>
			public Annulus(float3 origin, float3 normal, float3 direction, float innerRadius, float outerRadius, Angle sectorWidth)
				: this(origin, math.mul(quaternion.LookRotation(direction, normal), Arc.s_Base3DRotation), innerRadius, outerRadius, sectorWidth) { }
			
			/// <summary>
			/// Uniform annulus sampling.
			/// </summary>
			public float3 RandomPoint() => Origin + math.mul(Rotation, RandomPoint(InnerRadius, OuterRadius, SectorWidth).xy0());

			/// <summary>
			/// Uniform annulus sampling.
			/// </summary>
			public static float2 RandomPoint(float innerRadius, float outerRadius, Angle sectorWidth)
			{
				float rad = sectorWidth.Radians * 0.5f,
					a = UnityEngine.Random.Range(-rad, rad),
					outerRadiusSquared = outerRadius * outerRadius,
					innerRadiusSquared = innerRadius * innerRadius,
					differenceSquared = outerRadiusSquared - innerRadiusSquared,
					r = math.sqrt(UnityEngine.Random.value * differenceSquared + innerRadiusSquared),
					x = r * math.cos(a),
					y = r * math.sin(a);
				return new float2(x, y);
			}
			
			/// <summary>
			/// A naïve implementation of annulus sampling.
			/// Samples will bias towards the inner radius.
			/// See <see cref="RandomPoint(float, float, Angle)"/> for uniform sampling.<br/>
			/// https://gist.github.com/vertxxyz/e3fa0b033a266027992a715468e7dd1f
			/// </summary>
			public static float2 RandomPointNonUniform(float innerRadius, float outerRadius, Angle sectorWidth)
			{
				float  rad = sectorWidth.Radians * 0.5f,
					a = UnityEngine.Random.Range(-rad, rad),
					difference = outerRadius - innerRadius,
					r = UnityEngine.Random.value * difference + innerRadius,
					x = r * math.cos(a),
					y = r * math.sin(a);
				return new float2(x, y);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				new Arc(Origin, Rotation, InnerRadius, SectorWidth).Draw(commandBuilder, color, duration);
				if (math.abs(InnerRadius - OuterRadius) < 0.0001f)
					return;
				new Arc(Origin, Rotation, OuterRadius, SectorWidth).Draw(commandBuilder, color, duration);
				if (math.abs(SectorWidth.Turns - 1) < 0.0001f)
					return;
				quaternion rightRot = quaternion.AxisAngle(math.forward(), SectorWidth.Radians * 0.5f);
				float3 rightInner = Origin + math.mul(Rotation, math.mul(rightRot, new float3(InnerRadius, 0, 0)));
				float3 rightOuter = Origin + math.mul(Rotation, math.mul(rightRot, new float3(OuterRadius, 0, 0)));
				new Line(rightInner, rightOuter).Draw(commandBuilder, color, duration);
				if (SectorWidth.Turns == 0)
					return;
				quaternion leftRot = math.inverse(rightRot);
				float3 leftInner = Origin + math.mul(Rotation, math.mul(leftRot, new float3(InnerRadius, 0, 0)));
				float3 leftOuter = Origin + math.mul(Rotation, math.mul(leftRot, new float3(OuterRadius, 0, 0)));
				new Line(leftInner, leftOuter).Draw(commandBuilder, color, duration);
			}
#endif
		}

		public readonly struct Sphere : IDrawable
		{
			public readonly float4x4 Matrix;

			public Sphere(float4x4 matrix) => Matrix = matrix;

			public Sphere(float3 origin) => Matrix = float4x4.Translate(origin);

			public Sphere(float3 origin, float radius)
				=> Matrix = math.mul(float4x4.Translate(origin), float4x4.Scale(new float3(radius, radius, radius)));

			public Sphere(float3 origin, quaternion rotation, float radius)
				=> Matrix = float4x4.TRS(origin, rotation, new float3(radius, radius, radius));

			public Sphere(Transform transform, float radius) : this(transform.position, transform.rotation, radius) { }

			public Sphere(Transform transform) => Matrix = transform.localToWorldMatrix;

#if VERTX_PHYSICS
			public Sphere(SphereCollider sphereCollider)
			{
				Transform transform = sphereCollider.transform;
				float3 scale = transform.lossyScale;
				float radiusScaled = math.max(math.abs(scale.x), math.max(math.abs(scale.y), math.abs(scale.z))) * sphereCollider.radius;
				Matrix = float4x4.TRS(transform.TransformPoint(sphereCollider.center), transform.rotation, new float3(radiusScaled, radiusScaled, radiusScaled));
			}
#endif

			public Sphere GetTranslated(float3 translation) => new Sphere(math.mul(float4x4.Translate(translation), Matrix));

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				var coreArc = new Arc(Matrix);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateY(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateX(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
			}

			public void Draw(CommandBuilder commandBuilder, Color color, float duration, Axes visibleAxes)
			{
				var coreArc = new Arc(Matrix);
				if ((visibleAxes & Axes.Y) != 0)
					commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				if ((visibleAxes & Axes.X) != 0)
					commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateY(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				if ((visibleAxes & Axes.Z) != 0)
					commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateX(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
			}
#endif
		}

		public readonly struct Hemisphere : IDrawable
		{
			public readonly float3 Origin;
			public readonly quaternion Orientation;
			public readonly float Radius;

			public Hemisphere(float3 origin, quaternion orientation, float radius)
			{
				Origin = origin;
				Orientation = orientation;
				Radius = radius;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float3 direction = math.mul(Orientation, math.forward());

				// Cap ----
				float3 perpendicular = math.mul(Orientation, math.up());
				float3 perpendicular2 = math.mul(Orientation, math.right());
				Draw(commandBuilder, Origin, direction, perpendicular, perpendicular2, Radius, color, duration);
			}

			public static void Draw(CommandBuilder commandBuilder, float3 origin, float3 direction, float3 tangent, float3 bitangent, float radius, Color color, float duration)
			{
				// Cap ----
				float3 endPos = origin;
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
			public readonly float4x4 Matrix;
			public readonly bool Shade3D;

			internal Box(float4x4 matrix, bool shade3D = true)
			{
				Matrix = matrix;
				Shade3D = shade3D;
			}

			public Box(float3 position, float3 halfExtents, quaternion orientation, bool shade3D = true) : this(float4x4.TRS(position, orientation, halfExtents), shade3D) { }

			public Box(float3 position, float3 halfExtents, bool shade3D = true) : this(float4x4.TRS(position, quaternion.identity, halfExtents), shade3D) { }

			public Box(Transform transform, bool shade3D = true)
			{
				Shade3D = shade3D;
				Matrix = transform.localToWorldMatrix;
			}

			public Box(Bounds bounds, bool shade3D = true) : this(bounds.center, bounds.extents, quaternion.identity, shade3D) { }

			public Box(BoundsInt bounds, bool shade3D = true) : this(bounds.center, bounds.size.xyz() * 2f, quaternion.identity, shade3D) { }

#if VERTX_PHYSICS
			public Box(BoxCollider boxCollider)
			{
				Shade3D = true;
				Transform transform = boxCollider.transform;
				Matrix = float4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, (float3)(boxCollider.size * 0.5f) * transform.lossyScale);
			}
#endif

			public Box GetTranslated(float3 translation) => new Box(math.mul(float4x4.Translate(translation), Matrix));

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendBox(this, color, duration, Shade3D ? DrawModifications.NormalFade : DrawModifications.None);
#endif
		}

		public readonly struct Capsule : IDrawable
		{
			public readonly float3 SpherePosition1, SpherePosition2;
			public readonly float Radius;

			public Capsule(float3 center, quaternion rotation, float height, float radius)
			{
				float pointHeight = height * 0.5f - radius;
				float3 direction = math.mul(rotation, new float3(0, pointHeight, 0));
				SpherePosition1 = center + direction;
				SpherePosition2 = center - direction;
				Radius = radius;
			}

			public Capsule(float3 spherePosition1, float3 spherePosition2, float radius)
			{
				SpherePosition1 = spherePosition1;
				SpherePosition2 = spherePosition2;
				Radius = radius;
			}

			public Capsule(float3 lowestPosition, float3 direction, float height, float radius)
			{
				SpherePosition1 = lowestPosition + direction * radius;
				SpherePosition2 = SpherePosition1 + direction * (height - radius * 2);
				Radius = radius;
			}

#if VERTX_PHYSICS
			public Capsule(CharacterController characterController)
			{
				Transform transform = characterController.transform;
				float3 scale = transform.lossyScale;
				float radiusScale = math.max(math.abs(scale.x), math.abs(scale.z));
				Radius = characterController.radius * radiusScale;
				float offset = math.max(characterController.height * 0.5f * math.abs(scale.y), Radius) - Radius;
				float3 center = transform.TransformPoint(characterController.center);
				SpherePosition1 = new float3(center.x, center.y - offset, center.z);
				SpherePosition2 = new float3(center.x, center.y + offset, center.z);
			}

			public Capsule(CapsuleCollider collider)
			{
				Transform transform = collider.transform;
				float3 scale = transform.lossyScale;
				float radiusScale = math.max(math.abs(scale.x), math.abs(scale.z));
				Radius = collider.radius * radiusScale;
				float3 center = transform.TransformPoint(collider.center);
				float offsetY = math.max(collider.height * 0.5f * math.abs(scale.y), Radius) - Radius;
				float3 offset = transform.TransformDirection(new float3(0, offsetY, 0));
				SpherePosition1 = center - offset;
				SpherePosition2 = center + offset;
			}
#endif

			public Capsule GetTranslated(float3 translation) => new Capsule(SpherePosition1 + translation, SpherePosition2 + translation, Radius);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float3 up = SpherePosition2 - SpherePosition1;
				up.EnsureNormalized(out float length);
				if (length == 0)
				{
					new Sphere(SpherePosition1, Radius).Draw(commandBuilder, color, duration);
					return;
				}

				float3 down = -up;

				float3 perpendicular = GetValidPerpendicular(up);
				float3 perpendicular2 = math.cross(up, perpendicular);
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
			public readonly float3 Center;
			public readonly quaternion Rotation;
			public readonly float HalfHeight;
			public readonly float Radius;

			public Cylinder(float3 center, quaternion rotation, float height, float radius)
			{
				Center = center;
				Rotation = rotation;
				HalfHeight = height * 0.5f;
				Radius = radius;
			}

			public Cylinder(float3 point1, float3 point2, float radius)
			{
				float3 up = point2 - point1;
				up.EnsureNormalized(out HalfHeight);
				HalfHeight *= 0.5f;
				Center = point1 + up * HalfHeight;
				Radius = radius;
				if (HalfHeight == 0)
					Rotation = quaternion.identity;
				else
				{
					float3 perpendicular = GetValidPerpendicular(up);
					Rotation = quaternion.LookRotation(perpendicular, up);
				}
			}

			public Cylinder(float3 lowestPosition, float3 direction, float height, float radius)
			{
				direction.EnsureNormalized();
				HalfHeight = height * 0.5f;
				Center = lowestPosition + direction * HalfHeight;
				Radius = radius;
				if (HalfHeight == 0)
					Rotation = quaternion.identity;
				else
				{
					float3 perpendicular = GetValidPerpendicular(direction);
					Rotation = quaternion.LookRotation(perpendicular, direction);
				}
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				quaternion circleRotation = quaternion.LookRotation(math.mul(Rotation, math.up()), math.mul(Rotation, math.right()));
				if (HalfHeight == 0)
				{
					new Circle(Center, circleRotation, Radius).Draw(commandBuilder, color, duration);
					return;
				}

				float3 up = math.mul(Rotation, new float3(0, HalfHeight, 0));
				float3 point1 = Center + up;
				float3 point2 = Center - up;

				if (Radius == 0)
				{
					commandBuilder.AppendLine(new Line(point1, point2), color, duration);
					return;
				}

				float3 perpendicular = math.mul(Rotation, new float3(Radius, 0, 0));
				float3 perpendicular2 = math.mul(Rotation, new float3(0, 0, Radius));


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
			public readonly float3 PointOnPlane;
			public readonly float2 DisplaySize;

			/// <summary>
			/// Constructs a Plane
			/// </summary>
			/// <param name="plane">The backing plane</param>
			/// <param name="pointOnPlane">A point to center the debug visual on</param>
			/// <param name="displaySize">The size of the debugging plane</param>
			public Plane(UnityEngine.Plane plane, float3 pointOnPlane, float2 displaySize)
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
			public Plane(UnityEngine.Plane plane, float3 pointOnPlane)
			{
				Value = plane;
				PointOnPlane = plane.ClosestPointOnPlane(pointOnPlane);
				DisplaySize = new float2(2, 2);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float3 normal = Value.normal;
				float3 perpendicular = GetValidPerpendicular(normal);
				quaternion rotation = quaternion.LookRotation(normal, perpendicular);
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
			public readonly float3 A, B, C;

			public Outline(float3 a, float3 b, float radius)
			{
				A = a;
				B = b;
				C = new float3(radius, 0, 0);
			}

			public Outline(float3 a, float3 b, float3 c)
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
			public readonly float4x4 Matrix;
			public readonly float3 Vector;

			public Cast(float4x4 matrix, float3 vector)
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