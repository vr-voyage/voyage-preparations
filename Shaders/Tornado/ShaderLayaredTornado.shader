// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Voyage/Unlit/LayeredTornado"
{
    Properties
    {
        [HDR]_Color("Color", Color) = (2, 1.098039, 0.007843138, 0)
        [HDR]_Color_Second_("Color Second shell", Color) = (2, 1.098039, 0.007843138, 0)
        [HDR]_Color_Third_("Color Third shell", Color) = (2, 1.098039, 0.007843138, 0)

        Scale_("Scale", Range(0.1,2.0)) = 1.0
        Scale_Second_("Second shell scale", Range(0.1,2.0)) = 1.10
        Scale_Third_("Third shell scale", Range(0.15,2.5)) = 1.15

        Dissolve_("Dissolve", Range(0.0,1.0)) = 0
        Dissolve_Second_("Dissolve second shell", Range(0.0,1.0)) = 0
        Dissolve_Third_("Dissolve third shell", Range(0.0,1.0)) = 0

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

            fixed4 _Color_Second_;
            
            float Dissolve_Second_;
            float NoisePower_;
            float NoiseScale_;
            float TwirlAmount_;
            float2 TwirlCenter_;
            float2 TwirlSpeed_;
            float2 NoiseSpeed_;
            float Scale_Second_;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(
                    v.vertex
                    * float4(Scale_Second_, Scale_Second_, 1.0, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = tornado_effect(
                    float4(TwirlCenter_, TwirlAmount_, Dissolve_Second_),
                    float4(TwirlSpeed_, NoiseSpeed_),
                    float2(NoisePower_, NoiseScale_),
                    _Color_Second_, i.uv.xy);
                
                return col;
            }
            ENDCG
        }

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

            fixed4 _Color_Third_;
            
            float Dissolve_Third_;
            float NoisePower_;
            float NoiseScale_;
            float TwirlAmount_;
            float2 TwirlCenter_;
            float2 TwirlSpeed_;
            float2 NoiseSpeed_;
            float Scale_Third_;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(
                    v.vertex
                    * float4(Scale_Third_, Scale_Third_, 1.0, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = tornado_effect(
                    float4(TwirlCenter_, TwirlAmount_, Dissolve_Third_),
                    float4(TwirlSpeed_, NoiseSpeed_),
                    float2(NoisePower_, NoiseScale_),
                    _Color_Third_, i.uv.xy);
                
                return col;
            }
            ENDCG
        }

    }


}
