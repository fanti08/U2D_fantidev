using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    int[,] Map;
    private Quaternion[] directions = new Quaternion[4] { Quaternion.identity, Quaternion.LookRotation(Vector3.right),
                                        Quaternion.LookRotation(Vector3.back), Quaternion.LookRotation(Vector3.left)};
    DungeonDecoration wallsDeco;
    DungeonDecoration floorsDeco;
    DungeonDecoration cornersDeco;

    public Dictionary<Coord, MappingChunckInfo> GenerateSiteInformation(DungeonSite site, int[,] map)
    {
        Dictionary<Coord, MappingChunckInfo> information = new Dictionary<Coord, MappingChunckInfo>();
        Map = map;
        int start = 1;
        bool groundingStructures = DungeonGenerator.Instance.dungeon.segments[site.generation].useGroundInStructures;
        
        wallsDeco = null;
        floorsDeco = null;
        cornersDeco = null;

        bool hasDecoration = DungeonGenerator.Instance.dungeon.hasDecoration;
        if (hasDecoration)
        {
            if (DungeonGenerator.Instance.dungeon.segments[site.generation].wallDeco)
            {
                wallsDeco = DungeonGenerator.Instance.dungeon.segments[site.generation].wallDeco;
            }
            if (DungeonGenerator.Instance.dungeon.segments[site.generation].floorDeco)
            {
                floorsDeco = DungeonGenerator.Instance.dungeon.segments[site.generation].floorDeco;
            }
            if (DungeonGenerator.Instance.dungeon.segments[site.generation].cornerDeco)
            {
                cornersDeco = DungeonGenerator.Instance.dungeon.segments[site.generation].cornerDeco;
            }
        }

        int chunkSize = site.chunkSize;
        int chunkWidth = site.chunkWidth;
        int chunkHeight = site.chunkHeight;
        int index = site.order;
        int segment = site.generation;


        float posChunkX = -(((chunkWidth * chunkSize) / 2) - (chunkSize / 2));

        for (int chunkX = 0; chunkX < chunkWidth; chunkX++)
        {
            float posChunkY = -((chunkHeight * chunkSize) / 2 - chunkSize / 2);

            for (int chunkY = 0; chunkY < chunkHeight; chunkY++)
            {
                float posX = -((chunkSize / 2) - (1 / 2));

                Vector3 position = new Vector3(posChunkX, 0, posChunkY);
                MappingChunckInfo chunkBuffer = new MappingChunckInfo(position);
                if (hasDecoration)
                {
                    InitDecorations(ref chunkBuffer);
                }

                for (int x = start; x < chunkSize + start; x++)
                {
                    float posY = -((chunkSize / 2) - (1 / 2));
                    int indexX = (chunkX * chunkSize) + x;

                    for (int y = start; y < chunkSize + start; y++)
                    {
                        int indexY = (chunkY * chunkSize) + y;

                        if (Map[indexX, indexY] == 1)      //For walls
                        {
                            AddComplexBlockData(ref chunkBuffer.walls, posX, posY, indexX, indexY, 0, site.isMarged, 1);   //marged, only for edges
                            if (hasDecoration)
                            {
                                if (wallsDeco)
                                {
                                    if(DungeonGenerator.Instance.rng.Next(101) < wallsDeco.probability)
                                    {
                                        AddDecoration(wallsDeco, ref chunkBuffer.wallsDeco, posX, posY);
                                    }
                                }

                                if (cornersDeco)
                                {
                                    for(int i = 0; i < 4; i++)
                                    {
                                        if (DungeonGenerator.Instance.rng.Next(101) < cornersDeco.probability)
                                        {
                                            AddCornerDecoration(cornersDeco, ref chunkBuffer.cornerDeco, posX, posY, indexX, indexY, i);
                                        }
                                    }
                                }
                            }
                        }
                        else //if (mapa[x, y] == -1) -All rooms go from -1 to -inf (the passages are 0)
                        {
                            if (map[indexX, indexY] < 1)    //For floors
                            {
                                AddToSimpleList(ref chunkBuffer.floors, posX, posY, 0);
                                if (hasDecoration)
                                {
                                    if (floorsDeco)
                                    {
                                        if (DungeonGenerator.Instance.rng.Next(101) < floorsDeco.probability)
                                        {
                                            AddDecoration(floorsDeco, ref chunkBuffer.floorDeco, posX, posY);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (map[indexX, indexY] == 5)         //For margins
                                {
                                    chunkBuffer.isMarged = true;
                                    AddToRotativeList(ref chunkBuffer.floors, posX, posY, 0);
                                }
                                else if (map[indexX, indexY] == 2)    //For structures
                                {
                                    AddComplexBlockData(ref chunkBuffer.structures, posX, posY, indexX, indexY, 0, false, 2);

                                    if (groundingStructures)
                                    {
                                        AddToSimpleList(ref chunkBuffer.structuresFloors, posX, posY, 0);
                                    }
                                }
                            }


                        }

                        posY++;
                    }

                    posX++;
                }

                information.Add(new Coord(chunkX, chunkY), chunkBuffer);

                posChunkY += chunkSize;
            }

            posChunkX += chunkSize;
        }

        return information;
    }

    void AddCornerDecoration(DungeonDecoration dunDeco, ref List<DecoData> decoData, float posX, float posY, int indexX, int indexY, int dir)
    {
        int order = -1;

        if (dir == 0)
            order = CheckNeighbors(indexX, indexY, 1, 1, false, true, 0);
        else if (dir == 1)
            order = CheckNeighbors(indexX, indexY, 1, -1, true, true, 0);
        else if (dir == 2)
            order = CheckNeighbors(indexX, indexY, -1, -1, false, true, 0);
        else if (dir == 3)
            order = CheckNeighbors(indexX, indexY, -1, 1, true, true, 0);

        if (order > 0)
        {
            int proba = DungeonGenerator.Instance.rng.Next(101);
            for (int i = 0; i < dunDeco.decorations.Count; i++)
            {
                if (proba < dunDeco.decorations[i].probability)
                {
                    Matrix4x4 matrix = Matrix4x4.identity;
                    Vector3 position = new Vector3(posX, dunDeco.decorations[i].deltaH, posY);

                    matrix.SetTRS(position, directions[dir], Vector3.one);
                    decoData[i].matrices.Add(matrix);

                    break;
                }
            }
        }

    }

    void AddDecoration(DungeonDecoration dunDeco, ref List<DecoData> decoData, float posX, float posY)
    {
        Quaternion rotation;
        Vector3 randRot = Vector3.zero;

        int proba = DungeonGenerator.Instance.rng.Next(101);
        for (int i = 0; i < dunDeco.decorations.Count; i++)
        {
            if(proba < dunDeco.decorations[i].probability)
            {
                if (dunDeco.decorations[i].ranRot)
                {
                    randRot.y = DungeonGenerator.Instance.rng.Next(0, 360);
                    rotation = Quaternion.Euler(randRot);
                }else{
                    rotation = dunDeco.decorations[i].rotation;
                }

                Matrix4x4 matrix = Matrix4x4.identity;
                Vector3 position = new Vector3(posX, dunDeco.decorations[i].deltaH, posY);

                matrix.SetTRS(position, rotation, Vector3.one);
                decoData[i].matrices.Add(matrix);

                break;
            }
        }
    }

    void InitDecorations(ref MappingChunckInfo chunkBuffer)
    {
        if (wallsDeco)
        {
            for (int i = 0; i < wallsDeco.decorations.Count; i++)
            {
                chunkBuffer.wallsDeco.Add(new DecoData(wallsDeco.decorations[i].isStackable));
            }
        }

        if (floorsDeco)
        {
            for (int i = 0; i < floorsDeco.decorations.Count; i++)
            {
                chunkBuffer.floorDeco.Add(new DecoData(floorsDeco.decorations[i].isStackable));
            }
        }

        if (cornersDeco)
        {
            for (int i = 0; i < cornersDeco.decorations.Count; i++)
            {
                chunkBuffer.cornerDeco.Add(new DecoData(cornersDeco.decorations[i].isStackable));
            }
        }
    }

    void AddToSimpleList(ref List<Matrix4x4> matrices, float posX, float posY, float h)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        Vector3 position = new Vector3(posX, h, posY);

        matrix.SetTRS(position, directions[0], Vector3.one);

        matrices.Add(matrix);
    }

    void AddToRotativeList(ref List<Matrix4x4> matrices, float posX, float posY, float h)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        Vector3 position = new Vector3(posX, h, posY);

        for (int i = 0; i < 4; i++)
        {
            matrix.SetTRS(position, directions[i], Vector3.one);
            matrices.Add(matrix);
        }
    }

    void AddComplexBlockData(ref List<BlockData> list, float posX, float posY, int indexX, int indexY, float h, bool isMerged, int id)
    {
        AddToList(ref list, posX, posY, indexX, indexY, h,  1,  1, false, 0, isMerged, id);
        AddToList(ref list, posX, posY, indexX, indexY, h,  1, -1, true,  1, isMerged, id);
        AddToList(ref list, posX, posY, indexX, indexY, h, -1, -1, false, 2, isMerged, id);
        AddToList(ref list, posX, posY, indexX, indexY, h, -1,  1, true,  3, isMerged, id);
    }

    void AddToList(ref List<BlockData> list, float posX, float posY, int indexX, int indexY, float h, int dx, int dy, bool inverted, int rotacion, bool lazyCheck, int id)
    {
        Vector3 position = new Vector3(posX, h, posY);

        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.SetTRS(position, directions[rotacion], Vector3.one);

        int order = CheckNeighbors(indexX, indexY, dx, dy, inverted, lazyCheck, id);

        list.Add(new BlockData(matrix, order));
    }

    int CheckNeighbors(int x, int y, int dx, int dy, bool inverted, bool lazyCheck, int id)
    {
        int buffer = 0;

        if (lazyCheck)
        {
            if (!inverted)
            {
                if (Map[x, y + dy] % 2 == 1)
                    buffer += 4;
                if (Map[x + dx, y + dy] % 2 == 1)
                    buffer += 2;
                if (Map[x + dx, y] % 2 == 1)
                    buffer += 1;
            }
            else
            {
                if (Map[x + dx, y] % 2 == 1)
                    buffer += 4;
                if (Map[x + dx, y + dy] % 2 == 1)
                    buffer += 2;
                if (Map[x, y + dy] % 2 == 1)
                    buffer += 1;
            }
        }
        else
        {
            if (!inverted)
            {
                if (Map[x, y + dy] == id)
                    buffer += 4;
                if (Map[x + dx, y + dy] == id)
                    buffer += 2;
                if (Map[x + dx, y] == id)
                    buffer += 1;
            }
            else
            {
                if (Map[x + dx, y] == id)
                    buffer += 4;
                if (Map[x + dx, y + dy] == id)
                    buffer += 2;
                if (Map[x, y + dy] == id)
                    buffer += 1;
            }
        }


        if (buffer == 7)
            return 0;   //FillCube
        else if (buffer == 1 || buffer == 3)
            return 1;   //HCube
        else if (buffer == 4 || buffer == 6)
            return 2;   //VCube
        else if (buffer == 5)
            return 3;   //CornerCube
        else
            return 4;   //RoundCube
    }
}

public struct MappingChunckInfo
{
    public bool isMarged;
    public Vector3 position;

    public List<BlockData> walls;
    public List<Matrix4x4> floors;
    public List<BlockData> structures;
    public List<Matrix4x4> structuresFloors;

    public List<DecoData> wallsDeco; 
    public List<DecoData> floorDeco;
    public List<DecoData> cornerDeco;

    public MappingChunckInfo(Vector3 position)
    {
        this.position = position;
        isMarged = false;

        walls = new List<BlockData>();
        floors = new List<Matrix4x4>();
        structures = new List<BlockData>();
        structuresFloors = new List<Matrix4x4>();

        wallsDeco = new List<DecoData>();
        floorDeco = new List<DecoData>();
        cornerDeco = new List<DecoData>();
    }
}

public struct DecoData
{
    public bool isStackable;
    public List<Matrix4x4> matrices;

    public DecoData(bool isStackable)
    {
        this.isStackable = isStackable;
        matrices = new List<Matrix4x4>();
    }
}

public struct BlockData
{
    public Matrix4x4 matrix; 
    public int order;        

    public BlockData(Matrix4x4 matrix, int order)
    {
        this.matrix = matrix;
        this.order = order;
    }
}
