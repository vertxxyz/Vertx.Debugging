using System;
using System.Collections.Generic;
using System.Linq;
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
		/// <summary>
		/// A point expressed by the intersection of two <see cref="Line"/>s.
		/// </summary>
		public readonly struct Point2D : IDrawable
		{
			public readonly float3 Position;
			public readonly float Scale;

			public Point2D(float2 point, float z = 0, float scale = 0.3f)
			{
				Scale = scale;
				Position = new float3(point.x, point.y, z);
			}

			public Point2D(Vector2 point, float z = 0, float scale = 0.3f) : this((float2)point, z, scale)
			{
			}

			public Point2D(float3 position, float scale = 0.3f)
			{
				Scale = scale;
				Position = position;
			}

			public Point2D(Vector3 point, float scale = 0.3f) : this((float3)point, scale)
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
			}
#endif
		}

		/// <summary>
		/// A <see cref="Ray"/> with a 2D direction.
		/// </summary>
		public readonly struct Ray2D : IDrawable
		{
			public readonly float3 Origin;
			public readonly float2 Direction;

			public Ray2D(float3 origin, float2 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Ray2D(Vector3 origin, Vector2 direction) : this((float3)origin, (float2)direction)
			{
			}

			public Ray2D(float2 origin, float2 direction, float z = 0)
				: this(new float3(origin.x, origin.y, z), direction)
			{
			}

			public Ray2D(Vector2 origin, Vector2 direction, float z = 0) : this((float2)origin, (float2)direction, z)
			{
			}

			public Ray2D(float3 origin, float angleDegrees)
				: this(origin, GetDirectionFromAngle(Angle.FromDegrees(angleDegrees)))
			{
			}

			public Ray2D(Vector3 origin, float angleDegrees) : this((float3)origin, angleDegrees)
			{
			}

			public Ray2D(float2 origin, float angleDegrees, float z = 0)
				: this(origin, GetDirectionFromAngle(Angle.FromDegrees(angleDegrees)), z)
			{
			}

			public Ray2D(Vector2 origin, float angleDegrees, float z = 0) : this((float2)origin, angleDegrees, z)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendRay(new Ray(Origin, Direction.xy0()), color, duration);
#endif
		}

		/// <summary>
		/// A <see cref="Ray2D"/> terminating in an arrowhead.
		/// </summary>
		public readonly struct Arrow2D : IDrawable
		{
			public readonly float3 Origin;
			public readonly float2 Direction;
			public readonly float ArrowheadScale;

			public Arrow2D(float3 origin, float2 direction, float arrowheadScale = 1)
			{
				Origin = origin;
				Direction = direction;
				ArrowheadScale = arrowheadScale;
			}

			public Arrow2D(Vector3 origin, Vector2 direction, float arrowheadScale = 1) : this((float3)origin, (float2)direction, arrowheadScale)
			{
			}

			public Arrow2D(float2 origin, float2 direction, float z = 0, float arrowheadScale = 1)
				: this(new float3(origin.x, origin.y, z), direction)
			{
			}

			public Arrow2D(Vector2 origin, Vector2 direction, float z = 0, float arrowheadScale = 1) : this((float2)origin, (float2)direction, z, arrowheadScale)
			{
			}

			public Arrow2D(float3 origin, float angleDegrees, float arrowheadScale = 1)
				: this(origin, GetDirectionFromAngle(Angle.FromDegrees(angleDegrees)), arrowheadScale)
			{
			}

			public Arrow2D(Vector3 origin, float angleDegrees, float arrowheadScale = 1) : this((float3)origin, angleDegrees, arrowheadScale)
			{
			}

			public Arrow2D(float2 origin, float angleDegrees, float z = 0, float arrowheadScale = 1)
				: this(origin, GetDirectionFromAngle(Angle.FromDegrees(angleDegrees)), z, arrowheadScale)
			{
			}

			public Arrow2D(Vector2 origin, float angleDegrees, float z = 0, float arrowheadScale = 1) : this((float2)origin, angleDegrees, z, arrowheadScale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3 lineEnd = Origin + Direction.xy0();
				commandBuilder.AppendLine(new Line(Origin, lineEnd), color, duration);
				DrawArrowHead(ref commandBuilder, lineEnd, Direction, color, duration, ArrowheadScale);
			}

			internal static void DrawArrowHead(ref UnmanagedCommandBuilder commandBuilder, float3 arrowPoint, float2 dir, Color color, float duration, float scale = 1)
			{
				dir.EnsureNormalized();
				float3 direction = dir.xy0();
				DrawArrowHead(ref commandBuilder, arrowPoint, direction, PerpendicularClockwise(dir).xy0(), color, duration, scale);
			}

			internal static void DrawArrowHead(ref UnmanagedCommandBuilder commandBuilder, float3 arrowPoint, float3 direction, float3 cross, Color color, float duration, float scale = 1)
			{
				const float headLength = 0.075f;
				const float headWidth = 0.05f;
				cross *= headWidth * scale;
				float3 a = arrowPoint + cross;
				float3 b = arrowPoint - cross;
				float3 arrowEnd = arrowPoint + direction * (headLength * scale);
				commandBuilder.AppendLine(new Line(a, b), color, duration);
				commandBuilder.AppendLine(new Line(a, arrowEnd), color, duration);
				commandBuilder.AppendLine(new Line(b, arrowEnd), color, duration);
			}
#endif
		}

		/// <summary>
		/// A wireframe line of points that terminates in an <see cref="Arrow2D"/>
		/// </summary>
		public readonly struct ArrowStrip2D : IDrawable
		{
			public readonly IEnumerable<float2> Points;
			public readonly float Z, ArrowheadScale;

			public ArrowStrip2D(IEnumerable<float2> points, float z = 0, float arrowheadScale = 1)
			{
				Points = points;
				Z = z;
				ArrowheadScale = arrowheadScale;
			}

			public ArrowStrip2D(IEnumerable<Vector2> points, float z = 0, float arrowheadScale = 1) : this(points.Select(v => (float2)v), z, arrowheadScale)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float3? previous = null;
				float3? origin = null;
				foreach (float2 point in Points)
				{
					var point3D = new float3(point.x, point.y, Z);
					if (previous.HasValue)
						commandBuilder.AppendLine(new Line(previous.Value, point3D), color, duration);
					origin = previous;
					previous = point3D;
				}

				if (!origin.HasValue)
					return;
				Arrow2D.DrawArrowHead(ref commandBuilder, origin.Value, previous.Value.xy - origin.Value.xy, color, duration, ArrowheadScale);
			}
#endif
		}

		/// <summary>
		/// An XY axis made of two <see cref="Arrow2D"/> or <see cref="Ray2D"/>.
		/// </summary>
		public readonly struct Axis2D : IDrawable
		{
			public readonly float3 Position;
			public readonly float AngleDegrees;
			public readonly float Scale;
			public readonly bool ShowArrowHeads;

			public Axis2D(float2 origin, float angleDegrees, float z = 0, bool showArrowHeads = true)
			{
				Position = new float3(origin.x, origin.y, z);
				AngleDegrees = angleDegrees;
				ShowArrowHeads = showArrowHeads;
				Scale = 1;
			}

			public Axis2D(Vector2 origin, float angleDegrees, float z = 0, bool showArrowHeads = true) : this((float2)origin, angleDegrees, z, showArrowHeads)
			{
			}

			public Axis2D(float2 origin, float angleDegrees, float z = 0, float scale = 1, bool showArrowHeads = true)
			{
				Position = new float3(origin.x, origin.y, z);
				AngleDegrees = angleDegrees;
				ShowArrowHeads = showArrowHeads;
				Scale = scale;
			}

			public Axis2D(Vector2 origin, float angleDegrees, float z = 0, float scale = 1, bool showArrowHeads = true) : this((float2)origin, angleDegrees, z, scale, showArrowHeads)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				Angle a = Angle.FromDegrees(AngleDegrees);
				Angle b = Angle.FromDegrees(AngleDegrees + 90);
				if (ShowArrowHeads)
				{
					new Arrow2D(Position, GetDirectionFromAngle(a) * Scale).Draw(ref commandBuilder, XColor, duration);
					new Arrow2D(Position, GetDirectionFromAngle(b) * Scale).Draw(ref commandBuilder, YColor, duration);
				}
				else
				{
					new Ray2D(Position, GetDirectionFromAngle(a) * Scale).Draw(ref commandBuilder, XColor, duration);
					new Ray2D(Position, GetDirectionFromAngle(b) * Scale).Draw(ref commandBuilder, YColor, duration);
				}
			}
#endif
		}

		/// <summary>
		/// A <see cref="Circle"/> aligned to XY.
		/// </summary>
		public readonly struct Circle2D : IDrawable
		{
			private readonly Circle _circle;

			public float4x4 Matrix => _circle.Matrix;

			public Circle2D(float4x4 matrix) => _circle = new Circle(matrix);

			public Circle2D(Matrix4x4 matrix) : this((float4x4)matrix)
			{
			}

			public Circle2D(float2 origin, float radius, float z = 0)
				=> _circle = new Circle(new float3(origin.x, origin.y, z), quaternion.identity, radius);

			public Circle2D(Vector2 origin, float radius, float z = 0) : this((float2)origin, radius, z)
			{
			}

			internal Circle2D(float3 origin, float radius)
				=> _circle = new Circle(origin, quaternion.identity, radius);

#if VERTX_PHYSICS_2D
			public Circle2D(CircleCollider2D circleCollider)
			{
				Transform transform = circleCollider.transform;
				float3 scale = transform.lossyScale;
				float3 position = circleCollider.transform.TransformPoint(circleCollider.offset);
				position.z = transform.position.z;
				_circle = new Circle(position, quaternion.identity, circleCollider.radius * math.max(scale.x, scale.y));
			}
#endif

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => _circle.Draw(ref commandBuilder, color, duration);
#endif
		}

		/// <summary>
		/// This is 2D, the rotation or matrix used will align the arc facing right, aligned with XY.<br/>
		/// Use the helper constructors to create an Arc aligned how you require.
		/// </summary>
		public readonly struct Arc2D : IDrawable
		{
			public readonly Arc Arc;

			public Arc2D(float2 origin, float rotationDegrees, float radius, Angle angle, float z = 0)
				=> Arc = new Arc(new float3(origin.x, origin.y, z), quaternion.AxisAngle(math.forward(), rotationDegrees * math.TORADIANS), radius, angle);

			public Arc2D(Vector2 origin, float rotationDegrees, float radius, Angle angle, float z = 0) : this((float2)origin, rotationDegrees, radius, angle, z)
			{
			}

			public Arc2D(float2 origin, float rotationDegrees, float radius, float z = 0) : this(origin, rotationDegrees, radius, Angle.FromTurns(1), z)
			{
			}

			public Arc2D(Vector2 origin, float rotationDegrees, float radius, float z = 0) : this((float2)origin, rotationDegrees, radius, z)
			{
			}

			internal Arc2D(float4x4 matrix) => Arc = new Arc(matrix);

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration) => Arc.Draw(ref commandBuilder, color, duration);
#endif
		}

		public readonly struct Box2D : IDrawable
		{
			public readonly float4x4 Matrix;

			internal Box2D(float4x4 matrix) => Matrix = matrix;

			public Box2D(float2 origin, float2 size, float angleDegrees = default, float z = 0) => Matrix = float4x4.TRS(new float3(origin.x, origin.y, z), quaternion.AxisAngle(math.forward(), angleDegrees * math.TORADIANS), size.xy0());

			public Box2D(Vector2 origin, Vector2 size, float angleDegrees = default, float z = 0) : this((float2)origin, (float2)size, angleDegrees, z)
			{
			}

			public Box2D(float3 origin, float2 size, float angleDegrees = 0) => Matrix = float4x4.TRS(origin, quaternion.AxisAngle(math.forward(), angleDegrees * math.TORADIANS), size.xy0());

			public Box2D(Vector3 origin, Vector2 size, float angleDegrees = 0) : this((float3)origin, (float2)size, angleDegrees)
			{
			}

			public Box2D(float3 origin, quaternion rotation, float2 size) => Matrix = float4x4.TRS(origin, rotation, size.xy0());

			public Box2D(Vector3 origin, Quaternion rotation, Vector2 size) : this((float3)origin, (quaternion)rotation, (float2)size)
			{
			}

#if VERTX_PHYSICS_2D
			/// <summary>
			/// Creates a box using a <see cref="BoxCollider2D"/>.
			/// This does not support <see cref="BoxCollider2D.edgeRadius"/>, though <see cref="D.raw(Collider2D, Color, float)"/> and <see cref="Box2DWithEdgeRadius"/> does.<br/>
			/// </summary>
			public Box2D(BoxCollider2D boxCollider)
			{
				if (Mathf.Approximately(boxCollider.transform.lossyScale.sqrMagnitude, 0f))
				{
					Matrix = float4x4.Scale(new float3(0, 0, 0));
					return;
				}

				float4x4 handleMatrix = boxCollider.transform.localToWorldMatrix;
				handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
				handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
				handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, boxCollider.transform.position.z));

				Matrix = handleMatrix * float4x4.TRS(boxCollider.offset.xy0(), quaternion.identity, ColliderLocalSize(boxCollider).xy0());
			}

			private static float2 ColliderLocalSize(BoxCollider2D boxCollider)
			{
				if (!boxCollider.autoTiling
					|| !boxCollider.TryGetComponent(out SpriteRenderer renderer)
					|| renderer.drawMode == SpriteDrawMode.Simple)
					return boxCollider.size;

				return renderer.size;
			}

#endif

			[Flags]
			internal enum Point
			{
				Origin = 0,
				Top = 1,
				Right = 1 << 1,
				Bottom = 1 << 2,
				Left = 1 << 3,
				TopLeft = Top | Left,
				TopRight = Top | Right,
				BottomRight = Bottom | Right,
				BottomLeft = Bottom | Left
			}

			internal static float3 GetPoint(float4x4 matrix, Point point)
			{
				var position = new float3(
					(point & Point.Left) != 0 ? -0.5f : 0 + (point & Point.Right) != 0 ? 0.5f : 0,
					(point & Point.Bottom) != 0 ? -0.5f : 0 + (point & Point.Top) != 0 ? 0.5f : 0,
					0
				);
				return MultiplyPoint3x4(matrix, position);
			}

#if UNITY_EDITOR
			internal enum VertexCorner
			{
				BottomLeft,
				TopLeft,
				TopRight,
				BottomRight
			}

			internal static readonly float3[] s_Vertices =
			{
				new float3(-0.5f, -0.5f, 0), // 0
				new float3(-0.5f, +0.5f, 0), // 1
				new float3(+0.5f, +0.5f, 0), // 2
				new float3(+0.5f, -0.5f, 0) //  3
			};

			private static readonly int[] s_indices =
			{
				0, 1,
				1, 2,
				2, 3,
				3, 0
			};

			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				float4x4 m = Matrix;
				for (var i = 0; i < s_indices.Length; i += 2)
				{
					commandBuilder.AppendLine(
						new Line(
							m.MultiplyPoint3x4(s_Vertices[s_indices[i]]),
							m.MultiplyPoint3x4(s_Vertices[s_indices[i + 1]])
						),
						color,
						duration
					);
				}
			}
#endif
		}

		/// <summary>
		/// A <see cref="Box2D"/> with edge radius support.
		/// </summary>
		public readonly struct Box2DWithEdgeRadius : IDrawable
		{
			public readonly Box2D Box;
			public readonly float EdgeRadius;

			public Box2DWithEdgeRadius(Box2D box, float edgeRadius)
			{
				Box = box;
				EdgeRadius = edgeRadius;
			}

#if VERTX_PHYSICS_2D
			public Box2DWithEdgeRadius(BoxCollider2D collider) : this(new Box2D(collider), collider.edgeRadius)
			{
			}
#endif

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				Box.Draw(ref commandBuilder, color, duration);
				if (EdgeRadius == 0)
					return;
				float4x4 matrix = Box.Matrix;
				var rotation = new quaternion(matrix);
				float3 topRight = matrix.MultiplyPoint3x4(Box2D.s_Vertices[(int)Box2D.VertexCorner.TopRight]),
					topLeft = matrix.MultiplyPoint3x4(Box2D.s_Vertices[(int)Box2D.VertexCorner.TopLeft]),
					bottomLeft = matrix.MultiplyPoint3x4(Box2D.s_Vertices[(int)Box2D.VertexCorner.BottomLeft]),
					bottomRight = matrix.MultiplyPoint3x4(Box2D.s_Vertices[(int)Box2D.VertexCorner.BottomRight]);
				Angle angle = Angle.FromTurns(0.25f);
				quaternion topRightRot = math.mul(quaternion.AxisAngle(math.forward(), 45 * math.TORADIANS), rotation),
					topLeftRot = math.mul(quaternion.AxisAngle(math.forward(), 135 * math.TORADIANS), rotation),
					bottomLeftRot = math.mul(quaternion.AxisAngle(math.forward(), 225 * math.TORADIANS), rotation),
					bottomRightRot = math.mul(quaternion.AxisAngle(math.forward(), 315 * math.TORADIANS), rotation);
				new Arc(topRight, topRightRot, EdgeRadius, angle).Draw(ref commandBuilder, color, duration);
				new Arc(topLeft, topLeftRot, EdgeRadius, angle).Draw(ref commandBuilder, color, duration);
				new Arc(bottomLeft, bottomLeftRot, EdgeRadius, angle).Draw(ref commandBuilder, color, duration);
				new Arc(bottomRight, bottomRightRot, EdgeRadius, angle).Draw(ref commandBuilder, color, duration);
				float h = math.sqrt(EdgeRadius * EdgeRadius * 0.5f);
				float3 a = new float3(h, h, 0),
					b = new float3(h, -h, 0);
				new Line(topLeft + math.mul(topLeftRot, b), topRight + math.mul(topRightRot, a)).Draw(ref commandBuilder, color, duration);
				new Line(topRight + math.mul(topRightRot, b), bottomRight + math.mul(bottomRightRot, a)).Draw(ref commandBuilder, color, duration);
				new Line(bottomRight + math.mul(bottomRightRot, b), bottomLeft + math.mul(bottomLeftRot, a)).Draw(ref commandBuilder, color, duration);
				new Line(bottomLeft + math.mul(bottomLeftRot, b), topLeft + math.mul(topLeftRot, a)).Draw(ref commandBuilder, color, duration);
			}
#endif
		}

		/// <summary>
		/// An AABB that encapsulates two points.
		/// </summary>
		public readonly struct Area2D : IDrawable
		{
			public readonly float3 PointA, PointB;

			public Area2D(float2 pointA, float2 pointB, float z = 0)
			{
				PointA = new float3(pointA.x, pointA.y, z);
				PointB = new float3(pointB.x, pointB.y, z);
			}

			public Area2D(Vector2 pointA, Vector2 pointB, float z = 0) : this((float2)pointA, (float2)pointB, z)
			{
			}

			public Area2D(float3 pointA, float3 pointB)
			{
				PointA = pointA;
				PointB = pointB;
				PointB.z = PointA.z;
			}

			public Area2D(Vector3 pointA, Vector3 pointB) : this((float3)pointA, (float3)pointB)
			{
			}

			public Area2D(Bounds bounds2D, float z = 0)
			{
				PointA = bounds2D.min;
				PointA.z = z;
				PointB = bounds2D.max;
				PointB.z = z;
			}

			public Area2D(Rect rect, float z = 0)
			{
				float2 rectMin = rect.min;
				float2 rectMax = rect.max;
				PointA = new float3(rectMin.x, rectMin.y, z);
				PointB = new float3(rectMax.x, rectMax.y, z);
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				var pointC = new float3(PointA.x, PointB.y, PointA.z);
				commandBuilder.AppendLine(new Line(PointA, pointC), color, duration);
				commandBuilder.AppendLine(new Line(pointC, PointB), color, duration);
				var pointD = new float3(PointB.x, PointA.y, PointA.z);
				commandBuilder.AppendLine(new Line(PointB, pointD), color, duration);
				commandBuilder.AppendLine(new Line(pointD, PointA), color, duration);
			}
#endif
		}

		/// <summary>
		/// A wireframe 2D capsule.
		/// </summary>
		public readonly struct Capsule2D : IDrawable
		{
			public float3 PointA => _pointA;
			public float3 PointB => _pointB;
			public float Radius => _radius;

			// ReSharper disable InconsistentNaming
			internal readonly float3 _pointA, _pointB;
			internal readonly float _radius;
			internal readonly float3 _verticalDirection;
			internal readonly float3 _scaledLeft;
			// ReSharper restore InconsistentNaming

			public enum Direction
			{
				/// <summary>
				///   <para>The capsule sides extend vertically.</para>
				/// </summary>
				Vertical,

				/// <summary>
				///   <para>The capsule sides extend horizontally.</para>
				/// </summary>
				Horizontal
			}

#if VERTX_PHYSICS_2D
			public Capsule2D(float2 point, float2 size, CapsuleDirection2D capsuleDirection, float angleDegrees = 0)
				: this(point, size, capsuleDirection, angleDegrees, 0)
			{
			}

			public Capsule2D(Vector2 point, Vector2 size, CapsuleDirection2D capsuleDirection, float angleDegrees = 0) : this((float2)point, (float2)size, capsuleDirection, angleDegrees)
			{
			}

			public Capsule2D(float2 point, float2 size, CapsuleDirection2D capsuleDirection, float angleDegrees, float z)
				: this(point, size, (Direction)capsuleDirection, angleDegrees, z)
			{
			}

			public Capsule2D(Vector2 point, Vector2 size, CapsuleDirection2D capsuleDirection, float angleDegrees, float z) : this((float2)point, (float2)size, capsuleDirection, angleDegrees, z)
			{
			}


			public Capsule2D(CapsuleCollider2D collider)
			{
				float2 size = collider.size * 0.5f;

				Transform transform = collider.transform;
				float3 origin = transform.TransformPoint(collider.offset);

				float2 radius2D = transform.TransformVector(
					collider.direction == CapsuleDirection2D.Vertical
						? new float3(size.x, 0, 0)
						: new float3(0, size.y, 0)
				).xy();
				_radius = math.length(radius2D);

				float2 offset = transform.TransformVector(
					collider.direction == CapsuleDirection2D.Vertical
						? new float3(0, size.y, 0)
						: new float3(size.x, 0, 0)
				).xy();
				offset.EnsureNormalized(out float magnitude);
				offset *= math.max(magnitude - _radius, 0);

				_pointA = new float3(origin.x + offset.x, origin.y + offset.y, origin.z);
				_pointB = new float3(origin.x - offset.x, origin.y - offset.y, origin.z);
				_verticalDirection = math.normalize(_pointA - _pointB);
				_scaledLeft = PerpendicularCounterClockwise(_verticalDirection) * _radius;
			}
#endif

			public Capsule2D(float2 point, float2 size, Direction capsuleDirection, float angleDegrees = 0)
				: this(point, size, capsuleDirection, angleDegrees, 0)
			{
			}

			public Capsule2D(Vector2 point, Vector2 size, Direction capsuleDirection, float angleDegrees = 0) : this((float2)point, (float2)size, capsuleDirection, angleDegrees)
			{
			}

			internal Capsule2D(float3 pointA, float3 pointB, float radius, float3 verticalDirection, float3 scaledLeft)
			{
				_pointA = pointA;
				_pointB = pointB;
				_radius = radius;
				_verticalDirection = verticalDirection;
				_scaledLeft = scaledLeft;
			}

			public Capsule2D(float2 point, float2 size, Direction capsuleDirection, float angleDegrees, float z)
			{
				if (capsuleDirection == Direction.Horizontal)
				{
					// ReSharper disable once SwapViaDeconstruction
					float temp = size.y;
					size.y = size.x;
					size.x = temp;
					angleDegrees += 180;
				}

				_radius = size.x * 0.5f;
				float vertical = math.max(0, size.y - size.x) * 0.5f;
				GetRotationCoefficients(Angle.FromDegrees(angleDegrees), out float s, out float c);
				_verticalDirection = RotateUsingCoefficients(math.up(), s, c);
				float2 verticalOffset = RotateUsingCoefficients(new float2(0, vertical), s, c);
				_pointA = GetFloat3(point + verticalOffset);
				_pointB = GetFloat3(point - verticalOffset);
				_scaledLeft = (new float2(c, s) * _radius).xy0();
				return;

				float3 GetFloat3(float2 v2) => new float3(v2.x, v2.y, z);
			}

			public Capsule2D(Vector2 point, Vector2 size, Direction capsuleDirection, float angleDegrees, float z) : this((float2)point, (float2)size, capsuleDirection, angleDegrees, z)
			{
			}

			internal Capsule2D(float3 pointA, float3 pointB, float radius)
			{
				_verticalDirection = pointA - pointB;
				_verticalDirection.EnsureNormalized();
				_pointA = pointA;
				_pointB = pointB;
				_radius = radius;
				_scaledLeft = PerpendicularCounterClockwise(_verticalDirection) * _radius;
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (math.lengthsq(_verticalDirection) == 0)
				{
					new Circle2D(_pointA, _radius).Draw(ref commandBuilder, color, duration);
					return;
				}

				Angle halfCircle = Angle.FromTurns(0.5f);
				commandBuilder.AppendArc(new Arc(_pointA, math.forward(), _verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendArc(new Arc(_pointB, math.forward(), -_verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendLine(new Line(_pointA + _scaledLeft, _pointB + _scaledLeft), color, duration);
				commandBuilder.AppendLine(new Line(_pointA - _scaledLeft, _pointB - _scaledLeft), color, duration);
			}
#endif
		}

		/// <summary>
		/// A 2D spiral, useful for visualising the rotation of shapes at higher speeds.
		/// </summary>
		public readonly struct Spiral2D : IDrawable
		{
			public readonly float3 Origin;
			public readonly float Radius;
			public readonly Angle Angle;
			public readonly float Revolutions;

			public Spiral2D(float2 origin, float radius, Angle angle = default, float revolutions = 3, float z = 0)
			{
				Origin = new float3(origin.x, origin.y, z);
				Radius = radius;
				Angle = angle;
				Revolutions = revolutions;
			}

			public Spiral2D(Vector2 origin, float radius, Angle angle = default, float revolutions = 3, float z = 0) : this((float2)origin, radius, angle, revolutions, z)
			{
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				Angle fullCircle = Angle.FromTurns(1);
				commandBuilder.AppendArc(new Arc(Origin, quaternion.identity, Radius, fullCircle), color, duration);
				if (Revolutions == 0)
				{
					commandBuilder.AppendRay(new Ray(Origin, Rotate(new float3(Radius, 0, 0), Angle)), color, duration);
					return;
				}

				float absRevolutions = math.abs(Revolutions);
				float currentRevolutions = absRevolutions;
				float sign = math.sign(Revolutions);
				float3 direction = Rotate(new float3(-sign, 0, 0), Angle);
				var normal = new float3(0, 0, sign);
				float radiusSigned = sign * Radius;
				while (currentRevolutions > 0)
				{
					float innerR = (currentRevolutions - 1) / currentRevolutions;
					commandBuilder.AppendArc(
						new Arc(Origin, normal, direction, radiusSigned * (currentRevolutions / absRevolutions), Angle.FromTurns(innerR)),
						color,
						duration,
						DrawModifications.Custom2
					);
					currentRevolutions--;
				}
			}
#endif
		}
	}
}