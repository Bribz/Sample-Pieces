

Shader "Custom/ScreenSpaceUV" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
            #include "UnityLightingCommon.cginc" // for _LightColor0
			#include "UnityCG.cginc"

			float4 _Color;
			
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 diff : COLOR0;
                float4 vertex : SV_POSITION;
				float4 scrPos : TEXCOORD1;
				float3 wPos : TEXCOORD2;
				UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				half3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
                UNITY_TRANSFER_FOG(o,o.vertex);
				o.scrPos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col;

				
				float2 screenPosition = (i.scrPos.xy / i.scrPos.w);
				float dist = distance(_WorldSpaceCameraPos, i.wPos);

				screenPosition = 0.5*(screenPosition.xy + 1.0);
				
				//col = _Color;
				
				col = tex2D(_MainTex, screenPosition.xy);
				
				
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				//col += (half4(1, 1, 1, 1) * (1 - screenPosition.y)) * (i.vertex.y*.002);
				col += (half4(1, 1, 1, 1) * (22-i.wPos.y)*.045);//(20-i.wPos.y)*.09);//(i.vertex.y*.0015));
				col += (half4(1, 1, 1, 1) * (-dist*.015));
				col += col*(i.diff*.3);
				return col;
            }
            ENDCG
        }
	}
	//FallBack "Diffuse"
}
