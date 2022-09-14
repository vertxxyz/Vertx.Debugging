// ReSharper disable ConvertToNullCoalescingCompoundAssignment

using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public struct Point2D : IDrawable
		{
			public Vector3 Position;
			public float Scale;

			public Point2D(Vector2 point, float z, float scale = 0.3f)
			{
				Scale = scale;
				Position = new Vector3(point.x, point.y, z);
			}

			public Point2D(Vector2 point, float scale = 0.3f)
				: this(point, 0, scale) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				float distance = Scale * 0.5f;
				commandBuilder.AppendLine(new Line(new Vector3(Position.x - distance, Position.y, Position.z), new Vector3(Position.x + distance, Position.y, Position.z)), color, duration);
				commandBuilder.AppendLine(new Line(new Vector3(Position.x, Position.y - distance, Position.z), new Vector3(Position.x, Position.y + distance, Position.z)), color, duration);
			}
#endif
		}

		public struct Ray2D : IDrawable
		{
			public Vector3 Origin;
			public Vector2 Direction;

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

		public struct Arrow2D : IDrawable
		{
			public Vector3 Origin;
			public Vector2 Direction;

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
				const float headLength = 0.075f;
				const float headWidth = 0.05f;
				dir.EnsureNormalized();
				Vector3 direction = dir;
				Vector3 cross = PerpendicularClockwise(dir) * (headWidth * scale);
				Vector3 a = arrowPoint + cross;
				Vector3 b = arrowPoint - cross;
				Vector3 arrowEnd = arrowPoint + direction * (headLength * scale);
				commandBuilder.AppendLine(new Line(a, b), color, duration);
				commandBuilder.AppendLine(new Line(a, arrowEnd), color, duration);
				commandBuilder.AppendLine(new Line(b, arrowEnd), color, duration);
			}
#endif
		}
		
		public struct Axis2D : IDrawable
		{
			public Vector3 Position;
			public float Angle;
			public bool ShowArrowHeads;

			public Axis2D(Vector2 origin, float angle, float z = 0, bool showArrowHeads = true)
			{
				Position = new Vector3(origin.x, origin.y, z);
				Angle = angle;
				ShowArrowHeads = showArrowHeads;
			}
			
#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (ShowArrowHeads)
				{
					new Arrow2D(Position, Angle).Draw(commandBuilder, ColorX, duration);
					new Arrow2D(Position, Angle + 90).Draw(commandBuilder, ColorY, duration);
				}
				else
				{
					new Ray2D(Position, Angle).Draw(commandBuilder, ColorX, duration);
					new Ray2D(Position, Angle + 90).Draw(commandBuilder, ColorY, duration);
				}
			}
			#endif
		}

		public struct Circle2D : IDrawable
		{
			private Circle _circle;

			public Matrix4x4 Matrix
			{
				get => _circle.Matrix;
				set => _circle = new Circle(value);
			}

			public Circle2D(Matrix4x4 matrix) => _circle = new Circle(matrix);

			public Circle2D(Vector2 origin, float radius, float z = 0)
				=> _circle = new Circle(new Vector3(origin.x, origin.y, z), Quaternion.identity, radius);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => _circle.Draw(commandBuilder, color, duration);
#endif
		}

		public struct Box2D : IDrawable
		{
			public Matrix4x4 Matrix;

			public Box2D(Vector2 origin, Vector2 size, float angle = 0) => Matrix = Matrix4x4.TRS(origin, Quaternion.AngleAxis(angle, Vector3.forward), size);

			public Box2D(Vector2 origin, Vector2 size, float z, float angle) => Matrix = Matrix4x4.TRS(new Vector3(origin.x, origin.y, z), Quaternion.AngleAxis(angle, Vector3.forward), size);

			public Box2D(Vector3 origin, Vector2 size, float angle = 0) => Matrix = Matrix4x4.TRS(origin, Quaternion.AngleAxis(angle, Vector3.forward), size);

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => commandBuilder.AppendBox2D(this, color, duration);
#endif
		}

		public struct Area2D : IDrawable
		{
			public Vector3 PointA, PointB;

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

		public struct Capsule2D : IDrawable
		{
			public Vector3 PointA => _pointA;
			public Vector3 PointB => _pointB;
			public float Radius => _radius;

			private readonly Vector3 _pointA, _pointB;
			private readonly float _radius;
			private readonly Vector3 _verticalDirection;
			private readonly Vector3 _scaledLeft;

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
#endif

			public Capsule2D(Vector2 point, Vector2 size, Direction capsuleDirection, float angle = 0)
				: this(point, size, capsuleDirection, angle, 0) { }

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
				_verticalDirection = RotateUsingCoefficients(Vector2.up, s, c);
				Vector2 verticalOffset = RotateUsingCoefficients(new Vector2(0, vertical), s, c);
				_pointA = GetVector3(point + verticalOffset, z);
				_pointB = GetVector3(point - verticalOffset, z);
				_scaledLeft = new Vector2(c, s) * _radius;

				Vector3 GetVector3(Vector2 v2, float z) => new Vector3(v2.x, v2.y, z);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				Angle halfCircle = Angle.FromTurns(0.5f);
				commandBuilder.AppendArc(new Arc(_pointA, Vector3.forward, _verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendArc(new Arc(_pointB, Vector3.forward, -_verticalDirection, _radius, halfCircle), color, duration);
				commandBuilder.AppendLine(new Line(_pointA + _scaledLeft, _pointB + _scaledLeft), color, duration);
				commandBuilder.AppendLine(new Line(_pointA - _scaledLeft, _pointB - _scaledLeft), color, duration);
			}
#endif
		}
	}
}