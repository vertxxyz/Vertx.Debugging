using System;
using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		#region Casts

		#region CircleCast2D

		public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance)
			=> DrawCircleCast2D(origin, radius, direction, distance, StartColor, EndColor);

		public static void DrawCircleCast2D(Vector2 origin,
			float radius,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();

			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;
			
			Color color = colorStart;
			DrawCircleFast(origin, back, up, radius, DrawLine);
			
			color = colorEnd;
			DrawCircleFast(origin + direction * distance, back, up, radius, DrawLine);
			
			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region BoxCast2D
		
		/*private static Mesh cube;
		private static Mesh Cube => cube == null ? cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx") : cube;
		
		private static MaterialPropertyBlock mPB;
		private static readonly int idColorStart = Shader.PropertyToID("_ColorStart");
		private static readonly int idColorEnd = Shader.PropertyToID("_ColorEnd");
		private static readonly int idUL = Shader.PropertyToID("_UROrigin");
		private static readonly int idUR = Shader.PropertyToID("_ULOrigin");
		private static readonly int idOrigin = Shader.PropertyToID("_Origin");
		private static readonly int idOffset = Shader.PropertyToID("_Offset");
		private static MaterialPropertyBlock MaterialPropertyBlock => mPB ?? (mPB = new MaterialPropertyBlock());*/

		public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
			=> BoxCast2D(origin, size, angle, direction, distance, StartColor, EndColor);

		public static void BoxCast2D(
			Vector2 origin,
			Vector2 size,
			float angle,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();

			Color color = colorStart;

			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);

			DrawBox2DFast(Vector2.zero, boxStructure2D, DrawLine);

			color = colorEnd;

			Vector2 offset = direction * distance;
			DrawBox2DFast(offset, boxStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
			
			/*MaterialPropertyBlock block = MaterialPropertyBlock;
			block.SetVector(idUL, boxStructure2D.ULOrigin);
			block.SetVector(idUR, boxStructure2D.UROrigin);
			block.SetVector(idOrigin, origin);
			block.SetVector(idOffset, offset);
			block.SetVector(idColorStart, colorStart);
			block.SetVector(idColorEnd, colorEnd);
			
			Graphics.DrawMesh(Cube, Matrix4x4.identity, Material, 0, null, 0, block, false, false, false);*/
		}

		#endregion

		#region CapsuleCast2D

		public static void DrawCapsuleCast2D(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
			=> DrawCapsuleCast2D(origin, size, capsuleDirection, angle, direction, distance, StartColor, EndColor);

		public static void DrawCapsuleCast2D(
			Vector2 origin, 
			Vector2 size,
			CapsuleDirection2D capsuleDirection, 
			float angle,
			Vector2 direction,
			float distance,
			Color colorStart,
			Color colorEnd)
		{
			direction.EnsureNormalized();
			
			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);

			Color color = colorStart;
			DrawCapsule2DFast(origin, capsuleStructure2D, DrawLine);

			var scaledDirection = direction * distance;
			Vector2 destination = origin + scaledDirection;
			
			color = colorEnd;
			DrawCapsule2DFast(destination, capsuleStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#endregion
		
		#region RaycastHits

		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, int maxCount = -1)
			=> DrawBoxCast2DHits(hits, origin, size, angle, direction, HitColor, maxCount);

		public static void DrawBoxCast2DHits(RaycastHit2D[] hits, Vector2 origin, Vector2 size, float angle, Vector2 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;
			
			direction.EnsureNormalized();
			
			DrawBoxStructure2D boxStructure2D = new DrawBoxStructure2D(size, angle, origin);
			
			for (int i = 0; i < maxCount; i++)
				DrawBox2DFast(direction * hits[i].distance, boxStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}
		
		public static void DrawCircleCast2DHits(RaycastHit2D[] hits, Vector2 origin, float radius, Vector2 direction, int maxCount = -1)
			=> DrawCircleCast2DHits(hits, origin, radius, direction, HitColor, maxCount);

		public static void DrawCircleCast2DHits(RaycastHit2D[] hits, Vector2 origin, float radius, Vector2 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;
			
			direction.EnsureNormalized();
			
			Vector3 back = Vector3.back;
			Vector3 up = Vector3.up;
			
			for (int i = 0; i < maxCount; i++)
				DrawCircleFast(origin + direction * hits[i].distance, back, up, radius, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}
		
		public static void DrawCapsuleCast2DHits(RaycastHit2D[] hits, Vector2 origin, 
			Vector2 size,
			CapsuleDirection2D capsuleDirection, 
			float angle,
			Vector2 direction, int maxCount = -1)
			=> DrawCapsuleCast2DHits(hits, origin, size, capsuleDirection, angle, direction, HitColor, maxCount);

		public static void DrawCapsuleCast2DHits(RaycastHit2D[] hits, 
			Vector2 origin, 
			Vector2 size,
			CapsuleDirection2D capsuleDirection, 
			float angle,
			Vector2 direction, 
			Color color,
			int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;
			
			direction.EnsureNormalized();
			
			DrawCapsuleStructure2D capsuleStructure2D = new DrawCapsuleStructure2D(size, capsuleDirection, angle);
			
			for (int i = 0; i < maxCount; i++)
				DrawCapsule2DFast(origin + direction * hits[i].distance, capsuleStructure2D, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion
	}
}