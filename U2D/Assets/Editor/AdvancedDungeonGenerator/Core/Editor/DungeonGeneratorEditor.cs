using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    DungeonGenerator dg;

    private void OnEnable()
    {
        dg = (DungeonGenerator)target;
        EditorUtility.SetDirty(dg);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Dungeon Generation", EditorStyles.boldLabel);

        GUI.changed = false;
        dg.dungeon = (Dungeon)EditorGUILayout.ObjectField("Dungeon", dg.dungeon, typeof(Dungeon), false);
        if (GUI.changed)
        {
            if (dg.dungeon)
            {
                if (SecurityCheck.ValidateDungeon(dg.dungeon) == false)
                {
                    dg.dungeon = null;
                }
                else
                {
                    dg.sitesMax = 0;
                    for (int i = 0; i < dg.dungeon.segments.Count; i++)
                    {
                        dg.sitesMax += dg.dungeon.segments[i].floors;
                    }
                }
            }
        }
        GUI.changed = false;

        dg.usePrefabLoader = !EditorGUILayout.Toggle(new GUIContent("Use in build Loader",
            "If checked, the dungeon generator will create the standard loaders of the dungeon on its own, otherwise the user will specify the loader to use"), !dg.usePrefabLoader);

        EditorGUI.indentLevel++;

        if (!dg.usePrefabLoader) 
        { 
            dg.loaderLayer = EditorGUILayout.LayerField(new GUIContent("Loader Layer",
            "The layer that will be assigned to all Dungeon Loaders"), dg.loaderLayer);
        }
        else
        {
            GUI.changed = false;
            dg.loader = (GameObject)EditorGUILayout.ObjectField("Prefab Loader", dg.loader, typeof(GameObject), false);
            if (GUI.changed)
            {
                if (dg.loader)
                {
                    if (dg.loader.GetComponent<DungeonLoader>() == null)
                    {
                        Debug.LogError("The selected Dungeon Loader cannot be accepted if it does not have a DungeonLoaderScript component");
                        dg.loader = null;
                    }
                }
            }
            GUI.changed = false;

            EditorGUIUtility.labelWidth = 180;
            dg.roomsToLoadAtStart = EditorGUILayout.IntField(new GUIContent("Rooms to Load at the Start",
                "The number of rooms that will be loaded at the start of the dungeon creation (2 by default)"), dg.roomsToLoadAtStart);
            dg.roomsLoadOffset = EditorGUILayout.IntField(new GUIContent("Rooms Loading Offset", 
                "When the loader triggers, the room to load will be the current one plus this number (1 by default)"), dg.roomsLoadOffset);
            dg.destroyFarBackRooms = EditorGUILayout.Toggle(new GUIContent("Destroy Far Back Rooms",
                "Rooms that are too far back will be destroyed (True by default)"), dg.destroyFarBackRooms);
            dg.roomDestructionOffset = EditorGUILayout.IntField(new GUIContent("Room Destruction Offset",
                "When the loader triggers, the room to destroy will be the current one minus the Room Loading Offset and this number (2 by default)"), dg.roomDestructionOffset);
            EditorGUIUtility.labelWidth = 140;
        }

        EditorGUI.indentLevel--;

        dg.useRandomSeed = EditorGUILayout.Toggle("Use Random Seed", dg.useRandomSeed);

        if (!dg.useRandomSeed)
        {
            EditorGUI.indentLevel++;
            dg.seed = EditorGUILayout.IntField("Dungeon Seed", dg.seed);
            EditorGUI.indentLevel--;
        }

        dg.generateAtStart = EditorGUILayout.Toggle("Generate at Start", dg.generateAtStart);
        if (!dg.generateAtStart)
        {
            if(GUILayout.Button("Create Dungeon"))
            {
                dg.DungeonStartTrigger();
            }
        }

        EditorGUILayout.Space();

        dg.showDebugOptions = EditorGUILayout.Foldout(dg.showDebugOptions, "Dungeon Debug Options", true);
        if (dg.showDebugOptions)
        {
            if (dg.dungeon)
            {
                EditorGUILayout.LabelField("Room Loading", EditorStyles.boldLabel);

                dg.canGenerateSites = EditorGUILayout.Toggle(new GUIContent("Use Debug Loading",
                    "Gives you the option to load a specific room from the dungeon"), dg.canGenerateSites);

                if (dg.canGenerateSites)
                {
                    EditorGUI.indentLevel++;
                    dg.siteToCharge = Mathf.Clamp(EditorGUILayout.IntField("Room to Load", dg.siteToCharge), 0, dg.sitesMax - 1);
                    if (GUILayout.Button("Create Room"))
                    {
                        dg.DebugGenerateSite();
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Rooms Information", EditorStyles.boldLabel);
                base.OnInspectorGUI();
            }
        }
    }
}
