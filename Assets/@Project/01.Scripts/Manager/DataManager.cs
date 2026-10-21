using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public sealed class DataManager : Singleton<DataManager>
{
    [Singleton(typeof(ResourceManager))] private ResourceManager resources;
    private readonly IDialogueProvider dialogueProvider = new JsonDialogueProvider();
    private readonly Dictionary<string, List<CharacterLineClass>> dialogue = new Dictionary<string, List<CharacterLineClass>>();
    public override void Init()
    {
        dialogue.Clear();
        var entries = dialogueProvider.Load(resources.GetText("Data/Message/QuestData").text);
        foreach (var entry in entries)
        {
            if (!dialogue.TryGetValue(entry.SceneName, out var sceneLines))
            {
                sceneLines = new List<CharacterLineClass>();
                dialogue.Add(entry.SceneName, sceneLines);
            }
            sceneLines.Add(entry);
        }
    }
    public IReadOnlyList<CharacterLineClass> GetDialogue(string scene)
    {
        if (dialogue.TryGetValue(scene, out var lines)) return lines;
        return System.Array.Empty<CharacterLineClass>();
    }
    public void SavePlayer(PlayerStat player)
    {
        Directory.CreateDirectory(Application.persistentDataPath);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "playerStat.json"), JsonConvert.SerializeObject(player));
    }
    public PlayerStat LoadPlayer()
    {
        string path = Path.Combine(Application.persistentDataPath, "playerStat.json");
        return File.Exists(path) ? JsonConvert.DeserializeObject<PlayerStat>(File.ReadAllText(path)) : new PlayerStat();
    }
    public override void Release() { dialogue.Clear(); }
}
