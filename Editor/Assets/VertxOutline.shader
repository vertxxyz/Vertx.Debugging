Shader "Hidden/Vertx/Outline"
{
	Properties
	{
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _ZTest("__zt", Float) = 4.0
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Offset -1, -1
		Cull Off

		Pass // 0
		{
			ZTest Greater
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "VertxOutlineShared.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			fixed4 frag(v2f i) : SV_Target
			{
				i.color.a *= Z_GREATER_FADE;
				return i.color;
			}
			ENDCG
		}

		Pass // 1
		{
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "VertxOutlineShared.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}