using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int seed, Vector2 offset, NoiseInfo nI)
    {
        float[,] noiseMap = new float[nI.mapWidth, nI.mapHeight];

        if (nI.noiseScale <= 0)
        {
            nI.noiseScale = 0.0001f;
        }

        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[nI.octaves];
        for (int i = 0; i < nI.octaves; i++)
        {
            float offsetX = rng.Next(-10000, 10000) + offset.x;
            float offsetY = rng.Next(-10000, 10000) + offset.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = nI.mapWidth / 2;
        float halfHeight = nI.mapHeight / 2;

        for (int y = 0; y < nI.mapHeight; y++)
        {
            for (int x = 0; x < nI.mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < nI.octaves; i++)
                {
                    float sampleX = (x - halfWidth) / nI.noiseScale * frequency + octaveOffset[i].x;
                    float sampleY = (y - halfHeight) / nI.noiseScale * frequency + octaveOffset[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= nI.persistance;
                    frequency *= nI.lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < nI.mapHeight; y++)
        {
            for (int x = 0; x < nI.mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
	
}
