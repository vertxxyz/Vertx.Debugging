using UnityEngine;
using static Vertx.Debugging.Shapes;
using Ray = UnityEngine.Ray;

namespace Vertx.Debugging
{
	public static class Example
	{
		public static void Method(Transform transform)
		{
			D.raw(new Ray
			{
				origin = Vector3.zero,
				direction = transform.forward
			});
			
			D.raw(new SphereCast(new Ray(Vector3.zero, transform.forward))
			{
				Radius = 2,
				MaxDistance = 10
			});
		}
	}
}