using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vertx.Debugging
{
	public static class DrawPhysicsSettings
	{
		/// <summary>
		/// Override the length of time the casts draw for. You will need to reset this value manually.
		/// Calls to <see cref="Duration"/> cannot be stripped, I would recommend using <see cref="SetDuration"/> if this is important to you.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public static float Duration { 
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}
		
		/// <summary>
		/// Override the length of time the casts draw for. You will need to reset this value manually.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void SetDuration(float duration) => Duration = duration;
	}
}