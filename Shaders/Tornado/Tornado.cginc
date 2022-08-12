#ifndef MYY_TORNADO_INCLUDED
#define MYY_TORNADO_INCLUDED


/**
 * twirl.xy : TwirlCenter
 * twirl.z : TwirlAmount
 * twirl.w : Dissolve
 * speed.xy : TwirlSpeed
 * speed.zw : NoiseSpeed
 * noise.x : NoisePower
 * noise.y : NoiseScale
 * noise.z : Dissolve
 */
fixed4 tornado_effect(
    float4 twirl,
    float4 speed,
    float2 noise,
    fixed4 tornadoColor : COLOR,
    float2 uv)
{
    float twirl_center = twirl.xy;
    float twirl_amount = twirl.z;
    float dissolve = twirl.w;
    float2 twirl_speed = speed.xy;
    float2 noise_speed = speed.zw;
    float noise_power = noise.x;
    float noise_scale = noise.y;

    float time = unity_time();

    /* First noise */
    float2 twirledUV = unity_twirl(uv, twirl_center, twirl_amount, time * twirl_speed);
    float twirledNoiseAmount = unity_simple_noise(twirledUV, 15);
    
    /* Second noise */
    float2 shearOffset = noise_speed * time;
    //float2 shearOffset = shearOffsetSpeed;
    float2 shearedUV = unity_radial_shear(uv, float2(0.5,0.5), float2(10,10), shearOffset);
    float sharedNoiseAmount = unity_simple_noise(shearedUV, noise_scale);

    float noiseAmount = pow(abs(sharedNoiseAmount * twirledNoiseAmount), abs(noise_power));
    clip(noiseAmount - dissolve);
    return tornadoColor * noiseAmount;
}

float4 scale_particle_vertex(
    float4 pos,
    float4 scale,
    float4 emitter_center)
{
    return UnityObjectToClipPos((pos - emitter_center) * scale + emitter_center);
}

#endif

