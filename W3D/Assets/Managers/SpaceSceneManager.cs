using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

public class SpaceSceneManager : MonoBehaviour
{
    public string initialSpacePath;

    private async void Start()
    {
        if (!string.IsNullOrEmpty(initialSpacePath))
        {
            await LoadSpace(initialSpacePath);
        }
    }

    public async Task LoadSpace(string pathOrUrl)
    {
        string json = null;

        if (File.Exists(pathOrUrl))
        {
            json = File.ReadAllText(pathOrUrl);
        }
        else if (Uri.IsWellFormedUriString(pathOrUrl, UriKind.Absolute))
        {
            using var www = new UnityWebRequest(pathOrUrl, "GET");
            www.downloadHandler = new DownloadHandlerBuffer();
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                json = www.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"❌ Failed to download space JSON: {www.error}");
                return;
            }
        }
        else
        {
            Debug.LogError("❌ Invalid path or URL to space: " + pathOrUrl);
            return;
        }

        if (string.IsNullOrEmpty(json))
            return;

        var space = SpaceLoader.LoadSpaceFromJson(json);
        var root = new GameObject("Root");
        var holder = root.AddComponent<ExportedSpaceData>();
        holder.Space = space;

        var idMap = new Dictionary<string, GameObject>();

        // Step 1: Instantiate all GameObjects
        foreach (var obj in space.objects)
        {
            var go = SpaceObjectFactory.Instantiate(obj, space);
            idMap[obj.id] = go;
            go.transform.SetParent(root.transform, false);
        }

        // Step 2: Preload content (e.g., glTFs)
        foreach (var obj in space.objects)
        {
            if (idMap.TryGetValue(obj.id, out var go))
            {
                await SpaceObjectFactory.PreloadAsync(obj, space);
            }
        }

        // Step 3: Restore hierarchy
        foreach (var obj in space.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                idMap.TryGetValue(obj.id, out var child) &&
                idMap.TryGetValue(obj.parentId, out var parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }

        Debug.Log("✅ Space loaded at runtime.");
    }

}