using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DungeonBrush", menuName = "Dungeon Component/DungeonBrush", order = 2)]
public class DungeonBrush : ScriptableObject
{
    [Header("Walls (Fill, H, V, Corner, Round)")]
    public GameObject[] meshes = new GameObject[5];
    
    [Header("Floor")]
    public GameObject ground;
    public Material groundMaterial;

    [Header("Texture Atlas")]
    public Material material;
}
