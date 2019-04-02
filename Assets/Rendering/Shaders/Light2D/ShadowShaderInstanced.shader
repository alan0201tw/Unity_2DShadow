Shader "_FatshihShader/ShadowShaderInstanced"
{
	Properties
	{
		// [PerRendererData] _CenterWorldPos("_CenterWorldPos", Vector) = (0,0,0,0)
		// [PerRendererData] _Color ("_Color", Color) = (0,0,0,0)
		// [PerRendererData] _Radius ("_Radius", float) = 0
		// [PerRendererData] _StepCount("_StepCount", int) = 50
	}
	SubShader
	{
		Cull Off
        Lighting Off
        ZWrite Off
        Blend One One

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
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				
				float3 worldPos : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
			};
			
			uniform sampler2D _ObstacleTex;
			//float4 _CenterWorldPos; // the world space position of the light source
			//float4 _Color;
			//float _Radius;
			float _StepCount;
			
			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _CenterWorldPos)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
            UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				// necessary only if you want to access instanced properties in the fragment Shader.
                UNITY_TRANSFER_INSTANCE_ID(v, o);

				// this vertex is the ending position of the ray (in object space)
				float3 endPoint = mul(unity_ObjectToWorld, v.vertex);

				float4 centerWorldPos = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterWorldPos);
				//float4 centerWorldPos = _CenterWorldPos;
				
				const int stepCount = 75;
				const float3 stepSize = (endPoint - centerWorldPos.xyz) / stepCount;
				float3 currentWorldPos = centerWorldPos.xyz;

				for(int stepIndex = 0; stepIndex < stepCount; stepIndex++)
				{
					currentWorldPos += stepSize;
					float4 currentLocalPos = mul(unity_WorldToObject, float4(currentWorldPos,1));
					float4 currentScreenPos = ComputeScreenPos(UnityObjectToClipPos(currentLocalPos));
					currentScreenPos.xy = (currentScreenPos.xy / currentScreenPos.w);

					if(tex2Dlod(_ObstacleTex, float4(currentScreenPos.xy,0,0)).a > 0.001f)
					{
						o.vertex = UnityObjectToClipPos(currentLocalPos);
						o.worldPos = mul(unity_ObjectToWorld, currentLocalPos);
						return o;
					}
				}

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// necessary only if any instanced properties are going to be accessed in the fragment Shader.
				UNITY_SETUP_INSTANCE_ID(i);
                //return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				float4 centerWorldPos = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterWorldPos);
				float radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);

				float intensity = 1 - (distance(i.worldPos, centerWorldPos.xyz)) / radius;

				return UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * float4(intensity,intensity,intensity,intensity);
			}
			ENDCG
		}
	}
}
