#ifndef CUSTOM_TOON_LIGHTING_INCLUDED
#define CUSTOM_TOON_LIGHTING_INCLUDED

/**

 https://www.youtube.com/watch?v=GQyCPaThQnA

Toon Lighting assumes previous inclusion of 
#include "../GeometryTesting/GeometryHelper.hlsl"

Presence of TEXTURE2D(_RampTex); and sampler_BaseTex
Also Keyword _SPECULAR_HIGHLIGHT is required if highlights are desired
**/


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct CustomLightingData
{
    float3 albedo;
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoord;
    float smoothness;
    float4 highlightColor;
};

float GetSmoothnessPower(float rawSmoothness)
{
    return exp2(10 * rawSmoothness + 1);
}

float3 CustomLightHandling(CustomLightingData d, Light light)
{
    
    float3 radiance = light.color * light.distanceAttenuation;
    float diffuse = dot(d.normalWS, light.direction);
    if(diffuse < 0)
    {
        radiance = light.color;
        diffuse = saturate(diffuse);
    }

    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float specular = pow(specularDot, GetSmoothnessPower(d.smoothness));

    float uCoord = clamp(diffuse, 0.01, 0.99);
    float4 rampValue = SAMPLE_TEXTURE2D(_RampTex, sampler_BaseTex, float2(uCoord, 0.5));   

    if(light.shadowAttenuation > 0.5)
    {
        #ifdef _SPECULAR_HIGHLIGHT
            return (specular > 0.5) ? d.highlightColor : d.albedo * radiance * rampValue;
        #else
            return d.albedo * radiance * rampValue;
        #endif
    } 
    else 
    {
        return d.albedo * radiance * SAMPLE_TEXTURE2D(_RampTex, sampler_BaseTex, float2(0.01, 0.5)); 
    }
}

float3 CustomLighting(CustomLightingData d)
{
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

    float3 color = 0;

    color += CustomLightHandling(d, mainLight);

    #ifdef _ADDITIONAL_LIGHTS

        uint additionalLightsCount = GetAdditionalLightsCount();
        for(uint i = 0; i < additionalLightsCount; i++)
        {
            Light light = GetAdditionalLight(i, d.positionWS, 1);
            color += CustomLightHandling(d, light);
        }
    #endif

    return saturate(color);
}

#endif