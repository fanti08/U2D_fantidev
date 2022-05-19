using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonShape))]
public class DungeonShapeEditor : Editor
{
    DungeonShape ds;

    int index;
    public string[] replace = new string[] { "Walls", "Structures" };

    private void OnEnable()
    {
        ds = (DungeonShape)target;
        EditorUtility.SetDirty(ds);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle centeredBoldStyle = new GUIStyle(GUI.skin.label);
        centeredBoldStyle.alignment = TextAnchor.UpperCenter;
        centeredBoldStyle.fontStyle = FontStyle.Bold;

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Shape Type", centeredBoldStyle);
        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Generation Type", GUILayout.Width(140));
                ds.generation = (SegmentGeneration)EditorGUILayout.EnumPopup(ds.generation);
            EditorGUILayout.EndHorizontal();
            if (ds.generation == SegmentGeneration.Artificial)
            {
                ds.chunkSize = 16;
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("It's a Predefined Room?", GUILayout.Width(140));
                    ds.isAnHabitacion = EditorGUILayout.Toggle(ds.isAnHabitacion);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                ds.isAnHabitacion = false;
            }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (ds.generation == SegmentGeneration.Natural)
        {
            EditorGUILayout.LabelField("Generation Properties", centeredBoldStyle);
            EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Room Chunks", centeredBoldStyle);
                ds.chunkSize = Mathf.Clamp(EditorGUILayout.IntField("Chunk Size", ds.chunkSize), ds.minChunkSize, ds.maxChunkSize); 
                ds.chunkWidth = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Width", ds.chunkWidth), 1, ds.maxRoomChunkWidth);
                ds.chunkHeight = Mathf.Clamp(EditorGUILayout.IntField("Room Chunks Height", ds.chunkHeight), 1, ds.maxRoomChunkHeight);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                Information();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Falloff Map Mask", centeredBoldStyle);
                ds.concentration = Mathf.Clamp(EditorGUILayout.FloatField("Concentration", ds.concentration), 0.0001f, ds.maxConcentration);
                ds.remoteness = Mathf.Clamp(EditorGUILayout.FloatField("Remoteness", ds.remoteness), 0.0001f, ds.maxRemoteness);

            EditorGUILayout.EndVertical();
        }
        else
        {
            if (ds.isAnHabitacion)
            {
                EditorGUILayout.LabelField("Room Texture", centeredBoldStyle);
                EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Canvas");
                        ds.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(ds.canvasTexture, typeof(Texture2D), false);
                    EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("Room Mask Texture", centeredBoldStyle);
                EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Canvas");
                        ds.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(ds.canvasTexture, typeof(Texture2D), false);
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

    public void Information()
    {
        GUIStyle centeredBoldStyle = new GUIStyle(GUI.skin.label);
        centeredBoldStyle.alignment = TextAnchor.UpperCenter;
        centeredBoldStyle.fontStyle = FontStyle.Bold;

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;


        EditorGUILayout.LabelField("Noise Properties", centeredBoldStyle);
        ds.noiseInfo.noiseScale  = Mathf.Clamp(EditorGUILayout.FloatField("Noise Scale", ds.noiseInfo.noiseScale), 0.0001f, ds.maxNoiseScale);
        ds.noiseInfo.octaves     = Mathf.Clamp(EditorGUILayout.IntField("Octaves", ds.noiseInfo.octaves), 1, ds.maxOctaves);
        ds.noiseInfo.persistance = EditorGUILayout.Slider("Persistance", ds.noiseInfo.persistance, 0, 1);
        ds.noiseInfo.lacunarity  = Mathf.Clamp(EditorGUILayout.FloatField("Lacunarity", ds.noiseInfo.lacunarity), ds.minLacunarity, ds.maxLacunarity);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Noise Processing", centeredBoldStyle);
        EditorGUIUtility.labelWidth = 140;

            EditorGUILayout.MinMaxSlider("Ground Range", ref ds.noiseProcessInfo.minFloorValue, ref ds.noiseProcessInfo.minStructureValue, 0, 1);

        EditorGUIUtility.labelWidth = 240;

            ds.noiseProcessInfo.minFloorValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Minimum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map below this value will be considered as a Wall"),
            ds.noiseProcessInfo.minFloorValue), 0, ds.noiseProcessInfo.minStructureValue);

            ds.noiseProcessInfo.minStructureValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Maximum Value to be Considered Ground",
                "Any entrance of the 0-1 perlin noise map above this value will be considered as a Structure"),
            ds.noiseProcessInfo.minStructureValue), ds.noiseProcessInfo.minFloorValue, 1);

        EditorGUIUtility.labelWidth = 140;
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Room Processing", centeredBoldStyle);
        EditorGUIUtility.labelWidth = 225;

            ds.noiseProcessInfo.minTilesToGenWalls = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Wall",
                "Minimum number of connected wall tiles required to be considered as an appropriate wall"),
            ds.noiseProcessInfo.minTilesToGenWalls);

            ds.noiseProcessInfo.minTilesToGenRooms = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a SubRoom",
                "Minimum number of connected ground tiles required to be considered as an appropriate subroom"),
            ds.noiseProcessInfo.minTilesToGenRooms);

            index = ds.noiseProcessInfo.failedRoomValue - 1;
            ds.noiseProcessInfo.failedRoomValue = Mathf.Clamp(EditorGUILayout.Popup(new GUIContent("Failed SubRooms will be Converted to",
                "SubRooms with less than the minimum required tiles will be Converted to this Specification"),
            index, replace) + 1, 1, 2);

        EditorGUILayout.Space();

            ds.noiseProcessInfo.minTilesToGenStructures = EditorGUILayout.IntField(new GUIContent("Minimum of Tiles to Form a Structure",
                "Minimum number of connected structure tiles required to be considered as an appropriate structure"),
            ds.noiseProcessInfo.minTilesToGenStructures);

            ds.noiseProcessInfo.maxHallwayRadius = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Maximum Possible Size for Passages",
                "Maximum size for the passages that will connect distant subrooms"),
            ds.noiseProcessInfo.maxHallwayRadius), 1, ds.maxPassageSize);

        EditorGUIUtility.labelWidth = 120;
    }
}
