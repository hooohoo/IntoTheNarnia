using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using static Define;

public class FileIOManager
{
    private CharacterLineClass _questData = new CharacterLineClass();
    // Split 하고 생긴 여러 개의 CharacterLineClass 객체 담을 Array
    private string[] _questDataArr; 
    // QuestData 파일에서 가져온 Json 객체 각각 담을 List(CharacterLineClass 타입)
    private List<CharacterLineClass> _questDataList = new List<CharacterLineClass>();
    

    public void Init()
    {
        // Json 로드하면 _questDataList에 담김
        LoadQuestJson();
        // 위에서 담은 _questDataList를 ObjectManager 변수에 옮겨담음
        GameManager.Obj._characterLineList = _questDataList;
    }

    public void CreateFile()
    {
        CharacterLineClass messagePackage_1 = new CharacterLineClass();
        messagePackage_1._QuestLevel = 1;
        messagePackage_1._SceneName = SceneName.Professor_House.ToString();
        messagePackage_1._CharacterName = CharacterName.Lucy.ToString();
        messagePackage_1._Line += "'이 곳으로 피난 온 지 벌써 일주일째.',";
        messagePackage_1._Line += "'평화롭고 아름다운 곳이지만 조금 심심해',";
        messagePackage_1._Line += "'Peter에게 놀아달라고 해야겠어!'";
        CharacterLineClass messagePackage_2 = new CharacterLineClass();
        messagePackage_2._QuestLevel = 1;
        messagePackage_2._SceneName = SceneName.Professor_House.ToString();
        messagePackage_2._CharacterName = CharacterName.System.ToString();
        messagePackage_2._Line += "'Peter는 2층 방에 있습니다. 찾아가보세요.'";
        CharacterLineClass messagePackage_3 = new CharacterLineClass();
        messagePackage_3._CharacterName = CharacterName.Lucy.ToString();
        messagePackage_3._Line += "루시 대사3";

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
    }// end CreateFile()

    public void LoadQuestJson()
    {
        string fileName = "QuestData";
        // 경로
        string path = Application.dataPath + "/Resources/Data/Message/" + fileName + ".json";

        FileStream fileStream = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string json = Encoding.UTF8.GetString(data);

        // JsonUtility에 여러개의 {}{}{} 묶음을 꺼내서 변환하는 기능이 없으므로 직접 분리
        SplitJson(json);
        
        for(int i = 0; i < _questDataArr.Length-1; i++)
        {
            //Debug.Log(_questDataArr[i]);
            CharacterLineClass lineData = JsonUtility.FromJson<CharacterLineClass>(_questDataArr[i]);
            _questDataList.Add(lineData);
        }
    }// end LoadQuestJson()

    public void SplitJson(string jsonSentence)
    {
        _questDataArr = jsonSentence.Split("}");
        for(int i = 0; i < _questDataArr.Length - 1; i++)
        {
            _questDataArr[i] += "}";
        }
    }// end SplitJson()
}
