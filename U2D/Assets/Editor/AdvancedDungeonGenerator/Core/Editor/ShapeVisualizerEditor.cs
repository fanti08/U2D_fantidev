using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeVisualizer))]
public class ShapeVisualizerEditor : Editor
{
    ShapeVisualizer sv;

    int index;
    public string[] replace = new string[] { "Walls", "Structures" };

    int type;       
    int order;      
    
    string chain;  
    public string[] natural = new string[] { "Process Room Map", "Basic Noise Map", "Falloff Map" };
    public string[] artificial = new string[] { "Process Room Map", "Basic Noise Map", "Base Canvas Image" };
    public string[] habitacion = new string[] { "Base Canvas Image" };

    private void OnEnable()
    {
        sv = (ShapeVisualizer)target;
        EditorUtility.SetDirty(sv);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Display Properties", EditorStyles.boldLabel);
        sv.dSh = (DungeonShape)EditorGUILayout.ObjectField("DungeonShape", sv.dSh, typeof(DungeonShape), false);
        sv.canvas = (Renderer)EditorGUILayout.ObjectField("Canvas", sv.canvas, typeof(Renderer), true);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        sv.showShapeProperties = EditorGUILayout.Foldout(sv.showShapeProperties, "Draw Properties", true);
        if (sv.showShapeProperties)
        {
            CheckDungeonShape();

            if (chain == "Process Room Map")
            {
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Use Advanced Processing", GUILayout.Width(170));
                    sv.useAdvanceProcessing = EditorGUILayout.Toggle(sv.useAdvanceProcessing);
                EditorGUILayout.EndHorizontal();

                if (sv.useAdvanceProcessing)
                {
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Apply Procedural Access", GUILayout.Width(170));
                        sv.useProceduralAccess = EditorGUILayout.Toggle(sv.useProceduralAccess);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            if (chain == "Process Room Map" || chain == "Basic Noise Map")
            {
                EditorGUILayout.LabelField("Randomness", EditorStyles.boldLabel);
                sv.seed = EditorGUILayout.IntField("Generation Seed", sv.seed);
                sv.offset = EditorGUILayout.Vector2Field("Map Offset", sv.offset);
            }

        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        sv.canEditShapeParameters = EditorGUILayout.Foldout(sv.canEditShapeParameters, "Edit Shape Parameters", true);
        if (sv.canEditShapeParameters && sv.dSh != null)
        {

            GUIStyle centeredBoldStyle = new GUIStyle(GUI.skin.label);
            centeredBoldStyle.alignment = TextAnchor.UpperCenter;
            centeredBoldStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField("Shape Type", centeredBoldStyle);
            EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Generation Type", GUILayout.Width(140));
                    sv.dSh.generation = (SegmentGeneration)EditorGUILayout.EnumPopup(sv.dSh.generation);
                EditorGUILayout.EndHorizontal();

                if (sv.dSh.generation == SegmentGeneration.Artificial)
                {
                    sv.dSh.chunkSize = 16;
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("It's a Predefined Room?", GUILayout.Width(140));
                        sv.dSh.isAnHabitacion = EditorGUILayout.Toggle(sv.dSh.isAnHabitacion);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    sv.dSh.isAnHabitacion = false;
                }

            EditorGUILayout.EndVertical();
    
            EditorGUILayout.Space();

            if (sv.dSh.generation == SegmentGeneration.Natural)
            {
                EditorGUILayout.LabelField("Generation Properties", centeredBoldStyle);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Room Chunks", centeredBoldStyle);
                sv.dSh.chunkSize = Mathf.Clamp(EditorGUILayout.IntField("Chunk Size", sv.dSh.chunkSize), sv.dSh.minChunkSize, sv.dSh.maxChunkSize);
                sv.dSh.chunkWidth = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Width", sv.dSh.chunkWidth), 1, sv.dSh.maxRoomChunkWidth);
                sv.dSh.chunkHeight = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Height", sv.dSh.chunkHeight), 1, sv.dSh.maxRoomChunkHeight);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                Information();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Falloff Map Mask", centeredBoldStyle);
                sv.dSh.concentration = Mathf.Clamp(EditorGUILayout.FloatField("Concentration", sv.dSh.concentration), 0.0001f, sv.dSh.maxConcentration) ;
                sv.dSh.remoteness = Mathf.Clamp(EditorGUILayout.FloatField("Remoteness", sv.dSh.remoteness), 0.0001f, sv.dSh.maxRemoteness);

                EditorGUILayout.EndVertical();
            }
            else
            {
                if (sv.dSh.isAnHabitacion)
                {
                    EditorGUILayout.LabelField("Room Texture", centeredBoldStyle);
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Canvas");
                        sv.dSh.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(sv.dSh.canvasTexture, typeof(Texture2D), false);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.LabelField("Room Mask Texture", centeredBoldStyle);
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Canvas");
                        sv.dSh.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(sv.dSh.canvasTexture, typeof(Texture2D), false);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Generation Properties", centeredBoldStyle);
                    EditorGUILayout.BeginVertical("box");

                        Information();

                    EditorGUILayout.EndVertical();
                }
            }

        }

        EditorGUILayout.Space();

        sv.autoUpdate = EditorGUILayout.Toggle("AutoUpdate", sv.autoUpdate);
        if (GUI.changed && sv.autoUpdate)   
        {
            CheckDraw();
        }
        
        if (GUILayout.Button("Draw Map"))
        {
            CheckDraw();
        }
    }


    public void Information()
    {
        GUIStyle centeredBoldStyle = new GUIStyle(GUI.skin.label);
        centeredBoldStyle.alignment = TextAnchor.UpperCenter;
        centeredBoldStyle.fontStyle = FontStyle.Bold;

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;


        EditorGUILayout.LabelField("Noise Properties", centeredBoldStyle);
        sv.dSh.noiseInfo.noiseScale  = Mathf.Clamp(EditorGUILayout.FloatField("Noise Scale", sv.dSh.noiseInfo.noiseScale), 0.0001f, sv.dSh.maxNoiseScale);
        sv.dSh.noiseInfo.octaves     = Mathf.Clamp(EditorGUILayout.IntField("Octaves", sv.dSh.noiseInfo.octaves), 1, sv.dSh.maxOctaves);
        sv.dSh.noiseInfo.persistance = EditorGUILayout.Slider("Persistance", sv.dSh.noiseInfo.persistance, 0, 1);
        sv.dSh.noiseInfo.lacunarity  = Mathf.Clamp(EditorGUILayout.FloatField("Lacunarity", sv.dSh.noiseInfo.lacunarity), sv.dSh.minLacunarity, sv.dSh.maxLacunarity);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise Processing", centeredBoldStyle);
        EditorGUIUtility.labelWidth = 140;

            EditorGUILayout.MinMaxSlider("Ground Range", ref sv.dSh.noiseProcessInfo.minFloorValue, ref sv.dSh.noiseProcessInfo.minStructureValue, 0, 1);

        EditorGUIUtility.labelWidth = 240;

            sv.dSh.noiseProcessInfo.minFloorValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Minimum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map below this value will be considered as a Wall"), 
            sv.dSh.noiseProcessInfo.minFloorValue), 0, sv.dSh.noiseProcessInfo.minStructureValue);

            sv.dSh.noiseProcessInfo.minStructureValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Maximum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map above this value will be considered as a Structure"), 
            sv.dSh.noiseProcessInfo.minStructureValue), sv.dSh.noiseProcessInfo.minFloorValue, 1);

        EditorGUIUtility.labelWidth = 140;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Room Processing", centeredBoldStyle);
        EditorGUIUtility.labelWidth = 225;

            sv.dSh.noiseProcessInfo.minTilesToGenWalls = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Wall",
                "Minimum number of connected wall tiles required to be considered as an appropriate wall"), 
            sv.dSh.noiseProcessInfo.minTilesToGenWalls);

            sv.dSh.noiseProcessInfo.minTilesToGenRooms = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a SubRoom",
                "Minimum number of connected floor tiles required to be considered as an appropriate subroom"), 
            sv.dSh.noiseProcessInfo.minTilesToGenRooms);

            index = sv.dSh.noiseProcessInfo.failedRoomValue - 1;
            sv.dSh.noiseProcessInfo.failedRoomValue = Mathf.Clamp(EditorGUILayout.Popup(new GUIContent("Failed SubRooms will be Converted to",
                "SubRooms with less than the minimum required tiles will be Converted to this Specification"), 
            index, replace) + 1, 1, 2);

            EditorGUILayout.Space();

            sv.dSh.noiseProcessInfo.minTilesToGenStructures = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Structure",
                "Minimum number of connected structure tiles required to be considered as an appropriate structure"), 
            sv.dSh.noiseProcessInfo.minTilesToGenStructures);

            sv.dSh.noiseProcessInfo.maxHallwayRadius = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Maximum Possible Size for Passages",
                "Maximum size for the passages that will connect distant subrooms"), 
            sv.dSh.noiseProcessInfo.maxHallwayRadius), 1, sv.dSh.maxPassageSize);

        EditorGUIUtility.labelWidth = 120;
    }

    public void CheckDraw()
    {
        if (sv.canvas == null)
        {
            Debug.LogError("The room cannot be drawn due to lack of essential information (Scene Canvas is not assigned)");
            return;
        }

        if (CheckDungeonShape())
        {
            if (sv.dSh.generation != SegmentGeneration.Natural)
            {
                if (sv.dSh.canvasTexture != null)
                    sv.Draw(type, order);
                else
                    Debug.LogError("The room cannot be drawn due to lack of essential information (Room Texture Missing)");
            }else{
                sv.Draw(type, order);
            }
        }
        else
        {
            Debug.LogError("The room cannot be drawn due to lack of essential information (DungeonShape is not assigned)");
        }
    }

    public bool CheckDungeonShape()
    {
        if (sv.dSh != null)
        {
            if (sv.dSh.generation == SegmentGeneration.Natural)      //Natural
            {
                EditorGUILayout.LabelField("Natural Shape Options", EditorStyles.boldLabel);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, natural), 0, 2);
                chain = natural[sv.index];
                type = 0;
            }
            else if (sv.dSh.isAnHabitacion != true)                  //Artificial, No Habitation 
            {
                EditorGUILayout.LabelField("Artificial Shape Options", EditorStyles.boldLabel);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, artificial), 0, 2);
                chain = artificial[sv.index];
                type = 1;
            }
            else                                                    //Artificial & Habitation
            {
                EditorGUILayout.LabelField("Room Shape Options", EditorStyles.boldLabel);
                sv.index = Mathf.Clamp(EditorGUILayout.Popup("Draw Mode", sv.index, habitacion), 0, 0);
                chain = habitacion[sv.index];
                type = 2;
            }

            if (chain == "Process Room Map")
                order = 0;
            else if (chain == "Basic Noise Map")
                order = 1;
            else if (chain == "Falloff Map")
                order = 2;
            else   //chain == "Base Canvas Image"
                order = 3;

            return true;
        }
        else
            return false;
    }
}
