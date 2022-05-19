using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackgroundGenerator 
{
    public static float[,] GenerateFalloffMap(int width, int height, float a, float b)
    {
        float[,] map = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = (i / (float)width) * 2 - 1;
                float y = (j / (float)height) * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, a, b);
            }
        }

        return map;
    }

    static float Evaluate(float value, float a, float b)
    {
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a));
    }

    public static int[,] ApplyMargenMap(int[,] noiseMap, DungeonSite thisSite, DungeonSite nextSite, DungeonSite prevSite)
    {
        int start = 1;

        int thisSiteX = (thisSite.chunkWidth + 2) * thisSite.chunkSize;
        int thisSiteY = (thisSite.chunkHeight + 2) * thisSite.chunkSize;
        int[,] map = new int[thisSiteX + 2, thisSiteY + 2];

        //Room Border Creation
        for(int chunckX = 0; chunckX < thisSite.chunkWidth + 2; chunckX++)
        {
            for (int chunckY = 0; chunckY < thisSite.chunkHeight + 2; chunckY++)
            {
                if (chunckX == 0 || chunckX == thisSite.chunkWidth + 1 || chunckY == 0 || chunckY == thisSite.chunkHeight + 1)
                {
                    int startX = (chunckX == 0) ? thisSite.chunkSize - thisSite.margin : 0;
                    int startY = (chunckY == 0) ? thisSite.chunkSize - thisSite.margin : 0;
                    int endX = (chunckX == thisSite.chunkWidth + 1) ? thisSite.margin : thisSite.chunkSize;
                    int endY = (chunckY == thisSite.chunkHeight  + 1) ? thisSite.margin : thisSite.chunkSize;
                    
                    int dX = chunckX * thisSite.chunkSize;
                    for (int x = 0; x < thisSite.chunkSize; x++)
                    {
                        int dY = chunckY * thisSite.chunkSize;
                        for (int y = 0; y < thisSite.chunkSize; y++)
                        {
                            if (x >= startX && x < endX && y >= startY && y < endY)
                            {
                                map[start + dX + x, start + dY + y] = 5;
                            }
                            else
                            {
                                map[start + dX + x, start + dY + y] = 3;
                            }
                        }
                    }
                }
            }
        }

        int thisSiteSizeX = thisSite.chunkWidth * thisSite.chunkSize;
        int thisSiteSizeY = thisSite.chunkHeight * thisSite.chunkSize;

        int nextSiteSizeX = nextSite.chunkWidth * nextSite.chunkSize;
        int nextSiteSizeY = nextSite.chunkHeight * nextSite.chunkSize;

        int prevSiteSizeX;
        int prevSiteSizeY;
        if (prevSite.isMarged)
        {
            prevSiteSizeX = (prevSite.chunkWidth - 2) * prevSite.chunkSize;
            prevSiteSizeY = (prevSite.chunkHeight - 2) * prevSite.chunkSize;
        }else{
            prevSiteSizeX = prevSite.chunkWidth * prevSite.chunkSize;
            prevSiteSizeY = prevSite.chunkHeight * prevSite.chunkSize;
        }
        

        //Overlapping Removal
        if (nextSite.acInfo != null)
        {
            if (nextSite.position.y == thisSite.position.y)
            {
                float xof = nextSite.position.x - thisSite.position.x;
                float yof = nextSite.position.z - thisSite.position.z;

                xof -= (float)nextSiteSizeX / 2;
                yof -= (float)nextSiteSizeY / 2;

                xof += (float)thisSiteSizeX / 2;
                yof += (float)thisSiteSizeY / 2;

                xof += thisSite.chunkSize + start;
                yof += thisSite.chunkSize + start;

                int xo = Mathf.Clamp((int)xof, 0, thisSiteX + 2);
                int yo = Mathf.Clamp((int)yof, 0, thisSiteY + 2);

                int xf = Mathf.Clamp((int)xof + nextSiteSizeX, 0, thisSiteX + 2);
                int yf = Mathf.Clamp((int)yof + nextSiteSizeY, 0, thisSiteY + 2);

                for (int x = xo; x < xf; x++)
                {
                    for (int y = yo; y < yf; y++)
                    {
                        map[x, y] = 3;     //Removed
                    }
                }
            }
        }

        
        if(prevSite.acInfo != null)
        {
            if (prevSite.position.y == thisSite.position.y)
            {
                float xof = prevSite.position.x - thisSite.position.x;
                float yof = prevSite.position.z - thisSite.position.z;

                xof -= (float)prevSiteSizeX / 2;
                yof -= (float)prevSiteSizeY / 2;

                xof -= (float)prevSite.margin;
                yof -= (float)prevSite.margin;

                xof += (float)thisSiteSizeX / 2;
                yof += (float)thisSiteSizeY / 2;

                xof += thisSite.chunkSize + start;
                yof += thisSite.chunkSize + start;

                int xo = Mathf.Clamp((int)xof, 0, thisSiteX + 2);
                int yo = Mathf.Clamp((int)yof, 0, thisSiteY + 2);

                int xf = Mathf.Clamp((int)xof + prevSiteSizeX + (prevSite.margin * 2), 0, thisSiteX + 2);
                int yf = Mathf.Clamp((int)yof + prevSiteSizeY + (prevSite.margin * 2), 0, thisSiteY + 2);

                for (int x = xo; x < xf; x++)
                {
                    for (int y = yo; y < yf; y++)
                    {
                        map[x, y] = 3;     //Removed
                    }
                }
            }
        }

        //Map Filling
        int sizeX = thisSite.chunkSize;
        int sizeY = thisSite.chunkSize;
        int maxX = thisSite.chunkWidth * thisSite.chunkSize;
        int maxY = thisSite.chunkHeight * thisSite.chunkSize;
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                map[start + sizeX + x, start + sizeY + y] = noiseMap[start + x, start + y];
            }
        }

        return map;
    }

    public static float[,] ApplyBackground(float[,] noiseMap, float[,] backMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - backMap[x, y]);
            }
        }

        return noiseMap;
    }

    public static int[,] ApplyArtificialCanvas(int[,] canvas, int[,] noiseMap, bool cleanCanvas)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        if (!cleanCanvas)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (canvas[x, y] < 4)  //Allow modification
                    {
                        noiseMap[x, y] = canvas[x, y];
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (canvas[x, y] < 4)  //Allow modification
                    {
                        if(canvas[x, y] == 0)
                            noiseMap[x, y] = -1;
                        else
                            noiseMap[x, y] = canvas[x, y];
                    }
                }
            }
        }

        return noiseMap;
    }
}
