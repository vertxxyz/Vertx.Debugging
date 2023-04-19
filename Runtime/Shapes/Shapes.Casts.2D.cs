#if VERTX_PHYSICS_2D
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shape
	{
		public readonly struct Raycast2D : IDrawableCast
		{
			public readonly Vector2 Origin;
			public readonly Vector2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Result;

			public Raycast2D(Vector2 origin, Vector2 direction, RaycastHit2D? result, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction;
				Distance = distance;
				Result = result;
				MinDepth = minDepth;
				MaxDepth = Mathf.Max(minDepth, maxDepth);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				var originMin = new Vector3(Origin.x, Origin.y, MinDepth);
				if (MaxDepth - MinDepth < 0.001f)
				{
					commandBuilder.AppendRay(new Ray(originMin, Direction, Distance), castColor, duration);
				}
				else
				{
					var originMax = new Vector3(Origin.x, Origin.y, MaxDepth);
					float maxDistance = GetClampedMaxDistance(Distance);
					Vector3 direction = Direction * maxDistance;
					commandBuilder.AppendRay(new Ray(originMin, direction), castColor, duration);
					commandBuilder.AppendDashedLine(new DashedLine(originMin, originMax), castColor, duration);
					if (!float.IsInfinity(Distance))
						commandBuilder.AppendDashedLine(new DashedLine(originMin + direction, originMax + direction), castColor, duration);
					commandBuilder.AppendRay(new Ray(originMax, direction), castColor, duration);
				}

				if (!Result.HasValue)
					return;

				D.raw(Result.Value, hitColor, duration);
			}
#endif
		}

		public readonly struct Linecast2D : IDrawableCast
		{
			public readonly Vector2 PointA;
			public readonly Vector2 PointB;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Result;

			public Linecast2D(Vector2 pointA, Vector2 pointB, RaycastHit2D? result, float minDepth = 0, float maxDepth = 0)
			{
				PointA = pointA;
				PointB = pointB;
				Result = result;
				MinDepth = minDepth;
				MaxDepth = Mathf.Max(maxDepth, minDepth);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				var aMin = new Vector3(PointA.x, PointA.y, MinDepth);
				var bMin = new Vector3(PointB.x, PointB.y, MinDepth);
				if (MaxDepth - MinDepth < 0.001f)
				{
					commandBuilder.AppendLine(new Line(aMin, bMin), castColor, duration);
				}
				else
				{
					var aMax = new Vector3(PointA.x, PointA.y, MaxDepth);
					var bMax = new Vector3(PointB.x, PointB.y, MaxDepth);
					commandBuilder.AppendLine(new Line(aMin, bMin), castColor, duration);
					commandBuilder.AppendDashedLine(new DashedLine(aMin, aMax), castColor, duration);
					commandBuilder.AppendDashedLine(new DashedLine(bMin, bMax), castColor, duration);
					commandBuilder.AppendLine(new Line(aMax, bMax), castColor, duration);
				}

				if (!Result.HasValue)
					return;

				D.raw(Result.Value, hitColor, duration);
			}
#endif
		}

		public readonly struct RaycastAll2D : IDrawableCast
		{
			public readonly Raycast2D Raycast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public RaycastAll2D(Vector2 origin, Vector2 direction, IList<RaycastHit2D> results, int resultCount, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				Raycast = new Raycast2D(origin, direction, null, distance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public RaycastAll2D(Vector2 origin, Vector2 direction, IList<RaycastHit2D> results, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
				: this(origin, direction, results, results.Count, distance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Raycast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
					D.raw(Results[i], hitColor, duration);
			}
#endif
		}

		public readonly struct LinecastAll2D : IDrawableCast
		{
			public readonly Linecast2D Linecast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public LinecastAll2D(Vector2 pointA, Vector2 pointB, IList<RaycastHit2D> results, int resultCount, float minDepth = 0, float maxDepth = 0)
			{
				Linecast = new Linecast2D(pointA, pointB, null, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public LinecastAll2D(Vector2 pointA, Vector2 pointB, IList<RaycastHit2D> results, float minDepth = 0, float maxDepth = 0)
				: this(pointA, pointB, results, results.Count, minDepth, maxDepth) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Linecast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
					D.raw(Results[i], hitColor, duration);
			}
#endif
		}

		public readonly struct CircleCast : IDrawableCast
		{
			public readonly Vector2 Origin;
			public readonly float Radius;
			public readonly Vector2 Direction;
			public readonly float MaxDistance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public CircleCast(Vector2 origin, float radius, Vector2 direction, RaycastHit2D? hit, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction;
				Radius = radius;
				Hit = hit;
				MinDepth = minDepth;
				MaxDepth = Mathf.Max(minDepth, maxDepth);
				MaxDistance = maxDistance;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				float maxDistance = GetClampedMaxDistance(MaxDistance);
				Vector3 originMin = new Vector3(Origin.x, Origin.y, MinDepth);
				var offset = (Vector3)(Direction * maxDistance);
				if (MaxDepth - MinDepth < 0.001f)
				{
					new Circle(originMin, Quaternion.identity, Radius).Draw(commandBuilder, castColor, duration);
					new Capsule2D(originMin, originMin + offset, Radius).Draw(commandBuilder, castColor, duration);
				}
				else
				{
					new Circle(originMin, Quaternion.identity, Radius).Draw(commandBuilder, castColor, duration);
					new Capsule2D(originMin, originMin + offset, Radius).Draw(commandBuilder, castColor, duration);

					Vector3 originMax = new Vector3(Origin.x, Origin.y, MaxDepth);

					new Circle(originMax, Quaternion.identity, Radius).Draw(commandBuilder, castColor, duration);
					new Capsule2D(originMax, originMax + offset, Radius).Draw(commandBuilder, castColor, duration);
				}

				if (!Hit.HasValue || !Hit.Value)
					return;

				Vector2 hitPoint = Origin + Direction * Hit.Value.distance;
				new Circle(new Vector3(hitPoint.x, hitPoint.y, Hit.Value.transform.position.z), Quaternion.identity, Radius).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct CircleCastAll : IDrawableCast
		{
			public readonly CircleCast CircleCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public CircleCastAll(Vector2 origin, float radius, Vector2 direction, IList<RaycastHit2D> results, int resultCount, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				CircleCast = new CircleCast(origin, radius, direction, null, maxDistance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public CircleCastAll(Vector2 origin, float radius, Vector2 direction, IList<RaycastHit2D> results, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
				: this(origin, radius, direction, results, results.Count, maxDistance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				CircleCast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					Vector2 hitPoint = CircleCast.Origin + CircleCast.Direction * result.distance;
					new Circle(new Vector3(hitPoint.x, hitPoint.y, result.transform.position.z), Quaternion.identity, CircleCast.Radius).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct BoxCast2D : IDrawableCast
		{
			public readonly Box2D Box;
			public readonly Vector2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public BoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D? hit, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				MinDepth = minDepth;
				MaxDepth = Mathf.Max(minDepth, maxDepth);
				Box = new Box2D(origin, size, angle, minDepth);
				Hit = hit;
				direction.EnsureNormalized();
				Direction = direction;
				Distance = distance;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				float maxDistance = GetClampedMaxDistance(Distance);

				bool hasMax = MaxDepth - MinDepth > 0.001f;

				Box.Draw(commandBuilder, castColor, duration);
				if (hasMax)
					Box.GetWithZ(MaxDepth).Draw(commandBuilder, castColor, duration);

				Vector3 offset = Direction * maxDistance;

				Matrix4x4 boxMatrix = Box.Matrix;
				Vector3 origin = Box2D.GetPoint(boxMatrix, Box2D.Point.Origin);
				Vector3 tr = Box2D.GetPoint(boxMatrix, Box2D.Point.TopRight);
				Vector3 bl = Box2D.GetPoint(boxMatrix, Box2D.Point.BottomLeft);
				Vector3 tl = Box2D.GetPoint(boxMatrix, Box2D.Point.TopLeft);
				Vector3 br = Box2D.GetPoint(boxMatrix, Box2D.Point.BottomRight);

				float dotTR = Vector2.Dot(Direction, tr - origin);
				float dotTL = Vector2.Dot(Direction, tl - origin);

				// Joining lines
				if (Mathf.Abs(dotTR) < Mathf.Abs(dotTL))
				{
					commandBuilder.AppendLine(new Line(tr, tr + offset), castColor, duration);
					commandBuilder.AppendLine(new Line(bl, bl + offset), castColor, duration);
					if (hasMax)
					{
						tr.z = MaxDepth;
						bl.z = MaxDepth;
						commandBuilder.AppendLine(new Line(tr, tr + offset), castColor, duration);
						commandBuilder.AppendLine(new Line(bl, bl + offset), castColor, duration);
					}
				}
				else
				{
					commandBuilder.AppendLine(new Line(tl, tl + offset), castColor, duration);
					commandBuilder.AppendLine(new Line(br, br + offset), castColor, duration);
					if (hasMax)
					{
						tl.z = MaxDepth;
						br.z = MaxDepth;
						commandBuilder.AppendLine(new Line(tl, tl + offset), castColor, duration);
						commandBuilder.AppendLine(new Line(br, br + offset), castColor, duration);
					}
				}

				// Draw the edges that make up the end of the cast. This better indicates the cast direction without having to colour it,
				// and without drawing a whole box where there's not really explanatory value in seeing it.
				Matrix4x4 endBox = Box.GetTranslated(offset).Matrix;
				for (int i = 0; i < 4; i++)
				{
					var point = (Box2D.Point)(1 << i);
					Vector3 edge = Box2D.GetPoint(boxMatrix, point);
					if (Vector2.Dot(Direction, edge - origin) <= 0)
						continue;
					int min = i - 1;
					if (min < 0)
						min = 3;
					int max = i + 1;
					if (max > 3)
						max = 0;
					Vector3 p1 = Box2D.GetPoint(endBox, point | (Box2D.Point)(1 << min));
					Vector3 p2 = Box2D.GetPoint(endBox, point | (Box2D.Point)(1 << max));
					commandBuilder.AppendLine(new Line(p1, p2), castColor, duration);
					if (!hasMax)
						continue;
					p1.z = MaxDepth;
					p2.z = MaxDepth;
					commandBuilder.AppendLine(new Line(p1, p2), castColor, duration);
				}

				if (!Hit.HasValue || !Hit.Value)
					return;

				Box.GetTranslatedWithZ(Direction * Hit.Value.distance, Hit.Value.transform.position.z).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct BoxCast2DAll : IDrawableCast
		{
			public readonly BoxCast2D BoxCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public BoxCast2DAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, IList<RaycastHit2D> results, int resultsCount, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				BoxCast = new BoxCast2D(origin, size, angle, direction, null, distance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultsCount;
			}

			public BoxCast2DAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, IList<RaycastHit2D> results, float distance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
				: this(origin, size, angle, direction, results, results.Count, distance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				BoxCast.Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					BoxCast.Box.GetTranslatedWithZ(BoxCast.Direction * result.distance, result.transform.position.z).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct CapsuleCast2D : IDrawableCast
		{
			public readonly Capsule2D Capsule;
			public readonly Vector2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public CapsuleCast2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D? hit, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				Capsule = new Capsule2D(origin, size, capsuleDirection, angle, minDepth);
				direction.EnsureNormalized();
				Direction = direction;
				Hit = hit;
				Distance = maxDistance;
				MinDepth = minDepth;
				MaxDepth = Mathf.Max(minDepth, maxDepth);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				bool hasMax = MaxDepth - MinDepth > 0.001f;

				Capsule.Draw(commandBuilder, castColor, duration);
				if (hasMax)
					Capsule.GetWithZ(MaxDepth).Draw(commandBuilder, castColor, duration);

				float maxDistance = GetClampedMaxDistance(Distance);

				var verticalDirection = (Vector2)Capsule._verticalDirection;
				float dot = Vector2.Dot(PerpendicularClockwise(verticalDirection), Direction);
				float sign = Mathf.Sign(dot);
				float scaledRadius = sign * Capsule.Radius;
				Vector2 o1 = PerpendicularCounterClockwise(Direction) * scaledRadius;
				Vector2 o2 = PerpendicularClockwise(Direction) * scaledRadius;

				Capsule2D endCapsule = Capsule.GetTranslated(Direction * maxDistance);
				var lineA = new Line(Capsule.PointA.Add(o1), endCapsule.PointA.Add(o1));
				var lineB = new Line(Capsule.PointB.Add(o2), endCapsule.PointB.Add(o2));
				commandBuilder.AppendLine(lineA, castColor, duration);
				commandBuilder.AppendLine(lineB, castColor, duration);
				if (hasMax)
				{
					commandBuilder.AppendLine(lineA.GetWithZ(MaxDepth), castColor, duration);
					commandBuilder.AppendLine(lineB.GetWithZ(MaxDepth), castColor, duration);
				}

				// Toggle off hasMax if it's not going to be drawn on-screen anyway.
				hasMax &= !float.IsInfinity(Distance);

				// Termination capsule lines
				if (dot > 0)
				{
					lineA = new Line(endCapsule.PointA + endCapsule._scaledLeft, endCapsule.PointB + endCapsule._scaledLeft);
					commandBuilder.AppendLine(lineA, castColor, duration);
					if (hasMax)
						commandBuilder.AppendLine(lineA.GetWithZ(MaxDepth), castColor, duration);
				}
				else if (dot < 0)
				{
					lineB = new Line(endCapsule.PointA - endCapsule._scaledLeft, endCapsule.PointB - endCapsule._scaledLeft);
					commandBuilder.AppendLine(lineB, castColor, duration);
					if (hasMax)
						commandBuilder.AppendLine(lineA.GetWithZ(MaxDepth), castColor, duration);
				}

				// Termination capsule ends
				float angle = verticalDirection.ToAngleDegrees();

				float dotA = Vector2.Dot(verticalDirection, Direction);
				float dotB = -dotA;

				Angle offsetA = Angle.FromRadians(Mathf.Acos(dotA));
				Angle offsetB = Angle.FromRadians(Mathf.Acos(dotB));

				Arc arcA = new Arc(endCapsule.PointA, angle - offsetA.Degrees * 0.5f * sign, Capsule.Radius, offsetB.Abs());
				Arc arcB = new Arc(endCapsule.PointB, angle + 180 + offsetB.Degrees * 0.5f * sign, Capsule.Radius, offsetA.Abs());
				commandBuilder.AppendArc(arcA, castColor, duration);
				commandBuilder.AppendArc(arcB, castColor, duration);
				if (hasMax)
				{
					commandBuilder.AppendArc(arcA.GetWithZ(MaxDepth), castColor, duration);
					commandBuilder.AppendArc(arcB.GetWithZ(MaxDepth), castColor, duration);
				}

				if (!Hit.HasValue || !Hit.Value)
					return;
				
				Capsule.GetTranslatedWithZ(Direction * Hit.Value.distance, Hit.Value.transform.position.z).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct CapsuleCast2DAll : IDrawableCast
		{
			public readonly CapsuleCast2D CapsuleCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public CapsuleCast2DAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, IList<RaycastHit2D> results, int count, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
			{
				CapsuleCast = new CapsuleCast2D(origin, size, capsuleDirection, angle, direction, null, maxDistance, minDepth, maxDepth);
				Results = results;
				ResultCount = count;
			}

			public CapsuleCast2DAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, IList<RaycastHit2D> results, float maxDistance = Mathf.Infinity, float minDepth = 0, float maxDepth = 0)
				: this(origin, size, capsuleDirection, angle, direction, results, results.Count, maxDistance, minDepth, maxDepth) { }


#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(commandBuilder, CastColor, HitColor, duration);
				else
					Draw(commandBuilder, color, color, duration);
			}

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