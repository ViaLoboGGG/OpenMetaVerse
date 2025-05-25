using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

public static class SpaceObjectFactory
{
    public static GameObject Instantiate(ExportedObject obj, ExportedSpace spaceData)
    {
        GameObject go = new GameObject(obj.name);

        // Flag if it's a GLTF source
        if (IsGLTFSource(obj, spaceData))
        {
            go.AddComponent<InstantiatedGLTFObject>();
        }
        else if (!string.IsNullOrEmpty(obj.primitiveType) && Enum.TryParse(obj.primitiveType, out PrimitiveType primitive))
        {
            GameObject primitiveGo = GameObject.CreatePrimitive(primitive);
            primitiveGo.name = obj.name;
            primitiveGo.transform.SetParent(go.transform, false);

            // Apply materials if available
            var renderer = primitiveGo.GetComponent<Renderer>();
            if (renderer != null && obj.materials != null && obj.materials.Count > 0)
            {
                var materials = new List<Material>();
                foreach (var matName in obj.materials)
                {
                    var mat = Resources.Load<Material>("Materials/" + matName);
                    if (mat != null)
                        materials.Add(mat);
                    else
                        Debug.LogWarning($"Material '{matName}' not found in Resources.");
                }

                if (materials.Count > 0)
                    renderer.sharedMaterials = materials.ToArray();
            }
        }

        go.transform.localPosition = obj.position;
        go.transform.localEulerAngles = obj.rotation;
        go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;

        return go;
    }


    public static async Task<GameObject> PreloadAsync(ExportedObject obj, ExportedSpace space)
    {
        GameObject go = Instantiate(obj, space);

        string path = SpaceLoader.ResolveModelPath(space, obj);
        if (!string.IsNullOrEmpty(path))
        {
            GameObject gltf = null;

            if (File.Exists(path))
            {
                gltf = await SpaceLoader.LoadGltfFromDisk(path);
            }
            else if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                gltf = await SpaceLoader.LoadGltfFromUrl(path);
            }

            if (gltf != null)
            {
                gltf.transform.SetParent(go.transform, false);

                // Add MeshCollider to children that have MeshFilters but no collider
                foreach (var mf in gltf.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.sharedMesh != null && mf.GetComponent<Collider>() == null)
                    {
                        mf.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }
        }
        else
        {
            // Only apply materials if this is a primitive (not a GLTF root with children)
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && obj.materials != null && obj.materials.Count > 0)
            {
                var loadedMats = new List<Material>();
                foreach (var matName in obj.materials)
                {
                    var mat = Resources.Load<Material>("Materials/" + matName);
                    if (mat != null)
                        loadedMats.Add(mat);
                    else
                        Debug.LogWarning($"Material not found: {matName}");
                }

                if (loadedMats.Count > 0)
                    renderer.sharedMaterials = loadedMats.ToArray();
            }
        }

        return go;
    }






    private static bool IsGLTFSource(ExportedObject obj, ExportedSpace space)
    {
        return !string.IsNullOrEmpty(obj.overrideFilePath)
            || !string.IsNullOrEmpty(obj.overrideRemoteURL)
            || (!string.IsNullOrEmpty(space.BaseModelPath) &&
                (File.Exists(Path.Combine(space.BaseModelPath, obj.name + ".gltf")) ||
                 File.Exists(Path.Combine(space.BaseModelPath, obj.name + ".glb"))))
            || (!string.IsNullOrEmpty(space.WebModelLocation));
    }
}
