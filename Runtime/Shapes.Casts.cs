using UnityEngine;

// ReSharper disable ConvertToNullCoalescingCompoundAssignment

namespace Vertx.Debugging
{
	public static partial class Shapes
	{
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

		public struct SphereCast : IDrawableCast
		{
			public Vector3 Origin, Direction;
			public float Radius, MaxDistance;
			public RaycastHit? Hit;

			public SphereCast(Vector3 origin, Vector3 direction, float radius, RaycastHit? hit, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
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
			}
#endif
		}
		
		public struct SphereCastAll : IDrawableCast
		{
			public Vector3 Origin, Direction;
			public float Radius, MaxDistance;
			public RaycastHit[] Results;
			public int ResultCount;

			public SphereCastAll(Vector3 origin, Vector3 direction, float radius, RaycastHit[] results, int resultCount, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
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
					new Sphere(Origin + Direction * result.distance, Quaternion.LookRotation(result.normal), Radius).Draw(commandBuilder, hitColor, duration, Axes.X | Axes.Z);
				}
			}
#endif
		}
	}
}