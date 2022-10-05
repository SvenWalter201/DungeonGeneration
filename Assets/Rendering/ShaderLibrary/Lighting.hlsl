#ifndef LIGHTING_INCLUDED
#define LIGHTING_INCLUDED

/**

 https://www.youtube.com/watch?v=GQyCPaThQnA

Presence of TEXTURE2D(_RampTex); and sampler_BaseTex
Also Keyword _SPECULAR_HIGHLIGHT is required if highlights are desired
**/

#include "./GeometryHelper.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct CustomLightingData
{
    float3 albedo;
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoord;
    float smoothness;
    float metallic;
    float kd;
    float ks;
};

float GetSmoothnessPower(float rawSmoothness)
{
    return exp2(10 * rawSmoothness + 1);
}

float3 CustomLightHandling(CustomLightingData d, Light light)
{
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    float3 diffuse = saturate(dot(d.normalWS, light.direction));

    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float3 specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;

    return d.albedo * radiance * (specular + diffuse); 
    
}

float3 diffuseLighting(float3 normalWS, Light light)
{
    float3 diffuse = saturate(dot(normalWS, light.direction));
    return diffuse;
}



float3 diffuse(CustomLightingData d)
{
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

    float3 color = 0;

    color += diffuseLighting(d.normalWS, mainLight);

    #ifdef _ADDITIONAL_LIGHTS

        uint additionalLightsCount = GetAdditionalLightsCount();
        for(uint i = 0; i < additionalLightsCount; i++)
        {
            Light light = GetAdditionalLight(i, d.positionWS, 1);
            color += diffuseLighting(d.normalWS, light);
        }
    #endif

    return color;    
}

float3 specularLighting(CustomLightingData d, Light light)
{
    float3 diffuse = saturate(dot(d.normalWS, light.direction));

    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float3 specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;  
    specular = specular * lerp(float3(1,1,1), d.albedo, d.metallic);  
    return specular;
}



float3 specular(CustomLightingData d)
{
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

    float3 color = 0;

    color += specularLighting(d, mainLight);

    #ifdef _ADDITIONAL_LIGHTS

        uint additionalLightsCount = GetAdditionalLightsCount();
        for(uint i = 0; i < additionalLightsCount; i++)
        {
            Light light = GetAdditionalLight(i, d.positionWS, 1);
            color += specularLighting(d, light);
        }
    #endif

    return color;    
}

float3 plastic(CustomLightingData d)
{
    return  d.ks * specular(d) + d.kd * diffuse(d) * d.albedo;
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

    return color;
}

struct CustomShadowAttenuationData
{
    float3 positionWS;
    float4 shadowCoord;
};

float GetShadowAttenuationOnly(CustomShadowAttenuationData d)
{
    float shadowAttenuation = 0;
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);
    shadowAttenuation += mainLight.shadowAttenuation;


    #ifdef _ADDITIONAL_LIGHTS

        uint additionalLightsCount = GetAdditionalLightsCount();
        for(uint i = 0; i < additionalLightsCount; i++)
        {
            Light light = GetAdditionalLight(i, d.positionWS, 1);
            shadowAttenuation += light.shadowAttenuation;

        }
    #endif

    return saturate(shadowAttenuation);
}

#endif