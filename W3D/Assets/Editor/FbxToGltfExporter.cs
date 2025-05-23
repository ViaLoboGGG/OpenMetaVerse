// Assets/Editor/FbxToGltfExporter.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityGLTF;

public class FbxToGltfExporter : EditorWindow
{
    private string sourceFolder = "Assets/Models/FBX";
    private string outputFolder = "Assets/ExportedGLTF";

    [MenuItem("Tools/FBX to glTF Exporter")]
    public static void ShowWindow()
    {
        GetWindow<FbxToGltfExporter>("FBX to glTF Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX to glTF Export Tool", EditorStyles.boldLabel);

        sourceFolder = EditorGUILayout.TextField("Source Folder", sourceFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Export All FBX to glTF"))
        {
            ExportAllFbx();
        }
    }

    private void ExportAllFbx()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { sourceFolder });

        int exported = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string outputPath = Path.Combine(outputFolder, filename);

                ExportToGltf(fbx, outputPath);
                exported++;
            }
        }

        Debug.Log($"✅ Exported {exported} FBX file(s) to glTF.");
    }

    private void ExportToGltf(GameObject fbxPrefab, string outputPath)
    {
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);

        // 🔧 Rename shared materials to avoid collisions (e.g., "Material.001")
        var renderers = instance.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    mats[i].name = $"{fbxPrefab.name}_Mat_{i}";
                }
            }
        }

        var context = new ExportContext();

        var exporter = new GLTFSceneExporter(new[] { instance.transform }, context);
        exporter.SaveGLTFandBin(outputPath, Path.GetFileName(outputPath));

        DestroyImmediate(instance);
    }
}
