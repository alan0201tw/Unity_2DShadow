Shader "_FatshihShader/ShadowShader"
{
	Properties
	{
		[PerRendererData] _Color ("Main Color", Color) = (1,1,1,1)
		[PerRendererData] _ShadowCasterParam ("_ShadowCasterParam", Vector) = (0,0,0,0)
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

			fixed4 _Color;
			float4 _ShadowCasterParam; // ( _ShadowCasterPos, _ShadowCasterRadius )
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = _Color;

				float distance01 = distance(_ShadowCasterParam.xyz, i.worldPos) / _ShadowCasterParam.w;
				
				col.a *= (1-distance01);
				//col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
}
