using UnityEditor;
using UnityEngine;
using System.IO;
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
                previewSpace = JsonUtility.FromJson<ExportedSpace>(json);
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
                ImportSpace(jsonPath);
            }
        }
    }

    private void ImportSpace(string path)
    {
        Dictionary<string, GameObject> createdObjects = new();
        string json = File.ReadAllText(path);
        ExportedSpace Space = JsonUtility.FromJson<ExportedSpace>(json);

        // 🔄 Remove previous "Root" if it exists
        var existingRoot = GameObject.Find("Root");
        if (existingRoot != null)
            GameObject.DestroyImmediate(existingRoot);

        // 📦 Create new Root object
        GameObject root = new GameObject("Root");
        var holder = root.AddComponent<ExportedSpaceData>();
        holder.Space = Space;

        string baseModelPath = Space.BaseModelPath ?? "";

        // 🧱 Instantiate all Space objects
        foreach (var obj in Space.objects)
        {
            GameObject go = null;

            // 1️⃣ Override: Specific File Path
            if (!string.IsNullOrEmpty(obj.overrideFilePath) && File.Exists(obj.overrideFilePath))
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"[OVERRIDE FILE] {Path.GetFileName(obj.overrideFilePath)}";
                Debug.Log($"📁 Using overrideFilePath: {obj.overrideFilePath}");
            }
            // 2️⃣ Override: Remote URL
            else if (!string.IsNullOrEmpty(obj.overrideRemoteURL) && Uri.IsWellFormedUriString(obj.overrideRemoteURL, UriKind.Absolute))
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"[OVERRIDE URL] {Path.GetFileName(obj.overrideRemoteURL)}";
                Debug.Log($"🌐 Using overrideRemoteURL: {obj.overrideRemoteURL}");
            }
            // 3️⃣ Fallback to modelPath + modelSource
            else if (!string.IsNullOrEmpty(obj.modelPath))
            {
                switch (obj.modelSource)
                {
                    case ModelSourceType.Resources:
                        string resourcePath = Path.Combine("Models", obj.modelPath).Replace("\\", "/");
                        var prefab = Resources.Load<GameObject>(resourcePath);
                        if (prefab != null)
                        {
                            go = GameObject.Instantiate(prefab);
                            Debug.Log($"✅ Loaded prefab from Resources: {resourcePath}");
                        }
                        break;

                    case ModelSourceType.FileSystem:
                        string fullPath = Path.Combine(Space.BaseModelPath ?? "", obj.modelPath);
                        if (File.Exists(fullPath))
                        {
                            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.name = $"[FILE] {Path.GetFileName(obj.modelPath)}";
                            Debug.Log($"🟡 Placeholder for file: {fullPath}");
                        }
                        break;

                    case ModelSourceType.RemoteURL:
                        string remoteURL = $"{Space.WebModelLocation}/{obj.modelPath}".Replace("\\", "/");
                        if (Uri.IsWellFormedUriString(remoteURL, UriKind.Absolute))
                        {
                            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            go.name = $"[URL] {Path.GetFileName(obj.modelPath)}";
                            Debug.Log($"🌐 Placeholder for URL: {remoteURL}");
                        }
                        break;
                }
            }

            // 4️⃣ Fallback to primitive type
            if (go == null && !string.IsNullOrEmpty(obj.primitiveType))
            {
                if (Enum.TryParse(obj.primitiveType, out PrimitiveType primitive))
                {
                    go = GameObject.CreatePrimitive(primitive);
                    Debug.Log($"🧱 Created primitive: {primitive}");
                }
            }

            // 5️⃣ Final fallback
            if (go == null)
            {
                go = new GameObject(obj.name);
                Debug.LogWarning($"❌ Could not load model or primitive for: {obj.name}");
            }

            go.name = obj.name;
            createdObjects[obj.id] = go;

            go.transform.localPosition = obj.position;
            go.transform.localEulerAngles = obj.rotation;
            go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;

            // 🎨 Load materials
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                List<Material> loadedMats = new();
                if (obj.materials != null)
                {
                    foreach (var matName in obj.materials)
                    {
                        var mat = Resources.Load<Material>("Materials/" + matName);
                        if (mat != null)
                            loadedMats.Add(mat);
                        else
                            Debug.LogWarning($"Material '{matName}' not found in Resources.");
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

            // 🧩 Add components
            foreach (var comp in obj.components)
                AddComponentFromData(go, comp);

            foreach (var script in obj.scripts)
                Debug.Log($"TODO: Attach whitelisted script: {script}");

            go.transform.SetParent(root.transform, false);
        }

        // 🔗 Rebuild hierarchy
        foreach (var obj in Space.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                createdObjects.TryGetValue(obj.id, out GameObject child) &&
                createdObjects.TryGetValue(obj.parentId, out GameObject parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }

        Debug.Log("✅ Space imported successfully.");
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
