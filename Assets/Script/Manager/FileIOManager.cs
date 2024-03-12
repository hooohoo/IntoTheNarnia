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
        messagePackage_1.line += "'이 곳으로 피난 온 지 벌써 일주일째.',";
        messagePackage_1.line += "'평화롭고 아름다운 곳이지만 조금 심심해',";
        messagePackage_1.line += "'Peter에게 놀아달라고 해야겠어!'";
        CharacterLineClass messagePackage_2 = new CharacterLineClass();
        messagePackage_2.questLevel = 1;
        messagePackage_2.sceneName = SceneName.Professor_House.ToString();
        messagePackage_2.characterName = CharacterName.System.ToString();
        messagePackage_2.line += "'Peter는 2층 방에 있습니다. 찾아가보세요.'";
        CharacterLineClass messagePackage_3 = new CharacterLineClass();
        messagePackage_3.characterName = CharacterName.Lucy.ToString();
        messagePackage_3.line += "루시 대사3";

        string json;
        string fileName = SceneName.Professor_House.ToString() + "_1";
        // 경로
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
