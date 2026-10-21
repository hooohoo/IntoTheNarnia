using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class ResourceManagerByTeacher : SingleTon<ResourceManagerByTeacher>
{
    private GameObject[] characters;
    public void LoadCharacter()
    {
        #if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/@Project/02.Prefabs/Character/WithTeacher" });
        characters = new GameObject[guids.Length];
        for (int i = 0; i < guids.Length; i++) characters[i] = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
        #else
        throw new System.NotSupportedException("Teacher sample content is available only in the Editor.");
        #endif
    }
    public GameObject GetRcCharacter(string name)
    {
        if (characters == null) LoadCharacter();
        foreach (var character in characters) if (character.name == name) return character;
        throw new MissingReferenceException("Missing sample character: " + name);
    }
}
