using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonBrush))]
[CanEditMultipleObjects]
public class DungeonBrushEditor : Editor
{
    DungeonBrush db;
    int smallImageSize = 70;
    int BigImageSize = 100;

    private void OnEnable()
    {
        db = (DungeonBrush)target;      
        EditorUtility.SetDirty(db);

        foreach (GameObject go in db.meshes)
        {
            if (go != null)
                EditorUtility.SetDirty(go);
        }
        if (db.ground != null)
            EditorUtility.SetDirty(db.ground);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Wall Brush", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Meshes", centeredStyle);
        EditorGUILayout.BeginHorizontal("box");
            db.meshes[0] = ObjectField("Fill",        db.meshes[0], smallImageSize);
            db.meshes[1] = ObjectField("Horizontal",  db.meshes[1], smallImageSize);
            db.meshes[2] = ObjectField("Vertical",    db.meshes[2], smallImageSize);
            db.meshes[3] = ObjectField("Corner",      db.meshes[3], smallImageSize);
            db.meshes[4] = ObjectField("Round",       db.meshes[4], smallImageSize);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Texture Atlas", centeredStyle);
        GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (db.material != null)
                    GUILayout.Label(AssetPreview.GetAssetPreview(db.material), GUILayout.Height(BigImageSize), GUILayout.Width(BigImageSize));
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            db.material = (Material)EditorGUILayout.ObjectField("Wall Material", db.material, typeof(Material), false);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ground Brush", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            db.ground = ObjectField("Ground Mesh", db.ground, BigImageSize);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
                GUILayout.Label("Ground Material", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(BigImageSize));
                if (db.groundMaterial != null)
                    GUILayout.Label(AssetPreview.GetAssetPreview(db.groundMaterial), GUILayout.Height(BigImageSize), GUILayout.Width(BigImageSize));
                db.groundMaterial = (Material)EditorGUILayout.ObjectField(db.groundMaterial, typeof(Material), false, GUILayout.Width(BigImageSize));
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        /*
        EditorGUILayout.Space();
        if (GUILayout.Button("Visualize"))
        {
            foreach (GameObject go in db.meshes)
            {
                go.GetComponent<MeshRenderer>().sharedMaterial = db.material;
            }
            db.ground.GetComponent<MeshRenderer>().sharedMaterial = db.groundMaterial;
        }
        */
    }

    GameObject ObjectField(string name, GameObject mainObject, int size)
    {
        EditorGUILayout.BeginVertical();

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = size;

        GUILayout.Label(name, style);
        if (mainObject != null)
            GUILayout.Label(AssetPreview.GetAssetPreview(mainObject), GUILayout.Width(size), GUILayout.Height(size));
        mainObject = (GameObject)EditorGUILayout.ObjectField(mainObject, typeof(GameObject), false, GUILayout.Width(size));

        EditorGUILayout.EndVertical();

        return mainObject;
    }

}
