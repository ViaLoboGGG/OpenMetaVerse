// Models/ExportedSpace.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExportedSpace
{
    public List<ExportedObject> objects = new();

    public string Name;
    public string Description;
    public string Author;
    /// <summary>
    /// Will look for all models not found locally here
    /// </summary>
    public string WebModelLocation;
    /// <summary>
    /// Will look for all models not found in the cache here
    /// </summary>
    public string BaseModelPath;
    public bool AdultContent = false;
    public GameContentRating ContentRating = GameContentRating.E_Everyone;
    public Language PrimaryLanguage = Language.English;
    public string SkyboxImagePath; // ← New field
}


[System.Serializable]
public class ExportedObject
{
    public string id;
    public string parentId;
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public string primitiveType;   // null for models
    public ModelSourceType modelSource = ModelSourceType.Resources;

    // Optional overrides (optional: if null, use Space defaults)
    public string overrideFilePath;    // "D:/OtherModels/chair1.glb"
    public string overrideRemoteURL;   // "https://cdn.someone.com/free/chair1.glb"

    public List<string> materials;
    public List<string> scripts;
    public List<ExportedComponent> components;
}


[System.Serializable]
public class ExportedComponent
{
    public string type;
    public List<ComponentProperty> properties;
}

[System.Serializable]
public class ComponentProperty
{
    public string key;
    public string value;
}
public enum ModelSourceType
{
    Resources,
    FileSystem,
    RemoteURL
}
public enum GameContentRating
{
    Unrated = 0,

    // ESRB (North America)
    EC_EarlyChildhood,    // For ages 3+
    E_Everyone,           // For ages 6+
    E10_Everyone10Plus,   // For ages 10+
    T_Teen,               // For ages 13+
    M_Mature,             // For ages 17+
    AO_AdultsOnly,        // For ages 18+
    RP_RatingPending,     // Awaiting final rating

    // PEGI (Europe)
    PEGI_3,
    PEGI_7,
    PEGI_12,
    PEGI_16,
    PEGI_18,

    // USK (Germany)
    USK_0,
    USK_6,
    USK_12,
    USK_16,
    USK_18,

    // CERO (Japan)
    CERO_A,   // All ages
    CERO_B,   // 12+
    CERO_C,   // 15+
    CERO_D,   // 17+
    CERO_Z,   // 18+ (adults only)

    // Generic categories
    Kids,
    FamilyFriendly,
    TeenFriendly,
    MatureOnly
}
public enum Language
{
    English,        // en
    Spanish,        // es
    French,         // fr
    German,         // de
    Italian,        // it
    Portuguese,     // pt
    Russian,        // ru
    ChineseSimplified,  // zh-CN
    ChineseTraditional, // zh-TW
    Japanese,       // ja
    Korean,         // ko
    Arabic,         // ar
    Hindi,          // hi
    Turkish,        // tr
    Dutch,          // nl
    Polish,         // pl
    Swedish,        // sv
    Danish,         // da
    Finnish,        // fi
    Norwegian,      // no
    Greek,          // el
    Czech,          // cs
    Hungarian,      // hu
    Romanian,       // ro
    Thai,           // th
    Vietnamese,     // vi
    Hebrew,         // he
    Indonesian,     // id
    Malay,          // ms
    Ukrainian       // uk
}
