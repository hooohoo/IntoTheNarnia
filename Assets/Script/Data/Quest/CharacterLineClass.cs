using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterLineClass
{
    // 퀘스트 레벨
    [SerializeField]
    private int _questLevel;

    // 퀘스트 진행하는 씬 이름
    [SerializeField]
    private string _sceneName;

    // 말하는 캐릭터 이름
    [SerializeField]
    private string _characterName;

    // 대사
    [SerializeField]
    private string _line;

    public int _QuestLevel
    {
        get { return _questLevel; }
        set { _questLevel = value; }
    }
    public string _SceneName
    {
        get { return _sceneName; }
        set { _sceneName = value; }
    }
    public string _CharacterName
    {
        get { return _characterName; }
        set { _characterName = value; }
    }
    public string _Line
    {
        get { return _line; }
        set { _line = value; }
    }
}
