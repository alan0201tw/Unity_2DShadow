Shader "_FatshihShader/ShadowShaderGPU"
{
	Properties
	{
		//[PerRendererData] _ObstacleTex("_ObstacleTex", 2D) = "black" {}
		[PerRendererData] _CenterWorldPos("_CenterWorldPos", Vector) = (0,0,0,0)
		[PerRendererData] _Color ("_Color", Color) = (0,0,0,0)
		[PerRendererData] _Radius ("_Radius", float) = 0
		[PerRendererData] _StepCount("_StepCount", int) = 50
	}
	SubShader
	{
		Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				
				float3 worldPos : TEXCOORD1;
			};
			
			uniform sampler2D _ObstacleTex;
			float4 _CenterWorldPos; // the world space position of the light source
			float4 _Color;
			float _Radius;
			float _StepCount;
			
			v2f vert (appdata v)
			{
				v2f o;
				// compute the corresponding texture-space position
				//o.vertex = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
				//o.vertex.xy /= o.vertex.w;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// assign info for fragment shader
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				const int stepCount = _StepCount;
				const float3 stepSize = (i.worldPos - _CenterWorldPos.xyz) / stepCount;
				float3 currentWorldPos = _CenterWorldPos.xyz;

				for(int stepIndex = 0; stepIndex < stepCount; stepIndex++)
				{
					currentWorldPos += stepSize;
					float4 currentLocalPos = mul(unity_WorldToObject, float4(currentWorldPos,1));
					float4 currentScreenPos = ComputeScreenPos(UnityObjectToClipPos(currentLocalPos));
					//currentScreenPos.y *= _ProjectionParams.x;
					currentScreenPos.xy = (currentScreenPos.xy / currentScreenPos.w);

					if(tex2D(_ObstacleTex, currentScreenPos.xy).a > 0.001f)
					{
						discard;
					}
				}

				float intensity = 1 - (distance(i.worldPos, _CenterWorldPos.xyz) / _Radius);

				return _Color * float4(intensity,intensity,intensity,intensity);
			}
			ENDCG
		}
	}
}
