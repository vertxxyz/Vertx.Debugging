#if VERTX_PHYSICS_2D
using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public struct Raycast2D : IDrawableCast
		{
			public Vector3 Origin;
			public Vector2 Direction;
			public RaycastHit2D? Result;

			public Raycast2D(Vector2 origin, Vector2 direction, RaycastHit2D? result, float distance = Mathf.Infinity, float z = 0)
			{
				Origin = new Vector3(origin.x, origin.y, z);
				direction.EnsureNormalized();
				Direction = direction * GetClampedMaxDistance(distance);
				Result = result;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				commandBuilder.AppendRay(new Ray(Origin, Direction), castColor, duration);
				if (!Result.HasValue)
					return;
				commandBuilder.AppendRay(
					new Ray(
						new Vector3(Result.Value.point.x, Result.Value.point.y, Origin.z),
						Result.Value.normal
					),
					hitColor,
					duration
				);
			}
#endif
		}

		public struct RaycastAll2D : IDrawableCast
		{
			public Raycast2D Raycast;
			public RaycastHit2D[] Results;
			public int ResultCount;

			public RaycastAll2D(Vector2 origin, Vector2 direction, RaycastHit2D[] results, int resultCount, float distance = Mathf.Infinity, float z = 0)
			{
				Raycast = new Raycast2D(origin, direction, null, distance, z);
				Results = results;
				ResultCount = resultCount;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
				=> Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Raycast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					commandBuilder.AppendRay(new Ray(new Vector3(result.point.x, result.point.y, Raycast.Origin.z), result.normal), hitColor, duration);
				}
			}
#endif
		}

		public struct CircleCast : IDrawableCast
		{
			public Vector3 Origin;
			public float Radius;
			public Vector2 Direction;
			public float MaxDistance;
			public RaycastHit2D? Hit;

			public CircleCast(Vector2 origin, Vector2 direction, float radius, RaycastHit2D? hit, float maxDistance = Mathf.Infinity, float z = 0)
			{
				Origin = new Vector3(origin.x, origin.y, z);
				direction.EnsureNormalized();
				Direction = direction;
				Radius = radius;
				Hit = hit;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				new Circle(Origin, Quaternion.identity, Radius).Draw(commandBuilder, castColor, duration);
				new Capsule2D(Origin, Origin + (Vector3)(Direction * MaxDistance), Radius).Draw(commandBuilder, castColor, duration);
				if (Hit.HasValue)
					new Circle(Origin + (Vector3)(Direction * Hit.Value.distance), Quaternion.identity, Radius).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public struct CircleCastAll : IDrawableCast
		{
			public CircleCast CircleCast;
			public RaycastHit2D[] Results;
			public int ResultCount;

			public CircleCastAll(Vector2 origin, Vector2 direction, float radius, RaycastHit2D[] results, int resultCount, float maxDistance = Mathf.Infinity, float z = 0)
			{
				CircleCast = new CircleCast(origin, direction, radius, null, maxDistance, z);
				Results = results;
				ResultCount = resultCount;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				CircleCast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					new Circle(CircleCast.Origin + (Vector3)(CircleCast.Direction * result.distance), Quaternion.identity, CircleCast.Radius).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public struct BoxCast2D : IDrawableCast
		{
			public Box2D Box;
			public Vector2 Direction;
			public float MaxDistance;
			public RaycastHit2D? Hit;

			public BoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D? hit, float maxDistance = Mathf.Infinity, float z = 0)
			{
				Box = new Box2D(origin, size, angle, z);
				Hit = hit;
				direction.EnsureNormalized();
				Direction = direction;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Box.Draw(commandBuilder, castColor, duration);
				Vector3 offset = Direction * MaxDistance;

				Vector3 origin = Box.GetPoint(Box2D.Point.Origin);
				Vector3 tr = Box.GetPoint(Box2D.Point.TopRight);
				Vector3 bl = Box.GetPoint(Box2D.Point.BottomLeft);
				Vector3 tl = Box.GetPoint(Box2D.Point.TopLeft);
				Vector3 br = Box.GetPoint(Box2D.Point.BottomRight);

				float dotTR = Vector2.Dot(Direction, tr - origin);
				float dotTL = Vector2.Dot(Direction, tl - origin);

				// Joining lines
				if (Mathf.Abs(dotTR) < Mathf.Abs(dotTL))
				{
					commandBuilder.AppendLine(new Line(tr, tr + offset), castColor, duration);
					commandBuilder.AppendLine(new Line(bl, bl + offset), castColor, duration);
				}
				else
				{
					commandBuilder.AppendLine(new Line(tl, tl + offset), castColor, duration);
					commandBuilder.AppendLine(new Line(br, br + offset), castColor, duration);
				}

				// Draw the edges that make up the end of the cast. This better indicates the cast direction without having to colour it,
				// and without drawing a whole box where there's not really explanatory value in seeing it.
				Box2D endBox = Box.GetTranslated(offset);
				for (int i = 0; i < 4; i++)
				{
					var point = (Box2D.Point)(1 << i);
					Vector3 edge = Box.GetPoint(point);
					if (Vector2.Dot(Direction, edge - origin) <= 0)
						continue;
					int min = i - 1;
					if (min < 0)
						min = 3;
					int max = i + 1;
					if (max > 3)
						max = 0;
					Vector3 p1 = endBox.GetPoint(point | (Box2D.Point)(1 << min));
					Vector3 p2 = endBox.GetPoint(point | (Box2D.Point)(1 << max));
					commandBuilder.AppendLine(new Line(p1, p2), castColor, duration);
				}

				if (!Hit.HasValue)
					return;
				Box.GetTranslated(Direction * Hit.Value.distance).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public struct BoxCast2DAll : IDrawableCast
		{
			public BoxCast2D BoxCast;
			public RaycastHit2D[] Results;
			public int ResultCount;

			public BoxCast2DAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, int resultsCount, float maxDistance = Mathf.Infinity, float z = 0)
			{
				BoxCast = new BoxCast2D(origin, size, angle, direction, null, maxDistance, z);
				Results = results;
				ResultCount = resultsCount;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				BoxCast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					BoxCast.Box.GetTranslated(BoxCast.Direction * result.distance).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public struct CapsuleCast2D : IDrawableCast
		{
			public Capsule2D Capsule;
			public Vector2 Direction;
			public float MaxDistance;
			public RaycastHit2D? Hit;

			public CapsuleCast2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D? hit, float maxDistance = Mathf.Infinity, float z = 0)
			{
				Capsule = new Capsule2D(origin, size, capsuleDirection, angle, z);
				direction.EnsureNormalized();
				Direction = direction;
				Hit = hit;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Capsule.Draw(commandBuilder, castColor, duration);

				var verticalDirection = (Vector2)Capsule._verticalDirection;
				float dot = Vector2.Dot(PerpendicularClockwise(verticalDirection), Direction);
				float sign = Mathf.Sign(dot);
				float scaledRadius = sign * Capsule.Radius;
				Vector2 o1 = PerpendicularCounterClockwise(Direction) * scaledRadius;
				Vector2 o2 = PerpendicularClockwise(Direction) * scaledRadius;

				Capsule2D endCapsule = Capsule.GetTranslated(Direction * MaxDistance);
				commandBuilder.AppendLine(new Line(Capsule.PointA.Add(o1), endCapsule.PointA.Add(o1)), castColor, duration);
				commandBuilder.AppendLine(new Line(Capsule.PointB.Add(o2), endCapsule.PointB.Add(o2)), castColor, duration);

				// Termination capsule lines
				if (dot > 0)
					commandBuilder.AppendLine(new Line(endCapsule.PointA + endCapsule._scaledLeft, endCapsule.PointB + endCapsule._scaledLeft), castColor, duration);
				else if (dot < 0)
					commandBuilder.AppendLine(new Line(endCapsule.PointA - endCapsule._scaledLeft, endCapsule.PointB - endCapsule._scaledLeft), castColor, duration);

				// Termination capsule ends
				float angle = verticalDirection.ToAngleDegrees();

				float dotA = Vector2.Dot(verticalDirection, Direction);
				float dotB = -dotA;

				Angle offsetA = Angle.FromRadians(Mathf.Acos(dotA));
				Angle offsetB = Angle.FromRadians(Mathf.Acos(dotB));
				
				commandBuilder.AppendArc(new Arc(endCapsule.PointA, angle - offsetA.Degrees * 0.5f * sign, Capsule.Radius, offsetB.Abs()), castColor, duration);
				commandBuilder.AppendArc(new Arc(endCapsule.PointB, angle + 180 + offsetB.Degrees * 0.5f * sign, Capsule.Radius, offsetA.Abs()), castColor, duration);

				if (!Hit.HasValue)
					return;
				Capsule.GetTranslated(Direction * Hit.Value.distance).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public struct CapsuleCast2DAll : IDrawableCast
		{
			public CapsuleCast2D CapsuleCast;
			public RaycastHit2D[] Results;
			public int ResultCount;

			public CapsuleCast2DAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, int count, float maxDistance = Mathf.Infinity, float z = 0)
			{
				CapsuleCast = new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, null, maxDistance, z);
				Results = results;
				ResultCount = count;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				CapsuleCast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					CapsuleCast.Capsule.GetTranslated(CapsuleCast.Direction * result.distance).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}
	}
}
#endif