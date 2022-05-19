using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DungeonShape", menuName = "Dungeon Component/DungeonShape", order = 1)]
public class DungeonShape : ScriptableObject
{
    public int minChunkSize       { get; private set; } = 4;
    public int maxChunkSize       { get; private set; } = 32;
    public int maxRoomChunkWidth  { get; private set; } = 8;
    public int maxRoomChunkHeight { get; private set; } = 8;

    public int maxConcentration { get; private set; } = 128;
    public int maxRemoteness    { get; private set; } = 128;

    public int maxNoiseScale { get; private set; } = 128;
    public int maxOctaves    { get; private set; } = 8;
    public int minLacunarity { get; private set; } = -4;
    public int maxLacunarity { get; private set; } = 4;

    public int maxPassageSize   { get; private set; } = 16;

    [Header("Map Proportions")]
    public SegmentGeneration generation;

    //Default Values
    public int chunkSize   = 16;
    public int chunkWidth  = 4;
    public int chunkHeight = 4;

    public NoiseInfo noiseInfo;
    public NoiseProcessInfo noiseProcessInfo;
    
    [Header("Background Properties")]
    //Default Values
    public float concentration = 2;
    public float remoteness    = 15;

    public float[,] backgroundMap;

    [Header("Artificiality Properties")]
    public bool isAnHabitacion;
    public Texture2D canvasTexture;
    public int[,] artificialCanvas;

    public Vector3 spawnPosition;

    public void Initialize()
    {
        if (generation == SegmentGeneration.Natural)
        {
            noiseInfo.mapWidth = (chunkSize * chunkWidth) + 2;
            noiseInfo.mapHeight = (chunkSize * chunkHeight) + 2;
            backgroundMap = BackgroundGenerator.GenerateFalloffMap(noiseInfo.mapWidth, noiseInfo.mapHeight, concentration, remoteness);
        }
        else if (generation == SegmentGeneration.Artificial)
        {
            chunkSize = 16;
            chunkWidth = canvasTexture.width / 16;
            chunkHeight = canvasTexture.height / 16;

            noiseInfo.mapWidth = (chunkSize * chunkWidth) + 2;
            noiseInfo.mapHeight = (chunkSize * chunkHeight) + 2;

            artificialCanvas = NoiseProcessor.GenerateFromTexture(canvasTexture, ref spawnPosition);
        }
    }
}

public enum SegmentGeneration { Natural, Artificial };