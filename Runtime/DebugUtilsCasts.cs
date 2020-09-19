using UnityEngine;

namespace Vertx.Debugging
{
	public static partial class DebugUtils
	{
		#region Casts

		#region SphereCast

		public static void DrawSphereCast(
			Ray ray,
			float radius,
			float distance,
			Color colorStart,
			Color colorEnd,
			int iterationCount = 10)
			=> DrawSphereCast(ray.origin, radius, ray.direction, distance, colorStart, colorEnd, iterationCount);

		public static void DrawSphereCast(
			Vector3 origin,
			float radius,
			Vector3 direction,
			float distance,
			Color colorStart,
			Color colorEnd,
			int iterationCount = 10)
		{
			direction.EnsureNormalized();
			Vector3 crossA = GetAxisAlignedPerpendicular(direction);
			Vector3 crossB = Vector3.Cross(crossA, direction);
			Color color = colorStart;
			DrawCircleFast(origin, crossA, crossB, radius, DrawLine);
			DrawCircleFast(origin, crossB, crossA, radius, DrawLine);

			Vector3 scaledDirection = direction * distance;
			iterationCount += 2; //Add caps
			for (int i = 0; i < iterationCount; i++)
			{
				float t = i / ((float) iterationCount - 1);
				color = Color.Lerp(colorStart, colorEnd, t);
				DrawCircleFast(origin + scaledDirection * t, direction, crossA, radius, DrawLine);
			}

			Vector3 end = origin + scaledDirection;
			color = colorEnd;
			DrawCircleFast(end, crossA, crossB, radius, DrawLine);
			DrawCircleFast(end, crossB, crossA, radius, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region BoxCast

		public static void DrawBoxCast(
			Vector3 center,
			Vector3 halfExtents,
			Vector3 direction,
			Quaternion orientation,
			float distance,
			Color colorStart,
			Color colorEnd,
			int iterationCount = 1)
		{
			direction.EnsureNormalized();

			Vector3 upNormalised = orientation * new Vector3(0, 1, 0);
			Vector3 rightNormalised = orientation * new Vector3(1, 0, 0);
			Vector3 forwardNormalised = orientation * new Vector3(0, 0, 1);

			float dotUpValue = Vector3.Dot(upNormalised, direction);
			bool dotUp = dotUpValue > 0;
			float dotRightValue = Vector3.Dot(rightNormalised, direction);
			bool dotRight = dotRightValue > 0;
			float dotForwardValue = Vector3.Dot(forwardNormalised, direction);
			bool dotForward = dotForwardValue > 0;

			float dotUpAbsValue = Mathf.Abs(dotUpValue);

			bool aligned = dotUpAbsValue > 0.99999f || dotUpAbsValue < 0.00001f;

			Color color = colorStart;

			DrawBoxStructure structure = new DrawBoxStructure(halfExtents, orientation);

			Vector3
				uFL = structure.UFL,
				uFR = structure.UFR,
				uBL = structure.UBL,
				uBR = structure.UBR,
				dFL = structure.DFL,
				dFR = structure.DFR,
				dBL = structure.DBL,
				dBR = structure.DBR;

			DrawBox(center, structure, DrawLine);

			Vector3 endCenter = center + direction * distance;

			DrawBoxConnectors(center, endCenter);

			color = colorEnd;
			DrawBox(endCenter, structure, DrawLine);

			void DrawBoxConnectors(Vector3 boxCenterA, Vector3 boxCenterB)
			{
				if (iterationCount <= 0) return;

				if (aligned)
				{
					if (dotUpAbsValue > 0.5f)
					{
						//Up
						bool inverse = dotUpValue < 0;
						DrawConnectorIterationSpecialWithInverse(uFL, uFR, dFL, dFR, inverse);
						DrawConnectorIterationSpecialWithInverse(uFR, uBR, dFR, dBR, inverse);
						DrawConnectorIterationSpecialWithInverse(uBR, uBL, dBR, dBL, inverse);
						DrawConnectorIterationSpecialWithInverse(uBL, uFL, dBL, dFL, inverse);
					}
					else
					{
						//Forward
						float dotForwardAbsValue = Mathf.Abs(dotForwardValue);
						if (dotForwardAbsValue > 0.5f)
						{
							bool inverse = dotForwardValue < 0;
							DrawConnectorIterationSpecialWithInverse(uFL, uFR, uBL, uBR, inverse);
							DrawConnectorIterationSpecialWithInverse(uFR, dFR, uBR, dBR, inverse);
							DrawConnectorIterationSpecialWithInverse(dFR, dFL, dBR, dBL, inverse);
							DrawConnectorIterationSpecialWithInverse(dFL, uFL, dBL, uBL, inverse);
						}
						else
						{
							//Right
							bool inverse = dotRightValue < 0;
							DrawConnectorIterationSpecialWithInverse(uFR, uBR, uFL, uBL, inverse);
							DrawConnectorIterationSpecialWithInverse(uBR, dBR, uBL, dBL, inverse);
							DrawConnectorIterationSpecialWithInverse(dBR, dFR, dBL, dFL, inverse);
							DrawConnectorIterationSpecialWithInverse(dFR, uFR, dFL, uFL, inverse);
						}
					}
				}
				else
				{
					bool
						validUFL = ValidateConnector(dotUp, dotForward, !dotRight),
						validUFR = ValidateConnector(dotUp, dotForward, dotRight),
						validUBL = ValidateConnector(dotUp, !dotForward, !dotRight),
						validUBR = ValidateConnector(dotUp, !dotForward, dotRight),
						validDFL = ValidateConnector(!dotUp, dotForward, !dotRight),
						validDFR = ValidateConnector(!dotUp, dotForward, dotRight),
						validDBL = ValidateConnector(!dotUp, !dotForward, !dotRight),
						validDBR = ValidateConnector(!dotUp, !dotForward, dotRight);

					bool ValidateConnector(bool a, bool b, bool c)
					{
						int count = a ? 1 : 0;
						count += b ? 1 : 0;
						count += c ? 1 : 0;
						if (!aligned)
						{
							if (count == 0)
								return false;
							if (a && b && c) return false;
						}
						else
						{
							if (count != 1)
								return false;
						}

						return true;
					}

					//up
					DrawConnectorIteration(validUFL, validUFR, uFL, uFR);
					DrawConnectorIteration(validUFR, validUBR, uFR, uBR);
					DrawConnectorIteration(validUBR, validUBL, uBR, uBL);
					DrawConnectorIteration(validUBL, validUFL, uBL, uFL);
					//down
					DrawConnectorIteration(validDFL, validDFR, dFL, dFR);
					DrawConnectorIteration(validDFR, validDBR, dFR, dBR);
					DrawConnectorIteration(validDBR, validDBL, dBR, dBL);
					DrawConnectorIteration(validDBL, validDFL, dBL, dFL);
					//down to up
					DrawConnectorIteration(validDFL, validUFL, dFL, uFL);
					DrawConnectorIteration(validDFR, validUFR, dFR, uFR);
					DrawConnectorIteration(validDBR, validUBR, dBR, uBR);
					DrawConnectorIteration(validDBL, validUBL, dBL, uBL);
				}

				void DrawConnectorIteration(bool a, bool b, Vector3 aP, Vector3 bP)
				{
					if (!a || !b) return;
					DrawConnectorIterationSpecial(aP, bP, aP, bP);
				}

				void DrawConnectorIterationSpecialWithInverse(Vector3 aPS, Vector3 bPS, Vector3 aPE, Vector3 bPE, bool inverse)
				{
					if (inverse)
						DrawConnectorIterationSpecial(aPE, bPE, aPS, bPS);
					else
						DrawConnectorIterationSpecial(aPS, bPS, aPE, bPE);
				}

				void DrawConnectorIterationSpecial(Vector3 aPS, Vector3 bPS, Vector3 aPE, Vector3 bPE)
				{
					Vector3 startA = boxCenterA + aPS;
					Vector3 startB = boxCenterA + bPS;
					Vector3 endA = boxCenterB + aPE;
					Vector3 endB = boxCenterB + bPE;

					Vector3 currentA = startA;
					Vector3 currentB = startB;

					float diff = 1 / (float) (iterationCount + 1);

					for (int i = 1; i < iterationCount; i++)
					{
						float t = i / (float) iterationCount;
						color = Color.Lerp(colorStart, colorEnd, t + diff);
						Vector3 nextA = Vector3.Lerp(startA, endA, t);
						Vector3 nextB = Vector3.Lerp(startB, endB, t);

						DrawLine(currentA, nextA);
						DrawLine(currentB, nextB);
						DrawLine(nextA, nextB);

						currentA = nextA;
						currentB = nextB;
					}

					color = Color.Lerp(colorStart, colorEnd, 1 - diff);
					DrawLine(currentA, endA);
					DrawLine(currentB, endB);
				}
			}

			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region CapsuleCast

		public static void DrawCapsuleCast(
			Vector3 point1,
			Vector3 point2,
			float radius,
			Vector3 direction,
			float distance,
			Color colorStart,
			Color colorEnd,
			int iterationCount = 10)
		{
			direction.EnsureNormalized();

			Vector3 alignment = (point1 - point2).normalized;

			Vector3 crossA = GetAxisAlignedPerpendicular(alignment);
			Vector3 crossB = Vector3.Cross(crossA, alignment);
			Color color = colorStart;
			DrawCapsuleFast(point1, point2, radius, alignment, crossA, crossB, DrawLine);

			Vector3 dCrossA = Vector3.Cross(direction, alignment).normalized;
			//Vector3 dCrossB = Vector3.Cross(dCrossA, direction);

			Vector3 scaledDCrossA = dCrossA * radius;
			Vector3 a1 = point1 + scaledDCrossA;
			Vector3 a2 = point2 + scaledDCrossA;
			Vector3 b1 = point1 - scaledDCrossA;
			Vector3 b2 = point2 - scaledDCrossA;
			Vector3 scaledDirection = direction * distance;

			Vector3 aFrom1 = a1;
			Vector3 bFrom1 = b1;
			Vector3 aFrom2 = a2;
			Vector3 bFrom2 = b2;

			iterationCount += 2; //Add caps
			for (int i = 1; i < iterationCount; i++)
			{
				float t = i / (float) (iterationCount - 1);
				color = Color.Lerp(colorStart, colorEnd, t);
				Vector3 sDir = scaledDirection * t;

				Vector3 aTo1 = a1 + sDir;
				Vector3 bTo1 = b1 + sDir;
				Vector3 aTo2 = a2 + sDir;
				Vector3 bTo2 = b2 + sDir;

				DrawLine(aFrom1, aTo1, 0.5f);
				DrawLine(aFrom2, aTo2, 0.5f);
				DrawLine(bFrom2, bTo2, 0.5f);
				DrawLine(bFrom1, bTo1, 0.5f);

				aFrom1 = aTo1;
				bFrom1 = bTo1;
				aFrom2 = aTo2;
				bFrom2 = bTo2;
			}

			Vector3 end1 = point1 + scaledDirection;
			Vector3 end2 = point2 + scaledDirection;
			color = colorEnd;

			DrawCapsuleFast(end1, end2, radius, alignment, crossA, crossB, DrawLine);

			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#endregion
		
		#region RaycastHits

		public static void DrawSphereCastHits(RaycastHit[] hits, Ray ray, float radius, Color color, int maxCount = -1)
			=> DrawSphereCastHits(hits, ray.origin, radius, ray.direction, color, maxCount);

		public static void DrawSphereCastHits(RaycastHit[] hits, Vector3 origin, float radius, Vector3 direction, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;

			direction.EnsureNormalized();

			Vector3 zero = Vector3.zero;
			for (int i = 0; i < maxCount; i++)
			{
				RaycastHit hit = hits[i];

				//Zero position is to be interpreted as colliding with the start of the spherecast.
				if (hit.point == zero)
				{
					hit.point = origin;
					Vector3 crossA = GetAxisAlignedPerpendicular(direction);
					Vector3 crossB = Vector3.Cross(crossA, direction);
					DrawCircleFast(origin, crossA, crossB, radius, DrawLineSolid);
					DrawCircleFast(origin, crossB, crossA, radius, DrawLineSolid);
					DrawCircleFast(origin, direction, crossA, radius, DrawLineSolid);

					void DrawLineSolid(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, color);
					continue;
				}

				Vector3 localDirection = GetAxisAlignedAlternateWhereRequired(hit.normal, direction);
				Vector3 cross = Vector3.Cross(localDirection, hit.normal);

				Vector3 point = hit.point + hit.normal * radius;
				DrawCircleFast(point, cross, hit.normal, radius, DrawLine);
				Vector3 secondCross = Vector3.Cross(cross, hit.normal);
				DrawCircleFast(point, secondCross, hit.normal, radius, DrawLine);
			}

			void DrawLine(Vector3 a, Vector3 b, float f) => Debug.DrawLine(a, b, new Color(color.r, color.g, color.b, Mathf.Pow(1 - Mathf.Abs(f - 0.5f) * 2, 2) * color.a));
		}

		public static void DrawBoxCastHits(RaycastHit[] hits, Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, Color color, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;
			
			direction.EnsureNormalized();

			DrawBoxStructure structure = new DrawBoxStructure(halfExtents, orientation);

			for (int i = 0; i < maxCount; i++)
			{
				RaycastHit hit = hits[i];
				Vector3 center = origin + direction * hit.distance;
				DrawBox(center, structure, DrawLine);
			}

			void DrawLine(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, color);
		}

		public static void DrawRaycastHits(RaycastHit[] hits, float rayLength = 1, int maxCount = -1, float duration = 0)
			=> DrawRaycastHits(hits, HitColor, rayLength, maxCount, duration);

		public static void DrawRaycastHits(RaycastHit[] hits, Color color, float rayLength = 1, int maxCount = -1, float duration = 0)
		{
			if (maxCount < 0)
				maxCount = hits.Length;
			for (int i = 0; i < maxCount; i++)
				Debug.DrawRay(hits[i].point, hits[i].normal * rayLength, color, duration);
		}

		public static void DrawCapsuleCastHits(RaycastHit[] hits, Color color, Vector3 point1, Vector3 point2, float radius, Vector3 direction, int maxCount = -1)
		{
			if (maxCount < 0)
				maxCount = hits.Length;

			if (maxCount == 0) return;

			direction.EnsureNormalized();
			
			Vector3 alignment = (point1 - point2).normalized;

			Vector3 crossA = GetAxisAlignedPerpendicular(alignment);
			Vector3 crossB = Vector3.Cross(crossA, alignment);

			for (int i = 0; i < maxCount; i++)
			{
				RaycastHit hit = hits[i];
				Vector3 dir = direction * hit.distance;
				DrawCapsuleFast(point1 + dir, point2 + dir, radius, alignment, crossA, crossB, DrawLine);
			}

			void DrawLine(Vector3 a, Vector3 b, float v) => Debug.DrawLine(a, b, color);
		}

		#endregion

		#region Both

		public static void DrawSphereCast(Vector3 origin, float radius, Vector3 direction, RaycastHit[] hits, float distance, int count)
		{
			DrawSphereCast(origin, radius, direction, distance);
			DrawSphereCastHits(hits, origin, radius, direction, count);
		}
		
		public static void DrawBoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] hits, Quaternion orientation, float distance, int count)
		{
			DrawBoxCast(center, halfExtents, direction, orientation, distance);
			DrawBoxCastHits(hits, center, halfExtents, direction, orientation, count);
		}
		
		public static void DrawCapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] hits, float distance, int count)
		{
			DrawCapsuleCast(point1, point2, radius, direction, distance);
			DrawCapsuleCastHits(hits, point1, point2, radius, direction, count);
		}

		#endregion
	}
}