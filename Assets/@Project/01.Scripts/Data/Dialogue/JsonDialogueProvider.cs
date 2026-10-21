using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class JsonDialogueProvider : IDialogueProvider
{
    private readonly AddressableManager resources;
    public JsonDialogueProvider(AddressableManager resources = null) { this.resources = resources; }
    public async UniTask<IReadOnlyList<CharacterLineClass>> LoadAsync(object owner, CancellationToken token)
    {
        if (resources == null) throw new InvalidOperationException("Addressable source was not supplied to the provider.");
        if (!await resources.PreloadAsync<TextAsset>(AddressableLabels.Story, owner, token))
            throw new InvalidOperationException("Story preload failed.");
        token.ThrowIfCancellationRequested();
        return Load(resources.Load<TextAsset>(AssetKeys.QuestData).text);
    }

    public IReadOnlyList<CharacterLineClass> Load(string text)
    {
        var result = new List<CharacterLineClass>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        using (var input = new StringReader(text))
        using (var reader = new JsonTextReader(input) { SupportMultipleContent = true })
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.Comment) continue;
                var token = JToken.ReadFrom(reader);
                if (token is JArray array)
                {
                    foreach (var item in array) Add(item, result);
                }
                else Add(token, result);
            }
        }
        return result;
    }
    private static void Add(JToken token, List<CharacterLineClass> entries)
    {
        if (!(token is JObject)) throw new FormatException("Dialogue must contain JSON objects.");
        var entry = token.ToObject<CharacterLineClass>();
        if (entry == null || string.IsNullOrWhiteSpace(entry.SceneName) || string.IsNullOrWhiteSpace(entry.CharacterName) || entry.Line == null)
            throw new FormatException("Dialogue requires scene, character and line fields.");
        entries.Add(entry);
    }
}
