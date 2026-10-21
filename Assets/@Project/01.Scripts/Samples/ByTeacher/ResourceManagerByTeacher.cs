using UnityEngine;

public sealed class ResourceManagerByTeacher : SingleTon<ResourceManagerByTeacher>
{
    private GameObject[] characters;
    public void LoadCharacter() { characters = Resources.LoadAll<GameObject>("Prefabs/Character/WithTeacher"); }
    public GameObject GetRcCharacter(string name)
    {
        if (characters == null) LoadCharacter();
        foreach (var character in characters) if (character.name == name) return character;
        throw new MissingReferenceException("Missing sample character: " + name);
    }
}
