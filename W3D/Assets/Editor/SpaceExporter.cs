using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityGLTF;

public class SpaceExporter : EditorWindow
{
    private bool skyboxFoldout = false;
    private string exportFileName = "Space.json";
    private ExportedSpaceData spaceDataComponent;

    [MenuItem("Tools/Export Space to JSON")]
    public static void ShowWindow()
    {
        GetWindow<SpaceExporter>("Space Exporter");
    }

    private void OnEnable()
    {
        TryFindSpaceData();
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Current Space", EditorStyles.boldLabel);
        exportFileName = EditorGUILayout.TextField("File Name", exportFileName);

        if (spaceDataComponent == null)
        {
            EditorGUILayout.HelpBox("No Root object with ExportedSpaceData found in the Space.", MessageType.Warning);
            if (GUILayout.Button("Retry"))
                TryFindSpaceData();

            return;
        }

        SerializedObject so = new SerializedObject(spaceDataComponent);

        GUILayout.Space(10);
        GUILayout.Label("Space Metadata", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("Space.Name"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.Author"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.Description"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.WebModelLocation"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.BaseModelPath"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.AdultContent"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.ContentRating"));
        EditorGUILayout.PropertyField(so.FindProperty("Space.PrimaryLanguage"));

        // Skybox foldout
        EditorGUILayout.Space();
        skyboxFoldout = EditorGUILayout.Foldout(skyboxFoldout, "Skybox Settings", true);
        if (skyboxFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("Space.SkyboxImagePath"), new GUIContent("Skybox URL or Path"));
            EditorGUI.indentLevel--;
        }

        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        if (GUILayout.Button("Export Space"))
        {
            ExportSpaceToJson(exportFileName);
        }
    }

    private void TryFindSpaceData()
    {
        var root = GameObject.Find("Root");
        if (root != null)
        {
            spaceDataComponent = root.GetComponent<ExportedSpaceData>();
        }
    }

    private void ExportSpaceToJson(string filename)
    {
        if (spaceDataComponent == null)
        {
            Debug.LogError("❌ Root object with ExportedSpaceData not found.");
            return;
        }

        var space = spaceDataComponent.Space;
        space.objects = new List<ExportedObject>();

        var root = GameObject.Find("Root");
        if (root == null)
        {
            Debug.LogError("❌ Root GameObject not found.");
            return;
        }

        Dictionary<Transform, string> idLookup = new();

        foreach (Transform child in root.transform)
        {
            ExportGameObjectRecursive(child, space, idLookup, null);
        }

        string json = JsonUtility.ToJson(space, true);
        string path = EditorUtility.SaveFilePanel("Export Space JSON", "", filename, "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"✅ Space exported to {path}");
        }
    }

    private void ExportGameObjectRecursive(
        Transform transform,
        ExportedSpace space,
        Dictionary<Transform, string> idLookup,
        string parentId)
    {
        GameObject go = transform.gameObject;
        if (go.GetComponent<NoExport>() != null)
            return;

        //if (IsChildOfGLTFRootWithoutBeingRoot(go.transform))
        //    return;
        if (IsAutoInstantiatedGLTFChild(go))
            return;

        string id = System.Guid.NewGuid().ToString();
        idLookup[transform] = id;

        var obj = new ExportedObject
        {
            id = id,
            parentId = parentId,
            name = SpaceLoader.CleanName(go.name),
            position = transform.localPosition,
            rotation = transform.localEulerAngles,
            scale = transform.localScale,
            scripts = new List<string>(),
            components = new List<ExportedComponent>(),
            materials = new List<string>()
        };

        // Identify primitive
        if (go.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
        {
            string meshName = meshFilter.sharedMesh.name.ToLower();
            if (meshName.Contains("cube")) obj.primitiveType = "Cube";
            else if (meshName.Contains("sphere")) obj.primitiveType = "Sphere";
            else if (meshName.Contains("capsule")) obj.primitiveType = "Capsule";
            else if (meshName.Contains("cylinder")) obj.primitiveType = "Cylinder";
            else if (meshName.Contains("plane")) obj.primitiveType = "Plane";
            else if (meshName.Contains("quad")) obj.primitiveType = "Quad";
        }

        // ModelReference overrides
        var modelRef = go.GetComponent<ModelReference>();
        if (modelRef != null)
        {
            obj.overrideFilePath = modelRef.overrideFilePath;
            obj.overrideRemoteURL = modelRef.overrideRemoteURL;
        }

        var portal = go.GetComponent<PortalLink>();
        if (portal != null)
        {
            obj.components.Add(new ExportedComponent
            {
                type = nameof(PortalLink),
                properties = new List<ComponentProperty>
        {
            new ComponentProperty { key = "destinationUrl", value = portal.destinationUrl }
        }
            });
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

        space.objects.Add(obj);

        // ✅ Recurse only if this object is NOT a GLTF root
        if (go.GetComponent<InstantiatedGLTFObject>() == null)
        {
            foreach (Transform child in transform)
            {
                ExportGameObjectRecursive(child, space, idLookup, id);
            }
        }
    }




    private bool IsAutoInstantiatedGLTFChild(GameObject go)
    {
        // This object was not explicitly imported, it's just a child of a GLTF root
        return go.transform.parent != null &&
               go.transform.parent.GetComponent<InstantiatedGLTFObject>() != null &&
               go.GetComponent<InstantiatedGLTFObject>() == null;
    }
    private bool IsChildOfGLTFRootWithoutBeingRoot(Transform t)
    {
        Transform current = t.parent;
        while (current != null)
        {
            if (current.GetComponent<InstantiatedGLTFObject>() != null)
            {
                return t.GetComponent<InstantiatedGLTFObject>() == null;
            }
            current = current.parent;
        }
        return false;
    }

}
