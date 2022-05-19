using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonDecoration))]
public class DungeonDecorationEditor : Editor
{
    DungeonDecoration dd;
    SerializedObject sObject;
    SerializedProperty decorations;
    int listSize;

    int firstWidth = 80;

    private void OnEnable()
    {
        dd = (DungeonDecoration)target;      
        EditorUtility.SetDirty(dd);

        sObject = new SerializedObject(target);
        decorations = sObject.FindProperty("decorations");
    }

    public override void OnInspectorGUI()
    {
        sObject.UpdateIfRequiredOrScript();

        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Global Probability", centeredStyle);
        EditorGUILayout.BeginVertical("box");
            dd.probability = EditorGUILayout.FloatField(new GUIContent("Probability",
            "The probability of even trying to create a decoration."), dd.probability);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Decorations", centeredStyle);
        EditorGUILayout.BeginVertical("box");
            listSize = decorations.arraySize;

            EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 180;
                listSize = Mathf.Clamp(EditorGUILayout.IntField("Number of Decorations", listSize), 1, dd.maxDecoSize);
                EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.EndHorizontal();

            if (listSize != decorations.arraySize)
            {
                while (listSize > decorations.arraySize)
                {
                    decorations.InsertArrayElementAtIndex(decorations.arraySize);
                }
                while (listSize < decorations.arraySize)
                {
                    decorations.DeleteArrayElementAtIndex(decorations.arraySize - 1);
                }
            }
            
            if (GUILayout.Button("Add Decoration"))
            {
                dd.decorations.Add(new Decoration());
            }

            EditorGUILayout.Space();
            
            for (int i = 0; i < decorations.arraySize; i++)
            {
                SerializedProperty decoRef = decorations.GetArrayElementAtIndex(i);
                SerializedProperty gO = decoRef.FindPropertyRelative("objeto");
                SerializedProperty material = decoRef.FindPropertyRelative("material");
                SerializedProperty probability = decoRef.FindPropertyRelative("probability");
                SerializedProperty isStack = decoRef.FindPropertyRelative("isStackable");
                SerializedProperty ranRot = decoRef.FindPropertyRelative("ranRot");
                SerializedProperty rot = decoRef.FindPropertyRelative("rotation");
                SerializedProperty deltaH = decoRef.FindPropertyRelative("deltaH");

                EditorGUILayout.LabelField("Decoration " + (i + 1), centeredStyle);

                EditorGUILayout.BeginHorizontal("CN Box");

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginVertical();
                        if (gO.objectReferenceValue)
                        {
                            GUILayout.Label(AssetPreview.GetAssetPreview(gO.objectReferenceValue), GUILayout.Width(firstWidth), GUILayout.Height(firstWidth));
                            GameObject tempGo = (GameObject)gO.objectReferenceValue;
                            rot.quaternionValue = tempGo.transform.rotation;
                        }
                        gO.objectReferenceValue = EditorGUILayout.ObjectField(gO.objectReferenceValue, typeof(GameObject), false, GUILayout.Width(firstWidth));
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    EditorGUIUtility.labelWidth = 105;

                    float prevProbability = 0.0f;
                    if (i != 0)
                    {
                        SerializedProperty antDecoRef = decorations.GetArrayElementAtIndex(i - 1);
                        SerializedProperty antProbability = antDecoRef.FindPropertyRelative("probability");
                        prevProbability = antProbability.floatValue;
                    }

                    EditorGUILayout.BeginVertical();
                        probability.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Probability",
                            "A representation of the overall probability of the DungeonDecoration"), probability.floatValue), prevProbability, 100);

                        float deltaProbability = 0.0f;
                        deltaProbability = probability.floatValue - prevProbability;
                        EditorGUILayout.LabelField(new GUIContent(string.Format("Actual Probability  ({0}%)", deltaProbability),
                            "The actual probability of this item spawning is the probability of this decoration minus the previous one"));

                        EditorGUILayout.Space();

                        deltaH.floatValue = EditorGUILayout.FloatField(new GUIContent("Added Height",
                            "Height to be added to this decoration"), deltaH.floatValue);
                        ranRot.boolValue = EditorGUILayout.Toggle(new GUIContent("Random Rotation",
                            "If checked, when generated, the decoration will be randomly rotated on the Y axis, otherwise the rotation of the prefab will be used."), ranRot.boolValue);
                        isStack.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Stackable",
                            "If checked, the mesh of this decoration will be combined with similar ones to improve performance, otherwise these decorations will be individually instantiated. "), isStack.boolValue);

                        if (isStack.boolValue)
                        {
                            material.objectReferenceValue = EditorGUILayout.ObjectField("Material", material.objectReferenceValue, typeof(Material), false);
                        }
                    EditorGUILayout.EndVertical();

                    EditorGUIUtility.labelWidth = 120;

                    GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

            }

        EditorGUILayout.EndVertical();

        sObject.ApplyModifiedProperties();
    }
}
