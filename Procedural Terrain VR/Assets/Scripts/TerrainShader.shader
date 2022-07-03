// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Custom/TriplanarTextures" {
	Properties{
		_TexArr("Textures", 2DArray) = "" {}

		_TexScale("Texture Scale", Float) = 1
	}
		SubShader{
			Tags {"RenderType" = "Opaque" }
			LOD 100

			Pass
		{

			CGPROGRAM
			#include "UnityCG.cginc" 
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			// Use shader model 4.0 target, we need geometry shader support
			#pragma target 4.0

			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
			    float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD2;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				nointerpolation float3 uv_TexArr : TEXCOORD0;
				float2 barycentricCoordinates : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			UNITY_DECLARE_TEX2DARRAY(_TexArr);
			float _TexScale;

			v2g vert(appdata_full v)
			{
				float3 v0 = v.vertex.xyz;

				v2g OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.norm = mul(unity_ObjectToWorld, v.normal);;
				OUT.uv = v.texcoord;
				return OUT;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				float3 lightPosition = _WorldSpaceLightPos0;

				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[1].pos.xyz;
				float3 v2 = IN[2].pos.xyz;

				//float3 normal = normalize(cross(v0 - v1, v1 - v2));
				//float4 worldNormal = normalize(mul(unity_ObjectToWorld, normal));


				g2f OUT;

				OUT.pos = IN[0].pos;
				OUT.worldPos = IN[0].worldPos;
				OUT.norm = IN[0].norm;

				OUT.uv_TexArr.x = IN[0].uv.x;
				OUT.uv_TexArr.y = IN[1].uv.x;
				OUT.uv_TexArr.z = IN[2].uv.x;

				OUT.barycentricCoordinates = float2(1, 0);
				triStream.Append(OUT);

				OUT.pos = IN[1].pos;
				OUT.worldPos = IN[1].worldPos;
				OUT.norm = IN[1].norm;

				OUT.barycentricCoordinates = float2(0, 1);
				triStream.Append(OUT);

				OUT.pos = IN[2].pos;
				OUT.worldPos = IN[2].worldPos;
				OUT.norm = IN[2].norm;

				OUT.barycentricCoordinates = float2(0, 0);
				triStream.Append(OUT);
			}

			half4 frag(g2f IN) : COLOR
			{
				float3 scaledWorldPos = IN.worldPos / _TexScale;
				float3 pWeight = abs(IN.norm);
				pWeight /= pWeight.x + pWeight.y + pWeight.z;

				// Getting barycentric coordinates
				float3 barys;
				barys.xy = IN.barycentricCoordinates;
				barys.z = 1 - barys.x - barys.y;

				int v0, v1, v2;
				v0 = IN.uv_TexArr.x;
				v1 = IN.uv_TexArr.y;
				v2 = IN.uv_TexArr.z;

				float3 projected;

				// Triplanar mapping texture 1
				projected = float3(scaledWorldPos.y, scaledWorldPos.z, v0);
				float3 tex0X = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

				projected = float3(scaledWorldPos.x, scaledWorldPos.z, v0);
				float3 tex0Y = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

				projected = float3(scaledWorldPos.x, scaledWorldPos.y, v0);
				float3 tex0Z = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

				float3 tex0 = (tex0X + tex0Y + tex0Z) * barys.x;

				// Triplanar mapping texture2
				projected = float3(scaledWorldPos.y, scaledWorldPos.z, IN.uv_TexArr.y);
				float3 tex1X = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

				projected = float3(scaledWorldPos.x, scaledWorldPos.z, IN.uv_TexArr.y);
				float3 tex1Y = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

				projected = float3(scaledWorldPos.x, scaledWorldPos.y, IN.uv_TexArr.y);
				float3 tex1Z = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

				float3 tex1 = (tex1X + tex1Y + tex1Z) * barys.y;

				// Triplanar mapping texture3
				projected = float3(scaledWorldPos.y, scaledWorldPos.z, v2);
				float3 tex2X = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

				projected = float3(scaledWorldPos.x, scaledWorldPos.z, v2);
				float3 tex2Y = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

				projected = float3(scaledWorldPos.x, scaledWorldPos.y, v2);
				float3 tex2Z = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

				float3 tex2 = (tex2X + tex2Y + tex2Z) * barys.z;

				float3 mult = tex0 + tex1 + tex2 ;
				
				/*float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

				projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
				float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

				projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
				float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;*/
			

				return fixed4(mult, 1.0);
			}
			ENDCG

		}
		}
}
