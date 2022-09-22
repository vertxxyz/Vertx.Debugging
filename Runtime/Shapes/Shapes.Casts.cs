using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
		public struct Raycast : IDrawableCast
		{
			public Ray Ray;
			public RaycastHit Hit;

			public Raycast(Vector3 origin, Vector3 direction, RaycastHit hit, float distance = Mathf.Infinity)
			{
				direction.EnsureNormalized();
				Ray = new Ray(origin, direction * GetClampedMaxDistance(distance));
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

		public struct RaycastAll : IDrawableCast
		{
			public Ray Ray;
			public RaycastHit[] Results;
			public int ResultCount;

			public RaycastAll(Vector3 origin, Vector3 direction, RaycastHit[] results, int resultCount, float distance = Mathf.Infinity)
			{
				direction.EnsureNormalized();
				Ray = new Ray(origin, direction * GetClampedMaxDistance(distance));
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

		public struct SphereCast : IDrawableCast
		{
			// Not using a Sphere because it has unnecessary overhead baked into the orientation.
			public Vector3 Origin;
			public float Radius;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit? Hit;

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
				new Sphere(Origin + Direction * MaxDistance, orientation, Radius).Draw(commandBuilder, castColor, duration);
				// TODO draw connecting lines
				if (Hit.HasValue)
					new Sphere(Origin + Direction * Hit.Value.distance, Quaternion.LookRotation(Hit.Value.normal), Radius).Draw(commandBuilder, hitColor, duration, Axes.X | Axes.Z);
			}
#endif
		}

		public struct SphereCastAll : IDrawableCast
		{
			// Not using a Sphere because it has unnecessary overhead baked into the orientation.
			public Vector3 Origin;
			public float Radius;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit[] Results;
			public int ResultCount;

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

		public struct BoxCast : IDrawableCast
		{
			public Box Box;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit? Hit;

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

				// Draw connectors
				Vector3 position = Box.Matrix.MultiplyPoint3x4(Vector3.zero);
				Vector3 up = Box.Matrix.MultiplyPoint3x4(new Vector3(0, 1, 0)) - position;
				Vector3 right = Box.Matrix.MultiplyPoint3x4(new Vector3(1, 0, 0)) - position;
				Vector3 forward = Box.Matrix.MultiplyPoint3x4(new Vector3(0, 0, 1)) - position;

				BoxUtility.Direction direction = BoxUtility.ConstructDirection(
					Vector3.Dot(right, Direction),
					Vector3.Dot(up, Direction),
					Vector3.Dot(forward, Direction)
				);
				
				Vector3 offset = Direction * MaxDistance;

				foreach (var point in BoxUtility.Points)
				{
					BoxUtility.Direction matchedDirections = point.Direction & direction;
					if (matchedDirections == 0)
						continue;
					int count = BoxUtility.CountDirections(matchedDirections);
					if (count != 1 && count != 2)
						continue;
					Vector3 coordinate = Box.Matrix.MultiplyPoint3x4(point.Coordinate);
					commandBuilder.AppendLine(new Line(coordinate, coordinate + offset), castColor, duration);
				}
				
				// Draw box end
				var boxEnd = Box.GetTranslated(offset);
				foreach (var edge in BoxUtility.Edges)
				{
					BoxUtility.Direction matchedDirections = edge.Direction & direction;
					if (matchedDirections == 0)
						continue;
					Vector3 a = boxEnd.Matrix.MultiplyPoint3x4(edge.A);
					Vector3 b = boxEnd.Matrix.MultiplyPoint3x4(edge.B);
					commandBuilder.AppendLine(new Line(a, b), castColor, duration);
				}
			}
#endif
		}

		public struct BoxCastAll : IDrawableCast
		{
			public Box Box;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit[] Results;
			public int ResultCount;

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

		public struct CapsuleCast : IDrawableCast
		{
			public Capsule Capsule;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit? Hit;

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
				Capsule.GetTranslated(Direction * MaxDistance).Draw(commandBuilder, castColor, duration);
				// TODO draw connections
				if (Hit.HasValue)
					Capsule.GetTranslated(Direction * Hit.Value.distance).Draw(commandBuilder, hitColor, duration);
			}
#endif
		}


		public struct CapsuleCastAll : IDrawableCast
		{
			public Capsule Capsule;
			public Vector3 Direction;
			public float MaxDistance;
			public RaycastHit[] Results;
			public int ResultCount;

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