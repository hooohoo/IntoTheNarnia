using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterLineClass
{
    // ����Ʈ ����
    [SerializeField]
    private int _questLevel;

    // ����Ʈ �����ϴ� �� �̸�
    [SerializeField]
    private string _sceneName;

    // ���ϴ� ĳ���� �̸�
    [SerializeField]
    private string _characterName;

    // ���
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
