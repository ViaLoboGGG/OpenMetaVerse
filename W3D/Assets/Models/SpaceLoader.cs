using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityGLTF;
using UnityGLTF.Loader;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

public static class SpaceLoader
{
    public static GameObject LoadSpaceFromData(ExportedSpace spaceData, Dictionary<string, GameObject> preloaded)
    {
        var existingRoot = GameObject.Find("Root");
        if (existingRoot != null)
            GameObject.DestroyImmediate(existingRoot);

        GameObject root = new GameObject("Root");
        var holder = root.AddComponent<ExportedSpaceData>();
        holder.Space = spaceData;

        // Fire SkyboxLoadStartedEvent
        EventBus<SkyboxLoadStartedEvent>.Raise(new SkyboxLoadStartedEvent { PathOrUrl = spaceData.SkyboxImagePath });
        LoadSkyboxAsync(spaceData.SkyboxImagePath);

        // First, assign transforms and parent non-childed objects to root
        foreach (var obj in spaceData.objects)
        {
            if (preloaded.TryGetValue(obj.id, out var go))
            {
                go.transform.localPosition = obj.position;
                go.transform.localEulerAngles = obj.rotation;
                go.transform.localScale = obj.scale != Vector3.zero ? obj.scale : Vector3.one;

                if (string.IsNullOrEmpty(obj.parentId))
                {
                    go.transform.SetParent(root.transform, false);
                }
            }
        }

        // Then assign parent-child relationships
        foreach (var obj in spaceData.objects)
        {
            if (!string.IsNullOrEmpty(obj.parentId) &&
                preloaded.TryGetValue(obj.id, out var child) &&
                preloaded.TryGetValue(obj.parentId, out var parent))
            {
                child.transform.SetParent(parent.transform, false);
            }
        }

        // Fire SpaceLoadCompletedEvent
        EventBus<SpaceLoadCompletedEvent>.Raise(new SpaceLoadCompletedEvent { Space = spaceData, Root = root });

        return root;
    }

    public static async Task LoadSkyboxAsync(string pathOrUrl)
    {
        if (string.IsNullOrEmpty(pathOrUrl)) return;

        Texture2D tex = null;

        // Fire SkyboxLoadStartedEvent
        EventBus<SkyboxLoadStartedEvent>.Raise(new SkyboxLoadStartedEvent { PathOrUrl = pathOrUrl });

        if (File.Exists(pathOrUrl)) // Load from local file system
        {
            try
            {
                byte[] data = File.ReadAllBytes(pathOrUrl);
                tex = new Texture2D(2, 2);
                tex.LoadImage(data);
            }
            catch (Exception ex)
            {
                EventBus<SkyboxLoadFailedEvent>.Raise(new SkyboxLoadFailedEvent { PathOrUrl = pathOrUrl, ErrorMessage = ex.Message });
                Debug.LogError($"❌ Failed to load skybox from file: {ex.Message}");
                return;
            }
        }
        else if (Uri.IsWellFormedUriString(pathOrUrl, UriKind.Absolute)) // Load from URL
        {
            using UnityWebRequest www = UnityWebRequestTexture.GetTexture(pathOrUrl);
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success)
                tex = DownloadHandlerTexture.GetContent(www);
            else
            {
                EventBus<SkyboxLoadFailedEvent>.Raise(new SkyboxLoadFailedEvent { PathOrUrl = pathOrUrl, ErrorMessage = www.error });
                Debug.LogError($"❌ Failed to load skybox from URL: {www.error}");
                return;
            }
        }

        if (tex == null)
        {
            EventBus<SkyboxLoadFailedEvent>.Raise(new SkyboxLoadFailedEvent { PathOrUrl = pathOrUrl, ErrorMessage = "Skybox texture was null." });
            Debug.LogWarning("⚠️ Skybox texture was null.");
            return;
        }

        var skyboxMaterial = new Material(Shader.Find("Skybox/Panoramic"));
        skyboxMaterial.SetTexture("_MainTex", tex);
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment(); // Optional lighting refresh

        // Fire SkyboxLoadCompletedEvent
        EventBus<SkyboxLoadCompletedEvent>.Raise(new SkyboxLoadCompletedEvent { PathOrUrl = pathOrUrl, Texture = tex });
    }

    public static ExportedSpace LoadSpaceFromJson(string json)
    {
        return JsonUtility.FromJson<ExportedSpace>(json);
    }

    public static ExportedSpace LoadSpaceFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Space JSON file not found", filePath);

        // Fire SpaceLoadStartedEvent
        EventBus<SpaceLoadStartedEvent>.Raise(new SpaceLoadStartedEvent { PathOrUrl = filePath });

        var space = LoadSpaceFromJson(File.ReadAllText(filePath));

        // Fire SpaceLoadCompletedEvent (Root is null here, as not loaded yet)
        EventBus<SpaceLoadCompletedEvent>.Raise(new SpaceLoadCompletedEvent { Space = space, Root = null });

        return space;
    }

    public static string ResolveModelPath(ExportedSpace space, ExportedObject obj)
    {
        if (!string.IsNullOrEmpty(obj.overrideFilePath) && File.Exists(obj.overrideFilePath))
            return obj.overrideFilePath;

        if (!string.IsNullOrEmpty(obj.overrideRemoteURL))
            return obj.overrideRemoteURL;

        string baseName = CleanName(obj.name);
        if (!string.IsNullOrEmpty(space.BaseModelPath))
        {
            string gltf = Path.Combine(space.BaseModelPath, baseName + ".gltf");
            string glb = Path.Combine(space.BaseModelPath, baseName + ".glb");
            if (File.Exists(gltf)) return gltf;
            if (File.Exists(glb)) return glb;
        }

        if (!string.IsNullOrEmpty(space.WebModelLocation))
        {
            return $"{space.WebModelLocation.TrimEnd('/')}/{baseName}.gltf";
        }

        return null;
    }

    public static string CleanName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        int lastParen = name.LastIndexOf(" (");
        if (lastParen > 0 && name.EndsWith(")"))
        {
            string suffix = name.Substring(lastParen + 2, name.Length - lastParen - 3);
            if (int.TryParse(suffix, out _))
            {
                return name.Substring(0, lastParen);
            }
        }
        return name;
    }

    public static async Task<GameObject> LoadGltfFromDisk(string fullPath)
    {
        Debug.Log($"Loading GLTF from disk: {fullPath}");

        // Fire AssetLoadStartedEvent
        EventBus<AssetLoadStartedEvent>.Raise(new AssetLoadStartedEvent { AssetId = fullPath, PathOrUrl = fullPath });

        string directory = Path.GetDirectoryName(fullPath).Replace("\\", "/");
        string fileName = Path.GetFileName(fullPath);

        var loader = new FileLoader(directory);
        var importOptions = new ImportOptions
        {
            DataLoader = loader,
            AsyncCoroutineHelper = new AsyncCoroutineHelper()
        };

        var importer = new GLTFSceneImporter(fileName, importOptions);
        try
        {
            await importer.LoadSceneAsync(-1, true);
            GameObject gltfRoot = importer.LastLoadedScene;
            gltfRoot.name = Path.GetFileNameWithoutExtension(fileName);

            // Fire AssetLoadCompletedEvent
            EventBus<AssetLoadCompletedEvent>.Raise(new AssetLoadCompletedEvent { AssetId = fullPath, LoadedObject = gltfRoot });

            return gltfRoot;
        }
        catch (Exception ex)
        {
            EventBus<AssetLoadFailedEvent>.Raise(new AssetLoadFailedEvent { AssetId = fullPath, PathOrUrl = fullPath, ErrorMessage = ex.Message });
            Debug.LogError($"❌ Failed to load GLTF from disk: {ex.Message}");
            return null;
        }
    }

    public static async Task<GameObject> LoadGltfFromUrl(string url)
    {
        Debug.Log($"Loading GLTF from URL: {url}");

        // Fire AssetLoadStartedEvent
        EventBus<AssetLoadStartedEvent>.Raise(new AssetLoadStartedEvent { AssetId = url, PathOrUrl = url });

        string directory = url.Substring(0, url.LastIndexOf('/') + 1);
        string fileName = Path.GetFileName(url);

        var loader = new FileLoader(directory);
        var importOptions = new ImportOptions
        {
            DataLoader = loader,
            AsyncCoroutineHelper = new AsyncCoroutineHelper()
        };

        var importer = new GLTFSceneImporter(fileName, importOptions);
        try
        {
            await importer.LoadSceneAsync(-1, true);
            GameObject gltfRoot = importer.LastLoadedScene;
            gltfRoot.name = Path.GetFileNameWithoutExtension(fileName);

            // Fire AssetLoadCompletedEvent
            EventBus<AssetLoadCompletedEvent>.Raise(new AssetLoadCompletedEvent { AssetId = url, LoadedObject = gltfRoot });

            return gltfRoot;
        }
        catch (Exception ex)
        {
            EventBus<AssetLoadFailedEvent>.Raise(new AssetLoadFailedEvent { AssetId = url, PathOrUrl = url, ErrorMessage = ex.Message });
            Debug.LogError($"❌ Failed to load GLTF from URL: {ex.Message}");
            return null;
        }
    }

    public static async Task<Dictionary<string, GameObject>> PreloadSpaceAssetsAsync(ExportedSpace space)
    {
        var preloaded = new Dictionary<string, GameObject>();

        foreach (var obj in space.objects)
        {
            string assetId = obj.id;
            string pathOrUrl = ResolveModelPath(space, obj);

            // Fire AssetLoadStartedEvent
            EventBus<AssetLoadStartedEvent>.Raise(new AssetLoadStartedEvent { AssetId = assetId, PathOrUrl = pathOrUrl });

            try
            {
                GameObject loaded = await SpaceObjectFactory.PreloadAsync(obj, space);
                if (loaded != null)
                {
                    preloaded[obj.id] = loaded;
                    // Fire AssetLoadCompletedEvent
                    EventBus<AssetLoadCompletedEvent>.Raise(new AssetLoadCompletedEvent { AssetId = assetId, LoadedObject = loaded });
                }
                else
                {
                    // Fire AssetLoadFailedEvent
                    EventBus<AssetLoadFailedEvent>.Raise(new AssetLoadFailedEvent { AssetId = assetId, PathOrUrl = pathOrUrl, ErrorMessage = "Loaded object was null." });
                }
            }
            catch (Exception ex)
            {
                EventBus<AssetLoadFailedEvent>.Raise(new AssetLoadFailedEvent { AssetId = assetId, PathOrUrl = pathOrUrl, ErrorMessage = ex.Message });
            }
        }

        return preloaded;
    }
}


public struct SpaceLoadStartedEvent
{
    public string PathOrUrl;
}

public struct SpaceLoadCompletedEvent
{
    public ExportedSpace Space;
    public GameObject Root;
}

public struct SpaceLoadFailedEvent
{
    public string PathOrUrl;
    public string ErrorMessage;
}

public struct AssetLoadStartedEvent
{
    public string AssetId;
    public string PathOrUrl;
}

public struct AssetLoadCompletedEvent
{
    public string AssetId;
    public GameObject LoadedObject;
}

public struct AssetLoadFailedEvent
{
    public string AssetId;
    public string PathOrUrl;
    public string ErrorMessage;
}

public struct SkyboxLoadStartedEvent
{
    public string PathOrUrl;
}

public struct SkyboxLoadCompletedEvent
{
    public string PathOrUrl;
    public Texture2D Texture;
}

public struct SkyboxLoadFailedEvent
{
    public string PathOrUrl;
    public string ErrorMessage;
}