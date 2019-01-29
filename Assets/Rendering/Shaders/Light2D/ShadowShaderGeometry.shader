Shader "_FatshihShader/ShadowShaderGeometry"
{
	Properties
	{
		[PerRendererData] _CenterWorldPos ("_CenterWorldPos", Vector) = (0,0,0,0)
		[PerRendererData] _Color ("_Color", Color) = (0,0,0,0)
		[PerRendererData] _GeometryParams ("_GeometryParams", Vector) = (0,0,0,0)
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
			#pragma geometry geom
			
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
			float4 _GeometryParams; // (radius, stepCount, angle, TBD)
			
			// Vertex shader actuall doesn't matter at all, since we won't use any data generated here.
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			// Geometry shader
			/* CPU algorithm
					Vector3 rayDirection = transform.right * radius;
					float currentAngle = transform.eulerAngles.z;
					float angleStep = angle / indicesCount;
					// create a line-mesh
					vertices[0] = Vector3.zero;
					for (int i = 1; i < indicesCount + 2; i++)
					{
						rayDirection.x = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
						rayDirection.y = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
						rayDirection.Normalize();
						rayDirection *= radius;

						vertices[i] = transform.InverseTransformPoint(transform.position + rayDirection);

						currentAngle += angleStep;

						//Debug.DrawLine(transform.position, transform.TransformPoint(vertices[i]), shadowColor);

						if (i < indicesCount + 1)
						{
							indices[(i - 1) * 3] = 0;
							indices[(i - 1) * 3 + 1] = i;
							indices[(i - 1) * 3 + 2] = i + 1;
						}
					}
				*/
			[maxvertexcount(360)]
			void geom(point v2f input[1], inout TriangleStream<v2f> OutputStream)
			{
				// calculate object space right vector and transform to world space, also scale by radius
				float3 rayDirection = (mul(unity_ObjectToWorld, float4(1,0,0,1)) * _GeometryParams.x).xyz;
				// the starting angle, this can probably be another parameter
				float currentAngle = 0;
				// the delta of angle for each ray, this will be added to currentAngle
				// the 60 here is the # of rays we'll fire, it can probably be another parameter
				float angleStep = _GeometryParams.z / 60;
				// first we created the origin vertex, this will be the starting vertex of each triangles
				v2f origin;
				origin.vertex = UnityObjectToClipPos(float4(0,0,0,1));
				origin.worldPos = _CenterWorldPos;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				const int stepCount = _GeometryParams.y;
				const float3 stepSize = (i.worldPos - _CenterWorldPos.xyz) / stepCount;
				float3 currentWorldPos = _CenterWorldPos.xyz;

				for(int stepIndex = 0; stepIndex < stepCount; stepIndex++)
				{
					currentWorldPos += stepSize;
					float4 currentLocalPos = mul(unity_WorldToObject, float4(currentWorldPos,1));
					float4 currentScreenPos = ComputeScreenPos(UnityObjectToClipPos(currentLocalPos));
					currentScreenPos.xy = (currentScreenPos.xy / currentScreenPos.w);

					if(tex2D(_ObstacleTex, currentScreenPos.xy).a > 0.001f)
					{
						discard;
					}
				}

				float intensity = 1 - (distance(i.worldPos, _CenterWorldPos.xyz) / _GeometryParams.x);

				return _Color * float4(intensity,intensity,intensity,intensity);
			}
			ENDCG
		}
	}
}
