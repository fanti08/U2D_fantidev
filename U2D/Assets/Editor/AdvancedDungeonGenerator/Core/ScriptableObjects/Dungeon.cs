using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "Dungeon", order = -1)]
[System.Serializable]
public class Dungeon : ScriptableObject
{
    public int maxDungeonSegments   { get; private set; } = 32;
    public int maxSegmentRooms      { get; private set; } = 64;

    public int  walkableLayer;
    public bool applyMeshToWalls;
    public int  wallsLayer;

    public bool hidePredecesors = true;
    public bool useProceduralAsigment; 
    public bool useArtificialAsigment;

    public bool showGlobalSettings;
    public bool hasDecoration;
    public bool useGlobalDecoration;
    public bool useGlobalBrush;
    public bool useGlobalStructuresSpecifications;

    [Space]
    public List<DungeonSegment> segments = new List<DungeonSegment>();
}



