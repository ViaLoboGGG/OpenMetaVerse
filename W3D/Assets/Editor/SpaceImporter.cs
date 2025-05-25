using UnityEditor;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class SpaceImporter : EditorWindow
{
    private string jsonPath;
    private ExportedSpace previewSpace;

    [MenuItem("Tools/Import Space from JSON")]
    public static void ShowWindow()
    {
        GetWindow<SpaceImporter>("Space Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Space JSON", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Space JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Space JSON", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonPath = path;
                string json = File.ReadAllText(jsonPath);
                previewSpace = SpaceLoader.LoadSpaceFromJson(json);
            }
        }

        if (!string.IsNullOrEmpty(jsonPath))
        {
            GUILayout.Space(10);
            GUILayout.Label($"Selected: {Path.GetFileName(jsonPath)}");

            if (previewSpace != null)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Space Metadata", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name", previewSpace.Name);
                EditorGUILayout.LabelField("Author", previewSpace.Author);
                EditorGUILayout.LabelField("Description", previewSpace.Description, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Content Rating", previewSpace.ContentRating.ToString());
                EditorGUILayout.LabelField("Language", previewSpace.PrimaryLanguage.ToString());
                EditorGUILayout.Toggle("Adult Content", previewSpace.AdultContent);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Import"))
            {
                _ = ImportSpaceAsync(jsonPath);
            }
        }
    }

    private async Task ImportSpaceAsync(string path)
    {
        try
        {
            var space = SpaceLoader.LoadSpaceFromFile(path);
            var preloaded = await SpaceLoader.PreloadSpaceAssetsAsync(space);
            SpaceLoader.LoadSpaceFromData(space, preloaded);
            Debug.Log("✅ Space imported successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to import space: {ex.Message}");
        }
    }


}
