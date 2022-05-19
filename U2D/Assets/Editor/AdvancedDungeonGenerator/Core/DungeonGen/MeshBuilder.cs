using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour
{
    Queue<ThreadInfo<Info>> mainThreadQueue = new Queue<ThreadInfo<Info>>();
    object FrontDoor = new object();
    
    void Update()
    {
        if(mainThreadQueue.Count > 0)
        {
            ThreadInfo<Info> threadInfo = mainThreadQueue.Dequeue();
            threadInfo.callback(threadInfo.parameter);
        }
    }

    public void GenerateChunk(DungeonSite site, Coord start)
    {
        Dictionary<Coord, MappingChunckInfo> information = site.siteInfo;
        DungeonSegment segment = DungeonGenerator.Instance.dungeon.segments[site.generation];
        

        int[,] mapFlags = new int[site.chunkWidth, site.chunkHeight];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(start);
        mapFlags[start.tileX, start.tileY] = 1;

        MappingChunckInfo kvp;
        if (information.TryGetValue(start, out kvp))
        {
            lock (FrontDoor)
            {
                AddToQueue(kvp, site.order);
            }
        }

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y, site.chunkWidth, site.chunkHeight) && (y == tile.tileY || x == tile.tileX))
                    {
                        if(mapFlags[x,y] == 0)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));

                            MappingChunckInfo mci;
                            if (information.TryGetValue(new Coord(x, y), out mci))
                            {
                                lock (FrontDoor)
                                {
                                    AddToQueue(mci, site.order);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsInMapRange(int x, int y, int mapWidth, int mapHeight)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    void AddToQueue(MappingChunckInfo kvp, int order)
    {
        if (kvp.isMarged)
        {
            if (kvp.floors.Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                new Info(kvp.floors, kvp.position, order, 0, -1)));
            }
        }
        else
        {
            if (kvp.walls.Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                new Info(kvp.walls, kvp.position, order, 0, -1)));
            }

            if (kvp.floors.Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                new Info(kvp.floors, kvp.position, order, 1, -1)));
            }

            if (kvp.structures.Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                new Info(kvp.structures, kvp.position, order, 1, -1)));
            }

            if (kvp.structuresFloors.Count > 0)
            {
                mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                new Info(kvp.structuresFloors, kvp.position, order, 2, -1)));
            }

            for (int i = 0; i < kvp.wallsDeco.Count; i++)
            {
                if (kvp.wallsDeco[i].matrices.Count > 0)
                {
                    if (kvp.wallsDeco[i].isStackable)
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                        new Info(kvp.wallsDeco[i].matrices, kvp.position, order, 3, i)));
                    }
                    else
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(InstantiateNoStackableDecoration,
                        new Info(kvp.wallsDeco[i].matrices, kvp.position, order, 1, i)));
                    }
                }
            }

            for (int i = 0; i < kvp.floorDeco.Count; i++)
            {
                if (kvp.floorDeco[i].matrices.Count > 0)
                {
                    if (kvp.floorDeco[i].isStackable)
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                        new Info(kvp.floorDeco[i].matrices, kvp.position, order, 4, i)));
                    }
                    else
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(InstantiateNoStackableDecoration,
                        new Info(kvp.floorDeco[i].matrices, kvp.position, order, 2, i)));
                    }
                }
            }

            for (int i = 0; i < kvp.cornerDeco.Count; i++)
            {
                if (kvp.cornerDeco[i].matrices.Count > 0)
                {
                    if (kvp.cornerDeco[i].isStackable)
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(CostructChunck,
                        new Info(kvp.cornerDeco[i].matrices, kvp.position, order, 5, i)));
                    }
                    else
                    {
                        mainThreadQueue.Enqueue(new ThreadInfo<Info>(InstantiateNoStackableDecoration,
                        new Info(kvp.cornerDeco[i].matrices, kvp.position, order, 3, i)));
                    }
                }
            }
        }
    }

    void InstantiateNoStackableDecoration(Info info)
    {
        DungeonSite site = DungeonGenerator.Instance.sites[info.order];
        DungeonSegment segment = DungeonGenerator.Instance.dungeon.segments[site.generation];

        foreach (Matrix4x4 matrix in info.matrices)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;

            position += info.position;

            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            if (info.id == 1)
            {
                GameObject go = Instantiate(segment.wallDeco.decorations[info.idDeco].objeto, position, Quaternion.LookRotation(forward, upwards));
                go.transform.SetParent(site.mainObject.transform, false);
                go.isStatic = true;
            }
            else if (info.id == 2)
            {
                GameObject go = Instantiate(segment.floorDeco.decorations[info.idDeco].objeto, position, Quaternion.LookRotation(forward, upwards));
                go.transform.SetParent(site.mainObject.transform, false);
                go.isStatic = true;
            }
            else if (info.id == 3)
            {
                GameObject go = Instantiate(segment.cornerDeco.decorations[info.idDeco].objeto, position, Quaternion.LookRotation(forward, upwards));
                go.transform.SetParent(site.mainObject.transform, false);
                go.isStatic = true;
            }
        }
    }

    void CostructChunck(Info info)
    {
        DungeonSite site = DungeonGenerator.Instance.sites[info.order];
        DungeonSegment segment = DungeonGenerator.Instance.dungeon.segments[site.generation];

        GameObject go = new GameObject("GO", typeof(MeshRenderer), typeof(MeshFilter));
        go.transform.position = info.position;

        if (info.data.Count > 0)             //Is a wall, or a structure
        {
            if (info.id == 0)
            {
                go.name = "Chunck";
                CreateComplexMesh(info.data, ref segment.walls, ref go);
                if (DungeonGenerator.Instance.dungeon.applyMeshToWalls)
                {
                    go.AddComponent<MeshCollider>();
                    go.layer = DungeonGenerator.Instance.dungeon.wallsLayer;
                }
            }
            else if (info.id == 1)
            {
                go.name = "Structure";
                CreateComplexMesh(info.data, ref segment.structures, ref go);
                go.transform.position += new Vector3(0, segment.structuresHeight, 0);
                if (segment.applyMeshToStructures)
                {
                    go.AddComponent<MeshCollider>();
                    go.layer = segment.structuresLayer;
                }
            }
        }
        else if(info.matrices.Count > 0)    //Is a floor, or a structure floor, or a margin
        {
            if (info.id < 3)
            {
                if (info.id == 0)
                {
                    go.name = "Margen";
                    CreateSimpleMesh(info.matrices, segment.walls.meshes[0], segment.walls.material, ref go);
                }
                else if (info.id == 1)
                {
                    go.name = "Floor";
                    CreateSimpleMesh(info.matrices, segment.walls.ground, segment.walls.groundMaterial, ref go);
                    go.AddComponent<MeshCollider>();
                    go.layer = DungeonGenerator.Instance.dungeon.walkableLayer;
                }
                else if (info.id == 2)
                {
                    go.name = "StructureFloor";
                    CreateSimpleMesh(info.matrices, segment.structures.ground, segment.structures.groundMaterial, ref go);
                    go.transform.position += new Vector3(0, segment.structuresGroundHeight, 0);
                    if (segment.applyMeshToStructuresGround)
                    {
                        go.AddComponent<MeshCollider>();
                        go.layer = segment.structuresGroundLayer;
                    }
                }
            }
            else
            {
                if (info.id == 3)
                {
                    go.name = "WallsDeco";
                    CreateSimpleMesh(info.matrices, segment.wallDeco.decorations[info.idDeco].objeto, segment.wallDeco.decorations[info.idDeco].material, ref go);
                }
                if (info.id == 4)
                {
                    go.name = "FloorsDeco";
                    CreateSimpleMesh(info.matrices, segment.floorDeco.decorations[info.idDeco].objeto, segment.floorDeco.decorations[info.idDeco].material, ref go);
                }
                if (info.id == 5)
                {
                    go.name = "CornersDeco";
                    CreateSimpleMesh(info.matrices, segment.cornerDeco.decorations[info.idDeco].objeto, segment.cornerDeco.decorations[info.idDeco].material, ref go);
                }
            }
        }

        go.transform.SetParent(site.mainObject.transform, false);
        go.isStatic = true;
    }

    void CreateComplexMesh(List<BlockData> data, ref DungeonBrush dB, ref GameObject gO)    
    {
        Mesh finalMesh = new Mesh();

        int max = data.Count;
        CombineInstance[] combines = new CombineInstance[max];

        for (int i = 0; i < max; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = dB.meshes[data[i].order].GetComponent<MeshFilter>().sharedMesh;
            combines[i].transform = data[i].matrix;
        }

        finalMesh.CombineMeshes(combines);

        gO.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        gO.GetComponent<MeshRenderer>().sharedMaterial = dB.material;
    }

    void CreateSimpleMesh(List<Matrix4x4> matrices, GameObject gameObject, Material material, ref GameObject gO)  
    {
        Mesh finalMesh = new Mesh();

        int max = matrices.Count;
        CombineInstance[] combines = new CombineInstance[max];

        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < max; i++)
        {
            combines[i].subMeshIndex = 0;
            combines[i].mesh = mesh;
            combines[i].transform = matrices[i];
        }

        finalMesh.CombineMeshes(combines);

        gO.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        gO.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
}

public struct Coord
{
    public int tileX;
    public int tileY;

    public Coord(int x, int y)
    {
        tileX = x;
        tileY = y;
    }
}

public struct Info
{
    public List<BlockData> data;
    public List<Matrix4x4> matrices;

    public Vector3 position;
    public int order;           

    public int id;
    public int idDeco;

    public Info(List<Matrix4x4> matrices, Vector3 position, int order, int id, int idDeco)
    {
        this.matrices = matrices;
        this.position = position;
        this.order = order;
        this.id = id;
        this.idDeco = idDeco;

        data = new List<BlockData>();
    }

    public Info(List<BlockData> data, Vector3 position, int order, int id, int idDeco)
    {
        this.data = data;
        this.position = position;
        this.order = order;
        this.id = id;
        this.idDeco = idDeco;

        matrices = new List<Matrix4x4>();
    }
}

struct ThreadInfo<T>
{
    public readonly Action<T> callback;
    public readonly T parameter;

    public ThreadInfo(Action<T> callback, T parameter)
    {
        this.parameter = parameter;
        this.callback = callback;
    }
}

