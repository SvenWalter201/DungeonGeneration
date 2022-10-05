/*
Code is from GitHub, distributed under the MIT license

*/

#include "NoiseHelper.hlsl"

void MarbleNoise_float(float3 Pos, float3 Offset, float VeinFrequency, float VeinFalloff, float NoiseFrequency, out float Out) {
    float N = 0;
    float3 pos = Offset + Pos;

    float x = 0;
    x += dm_SimplexNoise(pos * NoiseFrequency * 1);
    x += dm_SimplexNoise(pos * NoiseFrequency * 2);
    x += dm_SimplexNoise(pos * NoiseFrequency * 3);

    N = sin(x * VeinFrequency);

    Out = 1 - pow((N + 1) * 0.5, VeinFalloff);
}

void BillowVolumeNoise_float(float3 Pos, float3 Offset, float Octaves, float Frequency, float Gain, float Lacunarity, float Persistence, float Bias, out float Out) {
    float F = Frequency;
    float A = Gain;
    float N = 0;
    for (int i = 1; i < Octaves; i++)
    {
        float3 pos = Offset + Pos;
        float s = dm_SimplexNoise(pos * F);
        s *= A;
        s = abs(s);
        N += (s * Lacunarity) + Bias;
        F *= Lacunarity;
        A *= Persistence;
    }
    Out = (N + 1) * 0.5;
}

void VoronoiNoise_float(float3 Pos, float3 Offset, float Frequency, out float Out) {

    float2 N = 0;
    float3 pos = Offset + Pos;
    N = dm_Cellular(pos * Frequency);
    Out = N.y - N.x;
}

void UnfilteredVolumeNoise_float(float3 Pos, float3 Offset, float Frequency, out float Out) {
    Out = dm_SimplexNoise((Offset + Pos) * Frequency) + 1 * 0.5;
}

void BarsNoise_float(float3 Pos, float3 Offset, float3 Scale, out float Out)
{
    float3 S = 1 / Scale;
    float3 N = (floor(Pos * S) * Scale);
    Out = length(Pos - N);
}

void MultiFractalVolumeNoise_float( float3 Pos, float3 Offset, float Octaves, float Frequency, float Gain, float Lacunarity, float Persistence, out float Out)
{
    float F = Frequency;
    float A = Gain;
    float N = 1.0;
    for (int i = 1; i < Octaves; i++)
    {
        float3 pos = Offset + Pos;
        float n = -1 * dm_SimplexNoise(pos * F);
        N += (n * A);
        A *= Persistence;
        F *= Lacunarity;
    }

    Out = (N + 1) * 0.5;
}

void WorleyNoise_float(float3 Pos, float3 Offset, float Frequency, out float Out)
{
    float2 N = 0;
    float3 pos = Offset + Pos;
    N = dm_Cellular(pos * Frequency);
    Out = min(N.y, N.x);
}

void CheckerNoise_float(float3 Pos, float3 Offset, float3 Scale, out float Out)
{
    float N = 0;
    float3 pos = floor((Offset + Pos) * (1 / Scale)) * Scale;
    N = frac(sin(dot(pos, float3(12.9898, 78.233, 43.141567))) * 43758.5453123);
    Out = N;
}

void RidgeVolumeNoise_float(float3 Pos, float3 Offset, float Octaves, float Frequency, float Gain, float Lacunarity, float Persistence, out float Out)
{
    float F = Frequency;
    float A = Gain;
    float3 pos = Offset + Pos;
    float N = 1.0 - abs(dm_SimplexNoise(pos * F));
    N *= N;
    float s = N;
    for (int i = 1; i < Octaves; i++)
    {
        float weight = saturate(s * 2);
        F *= Lacunarity;
        A *= Persistence;
        pos = Offset + Pos;
        s = 1.0 - abs(dm_SimplexNoise(pos * F));
        s *= s;
        s *= weight;
        N += s * A;
    }
    Out = N;
 }

void FractalVolumeNoise_float( float3 Pos, float3 Offset, float Octaves, float Frequency, float Gain, float Lacunarity, float Persistence, out float Out)
{
    float N = 0;
    float3 pos = Offset + Pos;
    N += dm_FractalNoise(pos, Frequency, Gain, Lacunarity, Persistence, Octaves);
    Out = (N + 1) * 0.5;
}

void CellularNoise_float(float3 Pos, float3 Offset, float Frequency, out float Out)
{
    float2 N = 0;
    float3 pos = Offset + Pos;
    N = dm_Cellular(pos * Frequency);
    Out = (N.y + N.x) * 0.5;
}

/*
static string BillowVolumeNoise(
    [Slot(0, Binding.ObjectSpacePosition)] Vector3 Pos,
    [Slot(8, Binding.None)] Vector3 Offset,
    [Slot(1, Binding.None, 3, 3, 3, 3)] Vector1 Octaves,
    [Slot(3, Binding.None, 1, 1, 1, 1)] Vector1 Frequency,
    [Slot(4, Binding.None, 1, 1, 1, 1)] Vector1 Gain,
    [Slot(5, Binding.None, 5, 5, 5, 5)] Vector1 Lacunarity,
    [Slot(6, Binding.None, 0.5f, 0.5f, 0.5f, 0.5f)] Vector1 Persistence,
    [Slot(9, Binding.None, 0.5f, 0.5f, 0.5f, 0.5f)] Vector1 Bias,
    [Slot(7, Binding.None)] out Vector1 Out)
{
    return @"
    {
        float F = Frequency;
        float A = Gain;
        float N = 0;
        for (int i = 1; i < Octaves; i++)
        {
            float3 pos = Offset + Pos;
            float s = dm_SimplexNoise(pos * F);
            s *= A;
            s = abs(s);
            N += (s * Lacunarity) + Bias;
            F *= Lacunarity;
            A *= Persistence;
        }
        Out = (N + 1) * 0.5;
    }

    */