// Assets/Editor/SceneExportSettings.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SceneExportSettings", menuName = "Export/Scene Export Settings")]
public class SceneExportSettings : ScriptableObject
{
    public string Name;
    public string Description;
    public string Author;
    public string WebModelLocation;
    public string BaseModelPath = "Resources/Models";
    public bool AdultContent;
    public GameContentRating ContentRating = GameContentRating.E_Everyone;
    public Language PrimaryLanguage = Language.English;
}
