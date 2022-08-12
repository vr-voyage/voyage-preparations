#ifndef MYY_SHADER_GRAPHS_PORTS_INCLUDED
#define MYY_SHADER_GRAPHS_PORTS_INCLUDED

float unity_fresnel_effect_float(float3 norm, float3 ViewDir, float Power)
{
    return pow((1.0 - saturate(dot(normalize(norm), normalize(ViewDir)))), Power);
}

float2 unity_twirl(float2 uv, float2 center, float strength, float2 offset)
{
    float2 delta = uv - center;
    float angle = strength * length(delta);
    float x = cos(angle) * delta.x - sin(angle) * delta.y;
    float y = sin(angle) * delta.x + cos(angle) * delta.y;
    return float2(x + center.x + offset.x, y + center.y + offset.y);
}

/* TODO: Replace by a fixed texture.
    * Make the texture big enough
    * Scan it diagonally
    * Use smoothstep to sample it
    */
inline float unity_noise_random_value (float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
}

inline float unity_noise_interpolate (float a, float b, float t)
{
    return (1.0-t)*a + (t*b);
}

inline float unity_value_noise (float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_noise_random_value(c0);
    float r1 = unity_noise_random_value(c1);
    float r2 = unity_noise_random_value(c2);
    float r3 = unity_noise_random_value(c3);

    float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
    float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}

float unity_simple_noise(float2 uv, float scale)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3-0));
    t += unity_value_noise(float2(uv.x*scale/freq, uv.y*scale/freq))*amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3-1));
    t += unity_value_noise(float2(uv.x*scale/freq, uv.y*scale/freq))*amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3-2));
    t += unity_value_noise(float2(uv.x*scale/freq, uv.y*scale/freq))*amp;

    return t;
}
/* --- */

float2 unity_radial_shear(float2 uv, float2 center, float strength, float2 offset)
{
    float2 delta = uv - center;
    float delta2 = dot(delta.xy, delta.xy);
    float2 delta_offset = delta2 * strength;
    return uv + float2(delta.y, -delta.x) * delta_offset + offset;
}

inline float unity_time()
{
    return _Time.y;
}

#endif

