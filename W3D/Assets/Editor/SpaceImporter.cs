using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityGLTF.Loader;
using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;

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
                _ = ImportSpaceAsync(jsonPath);
            }
        }
    }

    private async Task ImportSpaceAsync(string path)
    {
        Dictionary<string, GameObject> createdObjects = new();
        string json = File.ReadAllText(path);
        ExportedSpace space = JsonUtility.FromJson<ExportedSpace>(json);

        var existingRoot = GameObject.Find("Root");
        if (existingRoot != null)
            GameObject.DestroyImmediate(existingRoot);

        GameObject root = new GameObject("Root");
        var holder = root.AddComponent<ExportedSpaceData>();
        holder.Space = space;

        foreach (var obj in space.objects)
        {
            GameObject go = null;

            // Load from glTF if specified or fallback to primitives
            string gltfPath = ResolveModelPath(space, obj);
            if (!string.IsNullOrEmpty(gltfPath))
            {
                Debug.Log("Attmpting to load gltf");
                if (File.Exists(gltfPath))
                {
                    go = await LoadGltfFromDisk(gltfPath);
                }
                else if (Uri.IsWellFormedUriString(gltfPath, UriKind.Absolute))
                {
                    go = await LoadGltfFromUrl(gltfPath);
                }
            }

            // Fallback to primitive
            if (go == null && !string.IsNullOrEmpty(obj.primitiveType))
            {
                if (Enum.TryParse(obj.primitiveType, out PrimitiveType primitive))
                {
                    go = GameObject.CreatePrimitive(primitive);
                    Debug.Log($"🧱 Primitive created: {primitive}");
                }
            }

            if (go == null)
            {
                go = new GameObject(obj.name);
                Debug.LogWarning($"❌ Could not resolve model or primitive for: {obj.name}");
            }

            go.name = obj.name;
            createdObjects[obj.id] = go;
            go.transform.localPosition = obj.position;
            go.transform.localEulerAngles = obj.rotation;
            go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;

            // Materials
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mats = new List<Material>();
                if (obj.materials != null)
                {
                    foreach (var matName in obj.materials)
                    {
                        var mat = Resources.Load<Material>("Materials/" + matName);
                        if (mat != null) mats.Add(mat);
                        else Debug.LogWarning($"Material not found: {matName}");
                    }
                }

                if (mats.Count == 0)
                {
                    var fallback = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    fallback.name = "RuntimeDefaultMaterial";
                    fallback.color = Color.gray;
                    mats.Add(fallback);
                }

                renderer.sharedMaterials = mats.ToArray();
            }

            foreach (var comp in obj.components)
                AddComponentFromData(go, comp);

            foreach (var script in obj.scripts)
                Debug.Log($"TODO: Attach whitelisted script: {script}");

            go.transform.SetParent(root.transform, false);
        }

        // Restore hierarchy
        foreach (var obj in space.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                createdObjects.TryGetValue(obj.id, out GameObject child) &&
                createdObjects.TryGetValue(obj.parentId, out GameObject parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }

        Debug.Log("✅ Space imported with glTF support.");
    }

    private string ResolveModelPath(ExportedSpace space, ExportedObject obj)
    {
        if (!string.IsNullOrEmpty(obj.overrideFilePath) && File.Exists(obj.overrideFilePath))
            return obj.overrideFilePath;

        if (!string.IsNullOrEmpty(obj.overrideRemoteURL))
            return obj.overrideRemoteURL;

        if (!string.IsNullOrEmpty(space.BaseModelPath))
        {
            string path = Path.Combine(space.BaseModelPath, obj.name + ".gltf");
            if (File.Exists(path)) return path;
        }

        if (!string.IsNullOrEmpty(space.WebModelLocation))
        {
            return $"{space.WebModelLocation.TrimEnd('/')}/{obj.name}.gltf";
        }

        return null;
    }

    private async Task<GameObject> LoadGltfFromDisk(string fullPath)
    {
        Debug.Log($"Loading {fullPath} from disk.");
        string directory = Path.GetDirectoryName(fullPath).Replace("\\", "/");
        string fileName = Path.GetFileName(fullPath);

        var loader = new FileLoader(directory);
        var importOptions = new ImportOptions
        {
            DataLoader = loader,
            AsyncCoroutineHelper = new AsyncCoroutineHelper()
        };
        var importer = new GLTFSceneImporter(fileName, importOptions);


        await importer.LoadSceneAsync(-1, true);
        GameObject gltfRoot = importer.LastLoadedScene;
        gltfRoot.name = fileName;
        return gltfRoot;
    }

    private async Task<GameObject> LoadGltfFromUrl(string url)
    {
        Debug.Log($"Loading gltf from {url}.");
        string directory = url.Substring(0, url.LastIndexOf('/') + 1);
        string fileName = Path.GetFileName(url);

        var loader = new FileLoader(directory);
        var importOptions = new ImportOptions
        {
            DataLoader = loader,
            AsyncCoroutineHelper = new AsyncCoroutineHelper()
        };
        var importer = new GLTFSceneImporter(fileName, importOptions);

        await importer.LoadSceneAsync(-1, true);
        GameObject gltfRoot = importer.LastLoadedScene;
        gltfRoot.name = fileName;
        return gltfRoot;
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
            if (prop.key == key) return prop.value;
        return null;
    }

    private float ParseFloat(List<ComponentProperty> props, string key)
    {
        return float.TryParse(GetProperty(props, key), out var f) ? f : 0f;
    }

    private bool ParseBool(List<ComponentProperty> props, string key)
    {
        return bool.TryParse(GetProperty(props, key), out var b) && b;
    }

    private int ParseInt(List<ComponentProperty> props, string key)
    {
        return int.TryParse(GetProperty(props, key), out var i) ? i : 0;
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
