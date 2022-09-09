Shader "Hidden/Vertx/Line"
{
	Properties {}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
		}

		Offset -1, -1
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			struct Line
			{
				float3 A, B;
			};

			StructuredBuffer<Line> line_buffer;
			StructuredBuffer<float4> color_buffer;

			struct vertInput
			{
				float2 uv : TEXCOORD0;
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
				Line l = line_buffer[input.instanceID];
				o.position = UnityObjectToClipPos(input.vertexID == 0 ? l.A : l.B);
				o.color = color_buffer[input.instanceID];
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