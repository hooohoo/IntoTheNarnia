using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public sealed class DataManager : Singleton<DataManager>
{
    [Singleton(typeof(AddressableManager))] private AddressableManager resources;
    private IDialogueProvider dialogueProvider;
    private readonly Dictionary<string, List<CharacterLineClass>> dialogue = new Dictionary<string, List<CharacterLineClass>>();
    private bool loaded;
    public override void Init() { dialogueProvider = new JsonDialogueProvider(resources); }
    public void SetDialogueProvider(IDialogueProvider provider)
    {
        dialogueProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        ReleaseDialogue();
    }
    public async UniTask LoadDialogueAsync(object owner, CancellationToken token)
    {
        var provider = dialogueProvider;
        var entries = await provider.LoadAsync(owner, token);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(provider, dialogueProvider)) throw new OperationCanceledException("Dialogue provider changed during load.");
        if (loaded) return;
        dialogue.Clear();
        for (int i = 0; i < entries.Count; i++)
        {
            CharacterLineClass entry = entries[i];
            if (!dialogue.TryGetValue(entry.SceneName, out var lines))
            {
                lines = new List<CharacterLineClass>();
                dialogue.Add(entry.SceneName, lines);
            }
            lines.Add(entry);
        }
        loaded = true;
    }
    public IReadOnlyList<CharacterLineClass> GetDialogue(string scene)
    {
        if (!loaded) throw new InvalidOperationException("Dialogue has not been prepared.");
        return dialogue.TryGetValue(scene, out var lines) ? lines : Array.Empty<CharacterLineClass>();
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
    public void ReleaseDialogue() { dialogue.Clear(); loaded = false; }
    public override void Release() { ReleaseDialogue(); dialogueProvider = null; }
}
