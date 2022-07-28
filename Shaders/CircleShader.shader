Shader "Voyage/Unlit/CircleShader"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float4 screenPosition : TEXCOORD1;
                
            };

            float4 _Color;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPosition = ComputeScreenPos(o.vertex);
                //o.vertex.xyz = TransformObjectToWorld(v.vertex.xyz)
                return o;
            }

            /* FIXME Junction function
             * - stepped.x + stepped.y gives bad result on junctions
             * - max(stepped.x, stepped.y) gives some nice result,
             *   but not the desired effect
             *
             * The whole would be to just draw one on top of the other
             * with no branching.
             */
            fixed4 frag(v2f i) : SV_Target
            {
                float dist_from_center = distance(i.uv, float2(0.5,0.5));
                return smoothstep(0, 0.1, SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPosition)); // fixed4((fixed3)dist_from_center, 1.0);

            }
            ENDCG
        }
    }
}
