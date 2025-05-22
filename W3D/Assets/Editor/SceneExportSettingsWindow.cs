// Assets/Editor/SceneExportSettingsWindow.cs
using UnityEditor;
using UnityEngine;

public class SceneExportSettingsWindow : EditorWindow
{
    private SceneExportSettings settings;

    [MenuItem("Tools/Scene Export Settings")]
    public static void ShowWindow()
    {
        GetWindow<SceneExportSettingsWindow>("Scene Export Settings");
    }

    private void OnEnable()
    {
        LoadOrCreateSettings();
    }

    private void OnGUI()
    {
        if (settings == null)
        {
            GUILayout.Label("No settings asset found.");
            if (GUILayout.Button("Create New Settings"))
                CreateSettingsAsset();
            return;
        }

        SerializedObject so = new SerializedObject(settings);

        EditorGUILayout.LabelField("Scene Metadata", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("Name"));
        EditorGUILayout.PropertyField(so.FindProperty("Description"));
        EditorGUILayout.PropertyField(so.FindProperty("Author"));

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Model Paths", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("BaseModelPath"));
        EditorGUILayout.PropertyField(so.FindProperty("WebModelLocation"));

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Content Flags", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("AdultContent"));
        EditorGUILayout.PropertyField(so.FindProperty("ContentRating"));
        EditorGUILayout.PropertyField(so.FindProperty("PrimaryLanguage"));

        EditorGUILayout.Space();
        if (GUILayout.Button("Force Save"))
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        so.ApplyModifiedProperties();
    }

    private void LoadOrCreateSettings()
    {
        string path = "Assets/SceneExportSettings.asset";
        settings = AssetDatabase.LoadAssetAtPath<SceneExportSettings>(path);

        if (settings == null)
            CreateSettingsAsset();
    }

    private void CreateSettingsAsset()
    {
        settings = ScriptableObject.CreateInstance<SceneExportSettings>();
        AssetDatabase.CreateAsset(settings, "Assets/SceneExportSettings.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created new SceneExportSettings asset.");
    }

    public static SceneExportSettings GetSettings()
    {
        return AssetDatabase.LoadAssetAtPath<SceneExportSettings>("Assets/SceneExportSettings.asset");
    }
}
