using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		/// <summary>
		/// A simple line between two points.
		/// </summary>
		public readonly struct Line : IDrawable
		{
			public readonly float3 A, B;

			public Line(float3 a, float3 b)
			{
				A = a;
				B = b;
			}

			public Line(Vector3 a, Vector3 b) : this((float3)a, (float3)b)
			{
			}

			public Line(Ray ray)
			{
				A = ray.Origin;
				B = ray.Origin + ray.Direction;
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendLine(this, color, duration);
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

		/// <summary>
		/// A <see cref="Line"/> displayed as dashed.
		/// </summary>
		public readonly struct DashedLine : IDrawable
		{
			public readonly Line Line;

			public DashedLine(Line line) => Line = line;

			public DashedLine(float3 a, float3 b) : this(new Line(a, b))
			{
			}

			public DashedLine(Vector3 a, Vector3 b) : this((float3)a, (float3)b)
			{
			}

			public DashedLine(Ray ray) : this(new Line(ray))
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendDashedLine(this, color, duration);
#endif

			public DashedLine GetShortened(float shortenBy, float minShorteningNormalised = 0) => new(Line.GetShortened(shortenBy, minShorteningNormalised));
		}

		/// <summary>
		/// A line strip made of many points.
		/// </summary>
		public readonly struct LineStrip : IDrawable
		{
			public readonly IEnumerable<float3> Points;

			public LineStrip(IEnumerable<float3> points) => Points = points;

			public LineStrip(IEnumerable<Vector3> points) => Points = points.Select(v => (float3)v);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
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

		/// <summary>
		/// Equivalent of <see cref="UnityEngine.Ray"/>, but <see cref="Direction"/> is not normalized.
		/// </summary>
		public readonly struct Ray : IDrawable
		{
			public readonly float3 Origin, Direction;

			public Ray(float3 origin, float3 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Ray(Vector3 origin, Vector3 direction) : this((float3)origin, (float3)direction)
			{
			}

			public Ray(float3 origin, float3 direction, float distance)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction * GetClampedMaxDistance(distance);
			}

			public Ray(Vector3 origin, Vector3 direction, float distance) : this((float3)origin, (float3)direction, distance)
			{
			}

			public Ray(UnityEngine.Ray ray, float distance = math.INFINITY) : this(ray.origin, ray.direction * GetClampedMaxDistance(distance))
			{
			}

			public Ray(UnityEngine.Ray2D ray, float distance = math.INFINITY) : this(ray.origin.xy0(), (ray.direction * GetClampedMaxDistance(distance)).xy0())
			{
			}

			public static implicit operator Ray(UnityEngine.Ray ray) => new(ray.origin, ray.direction);

#if VERTX_PHYSICS
			public Ray(RaycastHit hit)
			{
				Origin = hit.point;
				Direction = hit.normal;
			}
#endif

#if VERTX_PHYSICS_2D
			public Ray(RaycastHit2D hit)
			{
				Origin = new float3(hit.point.x, hit.point.y, hit ? hit.transform.position.z : 0);
				Direction = new float3(hit.normal.x, hit.normal.y, 0);
			}
#endif

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendRay(this, color, duration);
#endif
		}

		/// <summary>
		/// A point drawn as the intersection of 3 <see cref="Line"/>s.
		/// </summary>
		public readonly struct Point : IDrawable
		{
			public readonly float3 Position;
			public readonly float Scale;

			public Point(float3 position, float scale = 0.3f)
			{
				Position = position;
				Scale = scale;
			}

			public Point(Vector3 position, float scale = 0.3f) : this((float3)position, scale)
			{
			}

			public Point(float2 position, float scale = 0.3f) : this(new float3(position.x, position.y, 0), scale)
			{
			}

			public Point(Vector2 position, float scale = 0.3f) : this(new float3(position.x, position.y, 0), scale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float distance = Scale * 0.5f;
				commandBuilder.AppendLine(new Line(new float3(Position.x - distance, Position.y, Position.z), new float3(Position.x + distance, Position.y, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new float3(Position.x, Position.y - distance, Position.z), new float3(Position.x, Position.y + distance, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new float3(Position.x, Position.y, Position.z - distance), new float3(Position.x, Position.y, Position.z + distance)), color, duration);
			}
#endif
		}

		/// <summary>
		/// A plain arrow; a ray with an arrowhead.
		/// </summary>
		public readonly struct Arrow : IDrawable
		{
			public readonly float3 Origin, Direction;
			public readonly float ArrowheadScale;

			public Arrow(float3 origin, float3 direction, float arrowheadScale = 1)
			{
				Origin = origin;
				Direction = direction;
				ArrowheadScale = arrowheadScale;
			}

			public Arrow(Vector3 origin, Vector3 direction, float arrowheadScale = 1) : this((float3)origin, (float3)direction, arrowheadScale)
			{
			}

			public Arrow(float3 origin, quaternion rotation, float length = 1, float arrowheadScale = 1) : this(origin, math.mul(rotation, math.forward()) * length, arrowheadScale)
			{
			}

			public Arrow(Vector3 origin, Quaternion rotation, float length = 1, float arrowheadScale = 1) : this((float3)origin, (quaternion)rotation, length, arrowheadScale)
			{
			}

			public Arrow(Line line, float arrowheadScale = 1) : this(line.A, line.B - line.A, arrowheadScale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				DrawArrowHead(ref commandBuilder, Origin, Direction, ArrowheadScale, color, duration);
			}

			internal const float HeadLength = 0.075f;
			internal const float HeadWidth = 0.05f;

			internal static void DrawArrowHead(ref UnmanagedCommandBuilder commandBuilder, float3 point, float3 dir, float scale, Color color, float duration = 0)
			{
				const int segments = 3;

				float3 arrowPoint = point + dir;
				dir.EnsureNormalized(out float length);

				float headLength = HeadLength * length * scale;
				float headWidth = HeadWidth * length * scale;
				DoDrawArrowHead(arrowPoint - dir * headLength, dir, headWidth, ref commandBuilder);
				return;

				void DoDrawArrowHead(float3 center, float3 normal, float radius, ref UnmanagedCommandBuilder commandBuilder)
				{
					const float max = math.TAU;
					float3 tangent = GetValidPerpendicular(normal);
					float3 bitangent = math.cross(normal, tangent);
					tangent *= radius;
					bitangent *= radius;
					float3 lastPos = center + tangent;
					for (var i = 1; i <= segments; i++)
					{
						float angle = i / (float)segments * max;
						float2 c = new(math.cos(angle), math.sin(angle));
						float3 nextPos = center + tangent * c.x + bitangent * c.y;
						commandBuilder.AppendLine(new Line(lastPos, nextPos), color, duration);
						commandBuilder.AppendLine(new Line(lastPos, arrowPoint), color, duration);
						lastPos = nextPos;
					}
				}
			}
			
			internal static void DrawHalfArrowHead(ref UnmanagedCommandBuilder commandBuilder, in Line line, float3 perpendicular, float scale, Color color, float duration = 0)
			{
				float3 end = line.B;
				float3 dir = line.A - line.B;

				dir.EnsureNormalized(out float length);
				float headLength = HeadLength * length * scale;
				float headWidth = HeadWidth * length * scale;
				float3 cross = math.cross(dir, perpendicular);
				float3 a = end + dir * headLength;
				float3 b = a + cross * headWidth;
				commandBuilder.AppendLine(new Line(end, b), color, duration);
				commandBuilder.AppendLine(new Line(a, b), color, duration);
			}
#endif
		}

		/// <summary>
		/// A line made of many points, the last of which is a <see cref="Arrow"/>.
		/// </summary>
		public readonly struct ArrowStrip : IDrawable
		{
			public readonly IEnumerable<float3> Points;
			public readonly float ArrowheadScale;

			public ArrowStrip(IEnumerable<float3> points, float arrowheadScale = 1)
			{
				Points = points;
				ArrowheadScale = arrowheadScale;
			}

			public ArrowStrip(IEnumerable<Vector3> points, float arrowheadScale = 1) : this(points.Select(v => (float3)v), arrowheadScale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
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
				Arrow.DrawArrowHead(ref commandBuilder, origin.Value, direction, ArrowheadScale, color, duration);
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
			public readonly float ArrowheadScale;

			public HalfArrow(Line line, float3 perpendicular, float arrowheadScale = 1)
			{
				Line = line;
				perpendicular.EnsureNormalized();
				Perpendicular = perpendicular;
				ArrowheadScale = arrowheadScale;
			}

			public HalfArrow(Line line, Vector3 perpendicular, float arrowheadScale = 1) : this(line, (float3)perpendicular, arrowheadScale)
			{
			}

			public HalfArrow(float3 origin, float3 direction, float3 perpendicular, float arrowheadScale = 1) : this(new Line(origin, origin + direction), perpendicular, arrowheadScale)
			{
			}

			public HalfArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular, float arrowheadScale = 1) : this((float3)origin, (float3)direction, (float3)perpendicular, arrowheadScale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendLine(Line, color, duration);
				Arrow.DrawHalfArrowHead(ref commandBuilder, Line, Perpendicular, ArrowheadScale, color, duration);
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

			public CurvedArrow(float3 origin, float3 direction, float3 perpendicular) : this(origin, direction, perpendicular, Angle.FromTurns(0.1f))
			{
			}

			public CurvedArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular) : this((float3)origin, (float3)direction, (float3)perpendicular)
			{
			}

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

			public CurvedArrow(Vector3 origin, Vector3 direction, Vector3 perpendicular, Angle angle) : this((float3)origin, (float3)direction, (float3)perpendicular, angle)
			{
			}

			public CurvedArrow(in Line line, float3 perpendicular) : this(line.A, line.B - line.A, perpendicular)
			{
			}

			public CurvedArrow(in Line line, Vector3 perpendicular) : this(line, (float3)perpendicular)
			{
			}

			public CurvedArrow(in Line line, float3 perpendicular, Angle angle) : this(line.A, line.B - line.A, perpendicular, angle)
			{
			}

			public CurvedArrow(in Line line, Vector3 perpendicular, Angle angle) : this(line, (float3)perpendicular, angle)
			{
			}

#if UNITY_EDITOR
			private const float ArrowHeadAngle = 30f * math.TORADIANS;
			private static readonly quaternion s_arrowheadRotation = quaternion.AxisAngle(math.up(), -ArrowHeadAngle * 2);

			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
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

				quaternion arrowheadRotation = math.mul(quaternion.LookRotation(cross, Perpendicular), quaternion.AxisAngle(math.up(), Angle.Radians * 0.5f - math.PI * 0.5f + ArrowHeadAngle));
				var arrowRay = new float3(0, 0, headLength);
				commandBuilder.AppendLine(new Line(end, end + math.mul(arrowheadRotation, arrowRay)), color, duration);
				commandBuilder.AppendLine(new Line(end, end + math.mul(math.mul(arrowheadRotation, s_arrowheadRotation), arrowRay)), color, duration);
			}
#endif
		}

		/// <summary>
		/// An <see cref="Arrow2D"/> but can be oriented in 3D space with the accompanying normal vector.
		/// </summary>
		public readonly struct Arrow2DFromNormal : IDrawable
		{
			public readonly float3 Origin, Direction, Normal;

			public Arrow2DFromNormal(float3 origin, float3 direction, float3 normal)
			{
				Origin = origin;
				Direction = direction;
				Normal = normal;
				Normal.EnsureNormalized();
			}

			public Arrow2DFromNormal(Vector3 origin, Vector3 direction, Vector3 normal) : this((float3)origin, (float3)direction, (float3)normal)
			{
			}

			public Arrow2DFromNormal(float3 origin, quaternion rotation, float length = 1)
			{
				Origin = origin;
				Direction = math.mul(rotation, math.forward()) * length;
				Normal = math.mul(rotation, math.right());
				Normal.EnsureNormalized();
			}

			public Arrow2DFromNormal(Vector3 origin, Quaternion rotation, float length = 1) : this((float3)origin, (quaternion)rotation, length)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
				Arrow2D.DrawArrowHead(ref commandBuilder, Origin + Direction, math.normalizesafe(Direction), Normal, color, duration);
			}
#endif
		}

		/// <summary>
		/// Rays in XYZ with optional axis visibility and arrow heads.
		/// </summary>
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

			public Axis(Vector3 origin, Quaternion rotation, bool showArrowHeads = true, Axes visibleAxes = Axes.All, float scale = 1) : this((float3)origin, (quaternion)rotation, showArrowHeads, visibleAxes, scale)
			{
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
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (ShowArrowHeads)
				{
					if ((VisibleAxes & Axes.X) != 0)
						new Arrow(Origin, math.mul(Rotation, new float3(Scale, 0, 0))).Draw(ref commandBuilder, XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						new Arrow(Origin, math.mul(Rotation, new float3(0, Scale, 0))).Draw(ref commandBuilder, YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						new Arrow(Origin, math.mul(Rotation, new float3(0, 0, Scale))).Draw(ref commandBuilder, ZColor, duration);
				}
				else
				{
					if ((VisibleAxes & Axes.X) != 0)
						commandBuilder.AppendRay(new Ray(Origin, math.mul(Rotation, new float3(Scale, 0, 0))), XColor, duration);
					if ((VisibleAxes & Axes.Y) != 0)
						commandBuilder.AppendRay(new Ray(Origin, math.mul(Rotation, new float3(0, Scale, 0))), YColor, duration);
					if ((VisibleAxes & Axes.Z) != 0)
						commandBuilder.AppendRay(new Ray(Origin, math.mul(Rotation, new float3(0, 0, Scale))), ZColor, duration);
				}
			}
#endif
		}

		/// <summary>
		/// A ray with a circle drawn around the origin.
		/// </summary>
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

			public SurfacePoint(Vector3 origin, Vector3 direction) : this((float3)origin, (float3)direction)
			{
			}

			public SurfacePoint(float3 origin, float3 direction, float radius)
			{
				Origin = origin;
				EnsureNormalized(ref direction, out _);
				Direction = direction;
				Radius = radius;
			}

			public SurfacePoint(Vector3 origin, Vector3 direction, float radius) : this((float3)origin, (float3)direction, radius)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
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

			public Circle(Matrix4x4 matrix) : this((float4x4)matrix)
			{
			}

			/// <summary>
			/// A circle oriented along XY
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="rotation">The rotation of the circle, the circle will be oriented along XY</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(float3 origin, quaternion rotation, float radius)
				=> _arc = new Arc(float4x4.TRS(origin, rotation, new float3(radius, radius, radius)));

			public Circle(Vector3 origin, Quaternion rotation, float radius) : this((float3)origin, (quaternion)rotation, radius)
			{
			}

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
					math.mul(quaternion.LookRotation(math.abs(math.dot(direction, normal)) > 0.999f ? GetValidPerpendicular(normal) : direction, normal), Arc.Base3DRotation),
					radius
				)
			{
			}

			public Circle(Vector3 origin, Vector3 normal, Vector3 direction, float radius) : this((float3)origin, (float3)normal, (float3)direction, radius)
			{
			}

			/// <summary>
			/// It's cheaper to use the <see cref="Circle(float3, float3, float3, float)"/> constructor if you already have a perpendicular facing direction for the circle.<br/>
			/// If <see cref="normal"/> is zero, this will spam logs to the console. Please validate your own inputs if you expect them to be incorrect.
			/// </summary>
			/// <param name="origin">The center of the circle.</param>
			/// <param name="normal">The normal facing outwards from the circle.</param>
			/// <param name="radius">The radius of the circle.</param>
			public Circle(float3 origin, float3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius)
			{
			}

			public Circle(Vector3 origin, Vector3 normal, float radius) : this((float3)origin, (float3)normal, radius)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendArc(_arc, color, duration);
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

			internal static readonly quaternion Base3DRotation = quaternion.Euler(90 * math.TORADIANS, -90 * math.TORADIANS, 0);


			public Arc(float4x4 matrix, Angle angle)
			{
				Matrix = matrix;
				Angle = angle;
			}

			public Arc(Matrix4x4 matrix, Angle angle) : this((float4x4)matrix, angle)
			{
			}

			/// <summary>
			/// Creates an Arc
			/// </summary>
			internal Arc(float4x4 matrix) : this(matrix, Angle.FromTurns(1))
			{
			}

			public Arc(Matrix4x4 matrix) : this((float4x4)matrix)
			{
			}

			public Arc(float3 origin, quaternion rotation, float radius, Angle angle)
			{
				Angle = angle;
				Matrix = float4x4.TRS(origin, rotation, new float3(radius, radius, radius));
			}

			public Arc(Vector3 origin, Quaternion rotation, float radius, Angle angle) : this((float3)origin, (quaternion)rotation, radius, angle)
			{
			}

			public Arc(float3 origin, quaternion rotation, float radius) : this(origin, rotation, radius, Angle.FromTurns(1))
			{
			}

			public Arc(Vector3 origin, Quaternion rotation, float radius) : this((float3)origin, (quaternion)rotation, radius)
			{
			}

			public Arc(float2 origin, float rotationDegrees, float radius, Angle angle)
				// If NaN is introduced externally by the user this protects against that silently.
				: this(origin.xy0(), float.IsNaN(rotationDegrees) ? quaternion.identity : quaternion.AxisAngle(math.forward(), rotationDegrees * math.TORADIANS), radius, angle)
			{
			}

			public Arc(Vector2 origin, float rotationDegrees, float radius, Angle angle) : this((float2)origin, rotationDegrees, radius, angle)
			{
			}

			public Arc(float2 origin, float rotationDegrees, float radius) : this(origin, rotationDegrees, radius, Angle.FromTurns(1))
			{
			}

			public Arc(Vector2 origin, float rotationDegrees, float radius) : this((float2)origin, rotationDegrees, radius)
			{
			}

			public Arc(float3 origin, float3 normal, float3 direction, float radius, Angle angle)
				: this(
					origin,
					math.mul(quaternion.LookRotation(direction, normal), Base3DRotation),
					radius,
					angle
				)
			{
			}

			public Arc(Vector3 origin, Vector3 normal, Vector3 direction, float radius, Angle angle) : this((float3)origin, (float3)normal, (float3)direction, radius, angle)
			{
			}

			public Arc(float3 origin, float3 normal, float3 direction, float radius) : this(origin, normal, direction, radius, Angle.FromTurns(1))
			{
			}

			public Arc(Vector3 origin, Vector3 normal, Vector3 direction, float radius) : this((float3)origin, (float3)normal, (float3)direction, radius)
			{
			}

			/// <summary>
			/// It's cheaper to use the <see cref="Arc(float3, float3, float3, float)"/> constructor if you already have a perpendicular facing direction for the circle.
			/// </summary>
			public Arc(float3 origin, float3 normal, float radius) : this(origin, normal, GetValidPerpendicular(normal), radius, Angle.FromTurns(1))
			{
			}

			/// <summary>
			/// It's cheaper to use the <see cref="Arc(float3, float3, float3, float)"/> constructor if you already have a perpendicular facing direction for the circle.
			/// </summary>
			public Arc(Vector3 origin, Vector3 normal, float radius) : this((float3)origin, (float3)normal, radius)
			{
			}

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
					math.mul(quaternion.LookRotation(direction, normal), Base3DRotation),
					new float3(radius, radius, radius)
				);
				Angle = angle;
			}

			public Arc(Line chord, Vector3 aim, float arcLength) : this(chord, (float3)aim, arcLength)
			{
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
				var iterations = 0;
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
			public static float GetChordLength(in Angle angle, float radius) => 2f * radius * math.sin(angle.Radians * 0.5f);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
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
				: this(origin, rotation, innerRadius, outerRadius, Angle.FromTurns(1))
			{
			}

			public Annulus(Vector3 origin, Quaternion rotation, float innerRadius, float outerRadius) : this((float3)origin, (quaternion)rotation, innerRadius, outerRadius)
			{
			}

			/// <summary>
			/// Creates an annulus.
			/// </summary>
			public Annulus(float3 origin, float3 normal, float3 direction, float innerRadius, float outerRadius)
				: this(origin, normal, direction, innerRadius, outerRadius, Angle.FromTurns(1))
			{
			}

			public Annulus(Vector3 origin, Vector3 normal, Vector3 direction, float innerRadius, float outerRadius) : this((float3)origin, (float3)normal, (float3)direction, innerRadius, outerRadius)
			{
			}

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

			public Annulus(Vector3 origin, Quaternion rotation, float innerRadius, float outerRadius, Angle sectorWidth) : this((float3)origin, (quaternion)rotation, innerRadius, outerRadius, sectorWidth)
			{
			}

			/// <summary>
			/// Creates an annulus sector.
			/// </summary>
			public Annulus(float3 origin, float3 normal, float3 direction, float innerRadius, float outerRadius, Angle sectorWidth)
				: this(origin, math.mul(quaternion.LookRotation(direction, normal), Arc.Base3DRotation), innerRadius, outerRadius, sectorWidth)
			{
			}

			public Annulus(Vector3 origin, Vector3 normal, Vector3 direction, float innerRadius, float outerRadius, Angle sectorWidth) : this((float3)origin, (float3)normal, (float3)direction, innerRadius, outerRadius, sectorWidth)
			{
			}

			/// <summary>
			/// Uniform annulus sampling.
			/// </summary>
			public float3 RandomPoint() => Origin + math.mul(Rotation, RandomPoint(InnerRadius, OuterRadius, SectorWidth).xy0());

			/// <summary>
			/// Uniform annulus sampling.
			/// </summary>
			[PublicAPI]
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
			[PublicAPI]
			public static float2 RandomPointNonUniform(float innerRadius, float outerRadius, Angle sectorWidth)
			{
				float rad = sectorWidth.Radians * 0.5f,
					a = UnityEngine.Random.Range(-rad, rad),
					difference = outerRadius - innerRadius,
					r = UnityEngine.Random.value * difference + innerRadius,
					x = r * math.cos(a),
					y = r * math.sin(a);
				return new float2(x, y);
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				commandBuilder.AppendArc(new Arc(Origin, Rotation, InnerRadius, SectorWidth), color, duration);
				if (math.abs(InnerRadius - OuterRadius) < 0.0001f)
					return;
				commandBuilder.AppendArc(new Arc(Origin, Rotation, OuterRadius, SectorWidth), color, duration);
				if (math.abs(SectorWidth.Turns - 1) < 0.0001f)
					return;
				quaternion rightRot = quaternion.AxisAngle(math.forward(), SectorWidth.Radians * 0.5f);
				float3 rightInner = Origin + math.mul(Rotation, math.mul(rightRot, new float3(InnerRadius, 0, 0)));
				float3 rightOuter = Origin + math.mul(Rotation, math.mul(rightRot, new float3(OuterRadius, 0, 0)));
				commandBuilder.AppendLine(new Line(rightInner, rightOuter), color, duration);
				if (SectorWidth.Turns == 0)
					return;
				quaternion leftRot = math.inverse(rightRot);
				float3 leftInner = Origin + math.mul(Rotation, math.mul(leftRot, new float3(InnerRadius, 0, 0)));
				float3 leftOuter = Origin + math.mul(Rotation, math.mul(leftRot, new float3(OuterRadius, 0, 0)));
				commandBuilder.AppendLine(new Line(leftInner, leftOuter), color, duration);
			}
#endif
		}

		/// <summary>
		/// A wireframe sphere.
		/// </summary>
		public readonly struct Sphere : IDrawable
		{
			public readonly float4x4 Matrix;

			public Sphere(float4x4 matrix) => Matrix = matrix;

			public Sphere(Matrix4x4 matrix) : this((float4x4)matrix)
			{
			}

			public Sphere(float3 origin) => Matrix = float4x4.Translate(origin);

			public Sphere(Vector3 origin) : this((float3)origin)
			{
			}

			public Sphere(float3 origin, float radius)
				=> Matrix = math.mul(float4x4.Translate(origin), float4x4.Scale(new float3(radius, radius, radius)));

			public Sphere(Vector3 origin, float radius) : this((float3)origin, radius)
			{
			}

			public Sphere(float3 origin, quaternion rotation, float radius)
				=> Matrix = float4x4.TRS(origin, rotation, new float3(radius, radius, radius));

			public Sphere(Vector3 origin, Quaternion rotation, float radius) : this((float3)origin, (quaternion)rotation, radius)
			{
			}

			public Sphere(Transform transform, float radius) : this(transform.position, transform.rotation, radius)
			{
			}

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

			public Sphere GetTranslated(float3 translation) => new(math.mul(float4x4.Translate(translation), Matrix));

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				var coreArc = new Arc(Matrix);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateY(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(new Arc(math.mul(Matrix, float4x4.RotateX(90 * math.TORADIANS))), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendArc(coreArc, color, duration, DrawModifications.Custom);
			}

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration, Axes visibleAxes)
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

		/// <summary>
		/// A wireframe hemisphere.
		/// </summary>
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

			public Hemisphere(Vector3 origin, Quaternion orientation, float radius) : this((float3)origin, (quaternion)orientation, radius)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 direction = math.mul(Orientation, math.forward());

				// Cap ----
				float3 perpendicular = math.mul(Orientation, math.up());
				float3 perpendicular2 = math.mul(Orientation, math.right());
				Draw(ref commandBuilder, Origin, direction, perpendicular, perpendicular2, Radius, color, duration);
			}

			internal static void Draw(ref UnmanagedCommandBuilder commandBuilder, float3 origin, float3 direction, float3 tangent, float3 bitangent, float radius, Color color, float duration)
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

		/// <summary>
		/// A wireframe box.
		/// </summary>
		public readonly struct Box : IDrawable
		{
			public readonly float4x4 Matrix;
			public readonly bool Shade3D;

			public Box(float4x4 matrix, bool shade3D = true)
			{
				Matrix = matrix;
				Shade3D = shade3D;
			}

			public Box(Matrix4x4 matrix, bool shade3D = true) : this((float4x4)matrix, shade3D)
			{
			}

			public Box(float3 position, float3 halfExtents, quaternion orientation, bool shade3D = true) : this(float4x4.TRS(position, orientation, halfExtents), shade3D)
			{
			}

			public Box(Vector3 position, Vector3 halfExtents, Quaternion orientation, bool shade3D = true) : this((float3)position, (float3)halfExtents, (quaternion)orientation, shade3D)
			{
			}

			public Box(float3 position, float3 halfExtents, bool shade3D = true) : this(float4x4.TRS(position, quaternion.identity, halfExtents), shade3D)
			{
			}

			public Box(Vector3 position, Vector3 halfExtents, bool shade3D = true) : this((float3)position, (float3)halfExtents, shade3D)
			{
			}

			public Box(Transform transform, bool shade3D = true)
			{
				Shade3D = shade3D;
				Matrix = transform.localToWorldMatrix;
			}

			public Box(Bounds bounds, bool shade3D = true) : this((float3)bounds.center, (float3)bounds.extents, quaternion.identity, shade3D)
			{
			}

			public Box(BoundsInt bounds, bool shade3D = true) : this((float3)bounds.center, bounds.size.xyz() * 2f, quaternion.identity, shade3D)
			{
			}

#if VERTX_PHYSICS
			public Box(BoxCollider boxCollider)
			{
				Shade3D = true;
				Transform transform = boxCollider.transform;
				Matrix = float4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, (float3)(boxCollider.size * 0.5f) * transform.lossyScale);
			}
#endif

			public Box GetTranslated(float3 translation) => new(math.mul(float4x4.Translate(translation), Matrix));

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendBox(this, color, duration, Shade3D ? DrawModifications.NormalFade : DrawModifications.None);
#endif
		}

		/// <summary>
		/// A wireframe capsule.
		/// </summary>
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

			public Capsule(Vector3 center, Quaternion rotation, float height, float radius) : this((float3)center, (quaternion)rotation, height, radius)
			{
			}

			public Capsule(float3 spherePosition1, float3 spherePosition2, float radius)
			{
				SpherePosition1 = spherePosition1;
				SpherePosition2 = spherePosition2;
				Radius = radius;
			}

			public Capsule(Vector3 spherePosition1, Vector3 spherePosition2, float radius) : this((float3)spherePosition1, (float3)spherePosition2, radius)
			{
			}

			public Capsule(float3 lowestPosition, float3 direction, float height, float radius)
			{
				SpherePosition1 = lowestPosition + direction * radius;
				SpherePosition2 = SpherePosition1 + direction * (height - radius * 2);
				Radius = radius;
			}

			public Capsule(Vector3 lowestPosition, Vector3 direction, float height, float radius) : this((float3)lowestPosition, (float3)direction, height, radius)
			{
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
				float offsetScalar = math.max(collider.height * 0.5f * math.abs(scale.y), Radius) - Radius;
				float3 offset;
				// ReSharper disable once ConvertSwitchStatementToSwitchExpression
				switch (collider.direction)
				{
					case 0:
						offset = transform.TransformDirection(new float3(offsetScalar, 0, 0));
						break;
					case 2:
						offset = transform.TransformDirection(new float3(0, 0, offsetScalar));
						break;
					default:
						offset = transform.TransformDirection(new float3(0, offsetScalar, 0));
						break;
				}

				SpherePosition1 = center - offset;
				SpherePosition2 = center + offset;
			}
#endif

			public Capsule GetTranslated(float3 translation) => new(SpherePosition1 + translation, SpherePosition2 + translation, Radius);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 up = SpherePosition2 - SpherePosition1;
				up.EnsureNormalized(out float length);
				if (length == 0)
				{
					new Sphere(SpherePosition1, Radius).Draw(ref commandBuilder, color, duration);
					return;
				}

				float3 down = -up;

				float3 perpendicular = GetValidPerpendicular(up);
				float3 perpendicular2 = math.cross(up, perpendicular);
				Hemisphere.Draw(ref commandBuilder, SpherePosition2, up, perpendicular, perpendicular2, Radius, color, duration);
				Hemisphere.Draw(ref commandBuilder, SpherePosition1, down, perpendicular, perpendicular2, Radius, color, duration);

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
		/// A wireframe cylinder.
		/// </summary>
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

			public Cylinder(Vector3 center, Quaternion rotation, float height, float radius) : this((float3)center, (quaternion)rotation, height, radius)
			{
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

			public Cylinder(Vector3 point1, Vector3 point2, float radius) : this((float3)point1, (float3)point2, radius)
			{
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

			public Cylinder(Vector3 lowestPosition, Vector3 direction, float height, float radius) : this((float3)lowestPosition, (float3)direction, height, radius)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				quaternion circleRotation = quaternion.LookRotation(math.mul(Rotation, math.up()), math.mul(Rotation, math.right()));
				if (HalfHeight == 0)
				{
					new Circle(Center, circleRotation, Radius).Draw(ref commandBuilder, color, duration);
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


				new Circle(point1, circleRotation, Radius).Draw(ref commandBuilder, color, duration);
				new Circle(point2, circleRotation, Radius).Draw(ref commandBuilder, color, duration);

				commandBuilder.AppendOutline(new Outline(point1 + perpendicular, point2 + perpendicular, perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 - perpendicular, point2 - perpendicular, -perpendicular), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 + perpendicular2, point2 + perpendicular2, perpendicular2), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(point1 - perpendicular2, point2 - perpendicular2, -perpendicular2), color, duration, DrawModifications.NormalFade);

				commandBuilder.AppendOutline(new Outline(point1, point2, Radius), color, duration);
				commandBuilder.AppendOutline(new Outline(point2, point1, Radius), color, duration);
			}
#endif
		}

		/// <summary>
		/// <see cref="Cone"/> faces in the Z direction, with its point towards Z+.
		/// </summary>
		public readonly struct Cone : IDrawable
		{
			public readonly float3 PointBase;
			public readonly quaternion Rotation;
			public readonly float Height;
			public readonly float RadiusBase;

			/// <summary>
			/// If <see cref="RadiusTip"/> is not 0, this may be a conical frustum, not a cone ;)
			/// </summary>
			public readonly float RadiusTip;

			public Cone(float3 pointBase, float3 pointTip, float radiusBase, float radiusTip = 0)
			{
				PointBase = pointBase;
				RadiusBase = radiusBase;
				RadiusTip = radiusTip;
				float3 up = pointTip - PointBase;
				up.EnsureNormalized(out Height);
				if (Height == 0)
					Rotation = quaternion.identity;
				else
				{
					float3 perpendicular = GetValidPerpendicular(up);
					Rotation = quaternion.LookRotation(up, perpendicular);
				}
			}

			public Cone(Vector3 pointBase, Vector3 pointTip, float radiusBase, float radiusTip = 0) : this((float3)pointBase, (float3)pointTip, radiusBase, radiusTip)
			{
			}

			public Cone(float3 pointBase, float3 direction, float height, float radiusBase, float radiusTip = 0)
			{
				PointBase = pointBase;
				RadiusBase = radiusBase;
				RadiusTip = radiusTip;
				Height = height;
				if (Height == 0)
					Rotation = quaternion.identity;
				else
				{
					float3 perpendicular = GetValidPerpendicular(direction);
					Rotation = quaternion.LookRotation(direction, perpendicular);
				}
			}

			public Cone(Vector3 pointBase, Vector3 direction, float height, float radiusBase, float radiusTip = 0) : this((float3)pointBase, (float3)direction, height, radiusBase, radiusTip)
			{
			}

			public Cone(float3 pointBase, quaternion rotation, float height, float radiusBase, float radiusTip = 0)
			{
				PointBase = pointBase;
				RadiusBase = radiusBase;
				RadiusTip = radiusTip;
				Height = height;
				Rotation = rotation;
			}

			public Cone(Vector3 pointBase, Quaternion rotation, float height, float radiusBase, float radiusTip = 0) : this((float3)pointBase, (quaternion)rotation, height, radiusBase, radiusTip)
			{
			}

			public Cone Flip() => new(PointBase + math.mul(Rotation, new float3(0, 0, Height)), math.mul(Rotation, quaternion.AxisAngle(math.up(), 180)), Height, RadiusBase, RadiusTip);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 pointTip = PointBase + math.mul(Rotation, new float3(0, 0, Height));

				if (RadiusBase == 0 && RadiusTip == 0)
				{
					if (Height != 0)
						commandBuilder.AppendLine(new Line(PointBase, pointTip), color, duration);
					return;
				}

				if (RadiusBase != 0)
					new Circle(PointBase, Rotation, RadiusBase).Draw(ref commandBuilder, color, duration);
				if (RadiusTip != 0)
					new Circle(pointTip, Rotation, RadiusTip).Draw(ref commandBuilder, color, duration);

				if (Height == 0)
					return;


				float3 perpendicularX = math.mul(Rotation, new float3(RadiusBase, 0, 0));
				float3 perpendicularY = math.mul(Rotation, new float3(0, RadiusBase, 0));
				float3 perpendicularXTip = math.mul(Rotation, new float3(RadiusTip, 0, 0));
				float3 perpendicularYTip = math.mul(Rotation, new float3(0, RadiusTip, 0));


				float3 normalX = math.mul(Rotation, math.normalize(new float3(Height, 0, RadiusBase - RadiusTip)));
				float3 normalXNegated = math.mul(Rotation, math.normalize(new float3(-Height, 0, RadiusBase - RadiusTip)));
				float3 normalY = math.mul(Rotation, math.normalize(new float3(0, Height, RadiusBase - RadiusTip)));
				float3 normalYNegated = math.mul(Rotation, math.normalize(new float3(0, -Height, RadiusBase - RadiusTip)));

				commandBuilder.AppendOutline(new Outline(PointBase + perpendicularX, pointTip + perpendicularXTip, normalX), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(PointBase - perpendicularX, pointTip - perpendicularXTip, normalXNegated), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(PointBase + perpendicularY, pointTip + perpendicularYTip, normalY), color, duration, DrawModifications.NormalFade);
				commandBuilder.AppendOutline(new Outline(PointBase - perpendicularY, pointTip - perpendicularYTip, normalYNegated), color, duration, DrawModifications.NormalFade);

				// Someone figure this one out lol
				// commandBuilder.AppendOutline(new Outline(point1, point2, _radiusTip, RadiusBase), color, duration, DrawModifications.Custom3);
				// commandBuilder.AppendOutline(new Outline(point2, point1, RadiusBase, _radiusTip), color, duration, DrawModifications.Custom3);
			}
#endif
		}

		/// <summary>
		/// A frustum similar to that of <see cref="Camera.projectionMatrix"/>.<br/>
		/// For a conical frustum see <see cref="Cone"/>.<br/>
		/// For a field of view more suitable for AI see <see cref="FieldOfView"/>.
		/// </summary>
		public readonly struct Frustum : IDrawable
		{
			public readonly float4x4 Matrix;

			public Frustum(float4x4 matrix) => Matrix = matrix;

			public Frustum(Matrix4x4 matrix) : this((float4x4)matrix)
			{
			}

			public Frustum(Camera camera, Camera.MonoOrStereoscopicEye eye = Camera.MonoOrStereoscopicEye.Mono)
			{
				Matrix4x4 projectionMatrix;
				switch (eye)
				{
					case Camera.MonoOrStereoscopicEye.Left:
						projectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
						break;
					case Camera.MonoOrStereoscopicEye.Right:
						projectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
						break;
					case Camera.MonoOrStereoscopicEye.Mono:
						projectionMatrix = camera.projectionMatrix;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(eye), eye, null);
				}

				Matrix = camera.cameraToWorldMatrix * projectionMatrix.inverse;
			}

			public Frustum(
				float3 position,
				quaternion rotation,
				float fieldOfViewDegrees,
				float aspect,
				float nearClipPlane,
				float farClipPlane
			)
			{
				float4x4 cameraMatrix = float4x4.TRS(position, rotation, new float3(1));
				for (var i = 0; i < 3; i++)
					cameraMatrix.c2[i] = -cameraMatrix.c2[i];
				Matrix = math.mul(cameraMatrix, math.inverse(float4x4.PerspectiveFov(math.radians(fieldOfViewDegrees), aspect, nearClipPlane, farClipPlane)));
			}

			public Frustum(
				Vector3 position,
				Quaternion rotation,
				float fieldOfViewDegrees,
				float aspect,
				float nearClipPlane,
				float farClipPlane
			) : this((float3)position, (quaternion)rotation, fieldOfViewDegrees, aspect, nearClipPlane, farClipPlane)
			{
			}

			public Frustum(
				float3 position,
				quaternion rotation,
				Angle fieldOfView,
				float aspect,
				float nearClipPlane,
				float farClipPlane
			)
			{
				float4x4 cameraMatrix = float4x4.TRS(position, rotation, new float3(1));
				for (var i = 0; i < 3; i++)
					cameraMatrix.c2[i] = -cameraMatrix.c2[i];
				Matrix = math.mul(cameraMatrix, math.inverse(float4x4.PerspectiveFov(fieldOfView.Radians, aspect, nearClipPlane, farClipPlane)));
			}

			public Frustum(
				Vector3 position,
				Quaternion rotation,
				Angle fieldOfView,
				float aspect,
				float nearClipPlane,
				float farClipPlane
			) : this((float3)position, (quaternion)rotation, fieldOfView, aspect, nearClipPlane, farClipPlane)
			{
			}


#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendBox(new Box(Matrix), color, duration);
#endif
		}

		/// <summary>
		/// A curved field of view similar to a <see cref="Frustum"/>, but with a constant distance to the origin.
		/// </summary>
		public readonly struct FieldOfView : IDrawable
		{
			public readonly float3 Position;
			public readonly quaternion Rotation;
			public readonly Angle HorizontalAngle;
			public readonly Angle VerticalAngle;
			public readonly float Distance;

			public FieldOfView(
				float3 position,
				quaternion rotation,
				Angle horizontalAngle,
				Angle verticalAngle,
				float distance
			)
			{
				Position = position;
				Rotation = rotation;
				HorizontalAngle = horizontalAngle;
				VerticalAngle = verticalAngle;
				Distance = distance;
			}

			public FieldOfView(
				Vector3 position,
				Quaternion rotation,
				Angle horizontalAngle,
				Angle verticalAngle,
				float distance
			) : this((float3)position, (quaternion)rotation, horizontalAngle, verticalAngle, distance)
			{
			}

			public static Angle VerticalFieldOfViewWithAspectToHorizontalFieldOfView(Angle verticalFieldOfView, float aspect) => Angle.FromRadians(2 * math.atan(math.tan(verticalFieldOfView.Radians * 0.5f) * aspect));

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				quaternion arcRotationV = quaternion.AxisAngle(math.up(), -math.PIHALF);
				quaternion arcRotationH = math.mul(quaternion.AxisAngle(math.up(), -math.PIHALF), quaternion.AxisAngle(math.right(), math.PIHALF));

				float angleHorizontal = HorizontalAngle.Radians;
				float angleVertical = VerticalAngle.Radians;

				float verticalRadians = VerticalAngle.Radians;
				float horizontalRadians = HorizontalAngle.Radians;
				float y = math.tan(verticalRadians * 0.5f) * Distance;
				float x = y * GetAspect(horizontalRadians, verticalRadians);
				float h = math.length(new float3(x, y, Distance));

				Angle vert = Angle.FromRadians(math.asin(y / h) * 2);
				Angle horz = Angle.FromRadians(math.asin(x / h) * 2);

				// Center vertical
				commandBuilder.AppendArc(new Arc(Position, math.mul(Rotation, arcRotationV), Distance, VerticalAngle), color, duration, DrawModifications.NormalFade);
				// Center horizontal
				commandBuilder.AppendArc(new Arc(Position, math.mul(Rotation, arcRotationH), Distance, HorizontalAngle), color, duration, DrawModifications.NormalFade);
				// Right vertical
				var rightVertical = new Arc(Position, math.mul(math.mul(Rotation, quaternion.AxisAngle(math.up(), angleHorizontal * 0.5f)), arcRotationV), Distance, vert);
				commandBuilder.AppendArc(rightVertical, color, duration);
				var leftVertical = new Arc(Position, math.mul(math.mul(Rotation, quaternion.AxisAngle(math.up(), -angleHorizontal * 0.5f)), arcRotationV), Distance, vert);
				commandBuilder.AppendArc(leftVertical, color, duration);
				var topHorizontal = new Arc(Position, math.mul(math.mul(Rotation, quaternion.AxisAngle(math.right(), -angleVertical * 0.5f)), arcRotationH), Distance, horz);
				commandBuilder.AppendArc(topHorizontal, color, duration);
				var bottomHorizontal = new Arc(Position, math.mul(math.mul(Rotation, quaternion.AxisAngle(math.right(), angleVertical * 0.5f)), arcRotationH), Distance, horz);
				commandBuilder.AppendArc(bottomHorizontal, color, duration);

				float length = Distance * math.cos(horz.Radians * 0.5f) * math.cos(VerticalAngle.Radians * 0.5f);

				var size = new float3(Arc.GetChordLength(horz, Distance) * 0.5f, Arc.GetChordLength(vert, Distance) * 0.5f, length);
				commandBuilder.AppendLine(new Line(Position, Position + math.mul(Rotation, new float3(size.x, size.y, size.z))), color, duration);
				commandBuilder.AppendLine(new Line(Position, Position + math.mul(Rotation, new float3(-size.x, size.y, size.z))), color, duration);
				commandBuilder.AppendLine(new Line(Position, Position + math.mul(Rotation, new float3(size.x, -size.y, size.z))), color, duration);
				commandBuilder.AppendLine(new Line(Position, Position + math.mul(Rotation, new float3(-size.x, -size.y, size.z))), color, duration);

				return;
				static float GetAspect(float hFov, float vFov) => math.tan(hFov * 0.5f) / math.tan(vFov * 0.5f);
			}
#endif
		}

		/// <summary>
		/// <see cref="Pyramid"/> faces in the Z direction, with its point towards Z+.
		/// </summary>
		public readonly struct Pyramid : IDrawable
		{
			public readonly float3 PointBase;
			public readonly quaternion Rotation;
			public readonly float3 Size;

			public Pyramid(
				float3 pointBase,
				quaternion rotation,
				float3 size
			)
			{
				PointBase = pointBase;
				Rotation = rotation;
				Size = size;
			}

			public Pyramid(
				Vector3 pointBase,
				Quaternion rotation,
				Vector3 size
			) : this((float3)pointBase,
				(quaternion)rotation,
				(float3)size
			)
			{
			}

			public Pyramid Flip() => new(PointBase + math.mul(Rotation, new float3(0, 0, Size.z)), math.mul(Rotation, quaternion.AxisAngle(new float3(0, 1, 0), 180)), Size);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 point = PointBase + math.mul(Rotation, new float3(0, 0, Size.z));
				new Box2D(PointBase, Rotation, new float2(Size.x * 2, Size.y * 2)).Draw(ref commandBuilder, color, duration);
				commandBuilder.AppendLine(new Line(PointBase + math.mul(Rotation, new float3(Size.x, Size.y, 0)), point), color, duration);
				commandBuilder.AppendLine(new Line(PointBase + math.mul(Rotation, new float3(-Size.x, Size.y, 0)), point), color, duration);
				commandBuilder.AppendLine(new Line(PointBase + math.mul(Rotation, new float3(Size.x, -Size.y, 0)), point), color, duration);
				commandBuilder.AppendLine(new Line(PointBase + math.mul(Rotation, new float3(-Size.x, -Size.y, 0)), point), color, duration);
			}
#endif
		}

		/// <summary>
		/// A plane drawn as a wireframe quad with an arrow at its center.
		/// </summary>
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
			[PublicAPI]
			public Plane(UnityEngine.Plane plane, float3 pointOnPlane, float2 displaySize)
			{
				Value = plane;
				PointOnPlane = plane.ClosestPointOnPlane(pointOnPlane);
				DisplaySize = displaySize;
			}

			[PublicAPI]
			public Plane(UnityEngine.Plane plane, Vector3 pointOnPlane, Vector2 displaySize) : this(plane, (float3)pointOnPlane, (float2)displaySize)
			{
			}

			/// <summary>
			/// Constructs a Plane
			/// </summary>
			/// <param name="plane">The backing plane</param>
			/// <param name="pointOnPlane">A point to center the debug visual on, defaults to 2x2m in size.</param>
			[PublicAPI]
			public Plane(UnityEngine.Plane plane, float3 pointOnPlane)
			{
				Value = plane;
				PointOnPlane = plane.ClosestPointOnPlane(pointOnPlane);
				DisplaySize = new float2(2, 2);
			}

			[PublicAPI]
			public Plane(UnityEngine.Plane plane, Vector3 pointOnPlane) : this(plane, (float3)pointOnPlane)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 normal = Value.normal;
				float3 perpendicular = GetValidPerpendicular(normal);
				quaternion rotation = quaternion.LookRotation(normal, perpendicular);
				new Box2D(PointOnPlane, rotation, DisplaySize).Draw(ref commandBuilder, color, duration);
				new Arrow(PointOnPlane, rotation).Draw(ref commandBuilder, color, duration);
			}
#endif

			[PublicAPI]
			public static float3 Intersection(UnityEngine.Plane a, UnityEngine.Plane b, UnityEngine.Plane c)
			{
				float det = math.dot(a.normal, math.cross(b.normal, c.normal));
				/*float det = a.normal[0] * b.normal[1] * c.normal[2]
				            - a.normal[0] * b.normal[2] * c.normal[1]
				            - a.normal[1] * b.normal[0] * c.normal[2]
				            + a.normal[1] * b.normal[2] * c.normal[0]
				            + a.normal[2] * b.normal[0] * c.normal[1]
				            - a.normal[2] * b.normal[1] * c.normal[0];*/

				if (Math.Abs(det) < 1e-4f)
					return float3.zero;

				return (
					a.distance * math.cross(b.normal, c.normal) + b.distance * math.cross(c.normal, a.normal) + c.distance * math.cross(a.normal, b.normal)
				) / -det;
			}
		}

		/// <summary>
		/// A wireframe catenary. A shape made when hanging a line between two points.
		/// </summary>
		public readonly struct Catenary : IDrawable
		{
			public readonly float3 A, B;
			public readonly float Length;
			private readonly float _a, _p, _q;
			private readonly bool _straightLine;

			/// <summary>
			/// Creates a catenary.<br/>
			/// The constructor caches details about the catenary's features, which is expensive.
			/// Depending on frequency, you may want to only recreate the curve when changes occur.
			/// </summary>
			/// <param name="a">An anchor point that the catenary is suspended between.</param>
			/// <param name="b">An anchor point that the catenary is suspended between.</param>
			/// <param name="length">The arc length of the catenary. It will be a straight line if not enough length is provided.</param>
			public Catenary(float3 a, float3 b, float length)
			{
				(A, B) = b.y < a.y ? (b, a) : (a, b);
				Length = length;
				_straightLine = !TryCalculateCatenaryArgs(A, B, Length, out var args);
				(_a, _p, _q) = args;
			}

			public Catenary(Vector3 a, Vector3 b, float length) : this((float3)a, (float3)b, length)
			{
			}

			private static bool TryCalculateCatenaryArgs(float3 p0, float3 p1, float length, out (float a, float p, float q) args)
			{
				// Logic modified from https://github.com/Donitzo/godot-catenary.
				// Which in turn is modified from https://www.alanzucconi.com/2020/12/13/catenary-2/
				const int minSearchIterations = 16;

				// Arc length.
				float3 direction = p1 - p0;
				if (math.length(direction) >= length)
				{
					args = default;
					return false;
				}

				// Approximate 'a'
				float h = math.length(direction.xz);
				float v = direction.y;
				float c = math.sqrt(length * length - v * v);

				if (h == 0)
				{
					args = default;
					return false;
				}

				// Exponentially grow 'a' range.
				float aMin = 0;
				float aMax = 1;
				var i = 0;
				for (; i < 32 && c < 2 * aMax * math.sinh(h / (2 * aMax)); i++)
				{
					aMin = aMax;
					aMax *= 2;
				}

				// Binary search for 'a'.
				i += minSearchIterations;

				float a = 0;
				while (i > 0)
				{
					i -= 1;
					a = (aMin + aMax) * 0.5f;
					if (c < 2 * a * math.sinh(h / (2 * a)))
						aMin = a;
					else
						aMax = a;
				}

				float p = (h - a * math.log((length + v) / (length - v))) / 2;
				float q = (v - length * (1 / math.tanh(h / (2 * a)))) / 2;
				args = (a, p, q);
				return true;
			}

			/// <summary>
			/// Evaluate the catenary curve for a position.
			/// </summary>
			/// <param name="t">A normalized sampling value.</param>
			/// <returns>A point on the curve.</returns>
			public float3 Evaluate(float t)
			{
				if (_straightLine)
					return math.lerp(A, B, t);

				float3 direction = B - A;
				float l = math.length(direction.xz);
				float wx = _a * asinh(t * Length / _a - math.sinh(_p / _a)) + _p;
				float v = wx / l;
				// y = a * cosh ( (x - p) / a) + q
				float y = _a * math.cosh((v * l - _p) / _a) + _q;
				return A + new float3(direction.x * v, y, direction.z * v);
				
				float asinh(float x) => math.log(x + math.sqrt(x * x + 1));
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (_straightLine)
				{
					commandBuilder.AppendLine(new Line(A, B), color, duration);
					return;
				}

				float3 a = Evaluate(0);
				for (var i = 0; i <= 100; i++)
				{
					float3 b = Evaluate(i / 100f);
					commandBuilder.AppendLine(new Line(a, b), color, duration);
					a = b;
				}
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

			public Outline(float3 a, float3 b, float radiusA, float radiusB)
			{
				A = a;
				B = b;
				C = new float3(radiusA, radiusB, 0);
			}

			public Outline(float3 a, float3 b, float3 c)
			{
				A = a;
				B = b;
				C = c;
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendOutline(this, color, duration);
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
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendCast(this, color, duration);
#endif
		}
	}
}