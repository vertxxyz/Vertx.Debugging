using UnityEngine;

namespace Vertx.Debugging
{
	public readonly struct Capsule
	{
		public Vector3 SpherePosition1 { get; }
		public Vector3 SpherePosition2 { get; }
		public float Radius { get; }

		public Capsule(Vector3 center, Quaternion rotation, float height, float radius)
		{
			float pointHeight = height - radius;
			Vector3 direction = rotation * new Vector3(0, 0, pointHeight);
			SpherePosition1 = center + direction;
			SpherePosition2 = center - direction;
			Radius = radius;
		}

		public Capsule(Vector3 spherePosition1, Vector3 spherePosition2, float radius)
		{
			SpherePosition1 = spherePosition1;
			SpherePosition2 = spherePosition2;
			Radius = radius;
		}

		public Capsule(Vector3 lowestPosition, Vector3 direction, float height, float radius)
		{
			SpherePosition1 = lowestPosition + direction * radius;
			SpherePosition2 = SpherePosition1 + direction * (height - radius * 2);
			Radius = radius;
		}
	}
	
	/*public static class ShapeUtils { }*/
}