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
				if (float.IsInfinity(distance))
					distance = MaxDrawDistance;
				direction.EnsureNormalized();
				Ray = new Ray(origin, direction * distance);
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
				if (float.IsInfinity(distance))
					distance = MaxDrawDistance;
				direction.EnsureNormalized();
				Ray = new Ray(origin, direction * distance);
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

			public SphereCast(Vector3 origin, Vector3 direction, float radius, float maxDistance = Mathf.Infinity)
			{
				Origin = origin;
				Direction = direction;
				Radius = radius;
				MaxDistance = maxDistance;
			}

			public SphereCast(in UnityEngine.Ray ray)
			{
				Origin = ray.origin;
				Direction = ray.direction;
				Radius = 0;
				MaxDistance = 0;
			}

			public SphereCast(in Ray ray)
			{
				Origin = ray.Origin;
				Direction = ray.Direction;
				Radius = 0;
				MaxDistance = 0;
			}

#if UNITY_EDITOR
			public void Draw(CommandBuilder commandBuilder, Color color, float duration) => Draw(commandBuilder, color, color, duration);
			
			public void Draw(CommandBuilder commandBuilder, Color castColor, Color hitColor, float duration) { }
#endif
		}
	}
}