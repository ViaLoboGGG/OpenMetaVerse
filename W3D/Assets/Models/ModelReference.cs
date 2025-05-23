using UnityEngine;

[DisallowMultipleComponent]
public class ModelReference : MonoBehaviour
{
    [Tooltip("Local file path to load this model from. Example: C:/Assets/MyModel.glb")]
    public string overrideFilePath;

    [Tooltip("Remote URL to fetch this model from. Example: https://cdn.mysite.com/models/castle.glb")]
    public string overrideRemoteURL;
}