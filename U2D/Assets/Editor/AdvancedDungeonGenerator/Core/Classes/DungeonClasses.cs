using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonSegment
{
    [Header("Artificial Displacement")]
    public Vector3 artificialDisplacement;
    public bool useLoader = true;
    public float loaderAngle;
    public Vector3 deltaLoaderPos;

    public bool useExtra;
    public GameObject extra;
    public Vector3 extraPos;

    [Header("Dungeon Elements")]
    public int floors;
    public DungeonShape shape;
    public bool useMargin;
    public int margin;

    [Header("Segment Brushes")]
    public DungeonBrush walls;
    public DungeonBrush structures;

    [Header("Decorations")]
    public DungeonDecoration wallDeco;
    public DungeonDecoration cornerDeco;
    public DungeonDecoration floorDeco;

    [Header("Structures Specifications")]
    public float structuresHeight;
    [Space]
    public bool  useGroundInStructures;
    public float structuresGroundHeight;
    [Space]
    public bool applyMeshToStructures;
    public int  structuresLayer;
    public bool applyMeshToStructuresGround;
    public int  structuresGroundLayer;
}

[System.Serializable]
public class DungeonSite
{
    public GameObject mainObject;
    public bool isGenerated = false;

    public Vector3 position;
    public AccessInfo acInfo;

    public int chunkSize;
    public int chunkWidth;
    public int chunkHeight;

    public bool isMarged = false;
    public bool useMargin;
    public int  margin;

    public int generation;
    public int order;

    public Dictionary<Coord, MappingChunckInfo> siteInfo;
}

public class ChunckMeshData
{
    public List<Matrix4x4> matrices;
    public List<int> matricesOrder;

    public bool isComplex;
    public Vector3 position;
    public int identifier;
    public int index;
    public int segment;

    public ChunckMeshData()
    {
        matrices = new List<Matrix4x4>();
        matricesOrder = new List<int>();

        isComplex = true;
        position = Vector3.zero;
        identifier = -1;
        index = -1;
        segment = -1;
    }
}

public class DecoMeshData
{
    public List<List<Matrix4x4>> matrices;

    public int decoCount;
    public Vector3 position;
    public int identifier;
    public int index;
    public int segment;

    public DecoMeshData()
    {
        matrices = new List<List<Matrix4x4>>();

        decoCount = -1;
        position = Vector3.zero;
        identifier = -1;
        index = -1;
        segment = -1;
    }
}

[System.Serializable]
public class NoiseInfo
{
    [HideInInspector]
    public int mapWidth;
    [HideInInspector]
    public int mapHeight;

    [Header("Noise Properties")]
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public NoiseInfo()      //Default values
    {
        noiseScale  = 9;
        octaves     = 4;
        persistance = 0.5f;
        lacunarity  = 2;
    }

    public NoiseInfo(float noiseScale, int octaves, float persistance, float lacunarity)
    {
        this.noiseScale  = noiseScale;
        this.octaves     = octaves;
        this.persistance = persistance;
        this.lacunarity  = lacunarity;
    }
}

[System.Serializable]
public class NoiseProcessInfo
{
    [Header("Noise Processing")]
    [Range(0, 1)]
    public float minFloorValue;
    [Range(0, 1)]
    public float minStructureValue;

    [Header("Floor Processing")]
    public int minTilesToGenWalls;
    public int minTilesToGenRooms;
    [Range(1, 2)]
    public int failedRoomValue;
    [Space]
    public int minTilesToGenStructures;
    public int maxHallwayRadius;

    public NoiseProcessInfo()   //Default values
    {
        minFloorValue           = 0.5f;
        minStructureValue       = 0.75f;
        minTilesToGenWalls      = 20;
        minTilesToGenRooms      = 50;
        failedRoomValue         = 1;
        minTilesToGenStructures = 3;
        maxHallwayRadius        = 3;
    }

    public NoiseProcessInfo(float mfv, float msfv, int mcp, int mch, int fhb, int mcs, int mrp)
    {
        minFloorValue      = mfv;
        minStructureValue  = msfv;
        minTilesToGenWalls = mcp;
        minTilesToGenRooms = mch;
        failedRoomValue    = fhb;
        minTilesToGenStructures = mcs;
        maxHallwayRadius   = mrp;
    }
}

[System.Serializable]
public class AccessInfo
{
    public bool normalAccessGen;
    [Space]

    [Header("Entrance")]
    public bool hasEntrance;
    [Range(1, 4)]
    public int entranceDirection;
    public Vector2 entranceEnd;

    [Header("Exit")]
    public bool hasExit;
    [Range(1, 4)]
    public int exitDirection;
    public Vector2 exitStart;

    public AccessInfo()         //Default values
    {
        this.normalAccessGen   = true;

        this.hasEntrance       = true;
        this.entranceDirection = 1;
        this.entranceEnd       = Vector2.zero;

        this.hasExit           = true;
        this.exitDirection     = 3;
        this.exitStart         = Vector2.zero;
    }

    public AccessInfo(AccessInfo acInfo)
    {
        this.normalAccessGen = acInfo.normalAccessGen;

        this.hasEntrance        = acInfo.hasEntrance;
        this.entranceDirection  = acInfo.entranceDirection;
        this.entranceEnd        = acInfo.entranceEnd;

        this.hasExit        = acInfo.hasExit;
        this.exitDirection  = acInfo.exitDirection;
        this.exitStart      = acInfo.exitStart;
    }
}

[System.Serializable]
public class Decoration
{
    public bool isStackable;

    public GameObject objeto;
    public Material material;

    public float probability;
    public float deltaH;
    public bool ranRot;
    public Quaternion rotation;

    public Decoration()         //Default values
    {
        isStackable = true;
        ranRot      = true;
        rotation    = Quaternion.identity;
    }
}
