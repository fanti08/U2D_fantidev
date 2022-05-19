using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DungeonDecoration", menuName = "Dungeon Component/DungeonDecoration", order = 3)]
[System.Serializable]
public class DungeonDecoration : ScriptableObject
{
    public int maxDecoSize  { get; private set; } = 8;

    public float probability;
    public List<Decoration> decorations;

    public DungeonDecoration()
    {
        probability = 10;
        decorations = new List<Decoration>();
    }
}
