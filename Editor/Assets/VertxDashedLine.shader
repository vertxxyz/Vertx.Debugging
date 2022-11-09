Shader "Hidden/Vertx/DashedLine"
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
			#include "VertxDashedLineShared.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			fixed4 frag(v2f i) : SV_Target
			{
				const float dashSize = 12.0;
				const float gapSize = 10.0;
				
				float distance = length(i.screenPos.xy / i.screenPos.w * _ScreenParams.xy - i.screenPosStart.xy / i.screenPosStart.w * _ScreenParams.xy);
				if (frac(distance / (dashSize + gapSize)) > dashSize / (dashSize + gapSize))
					discard;
				
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
			#include "VertxDashedLineShared.cginc"
			#pragma instancing_options assumeuniformscaling nolightmap nolightprobe nolodfade
			#pragma target 4.5

			fixed4 frag(v2f i) : SV_Target
			{
				const float dashSize = 12.0;
				const float gapSize = 10.0;
				
				float distance = length(i.screenPos.xy / i.screenPos.w * _ScreenParams.xy - i.screenPosStart.xy / i.screenPosStart.w * _ScreenParams.xy);
				if (frac(distance / (dashSize + gapSize)) > dashSize / (dashSize + gapSize))
					discard;
				
				return i.color;
			}
			ENDCG
		}
	}
}