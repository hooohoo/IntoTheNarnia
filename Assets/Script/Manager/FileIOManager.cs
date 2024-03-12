using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using static Define;

public class FileIOManager : MonoBehaviour
{
    void Start()
    {
        CreateFile();
    }

    void Update()
    {
        //
    }

    public void CreateFile()
    {
        CharacterLineClass messagePackage_1 = new CharacterLineClass();
        messagePackage_1.questLevel = 1;
        messagePackage_1.sceneName = SceneName.Professor_House.ToString();
        messagePackage_1.characterName = CharacterName.Lucy.ToString();
        messagePackage_1.line += "'�� ������ �ǳ� �� �� ���� ������°.',";
        messagePackage_1.line += "'��ȭ�Ӱ� �Ƹ��ٿ� �������� ���� �ɽ���',";
        messagePackage_1.line += "'Peter���� ��ƴ޶�� �ؾ߰ھ�!'";
        CharacterLineClass messagePackage_2 = new CharacterLineClass();
        messagePackage_2.questLevel = 1;
        messagePackage_2.sceneName = SceneName.Professor_House.ToString();
        messagePackage_2.characterName = CharacterName.System.ToString();
        messagePackage_2.line += "'Peter�� 2�� �濡 �ֽ��ϴ�. ã�ư�������.'";
        CharacterLineClass messagePackage_3 = new CharacterLineClass();
        messagePackage_3.characterName = CharacterName.Lucy.ToString();
        messagePackage_3.line += "��� ���3";

        string json;
        string fileName = SceneName.Professor_House.ToString() + "_1";
        // ���
        string path = Application.dataPath + "/Resources/Data/Message/" + fileName + ".json";

        List<CharacterLineClass> classes = new List<CharacterLineClass>();
        classes.Add(messagePackage_1);
        classes.Add(messagePackage_2);
        //classes.Add(messagePackage_3);

        for(int i = 0; i < classes.Count; i++)
        {
            json = JsonUtility.ToJson(classes[i]);
            
            FileStream fileStream = new FileStream(path, FileMode.Append);
            byte[] data = Encoding.UTF8.GetBytes(json);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
        }
        
    }
}

[System.Serializable]
public class CharacterLineClass
{
    [SerializeField]
    public int questLevel;

    [SerializeField]
    public string sceneName;

    [SerializeField]
    public string characterName;

    [SerializeField]
    public string line;
}
