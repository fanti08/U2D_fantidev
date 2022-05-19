using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections;

[RequireComponent(typeof(MeshBuilder))]
[RequireComponent(typeof(MeshGenerator))]
public class DungeonGenerator : MonoBehaviour
{
    [HideInInspector]
    public bool useRandomSeed = true;
    [HideInInspector]
    public bool generateAtStart = true;
    [HideInInspector]
    public int roomsToLoadAtStart = 2;
    [HideInInspector]
    public int roomsLoadOffset = 1;
    [HideInInspector]
    public bool destroyFarBackRooms = true;
    [HideInInspector]
    public int roomDestructionOffset = 2;
    [HideInInspector]
    public bool showDebugOptions = false;

    [HideInInspector]
    public bool canGenerateSites = false;
    [HideInInspector]
    public int siteToCharge;
    [HideInInspector]
    public Dungeon dungeon;
    [HideInInspector]
    public int seed;
    [HideInInspector]
    public bool usePrefabLoader = false;
    [HideInInspector]
    public GameObject loader;
    [HideInInspector]
    public int loaderLayer;

    public List<int[,]> maps;
    public List<DungeonSite> sites;

    private int dungeonSegments;
    [HideInInspector]
    public int sitesMax;

    private float siteLoadingWait = 0.1f;
    private bool theDungeonExist = false;
    private bool loaderExist = false;
    private bool isTheGameInitialized = false;
    private bool areGameObjectsCreated = false;
    private int dungeonType;
    private Vector2 offset;

    private MeshGenerator meshGenerator;
    private MeshBuilder meshBuilder;
    public Thread inicializer;
    public System.Random rng;

    #region Singleton

    public static DungeonGenerator _instance;
    public static DungeonGenerator Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    void Start()
    {
        //DungeonGenerator Inizialization
        if (useRandomSeed)
            seed = UnityEngine.Random.Range(-1000000, 1000000);
        rng = new System.Random(seed);
        meshGenerator = gameObject.GetComponent<MeshGenerator>();
        meshBuilder = gameObject.GetComponent<MeshBuilder>();

        maps = new List<int[,]>();
        sites = new List<DungeonSite>();

        isTheGameInitialized = false;

        //DungeonLoader Inizialization
        if (!usePrefabLoader)
        {
            loader = new GameObject("Loader", typeof(BoxCollider), typeof(DungeonLoader));
            loader.GetComponent<BoxCollider>().center = new Vector3(0, 0.5f, 0);
            loader.GetComponent<BoxCollider>().size = new Vector3(1, 1, 0);
            loader.GetComponent<BoxCollider>().isTrigger = true;
            loader.layer = loaderLayer;
            loader.SetActive(false);
            loaderExist = true;
        } else {
            loaderExist = (loader) ? true : false;
        }

        //Dungeon Inizialization
        if (dungeon)
        {
            theDungeonExist = true;
        }

        //DungeonSite Inizialization
        sitesMax = 0;
        dungeonSegments = dungeon.segments.Count;
        for (int i = 0; i < dungeonSegments; i++)
        {
            sitesMax += dungeon.segments[i].floors;
        }

        //DungeonSegment Inizialization
        for (int i = 0; i < dungeonSegments; i++)
        {
            SegmentInizializer(i);
        }

        //DungeonSites Info Inizialization
        inicializer = new Thread(GenerateAllDungeonInformation);
        inicializer.Start();

        //DungeonStart
        if (generateAtStart)
            DungeonStartTrigger();
    }

    public void DungeonStartTrigger()
    {
        if(theDungeonExist && loaderExist)
        {
            if (!inicializer.IsAlive)
            {
                if (!isTheGameInitialized)
                {
                    StartCoroutine(GenerateDungeonStart()); 
                    isTheGameInitialized = true;
                }else{
                    Debug.Log("The Dungeon is Already Initialized");
                }
            }else{
                Debug.Log("The Dungeon is Loading");
                while (inicializer.IsAlive){
                    //Debug.Log("Loading");
                }
                if (!isTheGameInitialized)
                {
                    StartCoroutine(GenerateDungeonStart()); 
                    isTheGameInitialized = true;
                }
            }
        }
    }

    public void DebugGenerateSite()
    {
        if (canGenerateSites && theDungeonExist && loaderExist)
        {
            if (!areGameObjectsCreated)
            {
                CreateAllGameObjects();
                areGameObjectsCreated = true;
            }
            ChargeSite(siteToCharge, true, true);
        }
    }

    IEnumerator GenerateDungeonStart()  
    {
        CreateAllGameObjects();
        areGameObjectsCreated = true;

        for (int i = 0; i < roomsToLoadAtStart; i++)
        {
            ChargeSite(i, true, true);
            yield return new WaitForSeconds(siteLoadingWait);
        }
    }

    public void DestroySite(int n)
    {
        if (n >= 0 && n < sitesMax)
        {
            if (sites[n].isGenerated)
            {
                foreach (Transform child in sites[n].mainObject.transform)
                {
                    if (child.GetComponent<DungeonLoader>() == null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            sites[n].isGenerated = false;
        }
    }

    public void ChargeSite(int n, bool visibility, bool isEntrance)
    {
        if(n >= 0 && n < sitesMax)
        {
            if (!sites[n].isGenerated)
            {
                sites[n].mainObject.SetActive(true);
                GenerateSiteMesh(n, isEntrance);
                sites[n].isGenerated = true;
            }
            else
            {
                sites[n].mainObject.SetActive(visibility);
            }
        }
    }

    void GenerateSiteMesh(int n, bool isEntrance)
    {
        ThreadStart thS = delegate
        {
            int startX = 0;
            int startY = 0;

            if (isEntrance)     
            {
                startX = (int)sites[n].acInfo.entranceEnd.x / sites[n].chunkSize;
                startY = (int)sites[n].acInfo.entranceEnd.y / sites[n].chunkSize;
            }else{
                startX = (int)sites[n].acInfo.exitStart.x / sites[n].chunkSize;
                startY = (int)sites[n].acInfo.exitStart.y / sites[n].chunkSize;
            }

            if (sites[n].isMarged)
            {
                startX++;
                startY++;
            }

            meshBuilder.GenerateChunk(sites[n], new Coord(startX, startY));
        };
        new Thread(thS).Start();
    }

    void GenerateAllDungeonInformation()
    {
        int sitesCount = 0;
        for (int i = 0; i < dungeonSegments; i++)
        {
            DungeonTypeDefiner(i);

            for (int j = 0; j < dungeon.segments[i].floors; j++)
            {
                sites.Add(new DungeonSite());
                DefineMapAccess(sitesCount);
                GenerateMap(dungeonType, i, sitesCount);

                sitesCount++;
            }
        }

        DefineSitesPositions();

        int count = 0;
        for (int i = 0; i < dungeonSegments; i++)
        {
            if (dungeon.segments[i].useMargin)
            {
                for (int j = 0; j < dungeon.segments[i].floors; j++)
                {
                    DungeonSite next = (count < sitesMax - 1) ? sites[count + 1] : new DungeonSite();
                    DungeonSite prev = (count > 0) ? sites[count - 1] : new DungeonSite();
                    maps[count] = BackgroundGenerator.ApplyMargenMap(maps[count], sites[count], next, prev);
                    sites[count].chunkWidth += 2;
                    sites[count].chunkHeight += 2;
                    sites[count].isMarged = true;
                    count++;
                }
            }
            else
            {
                count += dungeon.segments[i].floors;
            }
        }

        for (int i = 0; i < sitesMax; i++)
        {
            sites[i].siteInfo = meshGenerator.GenerateSiteInformation(sites[i], maps[i]);
        }
    }

    private void CreateAllGameObjects()
    {
        int count = 0;
        for (int i = 0; i < dungeonSegments; i++)
        {
            for(int j = 0; j < dungeon.segments[i].floors; j++)
            {
                sites[count].mainObject = new GameObject("Site " + count);
                sites[count].mainObject.transform.position = sites[count].position;
                sites[count].mainObject.SetActive(false);

                if (dungeon.segments[i].useExtra)
                {
                    if (dungeon.segments[i].extra)
                    {
                        GameObject extra = Instantiate(dungeon.segments[i].extra);
                        extra.transform.position = dungeon.segments[i].extraPos;
                        extra.transform.SetParent(sites[count].mainObject.transform, false);
                    }
                }

                count++;
            }
        }

        if (dungeon.useProceduralAsigment)
        {
            for (int i = 1; i < sitesMax - 1; i++)
            {
                int ratio;
                Vector3 size = Vector3.one;

                if (sites[i].acInfo.exitDirection < sites[i].acInfo.entranceDirection){
                    ratio = sites[i].acInfo.entranceDirection + sites[i].acInfo.exitDirection;
                }else{
                    ratio = (sites[i].acInfo.entranceDirection + sites[i].acInfo.exitDirection) - 4;
                }

                float angle = (ratio) * 45;
                
                if(sites[i].chunkWidth == sites[i].chunkHeight)   //It´s a square
                {
                    if (ratio % 2 == 0)  //It´s even
                    {
                        size.x = (sites[i].isMarged) ? (sites[i].chunkWidth - 2) * sites[i].chunkSize : sites[i].chunkWidth * sites[i].chunkSize;
                    }else{
                        size.x = (sites[i].isMarged) ? (sites[i].chunkWidth - 2) * sites[i].chunkSize * Mathf.Sqrt(2) : sites[i].chunkWidth * sites[i].chunkSize * Mathf.Sqrt(2);
                    }
                }
                else    //It´s a rectangle
                {
                    if (ratio % 2 == 0)
                    {
                        if ((ratio / 2) % 2 == 0)   //Horizontal
                        {
                            size.x = (sites[i].isMarged) ? (sites[i].chunkWidth - 2) * sites[i].chunkSize : sites[i].chunkWidth * sites[i].chunkSize;
                        }else{                      //Vertical
                            size.x = (sites[i].isMarged) ? (sites[i].chunkHeight - 2) * sites[i].chunkSize : sites[i].chunkHeight * sites[i].chunkSize;
                        }
                    }else{
                        float ang = (angle) * Mathf.Deg2Rad;

                        float x = (sites[i].isMarged) ? Mathf.Round(Mathf.Cos(ang)) * (sites[i].chunkWidth - 2) : Mathf.Round(Mathf.Cos(ang)) * sites[i].chunkWidth;
                        float y = (sites[i].isMarged) ? Mathf.Round(Mathf.Sin(ang)) * (sites[i].chunkHeight - 2) : Mathf.Round(Mathf.Sin(ang)) * sites[i].chunkHeight;

                        angle = Mathf.Atan(y / x) * Mathf.Rad2Deg;
                        if (x < 0)
                            angle += 180;

                        size.x = Mathf.Sqrt(Mathf.Pow(sites[i].chunkWidth, 2) + Mathf.Pow(sites[i].chunkHeight, 2)) * sites[i].chunkSize;
                    }
                }

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                GameObject load = Instantiate(loader, new Vector3(-0.5f, 0, -0.5f), rotation);
                load.transform.localScale = size;
                load.transform.SetParent(sites[i].mainObject.transform, false);
                load.SetActive(true);

                load.GetComponent<DungeonLoader>().site = i;
            }
        }
        else
        {
            for (int i = 1; i < sitesMax - 1; i++)
            {
                if (dungeon.segments[sites[i].generation].useLoader)
                {
                    float height = (sites[i].isMarged) ? (sites[i].chunkHeight - 2) * sites[i].chunkSize : sites[i].chunkHeight * sites[i].chunkSize;
                    float width = (sites[i].isMarged) ? (sites[i].chunkWidth - 2) * sites[i].chunkSize : sites[i].chunkWidth * sites[i].chunkSize;

                    Vector3 size = Vector3.one;
                    float angle = dungeon.segments[sites[i].generation].loaderAngle;

                    float absCosAngulo = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
                    float absSinAngulo = Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));
                    if (width / 2 * absSinAngulo <= height / 2 * absCosAngulo)
                    {
                        size.x = width / absCosAngulo;
                    }
                    else
                    {
                        size.x = height / absSinAngulo;
                    }

                    Quaternion rotation = Quaternion.Euler(0, angle + 270, 0);

                    GameObject load = Instantiate(loader, new Vector3(-0.5f, 0, -0.5f) + dungeon.segments[sites[i].generation].deltaLoaderPos, rotation);
                    load.transform.localScale = size;
                    load.transform.SetParent(sites[i].mainObject.transform, false);
                    load.SetActive(true);

                    load.GetComponent<DungeonLoader>().site = i;
                }
            }
        }

    }

    private void DefineSitesPositions()
    {
        for (int i = 1; i < sitesMax; i++)
        {
            if (dungeon.useProceduralAsigment)
            {
                int n = sites[i - 1].acInfo.exitDirection;

                float dxPre = (sites[i - 1].chunkWidth * sites[i - 1].chunkSize) / 2;
                float dyPre = (sites[i - 1].chunkHeight * sites[i - 1].chunkSize) / 2;

                float dxNow = (sites[i].chunkWidth * sites[i].chunkSize) / 2;
                float dyNow = (sites[i].chunkHeight * sites[i].chunkSize) / 2;

                float dx = (dxNow - dxPre) + (sites[i - 1].acInfo.exitStart.x - sites[i].acInfo.entranceEnd.x) + sites[i - 1].position.x;
                float dy = (dyNow - dyPre) + (sites[i - 1].acInfo.exitStart.y - sites[i].acInfo.entranceEnd.y) + sites[i - 1].position.z;

                float cx = dxPre + dxNow;
                float cy = dyPre + dyNow;

                if (n == 1)         //Up
                {
                    sites[i].position.x = dx;
                    sites[i].position.z = cy + sites[i - 1].position.z;
                }
                else if (n == 2)    //Right
                {
                    sites[i].position.x = cx + sites[i - 1].position.x;
                    sites[i].position.z = dy;
                }
                else if (n == 3)    //Down
                {
                    sites[i].position.x = dx;
                    sites[i].position.z = -cy + sites[i - 1].position.z;
                }
                else                //Left
                {
                    sites[i].position.x = -cx + sites[i - 1].position.x;
                    sites[i].position.z = dy;
                }
            }
            else
            {
                sites[i].position = sites[i - 1].position + dungeon.segments[sites[i].generation].artificialDisplacement;
            }
        }
    }

    private void DungeonTypeDefiner(int segmentId)
    {
        DungeonShape dSh = dungeon.segments[segmentId].shape;

        if (dSh.generation == SegmentGeneration.Natural)
        {
            dungeonType = 1;
        }
        else
        {
            if (!dSh.isAnHabitacion)
            {
                dungeonType = 2;
            }
            else
            {
                dungeonType = 3;
            }
        }
    }

    private void SegmentInizializer(int segmentId)    
    {
        DungeonShape dSh = dungeon.segments[segmentId].shape;

        if (dSh.generation == SegmentGeneration.Natural)
        {
            if (dSh.backgroundMap == null)
            {
                dSh.Initialize();
            }
        }else{
            if (dSh.artificialCanvas == null)
            {
                dSh.Initialize();
            }
        }
    }

    public void DefineMapAccess(int sitesCount)
    {
        sites[sitesCount].acInfo = new AccessInfo();
        AccessInfo acInfo = sites[sitesCount].acInfo;

        if (dungeon.useProceduralAsigment)
        {
            acInfo.normalAccessGen = true;
        }else{
            acInfo.normalAccessGen = false;
        }

        if (sitesCount < sitesMax - 1)
        {
            if (sitesCount == 0)
            {
                acInfo.hasEntrance = false;
                acInfo.hasExit = true;

                acInfo.entranceDirection = 0;
                acInfo.exitDirection = rng.Next(1, 5);
            }
            else
            {
                acInfo.hasEntrance = true;
                acInfo.hasExit = true;

                acInfo.entranceDirection = sites[sitesCount - 1].acInfo.exitDirection + 2;
                if (acInfo.entranceDirection > 4)
                    acInfo.entranceDirection -= 4;

                int extra = 0;
                ///*    Extra code to avoid room overlapping
                AccessInfo prevAc = sites[sitesCount - 1].acInfo;
                
                if (prevAc.exitDirection == 1 || prevAc.exitDirection == 3)
                {
                    if (prevAc.exitStart.x > (sites[sitesCount - 1].chunkWidth * sites[sitesCount - 1].chunkSize) / 2)
                    {
                        extra = 2;
                    }else{
                        extra = 4;
                    }
                }
                else
                {
                    if (prevAc.exitStart.y > (sites[sitesCount - 1].chunkHeight * sites[sitesCount - 1].chunkSize) / 2)
                    {
                        extra = 1;
                    }else{
                        extra = 3;
                    }
                }
                //*/

                do
                {
                    acInfo.exitDirection = rng.Next(1, 5);
                }
                while (acInfo.exitDirection == acInfo.entranceDirection || acInfo.exitDirection == extra);
            }
        }
        else
        {
            if(sitesMax == 1)
            {
                acInfo.hasEntrance = false;
                acInfo.hasExit = true;

                acInfo.entranceDirection = 0;
                acInfo.exitDirection = rng.Next(1, 5);
            }
            else
            {
                acInfo.hasEntrance = true;
                acInfo.hasExit = false;

                acInfo.exitDirection = 0;
                acInfo.entranceDirection = sites[sitesCount - 1].acInfo.exitDirection + 2;
                if (acInfo.entranceDirection > 4)
                    acInfo.entranceDirection -= 4;
            }
        }
    }

    public void GenerateMap(int dungeonType, int segmentId, int sitesCount)
    {
        int[,] noiseProcesado;
        DungeonShape dSh = dungeon.segments[segmentId].shape;
        AccessInfo acInfo = sites[sitesCount].acInfo;

        if (dungeonType == 1)       //Natural Generation
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(seed, offset, dSh.noiseInfo);
            noiseMap = BackgroundGenerator.ApplyBackground(noiseMap, dSh.backgroundMap);
            noiseProcesado = NoiseProcessor.ProcessNoise(noiseMap, dSh.noiseProcessInfo, ref acInfo, true, rng);
        }
        else if (dungeonType == 2)  //Artificial Generation
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(seed, offset, dSh.noiseInfo);
            noiseProcesado = NoiseProcessor.ProcessNoise(noiseMap, dSh.noiseProcessInfo, ref acInfo, false, rng);

            noiseProcesado = BackgroundGenerator.ApplyArtificialCanvas(dSh.artificialCanvas, noiseProcesado, false);
            noiseProcesado = NoiseProcessor.ProcessIntMap(noiseProcesado, dSh.noiseProcessInfo, ref acInfo, false, rng);
            noiseProcesado = BackgroundGenerator.ApplyArtificialCanvas(dSh.artificialCanvas, noiseProcesado, false);
        }
        else    //Artificial Generation (Room Only)
        {
            noiseProcesado = NoiseProcessor.ProcessIntMap(dSh.artificialCanvas, dSh.noiseProcessInfo, ref acInfo, true, rng);
        }

        AumentaOffset(acInfo.exitDirection, dSh.chunkWidth, dSh.chunkHeight, dSh.chunkSize);

        maps.Add(noiseProcesado);
        sites[sitesCount].chunkSize = dSh.chunkSize;
        sites[sitesCount].chunkWidth = dSh.chunkWidth;
        sites[sitesCount].chunkHeight = dSh.chunkHeight;

        sites[sitesCount].useMargin = dungeon.segments[segmentId].useMargin;
        sites[sitesCount].margin = dungeon.segments[segmentId].margin;

        sites[sitesCount].generation = segmentId;
        sites[sitesCount].order = sitesCount;

        sites[sitesCount].isGenerated = false;
    }

    public void AumentaOffset(int n, int chunksWidth, int chunksHeight, int chunkSize)
    {
        if (n == 1)
        {
            offset.x += 0;
            offset.y += chunksHeight * chunkSize;
        }
        else if (n == 2)
        {
            offset.x += chunksWidth * chunkSize;
            offset.y += 0;
        }
        else if (n == 3)
        {
            offset.x += 0;
            offset.y -= chunksHeight * chunkSize;
        }
        else
        {
            offset.x -= chunksWidth * chunkSize;
            offset.y += 0;

        }
    }
}


