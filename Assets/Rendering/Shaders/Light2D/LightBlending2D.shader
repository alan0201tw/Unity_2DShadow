Shader "_FatshihShader/LightBlending2DShader"
{
	Properties
	{
		_LightMap2D ("Light Map 2D", 2D) = "white" {}
		[HideInInspector]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0; 
			};

			sampler2D _MainTex;
			sampler2D _LightMap2D;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 _ambient = float4(0.2f,0.2f,0.2f,0.2f);
				fixed4 col = tex2D(_MainTex, i.uv) * (tex2D(_LightMap2D, i.uv) + _ambient);
				//fixed4 col = tex2D(_LightMap2D, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
