using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RegisterIngameElementAttribute : Attribute
{
    public string ID { get; }
    public string PrefabPath { get; }

    public RegisterIngameElementAttribute(string id, string prefabPath)
    {
        ID = id;
        PrefabPath = prefabPath;
    }
}