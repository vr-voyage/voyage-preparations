// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Voyage/Unlit/SimpleTornado"
{
    Properties
    {
        [HDR]_Color("Color", Color) = (2, 1.098039, 0.007843138, 0)

        Scale_("Scale", Range(0.1,2.0)) = 1.0

        Dissolve_("Dissolve", Range(0.0,1.0)) = 0

        NoisePower_("NoisePower", Float) = 1
        NoiseScale_("NoiseScale", Float) = 15
        [ShowAsVector2] NoiseSpeed_("NoiseSpeed", Vector) = (0, 0.5, 0, 0)
        
        TwirlAmount_("TwirlAmount", Float) = 2
        [ShowAsVector2] TwirlCenter_("TwirlCenter", Vector) = (0.5, 0.5, 0, 0)
        [ShowAsVector2] TwirlSpeed_("TwirlSpeed", Vector) = (0, 0.5, 0, 0)
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque"  }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "ShaderGraphPorts.cginc"
            #include "Tornado.cginc"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;            
            };

            fixed4 _Color;
            
            float Dissolve_;
            float NoisePower_;
            float NoiseScale_;
            float TwirlAmount_;
            float2 TwirlCenter_;
            float2 TwirlSpeed_;
            float2 NoiseSpeed_;
            float Scale_;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * float4(Scale_, Scale_, 1.0, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = tornado_effect(
                    float4(TwirlCenter_, TwirlAmount_, Dissolve_),
                    float4(TwirlSpeed_, NoiseSpeed_),
                    float2(NoisePower_, NoiseScale_),
                    _Color, i.uv.xy);
                
                return col;
            }
            ENDCG
        }

    }


}
