Shader "Voyage/CustomRenderTexture/Simple"
{
    Properties
    {
        _Tex("InputTex", 2D) = "black" {}
    }

    SubShader
    {
       Lighting Off
       //Blend SrcAlpha OneMinusSrcAlpha

       Pass
       {
           CGPROGRAM
           #include "UnityCustomRenderTexture.cginc"
           #pragma vertex CustomRenderTextureVertexShader
           #pragma fragment frag
           #pragma target 3.0

           sampler2D   _Tex;

           /*inline half3 GammaToLinearSpace(half3 sRGB)
           {
               // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
               return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
           }*/

           float frag(v2f_customrendertexture i) : COLOR
           {


               float4 col = tex2D(_Tex, i.localTexcoord.xy);
               //col.rgb = LinearToGammaSpace(col.rgb);
               col.r = LinearToGammaSpaceExact(col.r);
               col.g = LinearToGammaSpaceExact(col.g);
               col.b = LinearToGammaSpaceExact(col.b);
               //col.a = GammaToLinearSpaceExact(col.a);
               uint4 col_int = uint4(round(col * 255));
               int val =  (col_int.r << 0) | (col_int.g << 8) | (col_int.b << 16) | (col_int.a << 24);
               
               //float output = asfloat(dot(uint4(round(col * 255)), 1 << uint4(0, 8, 16, 24)));
               return asfloat(val);
           }
           ENDCG
           }
    }
}
