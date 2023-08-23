#if VERTX_PHYSICS_2D
using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
			public readonly float2 Origin;
			public readonly float2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Result;

			public Raycast2D(float2 origin, float2 direction, RaycastHit2D? result, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				Origin = origin;
				direction = math.normalizesafe(direction);
				Direction = direction;
				Distance = distance;
				Result = result;
				MinDepth = minDepth;
				MaxDepth = math.max(minDepth, maxDepth);
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				var originMin = new float3(Origin.x, Origin.y, MinDepth);
				if (MaxDepth - MinDepth < 0.001f)
				{
					commandBuilder.AppendRay(new Ray(originMin, Direction.xy0(), Distance), castColor, duration);
				}
				else
				{
					var originMax = new float3(Origin.x, Origin.y, MaxDepth);
					float maxDistance = GetClampedMaxDistance(Distance);
					float3 direction = (Direction * maxDistance).xy0();
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
			public readonly float2 PointA;
			public readonly float2 PointB;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Result;

			public Linecast2D(float2 pointA, float2 pointB, RaycastHit2D? result, float minDepth = 0, float maxDepth = 0)
			{
				PointA = pointA;
				PointB = pointB;
				Result = result;
				MinDepth = minDepth;
				MaxDepth = math.max(maxDepth, minDepth);
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				var aMin = new float3(PointA.x, PointA.y, MinDepth);
				var bMin = new float3(PointB.x, PointB.y, MinDepth);
				if (MaxDepth - MinDepth < 0.001f)
				{
					commandBuilder.AppendLine(new Line(aMin, bMin), castColor, duration);
				}
				else
				{
					var aMax = new float3(PointA.x, PointA.y, MaxDepth);
					var bMax = new float3(PointB.x, PointB.y, MaxDepth);
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

			public RaycastAll2D(float2 origin, float2 direction, IList<RaycastHit2D> results, int resultCount, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				Raycast = new Raycast2D(origin, direction, null, distance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public RaycastAll2D(float2 origin, float2 direction, IList<RaycastHit2D> results, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
				: this(origin, direction, results, results.Count, distance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Raycast.Draw(ref commandBuilder, castColor, hitColor, duration);
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

			public LinecastAll2D(float2 pointA, float2 pointB, IList<RaycastHit2D> results, int resultCount, float minDepth = 0, float maxDepth = 0)
			{
				Linecast = new Linecast2D(pointA, pointB, null, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public LinecastAll2D(float2 pointA, float2 pointB, IList<RaycastHit2D> results, float minDepth = 0, float maxDepth = 0)
				: this(pointA, pointB, results, results.Count, minDepth, maxDepth) { }

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);

			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Linecast.Draw(ref commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
					D.raw(Results[i], hitColor, duration);
			}
#endif
		}

		public readonly struct CircleCast : IDrawableCast
		{
			public readonly float2 Origin;
			public readonly float Radius;
			public readonly float2 Direction;
			public readonly float MaxDistance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public CircleCast(float2 origin, float radius, float2 direction, RaycastHit2D? hit, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction;
				Radius = radius;
				Hit = hit;
				MinDepth = minDepth;
				MaxDepth = math.max(minDepth, maxDepth);
				MaxDistance = maxDistance;
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				float maxDistance = GetClampedMaxDistance(MaxDistance);
				float3 originMin = new float3(Origin.x, Origin.y, MinDepth);
				float3 offset = (Direction * maxDistance).xy0();
				if (MaxDepth - MinDepth < 0.001f)
				{
					new Circle(originMin, quaternion.identity, Radius).Draw(ref commandBuilder, castColor, duration);
					new Capsule2D(originMin, originMin + offset, Radius).Draw(ref commandBuilder, castColor, duration);
				}
				else
				{
					new Circle(originMin, quaternion.identity, Radius).Draw(ref commandBuilder, castColor, duration);
					new Capsule2D(originMin, originMin + offset, Radius).Draw(ref commandBuilder, castColor, duration);

					float3 originMax = new float3(Origin.x, Origin.y, MaxDepth);

					new Circle(originMax, quaternion.identity, Radius).Draw(ref commandBuilder, castColor, duration);
					new Capsule2D(originMax, originMax + offset, Radius).Draw(ref commandBuilder, castColor, duration);
				}

				if (!Hit.HasValue || !Hit.Value)
					return;

				float2 hitPoint = Origin + Direction * Hit.Value.distance;
				new Circle(new float3(hitPoint.x, hitPoint.y, Hit.Value.transform.position.z), quaternion.identity, Radius).Draw(ref commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct CircleCastAll : IDrawableCast
		{
			public readonly CircleCast CircleCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public CircleCastAll(float2 origin, float radius, float2 direction, IList<RaycastHit2D> results, int resultCount, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				CircleCast = new CircleCast(origin, radius, direction, null, maxDistance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultCount;
			}

			public CircleCastAll(float2 origin, float radius, float2 direction, IList<RaycastHit2D> results, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
				: this(origin, radius, direction, results, results.Count, maxDistance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				CircleCast.Draw(ref commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					float2 hitPoint = CircleCast.Origin + CircleCast.Direction * result.distance;
					new Circle(new float3(hitPoint.x, hitPoint.y, result.transform.position.z), quaternion.identity, CircleCast.Radius).Draw(ref commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct BoxCast2D : IDrawableCast
		{
			public readonly Box2D Box;
			public readonly float2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public BoxCast2D(float2 origin, float2 size, float angleDegrees, float2 direction, RaycastHit2D? hit, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				MinDepth = minDepth;
				MaxDepth = math.max(minDepth, maxDepth);
				Box = new Box2D(origin, size, angleDegrees, minDepth);
				Hit = hit;
				direction.EnsureNormalized();
				Direction = direction;
				Distance = distance;
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				float maxDistance = GetClampedMaxDistance(Distance);

				bool hasMax = MaxDepth - MinDepth > 0.001f;

				Box.Draw(ref commandBuilder, castColor, duration);
				if (hasMax)
					Box.GetWithZ(MaxDepth).Draw(ref commandBuilder, castColor, duration);

				float3 offset = (Direction * maxDistance).xy0();

				float4x4 boxMatrix = Box.Matrix;
				float3 origin = Box2D.GetPoint(boxMatrix, Box2D.Point.Origin);
				float3 tr = Box2D.GetPoint(boxMatrix, Box2D.Point.TopRight);
				float3 bl = Box2D.GetPoint(boxMatrix, Box2D.Point.BottomLeft);
				float3 tl = Box2D.GetPoint(boxMatrix, Box2D.Point.TopLeft);
				float3 br = Box2D.GetPoint(boxMatrix, Box2D.Point.BottomRight);

				float dotTR = math.dot(Direction, (tr - origin).xy);
				float dotTL = math.dot(Direction, (tl - origin).xy);

				// Joining lines
				if (math.abs(dotTR) < math.abs(dotTL))
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
				float4x4 endBox = Box.GetTranslated(offset).Matrix;
				for (int i = 0; i < 4; i++)
				{
					var point = (Box2D.Point)(1 << i);
					float3 edge = Box2D.GetPoint(boxMatrix, point);
					if (math.dot(Direction, (edge - origin).xy) <= 0)
						continue;
					int min = i - 1;
					if (min < 0)
						min = 3;
					int max = i + 1;
					if (max > 3)
						max = 0;
					float3 p1 = Box2D.GetPoint(endBox, point | (Box2D.Point)(1 << min));
					float3 p2 = Box2D.GetPoint(endBox, point | (Box2D.Point)(1 << max));
					commandBuilder.AppendLine(new Line(p1, p2), castColor, duration);
					if (!hasMax)
						continue;
					p1.z = MaxDepth;
					p2.z = MaxDepth;
					commandBuilder.AppendLine(new Line(p1, p2), castColor, duration);
				}

				if (!Hit.HasValue || !Hit.Value)
					return;

				Box.GetTranslatedWithZ(Direction * Hit.Value.distance, Hit.Value.transform.position.z).Draw(ref commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct BoxCast2DAll : IDrawableCast
		{
			public readonly BoxCast2D BoxCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public BoxCast2DAll(float2 origin, float2 size, float angleDegrees, float2 direction, IList<RaycastHit2D> results, int resultsCount, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				BoxCast = new BoxCast2D(origin, size, angleDegrees, direction, null, distance, minDepth, maxDepth);
				Results = results;
				ResultCount = resultsCount;
			}

			public BoxCast2DAll(float2 origin, float2 size, float angleDegrees, float2 direction, IList<RaycastHit2D> results, float distance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
				: this(origin, size, angleDegrees, direction, results, results.Count, distance, minDepth, maxDepth) { }

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				BoxCast.Draw(ref commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					BoxCast.Box.GetTranslatedWithZ(BoxCast.Direction * result.distance, result.transform.position.z).Draw(ref commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct CapsuleCast2D : IDrawableCast
		{
			public readonly Capsule2D Capsule;
			public readonly float2 Direction;
			public readonly float Distance;
			public readonly float MinDepth, MaxDepth;
			public readonly RaycastHit2D? Hit;

			public CapsuleCast2D(float2 origin, float2 size, CapsuleDirection2D capsuleDirection, float angle, float2 direction, RaycastHit2D? hit, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				Capsule = new Capsule2D(origin, size, capsuleDirection, angle, minDepth);
				direction.EnsureNormalized();
				Direction = direction;
				Hit = hit;
				Distance = maxDistance;
				MinDepth = minDepth;
				MaxDepth = math.max(minDepth, maxDepth);
			}

#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				bool hasMax = MaxDepth - MinDepth > 0.001f;

				Capsule.Draw(ref commandBuilder, castColor, duration);
				if (hasMax)
					Capsule.GetWithZ(MaxDepth).Draw(ref commandBuilder, castColor, duration);

				float maxDistance = GetClampedMaxDistance(Distance);

				float2 verticalDirection = Capsule._verticalDirection.xy;
				float dot = math.dot(PerpendicularClockwise(verticalDirection), Direction);
				float sign = math.sign(dot);
				float scaledRadius = sign * Capsule.Radius;
				float2 o1 = PerpendicularCounterClockwise(Direction) * scaledRadius;
				float2 o2 = PerpendicularClockwise(Direction) * scaledRadius;

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

				float dotA = math.clamp(math.dot(verticalDirection, Direction), -1, 1); // We clamp to ensure Acos gets valid values.
				float dotB = -dotA;

				Angle offsetA = Angle.FromRadians(math.acos(dotA));
				Angle offsetB = Angle.FromRadians(math.acos(dotB));

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
				
				Capsule.GetTranslatedWithZ(Direction * Hit.Value.distance, Hit.Value.transform.position.z).Draw(ref commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct CapsuleCast2DAll : IDrawableCast
		{
			public readonly CapsuleCast2D CapsuleCast;
			public readonly IList<RaycastHit2D> Results;
			public readonly int ResultCount;

			public CapsuleCast2DAll(float2 origin, float2 size, CapsuleDirection2D capsuleDirection, float angleDegrees, float2 direction, IList<RaycastHit2D> results, int count, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
			{
				CapsuleCast = new CapsuleCast2D(origin, size, capsuleDirection, angleDegrees, direction, null, maxDistance, minDepth, maxDepth);
				Results = results;
				ResultCount = count;
			}

			public CapsuleCast2DAll(float2 origin, float2 size, CapsuleDirection2D capsuleDirection, float angleDegrees, float2 direction, IList<RaycastHit2D> results, float maxDistance = math.INFINITY, float minDepth = 0, float maxDepth = 0)
				: this(origin, size, capsuleDirection, angleDegrees, direction, results, results.Count, maxDistance, minDepth, maxDepth) { }


#if UNITY_EDITOR
			void IDrawable.Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
				=> Draw(ref commandBuilder, color, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color color, float duration)
			{
				if (IsWhite(color))
					Draw(ref commandBuilder, CastColor, HitColor, duration);
				else
					Draw(ref commandBuilder, color, color, duration);
			}

			void IDrawableCast.Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
				=> Draw(ref commandBuilder, castColor, hitColor, duration);
			
			internal void Draw(ref UnmanagedCommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				CapsuleCast.Draw(ref commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit2D result = Results[i];
					CapsuleCast.Capsule.GetTranslated(CapsuleCast.Direction * result.distance).Draw(ref commandBuilder, hitColor, duration);
				}
			}
#endif
		}
	}
}
#endif