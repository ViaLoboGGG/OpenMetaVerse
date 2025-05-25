using System.Collections.Generic;
using UnityEngine;

public static class SpaceImporterRuntime
{
    public static void InstantiateSpace(ExportedSpace space, Transform root)
    {
        Dictionary<string, GameObject> created = new();

        foreach (var obj in space.objects)
        {
            GameObject go = SpaceObjectFactory.Instantiate(obj, space);
            if (go == null) continue;

            go.name = obj.name;
            created[obj.id] = go;

            go.transform.localPosition = obj.position;
            go.transform.localEulerAngles = obj.rotation;
            go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;
            go.transform.SetParent(root, false);
        }

        // Rebuild hierarchy
        foreach (var obj in space.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                created.TryGetValue(obj.id, out GameObject child) &&
                created.TryGetValue(obj.parentId, out GameObject parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }
    }
}
