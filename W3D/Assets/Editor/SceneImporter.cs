// Editor/SceneImporter.cs
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class SceneImporter : EditorWindow
{
    private string jsonPath;

    [MenuItem("Tools/Import Scene from JSON")]
    public static void ShowWindow()
    {
        GetWindow<SceneImporter>("Scene Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Scene JSON", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Scene JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Import Scene JSON", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                jsonPath = path;
            }
        }

        if (!string.IsNullOrEmpty(jsonPath))
        {
            GUILayout.Label($"Selected: {Path.GetFileName(jsonPath)}");
            if (GUILayout.Button("Import"))
            {
                ImportScene(jsonPath);
            }
        }
    }

    private void ImportScene(string path)
    {
        Dictionary<string, GameObject> createdObjects = new();
        string json = File.ReadAllText(path);
        ExportedScene scene = JsonUtility.FromJson<ExportedScene>(json);
        GameObject root = new GameObject("ImportedScene_" + Path.GetFileNameWithoutExtension(path));

        string baseModelPath = scene.baseModelPath ?? "";

        foreach (var obj in scene.objects)
        {
            GameObject go = null;

            // 1. Try loading from Resources
            if (!string.IsNullOrEmpty(obj.modelPath))
            {
                string resourcePath = Path.Combine("Models", obj.modelPath).Replace("\\", "/");
                GameObject prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab != null)
                {
                    go = GameObject.Instantiate(prefab);
                    Debug.Log($"✅ Loaded model from Resources: {resourcePath}");
                }
                else
                {
                    // 2. Try local disk
                    string localFullPath = Path.Combine(baseModelPath, obj.modelPath);
                    if (File.Exists(localFullPath))
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = $"[LOCAL MODEL] {obj.modelPath}";
                        Debug.Log($"🟡 Placeholder for model on disk: {localFullPath}");
                    }
                    // 3. Remote URL (placeholder only)
                    else if (Uri.IsWellFormedUriString(obj.modelPath, UriKind.Absolute))
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = $"[REMOTE MODEL] {obj.modelPath}";
                        Debug.Log($"🌐 Placeholder for remote model: {obj.modelPath}");
                    }
                }
            }

            // Fallback to primitive
            if (go == null && !string.IsNullOrEmpty(obj.primitiveType))
            {
                if (Enum.TryParse(obj.primitiveType, out PrimitiveType primitive))
                {
                    go = GameObject.CreatePrimitive(primitive);
                }
            }

            if (go == null)
            {
                go = new GameObject(obj.name);
                Debug.LogWarning($"❌ Failed to instantiate model or primitive: {obj.name}");
            }

            go.name = obj.name;
            createdObjects[obj.id] = go;

            go.transform.localPosition = obj.position;
            go.transform.localEulerAngles = obj.rotation;
            go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;

            // Apply materials
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                List<Material> loadedMats = new List<Material>();
                if (obj.materials != null)
                {
                    foreach (var matName in obj.materials)
                    {
                        var mat = Resources.Load<Material>("Materials/" + matName);
                        if (mat != null)
                            loadedMats.Add(mat);
                        else
                            Debug.LogWarning($"Material '{matName}' not found.");
                    }
                }

                if (loadedMats.Count == 0)
                {
                    var fallback = Resources.Load<Material>("DefaultMaterial") ??
                                   new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    fallback.name = "RuntimeDefaultMaterial";
                    fallback.color = Color.gray;
                    loadedMats.Add(fallback);
                }

                renderer.sharedMaterials = loadedMats.ToArray();
            }

            // Add components
            foreach (var comp in obj.components)
                AddComponentFromData(go, comp);

            foreach (var script in obj.scripts)
                Debug.Log($"TODO: Attach whitelisted script: {script}");

            go.transform.SetParent(root.transform, false);
        }

        // Set parent hierarchy
        foreach (var obj in scene.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                createdObjects.TryGetValue(obj.id, out GameObject child) &&
                createdObjects.TryGetValue(obj.parentId, out GameObject parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }

        Debug.Log("Scene imported successfully.");
    }

    private void AddComponentFromData(GameObject go, ExportedComponent comp)
    {
        var props = comp.properties;

        if (comp.type.EndsWith("Collider") && go.GetComponent<Collider>() != null)
            return;

        switch (comp.type)
        {
            case "Rigidbody":
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = ParseFloat(props, "mass");
                rb.linearDamping = ParseFloat(props, "drag");
                rb.angularDamping = ParseFloat(props, "angularDrag");
                rb.useGravity = ParseBool(props, "useGravity");
                rb.isKinematic = ParseBool(props, "isKinematic");
                break;

            case "BoxCollider":
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = ParseBool(props, "isTrigger");
                box.center = ParseVector3(props, "center");
                box.size = ParseVector3(props, "size");
                break;

            case "SphereCollider":
                var sphere = go.AddComponent<SphereCollider>();
                sphere.isTrigger = ParseBool(props, "isTrigger");
                sphere.center = ParseVector3(props, "center");
                sphere.radius = ParseFloat(props, "radius");
                break;

            case "CapsuleCollider":
                var capsule = go.AddComponent<CapsuleCollider>();
                capsule.isTrigger = ParseBool(props, "isTrigger");
                capsule.center = ParseVector3(props, "center");
                capsule.radius = ParseFloat(props, "radius");
                capsule.height = ParseFloat(props, "height");
                capsule.direction = ParseInt(props, "direction");
                break;

            case "MeshCollider":
                var mesh = go.AddComponent<MeshCollider>();
                mesh.isTrigger = ParseBool(props, "isTrigger");
                mesh.convex = ParseBool(props, "convex");
                break;
        }
    }

    private string GetProperty(List<ComponentProperty> props, string key)
    {
        foreach (var prop in props)
            if (prop.key == key)
                return prop.value;
        return null;
    }

    private float ParseFloat(List<ComponentProperty> props, string key)
    {
        var str = GetProperty(props, key);
        return float.TryParse(str, out var f) ? f : 0f;
    }

    private bool ParseBool(List<ComponentProperty> props, string key)
    {
        var str = GetProperty(props, key);
        return bool.TryParse(str, out var b) && b;
    }

    private int ParseInt(List<ComponentProperty> props, string key)
    {
        var str = GetProperty(props, key);
        return int.TryParse(str, out var i) ? i : 0;
    }

    private Vector3 ParseVector3(List<ComponentProperty> props, string key)
    {
        var str = GetProperty(props, key);
        if (string.IsNullOrEmpty(str)) return Vector3.zero;

        str = str.Trim('(', ')');
        var parts = str.Split(',');
        if (parts.Length == 3 &&
            float.TryParse(parts[0], out var x) &&
            float.TryParse(parts[1], out var y) &&
            float.TryParse(parts[2], out var z))
        {
            return new Vector3(x, y, z);
        }
        return Vector3.zero;
    }
}
