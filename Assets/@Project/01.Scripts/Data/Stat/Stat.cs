using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, JsonObject(MemberSerialization.OptIn)]
public class Stat
{
    [FormerlySerializedAs("_Hp"), SerializeField, JsonProperty("_Hp")] private int hp;
    [FormerlySerializedAs("_Atk"), SerializeField, JsonProperty("_Atk")] private int atk;
    [FormerlySerializedAs("_Def"), SerializeField, JsonProperty("_Def")] private int def;
    [FormerlySerializedAs("_Lv"), SerializeField, JsonProperty("_Lv")] private int lv;
    [FormerlySerializedAs("_Max_Hp"), SerializeField, JsonProperty("_Max_Hp")] private int maxHp;
    public int Hp { get => hp; set => hp = value; }
    public int Atk { get => atk; set => atk = value; }
    public int Def { get => def; set => def = value; }
    public int Lv { get => lv; set => lv = value; }
    public int MaxHp => maxHp;
}
