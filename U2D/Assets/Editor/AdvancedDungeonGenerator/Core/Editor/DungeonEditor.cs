using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Dungeon))]
public class DungeonEditor : Editor
{
    Dungeon d;
    SerializedObject sObject;
    SerializedProperty segments;
    int listSize;
    bool isFirstTime;

    bool prevUseGlobalDeco;
    bool prevUseGlobalBrush;
    bool prevUseGlobalSpects;

    DungeonDecoration wallDeco;
    DungeonDecoration cornDeco;
    DungeonDecoration florDeco;

    DungeonBrush wallBrush;
    DungeonBrush struBrush;

    float structuresHeight;
    bool  useGroundInStructures;
    float structuresGroundHeight;

    bool applyMeshToStructs;
    int  structuresLayer;
    bool applyMeshToStructuresGround;
    int  structuresGroundLayer;

    private void OnEnable()
    {
        d = (Dungeon)target;
        EditorUtility.SetDirty(d);

        sObject = new SerializedObject(target);
        segments = sObject.FindProperty("segments");

        isFirstTime = true;
    }

    public override void OnInspectorGUI()
    {
        sObject.UpdateIfRequiredOrScript();

        if (d.segments.Count < 1)
        {
            segments.InsertArrayElementAtIndex(0);
        }
        SerializedProperty segRefZero = segments.GetArrayElementAtIndex(0);
        
        if (isFirstTime)
        {
            if (d.useGlobalDecoration)
            {
                SerializedProperty dDw = segRefZero.FindPropertyRelative("wallDeco");
                SerializedProperty dDc = segRefZero.FindPropertyRelative("cornerDeco");
                SerializedProperty dDf = segRefZero.FindPropertyRelative("floorDeco");
                wallDeco = (DungeonDecoration)dDw.objectReferenceValue;
                cornDeco = (DungeonDecoration)dDc.objectReferenceValue;
                florDeco = (DungeonDecoration)dDf.objectReferenceValue;
            }

            if (d.useGlobalBrush)
            {
                SerializedProperty dBw = segRefZero.FindPropertyRelative("walls");
                SerializedProperty dBs = segRefZero.FindPropertyRelative("structures");
                wallBrush = (DungeonBrush)dBw.objectReferenceValue;
                struBrush = (DungeonBrush)dBs.objectReferenceValue;
            }

            if (d.useGlobalStructuresSpecifications)
            {
                SerializedProperty dSShS  = segRefZero.FindPropertyRelative("structuresHeight");
                SerializedProperty dSSuSG = segRefZero.FindPropertyRelative("useGroundInStructures");
                SerializedProperty dSShSG = segRefZero.FindPropertyRelative("structuresGroundHeight");
                structuresHeight       = dSShS.floatValue;
                useGroundInStructures  = dSSuSG.boolValue;
                structuresGroundHeight = dSShSG.floatValue;

                SerializedProperty aMtS  = segRefZero.FindPropertyRelative("applyMeshToStructures");
                SerializedProperty sL    = segRefZero.FindPropertyRelative("structuresLayer");
                SerializedProperty aMtGS = segRefZero.FindPropertyRelative("applyMeshToStructuresGround");
                SerializedProperty GsL   = segRefZero.FindPropertyRelative("structuresGroundLayer");
                applyMeshToStructs          = aMtS.boolValue;
                structuresLayer             = sL.intValue;
                applyMeshToStructuresGround = aMtGS.boolValue;
                structuresGroundLayer       = GsL.intValue;
            }

            isFirstTime = false;
        }

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Mesh Attributes", centeredStyle);
        EditorGUILayout.BeginVertical("box");

        EditorGUIUtility.labelWidth = 170;
        d.walkableLayer = EditorGUILayout.LayerField(new GUIContent("Walkable Layer", 
            "The layer that will be assigned to the Dungeon Ground"), d.walkableLayer);
        d.applyMeshToWalls = EditorGUILayout.Toggle(new GUIContent("Apply Mesh Collider to Walls",
            "If checked, a mesh collider will be added to all Dungeon Walls, this option is a default for the Dungeon Ground"), d.applyMeshToWalls);
        if (d.applyMeshToWalls)
        {
            EditorGUI.indentLevel++;
            d.wallsLayer = EditorGUILayout.LayerField(new GUIContent("Walls Layer", 
                "The layer to which all Dungeon Walls will be assigned"), d.wallsLayer);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Rooms Loading", centeredStyle);
        EditorGUILayout.BeginVertical("box");
            d.hidePredecesors = EditorGUILayout.Toggle(new GUIContent("Hide Predecessors",
                "If checked, the rooms that are no longer visible on screen will be hidden"), d.hidePredecesors);
            d.useArtificialAsigment = EditorGUILayout.Toggle(new GUIContent("Use Artificial Loading",
                "If checked, the user will define the proper load and distribution of the Dungeon Rooms, otherwise the Dungeon Generator will do it on its own"), d.useArtificialAsigment);
            d.useProceduralAsigment = !d.useArtificialAsigment;
        EditorGUILayout.EndVertical();
        EditorGUIUtility.labelWidth = 120;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Global Attributes", centeredStyle);
        EditorGUILayout.BeginVertical("box");

        d.showGlobalSettings = EditorGUILayout.Foldout(d.showGlobalSettings, "Show Global Attributes", true);

        if (d.showGlobalSettings)
        {
            prevUseGlobalDeco = d.useGlobalDecoration;
            prevUseGlobalBrush = d.useGlobalBrush;
            prevUseGlobalSpects = d.useGlobalStructuresSpecifications;

            EditorGUI.indentLevel++;

            //=================================================================================================================================
            EditorGUILayout.LabelField("Dungeon Decorations", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 185;

            GUI.changed = false;
                d.hasDecoration = EditorGUILayout.Toggle("Enable Decorations", d.hasDecoration);
                if (GUI.changed)
                {
                    if(prevUseGlobalDeco != d.useGlobalDecoration)
                    {
                        if (d.useGlobalDecoration)
                        {
                            SerializedProperty dDw = segRefZero.FindPropertyRelative("wallDeco");
                            SerializedProperty dDc = segRefZero.FindPropertyRelative("cornerDeco");
                            SerializedProperty dDf = segRefZero.FindPropertyRelative("floorDeco");
                            wallDeco = (DungeonDecoration)dDw.objectReferenceValue;
                            cornDeco = (DungeonDecoration)dDc.objectReferenceValue;
                            florDeco = (DungeonDecoration)dDf.objectReferenceValue;
                        }
                    }
                }
            GUI.changed = false;

            if (d.hasDecoration)
            {
                EditorGUI.indentLevel++;
                d.useGlobalDecoration = EditorGUILayout.Toggle("Use Global Decoration", d.useGlobalDecoration);

                if (d.useGlobalDecoration)
                {
                    EditorGUI.indentLevel++;
                    EditorGUIUtility.labelWidth = 160;

                    GUI.changed = false;
                    wallDeco = (DungeonDecoration)EditorGUILayout.ObjectField("Wall Decoracion", wallDeco, typeof(DungeonDecoration), false);
                    if (GUI.changed)
                    {
                        if (wallDeco)
                        {
                            if(SecurityCheck.ValidateDungeonDecoration(wallDeco) == false)
                            {
                                wallDeco = null;
                            }
                        }
                    }
                    GUI.changed = false;

                    GUI.changed = false;
                    cornDeco = (DungeonDecoration)EditorGUILayout.ObjectField("Corner Decoracion", cornDeco, typeof(DungeonDecoration), false);
                    if (GUI.changed)
                    {
                        if (cornDeco)
                        {
                            if(SecurityCheck.ValidateDungeonDecoration(cornDeco) == false)
                            {
                                cornDeco = null;
                            }
                        }
                    }
                    GUI.changed = false;

                    GUI.changed = false;
                    florDeco = (DungeonDecoration)EditorGUILayout.ObjectField("Ground Decoracion", florDeco, typeof(DungeonDecoration), false);
                    if (GUI.changed)
                    {
                        if (florDeco)
                        {
                            if(SecurityCheck.ValidateDungeonDecoration(florDeco) == false)
                            {
                                florDeco = null;
                            }
                        }
                    }
                    GUI.changed = false;

                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            //=================================================================================================================================
            EditorGUILayout.LabelField("Dungeon Brush", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 185;

            GUI.changed = false;
                d.useGlobalBrush = EditorGUILayout.Toggle("Use Global Brush", d.useGlobalBrush);
                if (GUI.changed)
                {
                    if (prevUseGlobalBrush != d.useGlobalBrush)
                    {
                        if (d.useGlobalBrush)
                        {
                            SerializedProperty dBw = segRefZero.FindPropertyRelative("walls");
                            SerializedProperty dBs = segRefZero.FindPropertyRelative("structures");
                            wallBrush = (DungeonBrush)dBw.objectReferenceValue;
                            struBrush = (DungeonBrush)dBs.objectReferenceValue;
                        }
                    }
                }
            GUI.changed = false;

            if (d.useGlobalBrush)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 140;

                GUI.changed = false;
                    wallBrush = (DungeonBrush)EditorGUILayout.ObjectField("Wall Brush", wallBrush, typeof(DungeonBrush), false);
                    if (GUI.changed)
                    {
                        if (wallBrush)
                        {
                            if(SecurityCheck.ValidateDungeonBrush(wallBrush) == false)
                            {
                                wallBrush = null;
                            }
                        }
                    }
                GUI.changed = false;

                GUI.changed = false;
                    struBrush = (DungeonBrush)EditorGUILayout.ObjectField("Structure Brush", struBrush, typeof(DungeonBrush), false);
                    if (GUI.changed)
                    {
                        if (struBrush)
                        {
                            if(SecurityCheck.ValidateDungeonBrush(struBrush) == false)
                            {
                                struBrush = null;
                            }
                        }
                    }
                GUI.changed = false;

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            //=================================================================================================================================
            EditorGUILayout.LabelField("Structures Specifications", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 185;

            GUI.changed = false;
                d.useGlobalStructuresSpecifications = EditorGUILayout.Toggle("Use Global Structures Specs", d.useGlobalStructuresSpecifications);
                if (GUI.changed)
                {
                    if(prevUseGlobalSpects != d.useGlobalStructuresSpecifications)
                    {
                        if (d.useGlobalStructuresSpecifications)
                        {
                            SerializedProperty dSShS = segRefZero.FindPropertyRelative("structuresHeight");
                            SerializedProperty dSSuSG = segRefZero.FindPropertyRelative("useGroundInStructures");
                            SerializedProperty dSShSG = segRefZero.FindPropertyRelative("structuresGroundHeight");
                            structuresHeight = dSShS.floatValue;
                            useGroundInStructures = dSSuSG.boolValue;
                            structuresGroundHeight = dSShSG.floatValue;

                            SerializedProperty aMtS = segRefZero.FindPropertyRelative("applyMeshToStructures");
                            SerializedProperty sL = segRefZero.FindPropertyRelative("structuresLayer");
                            SerializedProperty aMtGS = segRefZero.FindPropertyRelative("applyMeshToStructuresGround");
                            SerializedProperty GsL = segRefZero.FindPropertyRelative("structuresGroundLayer");
                            applyMeshToStructs = aMtS.boolValue;
                            structuresLayer = sL.intValue;
                            applyMeshToStructuresGround = aMtGS.boolValue;
                            structuresGroundLayer = GsL.intValue;
                        }
                    }
                }
            GUI.changed = false;

            if (d.useGlobalStructuresSpecifications)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 212;

                structuresHeight = EditorGUILayout.FloatField("Structures Height", structuresHeight);
                useGroundInStructures = EditorGUILayout.Toggle("Use Ground Mesh in Structures", useGroundInStructures);
                if (useGroundInStructures)
                {
                    EditorGUI.indentLevel++;
                    structuresGroundHeight = EditorGUILayout.FloatField("Structures Ground Height", structuresGroundHeight); 
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Mesh Collider Application", EditorStyles.boldLabel);
                applyMeshToStructs = EditorGUILayout.Toggle("Apply to Structures", applyMeshToStructs);
                if (applyMeshToStructs)
                {
                    EditorGUI.indentLevel++;
                    structuresLayer = EditorGUILayout.LayerField("Structures Layer", structuresLayer);
                    EditorGUI.indentLevel--;
                }

                if (useGroundInStructures)
                {
                    applyMeshToStructuresGround = EditorGUILayout.Toggle("Apply to Structures Ground", applyMeshToStructuresGround);
                    if (applyMeshToStructuresGround)
                    {
                        EditorGUI.indentLevel++;
                        structuresGroundLayer = EditorGUILayout.LayerField("Structures Ground Layer", structuresGroundLayer);
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUIUtility.labelWidth = 120;
                EditorGUI.indentLevel--;
            }
            //=================================================================================================================================

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Dungeon Segments", centeredStyle);
        EditorGUILayout.BeginVertical("box");
            listSize = segments.arraySize;

            EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 160;
                listSize = Mathf.Clamp(EditorGUILayout.IntField("Number of Segments", listSize), 1, d.maxDungeonSegments);
                EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.EndHorizontal();

            if (listSize < 1)
                listSize = 1;

            if (listSize != segments.arraySize)
            {
                while (listSize > segments.arraySize)
                {
                    segments.InsertArrayElementAtIndex(segments.arraySize);
                }
                while (listSize < segments.arraySize)
                {
                    segments.DeleteArrayElementAtIndex(segments.arraySize - 1);
                }
            }

            if (GUILayout.Button("Add Segment"))
            {
                d.segments.Add(new DungeonSegment());
            }
        
            EditorGUILayout.Space();

        for (int i = 0; i < segments.arraySize; i++)
        {
            SerializedProperty segRef = segments.GetArrayElementAtIndex(i);

            SerializedProperty floors = segRef.FindPropertyRelative("floors");
            SerializedProperty shape = segRef.FindPropertyRelative("shape");
            SerializedProperty useMargen = segRef.FindPropertyRelative("useMargin");
            SerializedProperty margen = segRef.FindPropertyRelative("margin");

            SerializedProperty useExtra = segRef.FindPropertyRelative("useExtra");
            SerializedProperty extra = segRef.FindPropertyRelative("extra");
            SerializedProperty extraPos = segRef.FindPropertyRelative("extraPos");
            
            SerializedProperty dBw = segRef.FindPropertyRelative("walls");
            SerializedProperty dBs = segRef.FindPropertyRelative("structures");

            SerializedProperty dDw = segRef.FindPropertyRelative("wallDeco");
            SerializedProperty dDc = segRef.FindPropertyRelative("cornerDeco");
            SerializedProperty dDf = segRef.FindPropertyRelative("floorDeco");

            SerializedProperty dSShS = segRef.FindPropertyRelative("structuresHeight");
            SerializedProperty dSSuSG = segRef.FindPropertyRelative("useGroundInStructures");
            SerializedProperty dSShSG = segRef.FindPropertyRelative("structuresGroundHeight");

            SerializedProperty aMtS = segRef.FindPropertyRelative("applyMeshToStructures");
            SerializedProperty sL = segRef.FindPropertyRelative("structuresLayer");
            SerializedProperty aMtGS = segRef.FindPropertyRelative("applyMeshToStructuresGround");
            SerializedProperty GsL = segRef.FindPropertyRelative("structuresGroundLayer");


            EditorGUILayout.LabelField("Segment " + (i + 1), centeredStyle);

            floors.intValue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Segment Rooms", 
                "The number of rooms in this segment"), floors.intValue), 1, d.maxSegmentRooms);

            GUI.changed = false;
                shape.objectReferenceValue = EditorGUILayout.ObjectField("Segment Shape", shape.objectReferenceValue, typeof(DungeonShape), false);
                if (GUI.changed)
                {
                    if (shape.objectReferenceValue)
                    {
                        if(SecurityCheck.ValidateDungeonShape((DungeonShape)shape.objectReferenceValue) == false)
                        {
                            shape.objectReferenceValue = null;
                        }
                    }
                }
            GUI.changed = false;

            if (shape.objectReferenceValue)
            {
                SerializedObject buffer = new SerializedObject(shape.objectReferenceValue);
                SerializedProperty chunckSize = buffer.FindProperty("chunkSize");
                
                useMargen.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Room Border", 
                    "The rooms of this segment will have a smart border, very useful to avoid showing the raw edges of the room"), useMargen.boolValue);
                if (useMargen.boolValue)
                {
                    EditorGUI.indentLevel++;
                    margen.intValue = EditorGUILayout.IntSlider("Border Size", margen.intValue, 1, chunckSize.intValue);
                    EditorGUI.indentLevel--;
                }
            }

            useExtra.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Extra Prefab",
                "Will instantiate a given GameObject in each room of this segment"), useExtra.boolValue);
            if (useExtra.boolValue)
            {
                EditorGUI.indentLevel++;
                    extra.objectReferenceValue = EditorGUILayout.ObjectField("Game Object", extra.objectReferenceValue, typeof(GameObject), false);
                    extraPos.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Position",
                        "Position of the extra prefab relative to the room center"), extraPos.vector3Value);
                EditorGUI.indentLevel--;
            }

            if (d.useArtificialAsigment)
            {
                SerializedProperty displacement = segRef.FindPropertyRelative("artificialDisplacement");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Artificial Loading Attributes", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                displacement.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Room Offset",
                    "The amount of displacement a room of this segment has, relative to the previous one.\n" +
                    "To clarify, this movement moves the center of this room from the center of the previous one. "), displacement.vector3Value);

                SerializedProperty useLoader = segRef.FindPropertyRelative("useLoader");
                useLoader.boolValue = EditorGUILayout.Toggle(new GUIContent("Create Loader",
                    "Will instantiate a custom loader in all the rooms of the segment"), useLoader.boolValue);

                if (useLoader.boolValue)
                {
                    SerializedProperty loaderAngle = segRef.FindPropertyRelative("loaderAngle");
                    loaderAngle.floatValue = EditorGUILayout.FloatField(new GUIContent("Loader Angle",
                        "The Y angle at which the loader will be"), loaderAngle.floatValue);

                    SerializedProperty loaderPos = segRef.FindPropertyRelative("deltaLoaderPos");
                    loaderPos.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Loader Offset",
                        "The amount of displacement the loader will have, relative to the room center"), loaderPos.vector3Value);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUIUtility.labelWidth = 140;
            if (d.useGlobalBrush)
            {
                dBw.objectReferenceValue = wallBrush;
                dBs.objectReferenceValue = struBrush;
            }else{
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                GUI.changed = false;
                    dBw.objectReferenceValue = EditorGUILayout.ObjectField("Wall Brush", dBw.objectReferenceValue, typeof(DungeonBrush), false);
                    if (GUI.changed)
                    {
                        if (dBw.objectReferenceValue)
                        {
                            if (SecurityCheck.ValidateDungeonBrush((DungeonBrush)dBw.objectReferenceValue) == false)
                            {
                                dBw.objectReferenceValue = null;
                            }
                        }
                    }
                GUI.changed = false;

                GUI.changed = false;
                    dBs.objectReferenceValue = EditorGUILayout.ObjectField("Structure Brush", dBs.objectReferenceValue, typeof(DungeonBrush), false);
                    if (GUI.changed)
                    {
                        if (dBs.objectReferenceValue)
                        {
                            if(SecurityCheck.ValidateDungeonBrush((DungeonBrush)dBs.objectReferenceValue) == false)
                            {
                                dBs.objectReferenceValue = null;
                            }
                        }
                    }
                GUI.changed = false;

                EditorGUI.indentLevel--;
            }

            if (d.useGlobalDecoration)
            {
                dDw.objectReferenceValue = wallDeco;
                dDc.objectReferenceValue = cornDeco;
                dDf.objectReferenceValue = florDeco;
            }else{
                if (d.hasDecoration)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Decoration", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    GUI.changed = false;
                        dDw.objectReferenceValue = EditorGUILayout.ObjectField("Wall Decoration", dDw.objectReferenceValue, typeof(DungeonDecoration), false);
                        if (GUI.changed)
                        {
                            if (dDw.objectReferenceValue)
                            {
                                if(SecurityCheck.ValidateDungeonDecoration((DungeonDecoration)dDw.objectReferenceValue) == false)
                                {
                                    dDw.objectReferenceValue = null;
                                }
                            }
                        }
                    GUI.changed = false;

                    GUI.changed = false;
                        dDc.objectReferenceValue = EditorGUILayout.ObjectField("Corner Decoration", dDc.objectReferenceValue, typeof(DungeonDecoration), false);
                        if (GUI.changed)
                        {
                            if (dDc.objectReferenceValue)
                            {
                                if(SecurityCheck.ValidateDungeonDecoration((DungeonDecoration)dDc.objectReferenceValue) == false)
                                {
                                    dDc.objectReferenceValue = null;
                                }
                            }
                        }
                    GUI.changed = false;

                    GUI.changed = false;
                        dDf.objectReferenceValue = EditorGUILayout.ObjectField("Ground Decoration", dDf.objectReferenceValue, typeof(DungeonDecoration), false);
                        if (GUI.changed)
                        {
                            if (dDf.objectReferenceValue)
                            {
                                if(SecurityCheck.ValidateDungeonDecoration((DungeonDecoration)dDf.objectReferenceValue) == false)
                                {
                                    dDf.objectReferenceValue = null;
                                }
                            }
                        }
                    GUI.changed = false;

                    EditorGUI.indentLevel--;
                }
            }
            
            if (d.useGlobalStructuresSpecifications)
            {
                dSShS.floatValue = structuresHeight;
                dSSuSG.boolValue = useGroundInStructures;
                dSShSG.floatValue = structuresGroundHeight;

                aMtS.boolValue = applyMeshToStructs;
                sL.intValue = structuresLayer;
                aMtGS.boolValue = applyMeshToStructuresGround;
                GsL.intValue = structuresGroundLayer;
            }else{
                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = 200;
                EditorGUILayout.LabelField("Structures Specifications", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                dSShS.floatValue = EditorGUILayout.FloatField("Structures Height", dSShS.floatValue);
                dSSuSG.boolValue = EditorGUILayout.Toggle("Use Ground Mesh in Structures", dSSuSG.boolValue);
                if (dSSuSG.boolValue)
                {
                    dSShSG.floatValue = EditorGUILayout.FloatField("Structures Ground Height", dSShSG.floatValue);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.LabelField("Mesh Collider Application", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                aMtS.boolValue = EditorGUILayout.Toggle("Apply to Structures", aMtS.boolValue);
                if (aMtS.boolValue)
                {
                    sL.intValue = EditorGUILayout.LayerField("Structures Layer", sL.intValue);
                }
                if (dSSuSG.boolValue)
                {
                    aMtGS.boolValue = EditorGUILayout.Toggle("Apply to Structures Ground", aMtGS.boolValue);
                    if (aMtGS.boolValue)
                    {
                        GsL.intValue = EditorGUILayout.LayerField("Structures Ground Layer", GsL.intValue);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUIUtility.labelWidth = 120;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndVertical();

        sObject.ApplyModifiedProperties();
    }
}
