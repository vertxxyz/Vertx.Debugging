using UnityEngine;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public readonly struct Raycast : IDrawableCast
		{
			public readonly Ray Ray;
			public readonly RaycastHit Hit;

			public Raycast(Vector3 origin, Vector3 direction, RaycastHit hit, float distance = Mathf.Infinity)
			{
				Ray = new Ray(origin, direction, distance);
				Hit = hit;
			}

			public Raycast(UnityEngine.Ray ray, RaycastHit hit, float distance = Mathf.Infinity) : this(ray.origin, ray.direction, hit, distance) { }

#if UNITY_EDITOR

			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Ray.Draw(commandBuilder, castColor, duration);
				new SurfacePoint(Hit.point, Hit.normal).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}

		public readonly struct RaycastAll : IDrawableCast
		{
			public readonly Ray Ray;
			public readonly RaycastHit[] Results;
			public readonly int ResultCount;

			public RaycastAll(Vector3 origin, Vector3 direction, RaycastHit[] results, int resultCount, float distance = Mathf.Infinity)
			{
				direction.EnsureNormalized();
				Ray = new Ray(origin, direction, distance);
				Results = results;
				ResultCount = resultCount;
			}

			public RaycastAll(Vector3 origin, Vector3 direction, RaycastHit[] results, float distance = Mathf.Infinity) : this(origin, direction, results, results.Length, distance) { }

			public RaycastAll(UnityEngine.Ray ray, RaycastHit[] results, int resultCount, float distance = Mathf.Infinity) : this(ray.origin, ray.direction, results, resultCount, distance) { }

			public RaycastAll(UnityEngine.Ray ray, RaycastHit[] results, float distance = Mathf.Infinity) : this(ray, results, results.Length, distance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Ray.Draw(commandBuilder, castColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit result = Results[i];
					new SurfacePoint(result.point, result.normal).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct SphereCast : IDrawableCast
		{
			// Not using a Sphere because it has unnecessary overhead baked into the orientation.
			public readonly Vector3 Origin;
			public readonly float Radius;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit? Hit;

			public SphereCast(Vector3 origin, Vector3 direction, float radius, RaycastHit? hit, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction;
				Radius = radius;
				Hit = hit;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

			public SphereCast(UnityEngine.Ray ray, float radius, RaycastHit? hit, float maxDistance = Mathf.Infinity) : this(ray.origin, ray.direction, radius, hit, maxDistance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Quaternion orientation = Quaternion.LookRotation(Direction);
				new Sphere(Origin, orientation, Radius).Draw(commandBuilder, castColor, duration);
				Vector3 endPos = Origin + Direction * MaxDistance;
				new Hemisphere(endPos, orientation, Radius).Draw(commandBuilder, castColor, duration);

				commandBuilder.AppendOutline(new Outline(Origin, endPos, Radius), castColor, duration);
				commandBuilder.AppendOutline(new Outline(endPos, Origin, Radius), castColor, duration);
				if (Hit.HasValue)
					new Sphere(Origin + Direction * Hit.Value.distance, Quaternion.LookRotation(Hit.Value.normal), Radius).Draw(commandBuilder, hitColor, duration, Axes.X | Axes.Z);
			}
#endif
		}

		public readonly struct SphereCastAll : IDrawableCast
		{
			// Not using a Sphere because it has unnecessary overhead baked into the orientation.
			public readonly Vector3 Origin;
			public readonly float Radius;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit[] Results;
			public readonly int ResultCount;

			public SphereCastAll(Vector3 origin, Vector3 direction, float radius, RaycastHit[] results, int resultCount, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
				direction.EnsureNormalized();
				Direction = direction;
				Radius = radius;
				Results = results;
				ResultCount = resultCount;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

			public SphereCastAll(Vector3 origin, Vector3 direction, float radius, RaycastHit[] results, float maxDistance = Mathf.Infinity) : this(origin, direction, radius, results, results.Length, maxDistance) { }

			public SphereCastAll(UnityEngine.Ray ray, float radius, RaycastHit[] results, int resultCount, float distance = Mathf.Infinity) : this(ray.origin, ray.direction, radius, results, resultCount, distance) { }

			public SphereCastAll(UnityEngine.Ray ray, float radius, RaycastHit[] results, float distance = Mathf.Infinity) : this(ray, radius, results, results.Length, distance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				new SphereCast(Origin, Direction, Radius, null, MaxDistance).Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit result = Results[i];
					new Sphere(Origin + Direction * result.distance, Quaternion.LookRotation(result.normal), Radius)
						.Draw(commandBuilder, hitColor, duration, Axes.X | Axes.Z);
				}
			}
#endif
		}

		public readonly struct BoxCast : IDrawableCast
		{
			public readonly Box Box;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit? Hit;

			public BoxCast(Box box, Vector3 direction, RaycastHit? hit, float maxDistance = Mathf.Infinity)
			{
				Box = box;
				Direction = direction;
				Hit = hit;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

			public BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit? hit, Quaternion orientation, float maxDistance = Mathf.Infinity)
				: this(new Box(center, halfExtents, orientation), direction, hit, maxDistance) { }

			public BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit? hit, float maxDistance = Mathf.Infinity)
				: this(center, halfExtents, direction, hit, Quaternion.identity, maxDistance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Box.Draw(commandBuilder, castColor, duration);
				if (Hit.HasValue)
					Box.GetTranslated(Direction * Hit.Value.distance).Draw(commandBuilder, hitColor, duration);

				Vector3 offset = Direction * MaxDistance;
				
				// Draw connectors
				Matrix4x4 boxMatrix = Box.Matrix;
				Vector3 position = boxMatrix.MultiplyPoint3x4(Vector3.zero);
				Vector3 up = boxMatrix.MultiplyPoint3x4(new Vector3(0, 1, 0)) - position;
				Vector3 right = boxMatrix.MultiplyPoint3x4(new Vector3(1, 0, 0)) - position;
				Vector3 forward = boxMatrix.MultiplyPoint3x4(new Vector3(0, 0, 1)) - position;

				BoxUtility.Direction direction = BoxUtility.ConstructDirection(
					Vector3.Dot(right, Direction),
					Vector3.Dot(up, Direction),
					Vector3.Dot(forward, Direction)
				);

				/*foreach (var point in BoxUtility.Points)
				{
					BoxUtility.Direction matchedDirections = point.Direction & direction;
					if (matchedDirections == 0)
						continue;
					int count = BoxUtility.CountDirections(matchedDirections);
					if (count != 1 && count != 2)
						continue;
					Vector3 coordinate = Box.Matrix.MultiplyPoint3x4(point.Coordinate) - position;
					commandBuilder.AppendOutline(new Outline(position, offset, coordinate), castColor, duration, DrawModifications.Custom2);
				}*/
				commandBuilder.AppendCast(new Cast(boxMatrix, offset), castColor, duration);
				
				// Draw box end
				Matrix4x4 boxEnd = Box.GetTranslated(offset).Matrix;
				foreach (var edge in BoxUtility.Edges)
				{
					BoxUtility.Direction matchedDirections = edge.Direction & direction;
					if (matchedDirections == 0)
						continue;
					Vector3 a = boxEnd.MultiplyPoint3x4(edge.A);
					Vector3 b = boxEnd.MultiplyPoint3x4(edge.B);
					commandBuilder.AppendOutline(new Outline(a, b, boxEnd.MultiplyVector(edge.A + edge.B)), castColor, duration, DrawModifications.Custom2);
				}
			}
#endif
		}

		public readonly struct BoxCastAll : IDrawableCast
		{
			public readonly Box Box;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit[] Results;
			public readonly int ResultCount;

			public BoxCastAll(Box box, Vector3 direction, RaycastHit[] results, int count, float maxDistance = Mathf.Infinity)
			{
				Box = box;
				Direction = direction;
				Results = results;
				ResultCount = count;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

			public BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, int count, Quaternion orientation, float maxDistance = Mathf.Infinity)
				: this(new Box(center, halfExtents, orientation), direction, results, count, maxDistance) { }

			public BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, int count, float maxDistance = Mathf.Infinity)
				: this(center, halfExtents, direction, results, count, Quaternion.identity, maxDistance) { }

			public BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance = Mathf.Infinity)
				: this(center, halfExtents, direction, results, results.Length, orientation, maxDistance) { }

			public BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity)
				: this(center, halfExtents, direction, results, results.Length, Quaternion.identity, maxDistance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				new BoxCast(Box, Direction, null, MaxDistance).Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit result = Results[i];
					Box.GetTranslated(Direction * result.distance).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}

		public readonly struct CapsuleCast : IDrawableCast
		{
			public readonly Capsule Capsule;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit? Hit;

			public CapsuleCast(Capsule capsule, Vector3 direction, RaycastHit? hit, float maxDistance = Mathf.Infinity)
			{
				Capsule = capsule;
				Direction = direction;
				Hit = hit;
				MaxDistance = GetClampedMaxDistance(maxDistance);
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				Capsule.Draw(commandBuilder, castColor, duration);
				Capsule endCapsule = Capsule.GetTranslated(Direction * MaxDistance);
				endCapsule.Draw(commandBuilder, castColor, duration);
				Vector3 radiusA = (Capsule.SpherePosition1 - Capsule.SpherePosition2).normalized * Capsule.Radius;
				Vector3 radiusB = -radiusA;
				commandBuilder.AppendOutline(new Outline(Capsule.SpherePosition1, endCapsule.SpherePosition1, radiusA), castColor, duration, DrawModifications.Custom);
				commandBuilder.AppendOutline(new Outline(endCapsule.SpherePosition1, Capsule.SpherePosition1, radiusA), castColor, duration, DrawModifications.Custom);
				commandBuilder.AppendOutline(new Outline(endCapsule.SpherePosition2, Capsule.SpherePosition2, radiusB), castColor, duration, DrawModifications.Custom);
				commandBuilder.AppendOutline(new Outline(Capsule.SpherePosition2, endCapsule.SpherePosition2, radiusB), castColor, duration, DrawModifications.Custom);
				if (Hit.HasValue)
					Capsule.GetTranslated(Direction * Hit.Value.distance).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}


		public readonly struct CapsuleCastAll : IDrawableCast
		{
			public readonly Capsule Capsule;
			public readonly Vector3 Direction;
			public readonly float MaxDistance;
			public readonly RaycastHit[] Results;
			public readonly int ResultCount;

			public CapsuleCastAll(Capsule capsule, Vector3 direction, RaycastHit[] results, int count, float maxDistance = Mathf.Infinity)
			{
				Capsule = capsule;
				Direction = direction;
				Results = results;
				ResultCount = count;
				MaxDistance = maxDistance;
			}

			public CapsuleCastAll(Capsule capsule, Vector3 direction, RaycastHit[] results, float maxDistance = Mathf.Infinity)
				: this(capsule, direction, results, results.Length, maxDistance) { }

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);

			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration)
			{
				new CapsuleCast(Capsule, Direction, null, MaxDistance).Draw(commandBuilder, castColor, hitColor, duration);
				for (int i = 0; i < ResultCount; i++)
				{
					RaycastHit result = Results[i];
					Capsule.GetTranslated(Direction * result.distance).Draw(commandBuilder, hitColor, duration);
				}
			}
#endif
		}
	}
}