﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonLoader : MonoBehaviour
{
    public int site;

    private void OnTriggerExit(Collider collider)
    {
        Charge(collider.transform.position);
    }

    public void Charge(Vector3 colliderPos)
    {
        Vector3 playerDirection = colliderPos - transform.position;
        playerDirection.Normalize();

        float razon = Vector3.Dot(transform.forward, playerDirection);
        int offset = DungeonGenerator.Instance.roomsLoadOffset;

        if (razon < 0)
        {
            DungeonGenerator.Instance.ChargeSite(site + offset, true, true);
            if (DungeonGenerator.Instance.dungeon.hidePredecesors)
            {
                DungeonGenerator.Instance.ChargeSite(site - offset, false, true);
            }

            if (DungeonGenerator.Instance.destroyFarBackRooms)
            {
                DungeonGenerator.Instance.DestroySite(site - offset - DungeonGenerator.Instance.roomDestructionOffset);
            }
        }
        else
        {
            DungeonGenerator.Instance.ChargeSite(site - offset, true, false);
            if (DungeonGenerator.Instance.dungeon.hidePredecesors)
            {
                DungeonGenerator.Instance.ChargeSite(site + offset, false, false);
            }

            if (DungeonGenerator.Instance.destroyFarBackRooms)
            {
                DungeonGenerator.Instance.DestroySite(site + offset + DungeonGenerator.Instance.roomDestructionOffset);
            }
        }
    }
}
