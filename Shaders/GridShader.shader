Shader "Voyage/Unlit/GridShader"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0,0,0,0)
        GridWidth_ ("Grid Width", Range(0,0.1)) = 0.05
        GridScale_ ("Grid Scale", Range(0.1,8)) = 1
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
                float4 position_in_world_space : TEXCOORD0;
            };

            float4 _Color;
            float GridWidth_;
            float GridScale_;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.position_in_world_space = mul(unity_ObjectToWorld, v.vertex * GridScale_);
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
                //float x = step(frac(i.position_in_world_space.x), 0.1);
                //float y = step(frac(i.position_in_world_space.z), 0.1);
                //float y = smoothstep(GridWidth_, 0.0, distances.y);

                float2 zero_to_center = i.position_in_world_space.xz + 0.5;
                float2 distances = abs(frac(zero_to_center) - float2(0.5,0.5));
                float2 stepped = smoothstep(GridWidth_, 0.0, distances);
                

                float total = max(stepped.x, stepped.y);
                fixed4 col = fixed4(stepped.x, stepped.y, total, 1.0);
                return col * _Color;
            }
            ENDCG
        }
    }
}
