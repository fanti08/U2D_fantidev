using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseProcessor
{
    static int[,] map;
    static int[,] roomMap;
    static int mapWidth;
    static int mapHeight;

    static Coord buffer = new Coord(0, 0);
    static int bufferInt;
    static int amount = 0;
    static bool execute;

    static int divider = 3;
    static int deltaX = 0;
    static int deltaY = 0;
    static List<Vector2> directions = new List<Vector2> { Vector2.up, new Vector2(1,1), Vector2.right, new Vector2(1,-1),
                                                          Vector2.down, new Vector2(-1,-1), Vector2.left, new Vector2(-1,1)};
    static int[] verification = { 1, 1, 1, 1, 1, 1, 1, 1 };

    public static int[,] ProcessNoise(float[,] noise, NoiseProcessInfo nPI, ref AccessInfo acInfo, bool useAdvanceProcessing, System.Random rng)
    {
        int width = noise.GetLength(0);
        int height = noise.GetLength(1);

        int[,] noiseEvaluated = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noise[x, y] <= nPI.minFloorValue)
                {
                    noiseEvaluated[x, y] = 1;        //Number given to wall tiles (Set as Black in the Visualizer)
                }
                else
                {
                    if (noise[x, y] <= nPI.minStructureValue)
                        noiseEvaluated[x, y] = 0;    //Number given to floor tiles, specifically, path tiles (Set as Grey in the Visualizer) 
                    else
                        noiseEvaluated[x, y] = 2;    //Number given to structure tiles (Set as Blue in the Visualizer)  
                }
            }
        }

        if (useAdvanceProcessing)
        {
            mapWidth = width;
            mapHeight = height;
            map = noiseEvaluated;
            roomMap = noiseEvaluated;

            List<Room> rooms = ProcessRooms(nPI.minTilesToGenWalls, nPI.minTilesToGenRooms, nPI.minTilesToGenStructures, nPI.failedRoomValue);
            if(rooms.Count == 0)
            {
                Debug.LogWarning("It is impossible to make a proper room with the given specifications");
                return new int[width, height];
            }

            List<Coord> paths = CreatePaths(rooms);
            JoinRooms(rooms, paths, nPI.maxHallwayRadius, acInfo, rng);

            return map;
        }
        else
        {
            return noiseEvaluated;
        }
    }

    public static int[,] ProcessIntMap(int[,] noise, NoiseProcessInfo nPI, ref AccessInfo acInfo, bool isAnHabitacion, System.Random rng)
    {
        int width = noise.GetLength(0);
        int height = noise.GetLength(1);

        mapWidth = width;
        mapHeight = height;
        map = new int[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                map[i, j] = noise[i, j];
            }
        }
        roomMap = map;

        if (!isAnHabitacion)
        {
            List<Room> rooms = ProcessRooms(nPI.minTilesToGenWalls, nPI.minTilesToGenRooms, nPI.minTilesToGenStructures, nPI.failedRoomValue);
            if (rooms.Count == 0)
            {
                Debug.LogWarning("It is impossible to make a proper room with the given specifications");
                return new int[width, height];
            }

            List<Coord> paths = CreatePaths(rooms);
            JoinRooms(rooms, paths, nPI.maxHallwayRadius, acInfo, rng);
        }
        else
        {
            CreateAccess(new List<Coord>(), new List<Coord>(), 1, acInfo, rng);
        }

        return map;
    }

    public static int[,] GenerateFromTexture(Texture2D canvas, ref Vector3 position)
    {
        //int chunkSize = 16;
        int xO, yO;
        Color pixelColor;

        int roomWidth;  
        int roomHeight;   
        int mapWidth;  
        int mapHeight;

        roomWidth  = canvas.width;
        roomHeight = canvas.height;

        mapWidth  = roomWidth + 2;   //Map legal value
        mapHeight = roomHeight + 2;
        
        int[,] mapInt = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapInt[x, y] = 1;
            }
        }

        xO = 1;
        yO = 1;

        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                pixelColor = canvas.GetPixel(x, y);
                if(pixelColor.a == 0)   
                {
                    mapInt[x + xO, y + yO] = 3;    //Empty tiles are only set by an alpha of zero
                }
                else if (pixelColor == Color.green)
                {
                    mapInt[x + xO, y + yO] = 4;    //Tile to overwrite by normal map generation
                }
                else if (pixelColor == Color.black)
                {
                    mapInt[x + xO, y + yO] = 1;    //Wall
                }
                else if (pixelColor == Color.white)
                {
                    mapInt[x + xO, y + yO] = 0;    //Floor
                }
                else if (pixelColor == Color.blue)
                {
                    mapInt[x + xO, y + yO] = 2;    //Structures
                }
                else if (pixelColor == Color.red)
                {
                    mapInt[x + xO, y + yO] = -1;       
                    position = new Vector3(x + xO, 0, y + yO);  //Spawn
                }
            }
        }

        return mapInt;
    }

    static void DrawCircle(Coord c, int r, int filling)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int realX = c.tileX + x;
                    int realY = c.tileY + y;
                    if (IsInMapRange(realX, realY))
                    {
                        map[realX, realY] = filling;
                    }
                }
            }
        }
    }

    static List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = (int)Mathf.Sign(dx);
        int gradientStep = (int)Mathf.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = (int)Mathf.Sign(dy);
            gradientStep = (int)Mathf.Sign(dx);
        }

        int gradientAcumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAcumulation += shortest;
            if (gradientAcumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }

                gradientAcumulation -= longest;
            }
        }

        return line;
    }
    
    static void CreateAccess(List<Coord> start, List<Coord> end, int maxHallwayRadius, AccessInfo aI, System.Random rng)
    {
        int count = 0;
        Coord entranceStart; Coord entranceEnd;      
        Coord exitStart;     Coord exitEnd;      

        Coord maskStart = new Coord();
        Coord maskEnd   = new Coord();
        bool needVerification = false;

        if (aI.hasEntrance && aI.normalAccessGen)
        {
            Entries(aI.entranceDirection, out entranceStart, out entranceEnd, rng, needVerification, maskStart, maskEnd);
            start.Add(entranceStart); end.Add(entranceEnd);
            aI.entranceEnd = new Vector2(entranceStart.tileX, entranceStart.tileY);
            count++;

            //Code to prevent overlaping of entrance and exit in the same room 
            deltaX = mapWidth / divider;
            deltaY = mapHeight  / divider;
            int realCoordX = entranceStart.tileX / deltaX;
            int realCoordY = entranceStart.tileY / deltaY;

            int maskStartX;     int maskStartY;
            int maskEndX;       int maskEndY;

            bool swapX = false; bool swapY = false;
            int bufferXA = deltaX;
            int bufferXB = deltaX * (divider - 1);
            int bufferYA = deltaY;
            int bufferYB = deltaY * (divider - 1);

            maskStartX = 0;    maskEndX = mapWidth;
            maskStartY = 0;    maskEndY = mapHeight;

            if (realCoordX > (divider - 2)){
                maskStartX = bufferXA;
            } else if (realCoordX < 1){
                maskEndX    = bufferXB;
            } else {
                swapY = true;
            }

            if(realCoordY > (divider - 2)){
                maskStartY = bufferYA;
            } else if (realCoordY < 1){
                maskEndY    = bufferYB;
            } else {
                swapX = true;
            }

            if (swapX == true) {
                if (maskStartX == bufferXA)
                    maskStartX = bufferXB;
                else {
                    if (maskEndX == bufferXB)
                        maskEndX = bufferXA;
                }
            }

            if(swapY == true) {
                if (maskStartY == bufferYA)
                    maskStartY = bufferYB;
                else {
                    if (maskEndY == bufferYB)
                        maskEndY = bufferYA;
                }
            }

            maskStart.tileX  = maskStartX;      maskStart.tileY  = maskStartY;
            maskEnd.tileX    = maskEndX;        maskEnd.tileY    = maskEndY;
            needVerification = !(swapX && swapY);
        }

        if (aI.hasExit && aI.normalAccessGen)
        {
            Entries(aI.exitDirection, out exitStart, out exitEnd, rng, needVerification, maskStart, maskEnd);
            start.Add(exitStart); end.Add(exitEnd);
            aI.exitStart = new Vector2(exitStart.tileX, exitStart.tileY);
            count++;
        }

        int size = start.Count;
        for (int j = 0; j < size; j++)
        {
            List<Coord> line = GetLine(start[j], end[j]);
            foreach (Coord c in line)
            {
                if (j < size - count)
                {
                    int radio = rng.Next(1, maxHallwayRadius + 1);
                    DrawCircle(c, radio, 0);
                }
                else
                {
                    map[c.tileX, c.tileY] = 0;     //Number for entries and outputs
                }
            }
        }
    }

    static void JoinRooms(List<Room> rooms, List<Coord> paths, int maxHallwayRadius, AccessInfo aI, System.Random rng)
    {
        List<Coord> start = new List<Coord>();
        List<Coord> end = new List<Coord>();

        foreach (Coord c in paths)
        {
            int minDistance = int.MaxValue;

            Coord bufferA = new Coord(0, 0);
            Coord bufferB = new Coord(0, 0);
            foreach (Coord coordA in rooms[c.tileX].edges)
            {
                foreach (Coord coordB in rooms[c.tileY].edges)
                {
                    int distance = (coordB.tileX - coordA.tileX) * (coordB.tileX - coordA.tileX) + (coordB.tileY - coordA.tileY) * (coordB.tileY - coordA.tileY);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bufferA.tileX = coordA.tileX;
                        bufferA.tileY = coordA.tileY;
                        bufferB.tileX = coordB.tileX;
                        bufferB.tileY = coordB.tileY;
                    }
                }
            }

            start.Add(bufferA);
            end.Add(bufferB);
        }

        CreateAccess(start, end, maxHallwayRadius, aI, rng);
    }
    
    static void Entries(int orientation, out Coord entrance, out Coord exit, System.Random rng, bool needVerification, Coord A, Coord B)
    {
        int startX = 0;  int endX = mapWidth;   int incX = 1;
        int startY = 0;  int endY = mapHeight;  int incY = 1;

        List<Coord> entries = new List<Coord>();
        List<Coord> outputs = new List<Coord>();

        entrance = new Coord(0, 0);
        exit = new Coord(0, 0);

        if (orientation == 1)
        {
            startX = 0;            endX = mapWidth;     incX = 1;
            startY = mapHeight-1;  endY = 0;            incY = -1;

            for (int y = startY; y > endY; y += incY)
            {
                for (int x = startX; x < endX; x += incX)
                {
                    if (needVerification)
                    {
                        if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                            continue;
                    }

                    if (map[x, y] < 1)
                    {
                        endY = y;
                        entries.Add(new Coord(x, y));
                        outputs.Add(new Coord(x, startY));
                    }
                }
            }
        }
        else if (orientation == 2)
        {
            startX = mapWidth-1;    endX = 0;           incX = -1;
            startY = mapHeight-1;   endY = 0;           incY = -1;

            for (int x = startX; x > endX; x += incX)
            {
                for (int y = startY; y > endY; y += incY)
                {
                    if (needVerification)
                    {
                        if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                            continue;
                    }

                    if (map[x, y] < 1)
                    {
                        endX = x;
                        entries.Add(new Coord(x, y));
                        outputs.Add(new Coord(startX, y));
                    }
                }
            }
        }
        else if (orientation == 3)
        {
            startX = 0;             endX = mapWidth;    incX = 1;
            startY = 0;             endY = mapHeight;   incY = 1;

            for (int y = startY; y < endY; y += incY)
            {
                for (int x = startX; x < endX; x += incX)
                {
                    if (needVerification)
                    {
                        if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                            continue;
                    }

                    if (map[x, y] < 1)
                    {
                        endY = y;
                        entries.Add(new Coord(x, y));
                        outputs.Add(new Coord(x, startY));
                    }
                }
            }
        }
        else
        {
            startX = 0;             endX = mapWidth;    incX = 1;
            startY = mapHeight-1;   endY = 0;           incY = -1;

            for (int x = startX; x < endX; x += incX)
            {
                for (int y = startY; y > endY; y += incY)
                {
                    if (needVerification)
                    {
                        if (x > A.tileX && x < B.tileX && y > A.tileY && y < B.tileY)
                            continue;
                    }

                    if (map[x, y] < 1)
                    {
                        endX = x;
                        entries.Add(new Coord(x, y));
                        outputs.Add(new Coord(startX, y));
                    }
                }
            }
        }
        
        int n = rng.Next(0, entries.Count);
        entrance = entries[n];
        exit = outputs[n];
    }

    static List<Coord> CreatePaths(List<Room> rooms)
    {
        int size = rooms.Count;
        int[] DontHasPath = new int[size];
        for (int i = 0; i < size; i++)
        {
            DontHasPath[i] = 1;
        }
        DontHasPath[0] = 0;

        List<Coord> paths = new List<Coord>();

        int currentRoom = 0;
        bool basicComprobation = true;

        for (int j = 0; j < size - 1; j++)
        {
            int minDistance = int.MaxValue;
            if (j == size - 2)
            {
                basicComprobation = false;

                for (int i = 0; i < size; i++)
                {
                    if (DontHasPath[i] == 1)
                    {
                        currentRoom = i;
                    }
                }
            }

            for (int i = 0; i < size; i++)
            {
                if (basicComprobation)
                {
                    if (DontHasPath[i] == 1)
                        execute = true;
                    else
                        execute = false;
                }
                else
                {
                    if (i != currentRoom)
                        execute = true;
                    else
                        execute = false;
                }

                if (execute)
                {
                    int xA = rooms[currentRoom].center.tileX;
                    int yA = rooms[currentRoom].center.tileY;
                    int xB = rooms[i].center.tileX;
                    int yB = rooms[i].center.tileY;
                    
                    int distance = (xB - xA) * (xB - xA) + (yB - yA) * (yB - yA);
                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        buffer.tileX = currentRoom;
                        buffer.tileY = i;
                        bufferInt = i;
                    }
                }
            }

            DontHasPath[bufferInt] = 0;
            paths.Add(buffer);
            currentRoom = bufferInt;
        }

        return paths;
    }

    static List<Room> ProcessRooms(int wallMinSize, int roomMinSize, int structMinSize, int failedHabValue)
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        foreach (List<Coord> region in wallRegions)
        {
            if (region.Count < wallMinSize)
            {
                foreach (Coord cor in region)
                {
                    map[cor.tileX, cor.tileY] = 0;
                }
            }
        }

        List<List<Coord>> structRegions = GetRegions(2);
        foreach (List<Coord> region in structRegions)
        {
            if (region.Count < structMinSize)
            {
                foreach (Coord cor in region)
                {
                    map[cor.tileX, cor.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRooms = new List<Room>();

        int count = -1;
        foreach (List<Coord> region in roomRegions)
        {
            if (region.Count < roomMinSize)
            {
                foreach (Coord cor in region)
                {
                    map[cor.tileX, cor.tileY] = failedHabValue;
                }
            }
            else
            {
                int size = region.Count;
                Coord center = new Coord(0, 0);

                foreach (Coord cor in region)
                {
                    center.tileX += cor.tileX;
                    center.tileY += cor.tileY;
                    if (map[cor.tileX, cor.tileY] == 0)
                        roomMap[cor.tileX, cor.tileY] = count;
                }
                center.tileX = center.tileX / size;
                center.tileY = center.tileY / size;

                int side = (int)Mathf.Sqrt(size);
                List<Coord> edges = new List<Coord>();

                for (int j = side; j > -side; j--)
                {
                    amount = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        if (verification[i] == 1)
                        {
                            int trueX = center.tileX + (int)directions[i].x * j;
                            int trueY = center.tileY + (int)directions[i].y * j;
                            if (IsInMapRange(trueX, trueY))
                            {
                                if (roomMap[trueX, trueY] == count)
                                {
                                    verification[i] = 0;
                                    edges.Add(new Coord(trueX, trueY));
                                }
                            }
                        }
                        amount += verification[i];
                    }

                    if (amount == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            verification[i] = 1;
                        }

                        break;
                    }

                }

                survivingRooms.Add(new Room(region, count, size, center, edges));
                count--;
            }
        }

        return survivingRooms;
    }
    
    static List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord cor in newRegion)
                    {
                        mapFlags[cor.tileX, cor.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }
    
    static List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[mapWidth, mapHeight];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    static bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    struct Room
    {
        public int id;
        public List<Coord> tiles;
        public int size;
        public Coord center;
        public List<Coord> edges;

        public Room(List<Coord> roomTiles, int ID, int roomSize, Coord center, List<Coord> edges)
        {
            id = ID;
            tiles = roomTiles;
            size = roomSize;
            this.center = center;
            this.edges = edges;
        }
    }

}