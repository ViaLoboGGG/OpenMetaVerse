// Models/ExportedScene.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExportedScene
{
    public List<ExportedObject> objects = new();
    public string baseModelPath; // e.g., "Resources/Models"
}


[System.Serializable]
public class ExportedObject
{
    public string modelPath;  // e.g., "Crystal.fbx"
    public string id;
    public string parentId;
    public string name;

    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public string primitiveType;
    public List<string> scripts;
    public List<ExportedComponent> components;
    public List<string> materials;
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
