// Editor/SceneExporter.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class SceneExporter : EditorWindow
{
    private string exportFileName = "scene.json";
    private string baseModelPath = "Resources/Models"; // new base path field

    [MenuItem("Tools/Export Scene to JSON")]
    public static void ShowWindow()
    {
        GetWindow<SceneExporter>("Scene Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Current Scene", EditorStyles.boldLabel);
        exportFileName = EditorGUILayout.TextField("File Name", exportFileName);
        baseModelPath = EditorGUILayout.TextField("Base Model Path", baseModelPath);

        if (GUILayout.Button("Export Scene"))
        {
            ExportSceneToJson(exportFileName);
        }
    }

    private void ExportSceneToJson(string filename)
    {
        var sceneData = new ExportedScene
        {
            objects = new List<ExportedObject>(),
            baseModelPath = baseModelPath
        };

        Dictionary<Transform, string> idLookup = new();

        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            ExportGameObjectRecursive(root.transform, sceneData, idLookup, null);
        }

        string json = JsonUtility.ToJson(sceneData, true);
        string path = EditorUtility.SaveFilePanel("Export Scene JSON", "", filename, "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"Scene exported to {path}");
        }
    }

    private void ExportGameObjectRecursive(
        Transform transform,
        ExportedScene sceneData,
        Dictionary<Transform, string> idLookup,
        string parentId)
    {
        GameObject go = transform.gameObject;
        if (go.GetComponent<NoExport>() != null)
            return;

        string id = System.Guid.NewGuid().ToString();
        idLookup[transform] = id;

        var obj = new ExportedObject
        {
            id = id,
            parentId = parentId,
            name = go.name,
            position = transform.localPosition,
            rotation = transform.localEulerAngles,
            scale = transform.localScale,
            scripts = new List<string>(),
            components = new List<ExportedComponent>(),
            materials = new List<string>()
        };

        // Primitive type detection
        if (go.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
        {
            var meshName = meshFilter.sharedMesh.name.ToLower();
            if (meshName.Contains("cube")) obj.primitiveType = "Cube";
            else if (meshName.Contains("sphere")) obj.primitiveType = "Sphere";
            else if (meshName.Contains("capsule")) obj.primitiveType = "Capsule";
            else if (meshName.Contains("cylinder")) obj.primitiveType = "Cylinder";
            else if (meshName.Contains("plane")) obj.primitiveType = "Plane";
            else if (meshName.Contains("quad")) obj.primitiveType = "Quad";
        }

        // Detect model path if prefab
        var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(go);
        if (prefabSource != null)
        {
            var path = AssetDatabase.GetAssetPath(prefabSource);
            if (path.Contains("/Resources/Models/"))
            {
                string relative = path.Substring(path.IndexOf("Resources/Models/") + "Resources/Models/".Length);
                relative = Path.ChangeExtension(relative, null);
                obj.modelPath = relative;
            }
            else
            {
                obj.modelPath = Path.GetFileNameWithoutExtension(path);
            }
        }

        // Materials
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat != null)
                    obj.materials.Add(mat.name);
            }
        }

        // Rigidbody
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            obj.components.Add(new ExportedComponent
            {
                type = "Rigidbody",
                properties = new List<ComponentProperty>
                {
                    new ComponentProperty { key = "mass", value = rb.mass.ToString() },
                    new ComponentProperty { key = "drag", value = rb.linearDamping.ToString() },
                    new ComponentProperty { key = "angularDrag", value = rb.angularDamping.ToString() },
                    new ComponentProperty { key = "useGravity", value = rb.useGravity.ToString() },
                    new ComponentProperty { key = "isKinematic", value = rb.isKinematic.ToString() }
                }
            });
        }

        // Collider
        var col = go.GetComponent<Collider>();
        if (col != null)
        {
            var props = new List<ComponentProperty>
            {
                new ComponentProperty { key = "isTrigger", value = col.isTrigger.ToString() }
            };

            if (col is BoxCollider box)
            {
                props.Add(new ComponentProperty { key = "center", value = box.center.ToString("F3") });
                props.Add(new ComponentProperty { key = "size", value = box.size.ToString("F3") });
            }
            else if (col is SphereCollider sphere)
            {
                props.Add(new ComponentProperty { key = "center", value = sphere.center.ToString("F3") });
                props.Add(new ComponentProperty { key = "radius", value = sphere.radius.ToString() });
            }
            else if (col is CapsuleCollider capsule)
            {
                props.Add(new ComponentProperty { key = "center", value = capsule.center.ToString("F3") });
                props.Add(new ComponentProperty { key = "radius", value = capsule.radius.ToString() });
                props.Add(new ComponentProperty { key = "height", value = capsule.height.ToString() });
                props.Add(new ComponentProperty { key = "direction", value = capsule.direction.ToString() });
            }
            else if (col is MeshCollider mesh)
            {
                props.Add(new ComponentProperty { key = "convex", value = mesh.convex.ToString() });
            }

            obj.components.Add(new ExportedComponent { type = col.GetType().Name, properties = props });
        }

        sceneData.objects.Add(obj);

        foreach (Transform child in transform)
        {
            ExportGameObjectRecursive(child, sceneData, idLookup, id);
        }
    }
}