using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ResourceManager : Singleton<ResourceManager>
{
    private readonly Dictionary<string, GameObject> characters = new Dictionary<string, GameObject>(StringComparer.Ordinal);
    private readonly Dictionary<string, Sprite> images = new Dictionary<string, Sprite>(StringComparer.Ordinal);
    public override void Init()
    {
        foreach (var character in Resources.LoadAll<GameObject>("Prefabs/Character")) characters.Add(character.name, character);
        foreach (var image in Resources.LoadAll<Sprite>("Resource/Image")) images.Add(image.name, image);
    }
    public GameObject GetPlayer(string name)
    {
        if (!characters.TryGetValue(name, out var character)) throw new InvalidOperationException("Missing character resource: " + name);
        return character;
    }
    public Sprite GetImage(string name)
    {
        if (!images.TryGetValue(name, out var image)) throw new InvalidOperationException("Missing image resource: " + name);
        return image;
    }
    public TextAsset GetText(string path)
    {
        var text = Resources.Load<TextAsset>(path);
        if (text == null) throw new InvalidOperationException("Missing text resource: " + path);
        return text;
    }
    public override void Release() { characters.Clear(); images.Clear(); }
}
