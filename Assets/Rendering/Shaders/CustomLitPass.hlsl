#ifndef TOON_PASS_INCLUDED
#define TOON_PASS_INCLUDED

#include "./../ShaderLibrary/GeometryHelper.hlsl"

//TEXTURE2D(_BaseTex);
//SAMPLER(sampler_BaseTex);
//TEXTURE2D(_NormalMap);  

#include "../ShaderLibrary/Lighting.hlsl"

#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial) 
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _NormalScale)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _KS)
    UNITY_DEFINE_INSTANCED_PROP(float, _KD)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float4 GetBase(float2 baseUV)
{
    return INPUT_PROP(_BaseColor);
}

struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    float4 tangentOS : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID //for GPU instancing
};

struct Varyings
{
    float3 positionWS : VAR_P;
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    float3 normalWS : VAR_NORMAL_WS;
    float4 tangentWS : VAR_TANGENT_WS;
};

Varyings vert (Attributes input)
{
    Varyings output;
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionCS = CalculatePositionCSWithShadowCasterLogic( output.positionWS,  output.normalWS);
    output.baseUV = input.baseUV;
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

    return output;
}



float4 frag (Varyings input) : SV_TARGET
{
    float4 base = GetBase(input.baseUV);// * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.baseUV);

    #if defined(_CLIPPING)
        clip(base.a - INPUT_PROP(_Cutoff)); //abort and discard fragment if the result of is below zero
	#endif

    #ifdef SHADOW_CASTER_PASS
        return 0;
    #endif

    //NORMALS
    //float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseTex, input.baseUV);
    //float scale = INPUT_PROP(_NormalScale);
    //float3 normal = DecodeNormal(normalSample, scale);
    //float3 normalWSApplied = NormalTangentToWorld(normal, input.normalWS, input.tangentWS); //apply normalmap

    //LIGHTING
    CustomLightingData d;
    d.albedo = base.rgb;
    d.normalWS = NormalizeNormalPerPixel(input.normalWS);
    d.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS);
    d.shadowCoord = CalculateShadowCoord(input.positionWS, input.positionCS);
    d.positionWS = input.positionWS;
    d.smoothness = INPUT_PROP(_Smoothness);
    d.metallic = INPUT_PROP(_Metallic);
    d.kd = INPUT_PROP(_KD);
    d.ks = INPUT_PROP(_KS);
    float3 color = plastic(d);
    return float4(color, 1.0);
}

#endif 
