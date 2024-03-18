using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using static Define;

public class FileIOManager
{
    private CharacterLineClass _questData = new CharacterLineClass();
    // Split �ϰ� ���� ���� ���� CharacterLineClass ��ü ���� Array
    private string[] _questDataArr; 
    // QuestData ���Ͽ��� ������ Json ��ü ���� ���� List(CharacterLineClass Ÿ��)
    private List<CharacterLineClass> _questDataList = new List<CharacterLineClass>();
    

    public void Init()
    {
        // Json �ε��ϸ� _questDataList�� ���
        LoadQuestJson();
        // ������ ���� _questDataList�� ObjectManager ������ �Űܴ���
        GameManager.Obj._characterLineList = _questDataList;
    }

    public void CreateFile()
    {
        CharacterLineClass messagePackage_1 = new CharacterLineClass();
        messagePackage_1._QuestLevel = 1;
        messagePackage_1._SceneName = SceneName.Professor_House.ToString();
        messagePackage_1._CharacterName = CharacterName.Lucy.ToString();
        messagePackage_1._Line += "'�� ������ �ǳ� �� �� ���� ������°.',";
        messagePackage_1._Line += "'��ȭ�Ӱ� �Ƹ��ٿ� �������� ���� �ɽ���',";
        messagePackage_1._Line += "'Peter���� ��ƴ޶�� �ؾ߰ھ�!'";
        CharacterLineClass messagePackage_2 = new CharacterLineClass();
        messagePackage_2._QuestLevel = 1;
        messagePackage_2._SceneName = SceneName.Professor_House.ToString();
        messagePackage_2._CharacterName = CharacterName.System.ToString();
        messagePackage_2._Line += "'Peter�� 2�� �濡 �ֽ��ϴ�. ã�ư�������.'";
        CharacterLineClass messagePackage_3 = new CharacterLineClass();
        messagePackage_3._CharacterName = CharacterName.Lucy.ToString();
        messagePackage_3._Line += "��� ���3";

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
    }// end CreateFile()

    public void LoadQuestJson()
    {
        string fileName = "QuestData";
        // ���
        string path = Application.dataPath + "/Resources/Data/Message/" + fileName + ".json";

        FileStream fileStream = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string json = Encoding.UTF8.GetString(data);

        // JsonUtility�� �������� {}{}{} ������ ������ ��ȯ�ϴ� ����� �����Ƿ� ���� �и�
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
