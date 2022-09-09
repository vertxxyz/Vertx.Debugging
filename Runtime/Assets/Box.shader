Shader "Hidden/Vertx/Box"
{
	Properties {}
	SubShader
	{
		Tags
		{
			"RenderType"="Transparent"
		}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Offset -1, -1
		Cull Off

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

			struct Box
			{
				float4x4 Matrix;
			};

			StructuredBuffer<Box> box_buffer;
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
			};

			v2f vert(vertInput input)
			{
				v2f o;

				Box a = box_buffer[input.instanceID];
				unity_ObjectToWorld = a.Matrix;

				int i = shared_buffer_start + input.instanceID;
				o.color = color_buffer[i];
				int modifications = modifications_buffer[i];

				float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));

				o.position = mul(UNITY_MATRIX_VP, worldPos);

				if (has_normal_fade(modifications))
				{
					float3 worldViewDir = _WorldSpaceCameraPos.xyz - worldPos;
					float d = dot(worldViewDir, UnityObjectToWorldNormal(input.normal));
					d = saturate(
						smoothstep(0, 0.1, d) // front face
					);

					o.color.a *= max(0.3, d);
				}
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}