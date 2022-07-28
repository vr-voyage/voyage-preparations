Shader "Voyage/Unlit/DepthCamera"
{
    Properties
    {
        [HDR] _NearColor ("Color", Color) = (1,1,1,1)
        [HDR] _FarColor ("Color", Color) = (0,0,0,0)
        FadeDistance_ ("Fade Distance", Range(1,100)) = 50
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
                
            };

            float4 _NearColor;
            float4 _FarColor;
            sampler2D _CameraDepthTexture;
            float FadeDistance_;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float smooth_distance = smoothstep(0, 1 / FadeDistance_, SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPosition));
                return smooth_distance;
            }
            ENDCG
        }
    }
}
