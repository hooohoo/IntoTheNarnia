using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, JsonObject(MemberSerialization.OptIn)]
public sealed class CharacterLineClass
{
    [SerializeField, FormerlySerializedAs("_questLevel"), JsonProperty("_questLevel")] private int questLevel;
    [SerializeField, FormerlySerializedAs("_sceneName"), JsonProperty("_sceneName")] private string sceneName;
    [SerializeField, FormerlySerializedAs("_characterName"), JsonProperty("_characterName")] private string characterName;
    [SerializeField, FormerlySerializedAs("_line"), JsonProperty("_line")] private string line;
    public int QuestLevel { get => questLevel; set => questLevel = value; }
    public string SceneName { get => sceneName; set => sceneName = value; }
    public string CharacterName { get => characterName; set => characterName = value; }
    public string Line { get => line; set => line = value; }
}
