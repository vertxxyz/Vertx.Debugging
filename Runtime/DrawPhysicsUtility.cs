using static Vertx.Debugging.D;

namespace Vertx.Debugging
{
	public static class DrawPhysicsUtility
	{
		/// <summary>
		/// Helper method for getting a corrected duration for use in jobs started from FixedUpdate or FixedStepSimulationSystemGroup.<br/>
		/// <see cref="D.raw"/> functions presume drawing from Update, and detects drawing from FixedUpdate outside of jobs.
		/// </summary>
		/// <param name="baseDuration">A duration override</param>
		/// <returns>An adjusted duration that will mean the shape is drawn until the next fixed frame.</returns>
		public static float GetFixedFrameJobDuration(float baseDuration = 0)
		{
			float duration = baseDuration;
#if UNITY_EDITOR
			ForceAdjustDuration(ref duration);
#endif
			return duration;
		}
	}
}