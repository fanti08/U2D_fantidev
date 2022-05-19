using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SecurityCheck
{
    public static bool ValidateDungeon(Dungeon dungeon)
    {
        foreach(DungeonSegment segment in dungeon.segments)
        {
            if (!segment.shape)
            {
                Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonShape)");
                return false;
            }

            if (!segment.walls)
            {
                Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonBrush Walls)");
                return false;
            }

            if (!segment.structures)
            {
                Debug.LogError("The selected Dungeon cannot be accepted due to lack of essential information (DungeonBrush Structures)");
                return false;
            }
        }

        return true;
    }

    public static bool ValidateDungeonDecoration(DungeonDecoration deco)
    {
        foreach(Decoration dd in deco.decorations)
        {
            if (!dd.objeto)
            {
                Debug.LogError("The selected DungeonDecoration cannot be accepted due to lack of essential information (Decoration Mesh)");
                return false;
            }

            if (dd.isStackable)
            {
                if (!dd.material)
                {
                    Debug.LogError("The selected DungeonDecoration cannot be accepted due to lack of essential information (Decoration Material)");
                    return false;
                }
            }
        }

        return true;
    }

    public static bool ValidateDungeonBrush(DungeonBrush brush)
    {
        foreach(GameObject gO in brush.meshes)
        {
            if (!gO)
            {
                Debug.LogError("The selected DungeonBrush cannot be accepted due to lack of essential information (Walls Meshes)");
                return false;
            }
        }

        if(!brush.material)
        {
            Debug.LogError("The selected DungeonBrush cannot be accepted due to lack of essential information (Texture Atlas)");
            return false;
        }

        if (!brush.ground)
        {
            Debug.LogError("The selected DungeonBrush cannot be accepted due to lack of essential information (Floor Mesh)");
            return false;
        }

        if(!brush.groundMaterial)
        {
            Debug.LogError("The selected DungeonBrush cannot be accepted due to lack of essential information (Floor Material)");
            return false;
        }

        return true;
    }

    public static bool ValidateDungeonShape(DungeonShape shape)
    {
        if (shape.generation == SegmentGeneration.Artificial)
        {
            if (!shape.canvasTexture)
            {
                Debug.LogError("The selected DungeonShape cannot be accepted due to lack of essential information (Room Mask Texture)");
                return false;
            }
        }

        return true;
    }
}
