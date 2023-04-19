using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		public readonly struct Point2D : IDrawable
		{
			public readonly Vector3 Position;
			public readonly float Scale;

			public Point2D(Vector2 point, float z = 0, float scale = 0.3f)
			{
				Scale = scale;
				Position = new Vector3(point.x, point.y, z);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float distance = Scale * 0.5f;
				commandBuilder.AppendLine(new Line(new Vector3(Position.x - distance, Position.y, Position.z), new Vector3(Position.x + distance, Position.y, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new Vector3(Position.x, Position.y - distance, Position.z), new Vector3(Position.x, Position.y + distance, Position.z)), color, duration);
			}
#endif
		}

		public readonly struct Ray2D : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly Vector2 Direction;

			public Ray2D(Vector3 origin, Vector2 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Ray2D(Vector2 origin, Vector2 direction, float z = 0)
				: this(new Vector3(origin.x, origin.y, z), direction) { }

			public Ray2D(Vector3 origin, float angle)
				: this(origin, GetDirectionFromAngle(angle)) { }

			public Ray2D(Vector2 origin, float angle, float z = 0)
				: this(origin, GetDirectionFromAngle(angle), z) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> commandBuilder.AppendRay(new Ray(Origin, Direction), color, duration);
#endif
		}

		public readonly struct Arrow2D : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly Vector2 Direction;

			public Arrow2D(Vector3 origin, Vector2 direction)
			{
				Origin = origin;
				Direction = direction;
			}

			public Arrow2D(Vector2 origin, Vector2 direction, float z = 0)
				: this(new Vector3(origin.x, origin.y, z), direction) { }

			public Arrow2D(Vector3 origin, float angle)
				: this(origin, GetDirectionFromAngle(angle)) { }

			public Arrow2D(Vector2 origin, float angle, float z = 0)
				: this(origin, GetDirectionFromAngle(angle), z) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 lineEnd = Origin + (Vector3)Direction;
				commandBuilder.AppendLine(new Line(Origin, lineEnd), color, duration);
				DrawArrowHead(commandBuilder, lineEnd, Direction, color, duration);
			}

			public static void DrawArrowHead(CommandBuilder commandBuilder, Vector3 arrowPoint, Vector2 dir, Color color, float duration, float scale = 1)
			{
				dir.EnsureNormalized();
				Vector3 direction = dir;
				DrawArrowHead(commandBuilder, arrowPoint, direction, PerpendicularClockwise(dir), color, duration, scale);
			}

			public static void DrawArrowHead(CommandBuilder commandBuilder, Vector3 arrowPoint, Vector3 direction, Vector3 cross, Color color, float duration, float scale = 1)
			{
				const float headLength = 0.075f;
				const float headWidth = 0.05f;
				cross *= headWidth * scale;
				Vector3 a = arrowPoint + cross;
				Vector3 b = arrowPoint - cross;
				Vector3 arrowEnd = arrowPoint + direction * (headLength * scale);
				commandBuilder.AppendLine(new Line(a, b), color, duration);
				commandBuilder.AppendLine(new Line(a, arrowEnd), color, duration);
				commandBuilder.AppendLine(new Line(b, arrowEnd), color, duration);
			}
#endif
		}

		public readonly struct ArrowStrip2D : IDrawable
		{
			public readonly IEnumerable<Vector2> Points;
			public readonly float Z;

			public ArrowStrip2D(IEnumerable<Vector2> points, float z = 0)
			{
				Points = points;
				Z = z;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3? previous = null;
				Vector3? origin = null;
				foreach (Vector2 point in Points)
				{
					Vector3 point3D = new Vector3(point.x, point.y, Z);
					if (previous.HasValue)
						commandBuilder.AppendLine(new Line(previous.Value, point3D), color, duration);
					origin = previous;
					previous = point3D;
				}

				if (!origin.HasValue)
					return;
				Arrow2D.DrawArrowHead(commandBuilder, origin.Value, previous.Value - origin.Value, color, duration);
			}
#endif
		}

		public readonly struct Axis2D : IDrawable
		{
			public readonly Vector3 Position;
			public readonly float Angle;
			public readonly float Scale;
			public readonly bool ShowArrowHeads;

			public Axis2D(Vector2 origin, float angle, float z = 0, bool showArrowHeads = true)
			{
				Position = new Vector3(origin.x, origin.y, z);
				Angle = angle;
				ShowArrowHeads = showArrowHeads;
				Scale = 1;
			}
			
			public Axis2D(Vector2 origin, float angle, float z = 0, float scale = 1, bool showArrowHeads = true)
			{
				Position = new Vector3(origin.x, origin.y, z);
				Angle = angle;
				ShowArrowHeads = showArrowHeads;
				Scale = scale;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (ShowArrowHeads)
				{
					new Arrow2D(Position, GetDirectionFromAngle(Angle) * Scale).Draw(commandBuilder, XColor, duration);
					new Arrow2D(Position, GetDirectionFromAngle(Angle + 90) * Scale).Draw(commandBuilder, YColor, duration);
				}
				else
				{
					new Ray2D(Position, GetDirectionFromAngle(Angle) * Scale).Draw(commandBuilder, XColor, duration);
					new Ray2D(Position, GetDirectionFromAngle(Angle + 90) * Scale).Draw(commandBuilder, YColor, duration);
				}
			}
#endif
		}

		public readonly struct Circle2D : IDrawable
		{
			private readonly Circle _circle;

			public Matrix4x4 Matrix => _circle.Matrix;
			// set => _circle = new Circle(value);

			public Circle2D(Matrix4x4 matrix) => _circle = new Circle(matrix);

			public Circle2D(Vector2 origin, float radius, float z = 0)
				=> _circle = new Circle(new Vector3(origin.x, origin.y, z), Quaternion.identity, radius);

			internal Circle2D(Vector3 origin, float radius)
				=> _circle = new Circle(origin, Quaternion.identity, radius);

#if VERTX_PHYSICS_2D
			public Circle2D(CircleCollider2D circleCollider)
			{
				Transform transform = circleCollider.transform;
				Vector3 scale = transform.lossyScale;
				Vector3 position = circleCollider.transform.TransformPoint(circleCollider.offset);
				position.z = transform.position.z;
				_circle = new Circle(position, Quaternion.identity, circleCollider.radius * Mathf.Max(scale.x, scale.y));
			}
#endif

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => _circle.Draw(commandBuilder, color, duration);
#endif
		}

		/// <summary>
		/// This is 2D, the rotation or matrix used will align the arc facing right, aligned with XY.<br/>
		/// Use the helper constructors to create an Arc aligned how you require.
		/// </summary>
		public readonly struct Arc2D : IDrawable
		{
			public readonly Arc Arc;

			public Arc2D(Vector2 origin, float rotationDegrees, float radius, Angle angle, float z = 0)
				=> Arc = new Arc(new Vector3(origin.x, origin.y, z), Quaternion.AngleAxis(rotationDegrees, Vector3.forward), radius, angle);

			public Arc2D(Vector2 origin, float rotationDegrees, float radius, float z = 0) : this(origin, rotationDegrees, radius, Angle.FromTurns(1), z) { }

			internal Arc2D(Matrix4x4 matrix) => Arc = new Arc(matrix);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Arc.Draw(commandBuilder, color, duration);
#endif
		}

		public readonly struct Box2D : IDrawable
		{
			public readonly Matrix4x4 Matrix;

			internal Box2D(Matrix4x4 matrix) => Matrix = matrix;

			public Box2D(Vector2 origin, Vector2 size, float angle = 0, float z = 0) => Matrix = Matrix4x4.TRS(new Vector3(origin.x, origin.y, z), Quaternion.AngleAxis(angle, Vector3.forward), size);

			public Box2D(Vector3 origin, Vector2 size, float angle = 0) => Matrix = Matrix4x4.TRS(origin, Quaternion.AngleAxis(angle, Vector3.forward), size);

			public Box2D(Vector3 origin, Quaternion rotation, Vector2 size) => Matrix = Matrix4x4.TRS(origin, rotation, size);

#if VERTX_PHYSICS_2D
			/// <summary>
			/// Creates a box using a <see cref="BoxCollider2D"/>.
			/// This does not support <see cref="BoxCollider2D.edgeRadius"/>, though <see cref="D.raw(Collider2D, Color, float)"/> does.<br/>
			/// </summary>
			public Box2D(BoxCollider2D boxCollider)
			{
				if (Mathf.Approximately(boxCollider.transform.lossyScale.sqrMagnitude, 0f))
				{
					Matrix = Matrix4x4.Scale(new Vector3(0, 0, 0));
					return;
				}
				Matrix4x4 handleMatrix = boxCollider.transform.localToWorldMatrix;
				handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
				handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
				handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, boxCollider.transform.position.z));
				
				Matrix = handleMatrix * Matrix4x4.TRS(boxCollider.offset, Quaternion.identity, ColliderLocalSize(boxCollider));
			}

			private static Vector2 ColliderLocalSize(BoxCollider2D boxCollider)
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

			internal static Vector3 GetPoint(Matrix4x4 matrix, Point point)
			{
				Vector3 position = new Vector3(
					(point & Point.Left) != 0 ? -0.5f : 0 + (point & Point.Right) != 0 ? 0.5f : 0,
					(point & Point.Bottom) != 0 ? -0.5f : 0 + (point & Point.Top) != 0 ? 0.5f : 0
				);
				return matrix.MultiplyPoint3x4(position);
			}

#if UNITY_EDITOR
			internal enum VertexCorner
			{
				BottomLeft,
				TopLeft,
				TopRight,
				BottomRight
			}
			
			internal static readonly Vector3[] s_Vertices =
			{
				new Vector3(-0.5f, -0.5f, 0), // 0
				new Vector3(-0.5f, +0.5f, 0), // 1
				new Vector3(+0.5f, +0.5f, 0), // 2
				new Vector3(+0.5f, -0.5f, 0) //  3
			};

			private static readonly int[] s_Indices =
			{
				0, 1,
				1, 2,
				2, 3,
				3, 0
			};

			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Matrix4x4 m = Matrix;
				for (int i = 0; i < s_Indices.Length; i += 2)
				{
					commandBuilder.AppendLine(
						new Line(
							m.MultiplyPoint3x4(s_Vertices[s_Indices[i]]),
							m.MultiplyPoint3x4(s_Vertices[s_Indices[i + 1]])
						),
						color,
						duration
					);
				}
			}
#endif
		}

		public readonly struct Area2D : IDrawable
		{
			public readonly Vector3 PointA, PointB;

			public Area2D(Vector2 pointA, Vector2 pointB, float z = 0)
			{
				PointA = new Vector3(pointA.x, pointA.y, z);
				PointB = new Vector3(pointB.x, pointB.y, z);
			}

			public Area2D(Vector3 pointA, Vector3 pointB)
			{
				PointA = pointA;
				PointB = pointB;
				PointB.z = PointA.z;
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
				Vector2 rectMin = rect.min;
				Vector2 rectMax = rect.max;
				PointA = new Vector3(rectMin.x, rectMin.y, z);
				PointB = new Vector3(rectMax.x, rectMax.y, z);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Vector3 pointC = new Vector3(PointA.x, PointB.y, PointA.z);
				commandBuilder.AppendLine(new Line(PointA, pointC), color, duration);
				commandBuilder.AppendLine(new Line(pointC, PointB), color, duration);
				Vector3 pointD = new Vector3(PointB.x, PointA.y, PointA.z);
				commandBuilder.AppendLine(new Line(PointB, pointD), color, duration);
				commandBuilder.AppendLine(new Line(pointD, PointA), color, duration);
			}
#endif
		}

		public readonly struct Capsule2D : IDrawable
		{
			public Vector3 PointA => _pointA;
			public Vector3 PointB => _pointB;
			public float Radius => _radius;

			// ReSharper disable InconsistentNaming
			internal readonly Vector3 _pointA, _pointB;
			internal readonly float _radius;
			internal readonly Vector3 _verticalDirection;
			internal readonly Vector3 _scaledLeft;
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
			public Capsule2D(Vector2 point, Vector2 size, CapsuleDirection2D capsuleDirection, float angle = 0)
				: this(point, size, capsuleDirection, angle, 0) { }

			public Capsule2D(Vector2 point, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, float z)
				: this(point, size, (Direction)capsuleDirection, angle, z) { }

			public Capsule2D(CapsuleCollider2D collider)
			{
				Vector2 size = collider.size * 0.5f;

				Transform transform = collider.transform;
				Vector3 origin = transform.TransformPoint(collider.offset);

				Vector2 radius2D = transform.TransformVector(
					collider.direction == CapsuleDirection2D.Vertical
						? new Vector3(size.x, 0, 0)
						: new Vector3(0, size.x, 0)
				);
				_radius = radius2D.magnitude;

				Vector2 offset = transform.TransformVector(
					collider.direction == CapsuleDirection2D.Vertical
						? new Vector3(0, size.y, 0)
						: new Vector3(size.y, 0, 0)
				);
				offset.EnsureNormalized(out float magnitude);
				offset *= Mathf.Max(magnitude - _radius, 0);

				_pointA = new Vector3(origin.x + offset.x, origin.y + offset.y, origin.z);
				_pointB = new Vector3(origin.x - offset.x, origin.y - offset.y, origin.z);
				_verticalDirection = (_pointA - _pointB).normalized;
				_scaledLeft = PerpendicularCounterClockwise(_verticalDirection) * _radius;
			}
#endif

			public Capsule2D(Vector2 point, Vector2 size, Direction capsuleDirection, float angle = 0)
				: this(point, size, capsuleDirection, angle, 0) { }

			internal Capsule2D(Vector3 pointA, Vector3 pointB, float radius, Vector3 verticalDirection, Vector3 scaledLeft)
			{
				_pointA = pointA;
				_pointB = pointB;
				_radius = radius;
				_verticalDirection = verticalDirection;
				_scaledLeft = scaledLeft;
			}

			public Capsule2D(Vector2 point, Vector2 size, Direction capsuleDirection, float angle, float z)
			{
				if (capsuleDirection == Direction.Horizontal)
				{
					// ReSharper disable once SwapViaDeconstruction
					float temp = size.y;
					size.y = size.x;
					size.x = temp;
					angle += 180;
				}

				_radius = size.x * 0.5f;
				float vertical = Mathf.Max(0, size.y - size.x) * 0.5f;
				GetRotationCoefficients(angle, out float s, out float c);
				_verticalDirection = RotateUsingCoefficients(Vector3.up, s, c);
				Vector2 verticalOffset = RotateUsingCoefficients(new Vector2(0, vertical), s, c);
				_pointA = GetVector3(point + verticalOffset);
				_pointB = GetVector3(point - verticalOffset);
				_scaledLeft = new Vector2(c, s) * _radius;

				Vector3 GetVector3(Vector2 v2) => new Vector3(v2.x, v2.y, z);
			}

			internal Capsule2D(Vector3 pointA, Vector3 pointB, float radius)
			{
				_verticalDirection = pointA - pointB;
				_verticalDirection.EnsureNormalized();
				_pointA = pointA;
				_pointB = pointB;
				_radius = radius;
				_scaledLeft = PerpendicularCounterClockwise(_verticalDirection) * _radius;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (_verticalDirection.sqrMagnitude == 0)
				{
					new Circle2D(_pointA, _radius).Draw(commandBuilder, color, duration);
					return;
				}

				Angle halfCircle = Angle.FromTurns(0.5f);
				commandBuilder.AppendArc(new Arc(_pointA, Vector3.forward, _verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendArc(new Arc(_pointB, Vector3.forward, -_verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendLine(new Line(_pointA + _scaledLeft, _pointB + _scaledLeft), color, duration);
				commandBuilder.AppendLine(new Line(_pointA - _scaledLeft, _pointB - _scaledLeft), color, duration);
			}
#endif
		}

		public readonly struct Spiral2D : IDrawable
		{
			public readonly Vector3 Origin;
			public readonly float Radius;
			public readonly float Angle;
			public readonly float Revolutions;

			public Spiral2D(Vector2 origin, float radius, float angle = 0, float revolutions = 3, float z = 0)
			{
				Origin = new Vector3(origin.x, origin.y, z);
				Radius = radius;
				Angle = angle;
				Revolutions = revolutions;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Angle fullCircle = Shape.Angle.FromTurns(1);
				commandBuilder.AppendArc(new Arc(Origin, Quaternion.identity, Radius, fullCircle), color, duration);
				if (Revolutions == 0)
				{
					commandBuilder.AppendRay(new Ray(Origin, Rotate(new Vector3(Radius, 0, 0), Angle)), color, duration);
					return;
				}

				float absRevolutions = Mathf.Abs(Revolutions);
				float currentRevolutions = absRevolutions;
				float sign = Mathf.Sign(Revolutions);
				Vector3 direction = Rotate(new Vector3(-sign, 0, 0), Angle);
				Vector3 normal = new Vector3(0, 0, sign);
				float radiusSigned = sign * Radius;
				while (currentRevolutions > 0)
				{
					float innerR = (currentRevolutions - 1) / currentRevolutions;
					commandBuilder.AppendArc(
						new Arc(Origin, normal, direction, radiusSigned * (currentRevolutions / absRevolutions), Shape.Angle.FromTurns(innerR)),
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