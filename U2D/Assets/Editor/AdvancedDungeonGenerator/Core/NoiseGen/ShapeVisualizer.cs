using System;
using System.Collections.Generic;
using UnityEngine;


public class ShapeVisualizer : MonoBehaviour
{
    [Header("Draw Mode")]
    public bool useAdvanceProcessing = true;
    public bool useProceduralAccess  = true;

    [Header("Randomness")]
    public int seed;
    public Vector2 offset;

    [Header("Shape Properties")]
    public DungeonShape dSh;
    public Renderer canvas;
    public AccessInfo acInfo;
    [Space]
    public bool autoUpdate = false;

    [HideInInspector]
    public int index;
    [HideInInspector]
    public bool showShapeProperties = true;
    [HideInInspector]
    public bool canEditShapeParameters = false;

    private bool drawZeroGrey;
    private float[,] noiseMap;
    private int[,] processedNoise;

    public System.Random rng;

    private void Start()
    {
        canvas = GetComponent<Renderer>();
    }

    public void Draw(int type, int order)
    {
        dSh.Initialize();

        rng = new System.Random(seed);

        acInfo.normalAccessGen = useProceduralAccess;

        if (type == 0)      //For Natural Shapes
        {
            noiseMap = Noise.GenerateNoiseMap(seed, offset, dSh.noiseInfo);
            noiseMap = BackgroundGenerator.ApplyBackground(noiseMap, dSh.backgroundMap);
            processedNoise = NoiseProcessor.ProcessNoise(noiseMap, dSh.noiseProcessInfo, ref acInfo, useAdvanceProcessing, rng);
        }
        else if (type == 1) //For Artificial Shapes
        {
            noiseMap = Noise.GenerateNoiseMap(seed, offset, dSh.noiseInfo);
            processedNoise = NoiseProcessor.ProcessNoise(noiseMap, dSh.noiseProcessInfo, ref acInfo, false, rng);

            processedNoise = BackgroundGenerator.ApplyArtificialCanvas(dSh.artificialCanvas, processedNoise, false);
            if (useAdvanceProcessing)
            {
                processedNoise = NoiseProcessor.ProcessIntMap(processedNoise, dSh.noiseProcessInfo, ref acInfo, false, rng);
                processedNoise = BackgroundGenerator.ApplyArtificialCanvas(dSh.artificialCanvas, processedNoise, true);
            }
        }
        else                //For Rooms
        {
            processedNoise = NoiseProcessor.ProcessIntMap(dSh.artificialCanvas, dSh.noiseProcessInfo, ref acInfo, true, rng);
            drawZeroGrey = false;
        }


        if (order == 0)         //Process Room Map
        {
            drawZeroGrey = (useAdvanceProcessing) ? true : false;
            DrawNoiseMap(processedNoise, drawZeroGrey);
        }
        else if (order == 1)    //Basic Noise Map
        {
            DrawNoiseMap(noiseMap);
        }
        else if (order == 2)    //Falloff Map
        {
            DrawNoiseMap(dSh.backgroundMap);
        }
        else                    //Base Canvas Image
        {
            DrawNoiseMap(dSh.artificialCanvas, false);
        }
    }

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        canvas.sharedMaterial.SetTexture("_BaseMap", texture);
        //canvas.sharedMaterial.mainTexture = texture;
    }

    public void DrawNoiseMap(int[,] noiseMap, bool drawZeroGrey)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(noiseMap[x,y] > 0)
                {
                    if (noiseMap[x, y] == 1)
                        colorMap[y * width + x] = Color.black;  //Walls
                    else if (noiseMap[x, y] == 2)
                        colorMap[y * width + x] = Color.blue;   //Structures
                    else if (noiseMap[x, y] == 3)
                        colorMap[y * width + x] = Color.red;    //Void
                    else if (noiseMap[x, y] == 4)
                        colorMap[y * width + x] = Color.green;  //Canvas overlapping
                }
                else 
                {
                    if (noiseMap[x, y] == 0)
                    {
                        if (drawZeroGrey)
                        {
                            colorMap[y * width + x] = Color.grey;   //Hallways
                        }else{
                            colorMap[y * width + x] = Color.white;  //Hallways
                        }
                    }
                    else
                        colorMap[y * width + x] = Color.white;      //Rooms (Any negative value)
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        canvas.sharedMaterial.SetTexture("_BaseMap", texture);
        //canvas.sharedMaterial.mainTexture = texture;
    }
}
