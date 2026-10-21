using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, JsonObject(MemberSerialization.OptIn)]
public sealed class PlayerStat : Stat
{
    [FormerlySerializedAs("_Name"), SerializeField, JsonProperty("_Name")] private string playerName;
    [FormerlySerializedAs("_Job"), SerializeField, JsonProperty("_Job")] private string job;
    [FormerlySerializedAs("_Exp"), SerializeField, JsonProperty("_Exp")] private int exp;
    [FormerlySerializedAs("_Lv_Exp"), SerializeField, JsonProperty("_Lv_Exp")] private int levelExperience;
    public string Name { get => playerName; set => playerName = value; }
    public string Job { get => job; set => job = value; }
    public int Exp { get => exp; set => exp = value; }
    public int LevelExperience => levelExperience;
}
