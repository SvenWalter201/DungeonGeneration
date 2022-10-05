using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CaveGenerationCellularAutomata : MonoBehaviour
{
    [SerializeField] RawImage uiImageTarget;
    [SerializeField] ComputeShader generationShader;
    [SerializeField, Range(16, 2048)] int resolution = 256;
    [SerializeField, Range(0f,1f)] float fillPercentage = 0.5f;
    [SerializeField, Range(0,8)] int terrainMargin = 4, wallMargin = 4;
    [SerializeField, Range(0,6)] int smoothingIterations = 3;
    [SerializeField] bool autoUpdate = false, reseed = false;

    static readonly int
        resultTextureId = Shader.PropertyToID("_Result"),
        tempTextureId = Shader.PropertyToID("_Temp"),
        seedId = Shader.PropertyToID("_Seed"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        wallMarginId = Shader.PropertyToID("_WallMargin"),
        terrainMarginId = Shader.PropertyToID("_TerrainMargin"),
        fillPercentageId = Shader.PropertyToID("_RandomFillPercentage"); 

    int generateKernelId, smoothKernelId;
    string seed;

    [HideInInspector] public RenderTexture renderTexture;
    
    RenderTexture CreateRenderTexture()
    {
        return new RenderTexture(resolution, resolution, 24)
        {
            format = RenderTextureFormat.ARGB32,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            enableRandomWrite = true,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB
        };
    }


    public void Generate()
    {
        int threadGroupSize = resolution / 8;

        generateKernelId = generationShader.FindKernel("Generate");
        smoothKernelId = generationShader.FindKernel("Smooth");

        if(seed == null || reseed)
            seed = (Time.unscaledTime * Time.time).ToString();
            
        int seedHash = (int)(seed.GetHashCode() * 0.00001f);

        renderTexture = CreateRenderTexture();

        generationShader.SetTexture(generateKernelId, resultTextureId, renderTexture);
        generationShader.SetFloat(fillPercentageId, fillPercentage);
        generationShader.SetInt(seedId, seedHash);
        generationShader.SetInt(resolutionId, resolution);
        generationShader.SetInt(wallMarginId, wallMargin);
        generationShader.SetInt(terrainMarginId, terrainMargin);

        generationShader.Dispatch(generateKernelId, threadGroupSize, threadGroupSize, 1);

        for (int i = 0; i < smoothingIterations; i++)
        {
            RenderTexture temp = CreateRenderTexture();
            Graphics.Blit(renderTexture,temp); //tranfer output from previous generation to new texture
            generationShader.SetTexture(smoothKernelId, tempTextureId, temp);
            generationShader.SetTexture(smoothKernelId, resultTextureId, renderTexture);
            generationShader.Dispatch(smoothKernelId, threadGroupSize, threadGroupSize, 1);
        }

        if(uiImageTarget != null)
            uiImageTarget.texture = renderTexture;

    }

    void OnValidate() 
    {
        if(autoUpdate)
            Generate();
    }
}
