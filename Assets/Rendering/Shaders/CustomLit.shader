Shader "Unlit/CustomLit"
{
    Properties
    {
        _BaseColor ("Main Color", Color) = (0.5,0.5,0.5,1)
		_BaseTex ("Tex", 2D) = "white" {}
		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
		_NormalScale ("Normal Scale", Range(0.0, 1.0)) = 1.0
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _KS ("KS", Range(0.0, 1.0)) = 0.5
        _KD ("KD", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0 //Toggle the Keyword _CLIPPING
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM


                #pragma target 3.5
 			    #pragma shader_feature _CLIPPING
                //Lighting Keywords
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ _ADDITIONAL_LIGHTS
                #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile _ _SHADOWS_SOFT

                #pragma vertex vert
                #pragma fragment frag  
                #include "./CustomLitPass.hlsl"  
            ENDHLSL
        }

        Pass
        {
			Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}

            HLSLPROGRAM
                #pragma target 3.5
                
 			    #pragma shader_feature _CLIPPING

                //Lighting Keywords
                #pragma multi_compile_shadowcaster

                #pragma vertex vert
                #pragma fragment frag 

                #define SHADOW_CASTER_PASS

                #include "./CustomLitPass.hlsl" 
            ENDHLSL
		} 
    }
}
