Shader "_FatshihShader/ShadowShaderGPU"
{
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend One One

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
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

			sampler2D _ObstacleTex;

			matrix _ObstacleCameraViewMatrix;
			matrix _ObstacleCameraProjMatrix;

			float4 _CenterWorldPos; // the world space position of the light source
			float4 _Color;
			float _Radius;
			float _StepCount;

			float4 ComputeObstacleSpacePos(float4 clipPos)
			{
				return ComputeScreenPos(clipPos);
			}

			v2f vert(appdata v)
			{
				v2f o;
				// this vertex is the ending position of the ray (in object space)

				// the texture is rendered by obstacleCamera
				// however, the current VP matrix is set by the main camera
				// the M matrix is the model matrix used by obstacleCamera
				// _CenterWorldPos is in world space

				float3 endPoint = mul(unity_ObjectToWorld, v.vertex);

				const int stepCount = _StepCount;
				const float3 stepSize = (endPoint - _CenterWorldPos.xyz) / stepCount;
				float3 currentWorldPos = _CenterWorldPos.xyz;

				for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
				{
					currentWorldPos += stepSize;
					float4 currentLocalPos = mul(unity_WorldToObject, float4(currentWorldPos,1));
					//float4 currentPosInObstacleTextureSpace = 
					//	ComputeScreenPos(UnityWorldToClipPos(currentWorldPos - _CenterWorldPos));

					float4 currentPosInObstacleTextureSpace =
						ComputeObstacleSpacePos(
							mul(_ObstacleCameraProjMatrix,
								mul(_ObstacleCameraViewMatrix, float4(currentWorldPos, 1))));

					currentPosInObstacleTextureSpace.xy =
						(currentPosInObstacleTextureSpace.xy / currentPosInObstacleTextureSpace.w);

#if UNITY_UV_STARTS_AT_TOP
					currentPosInObstacleTextureSpace.y = 1 - currentPosInObstacleTextureSpace.y;
#endif
					if (tex2Dlod(_ObstacleTex, float4(currentPosInObstacleTextureSpace.xy,0,0)).a > 0.001f)
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

			fixed4 frag(v2f i) : SV_Target
			{
				float intensity = 1 - (distance(i.worldPos, _CenterWorldPos.xyz) / _Radius);

				return _Color * intensity;
			}
			ENDCG
		}
	}
}