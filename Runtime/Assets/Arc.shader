Shader "Hidden/Vertx/Arc"
{
	Properties {}
	SubShader
	{
		Tags
		{
			"RenderType"="Transparent"
		}
		
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "DebuggingShared.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			struct Arc
			{
				float4x4 Matrix;
				float Turns;
			};

			StructuredBuffer<Arc> arc_buffer;
			int shared_buffer_start;

			struct vertInput
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				uint vertexID : SV_VertexID;
				uint instanceID : SV_InstanceID;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 color : TEXCOORD1;
				float4 uvAndTurns : TEXCOORD2;
			};

			v2f vert(vertInput input)
			{
				v2f o;

				Arc a = arc_buffer[input.instanceID];
				unity_ObjectToWorld = a.Matrix;
				UNITY_MATRIX_MV = mul(unity_MatrixV, unity_ObjectToWorld);

				int i = shared_buffer_start + input.instanceID;
				o.color = color_buffer[i];
				o.uvAndTurns = float4(input.uv, a.Turns, 0);
				int modifications = modifications_buffer[i];

				if (HasFaceCamera(modifications))
				{
					// Orthographic
					if (unity_OrthoParams.w == 1)
					{
						// Get the origin (it's not affected by rotation or scale)
						float4 origin = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));

						float scale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
						o.position = mul(UNITY_MATRIX_P,
						                 origin
						                 + float4(input.vertex.xyz, 0.0) // Position the unscaled verts
						                 * float4(scale.xxx, 0.0) // Scale the verts
						);
					}
					// Perspective
					else
					{
						// Get the origin (it's not affected by rotation or scale)
						float4 origin = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));

						float scale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
						o.position = mul(UNITY_MATRIX_P,
						                 origin
						                 + float4(input.vertex.xyz, 0.0) // Position the unscaled verts
						                 * float4(scale.xxx, 0.0) // Scale the verts
						);
					}
				}
				else
				{
					// return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
					float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));

					o.position = mul(UNITY_MATRIX_VP, worldPos);

					if (HasNormalFade(modifications))
					{
						float3 worldViewDir = _WorldSpaceCameraPos.xyz - worldPos;
						float d = dot(worldViewDir, UnityObjectToWorldNormal(input.normal));
						d = saturate(
							smoothstep(0, 0.1, d) // front face
						);

						o.color.a *= max(0.4, d);
					}
				}
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// turns - uv.v
				clip(i.uvAndTurns.z - i.uvAndTurns.y);

				return i.color;
			}
			ENDCG
		}
	}
}