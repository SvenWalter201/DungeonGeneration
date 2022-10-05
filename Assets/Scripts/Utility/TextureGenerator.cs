using UnityEngine;
using Unity.Mathematics;

public static class TextureGenerator
{
    public static Texture2D TextureFromBoolMap(bool[] boolMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] colorMap = new Color[boolMap.Length];
        for (int i = 0; i < colorMap.Length; i++)
            colorMap[i] = boolMap[i] ? Color.black : Color.white;

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    } 

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }       
}
