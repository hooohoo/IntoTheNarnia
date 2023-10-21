using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.IO;
using System.Text;

public class Main : MonoBehaviour
{
    //private PlayerStat _playerStat;
    PlayerStat player = PlayerStat.Instance();
    void Start()
    {
        //ps.Hp = 0;
        Save();
        LoadJson();
        //Debug.Log(ps.Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadJson()
    {
        string fileName = "playerStat";
        string path = Application.dataPath + "/Resources/Data/Json/" + fileName + ".json";

        FileStream fileStream = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string json = Encoding.UTF8.GetString(data);

        PlayerStat.SetInstance = JsonUtility.FromJson<PlayerStat>(json);

        Debug.Log(player.Name);
        Debug.Log(player.Lv);
        Debug.Log(player.Job);
        
        //JsonData jsonData = JsonMapper.ToObject(jsonPath);
        //ParsingJson(jsonData);
    }

    void ParsingJson(JsonData data)
    {
        string tmpName = data["_Name"].ToString();

        //_playerStat.Name = tmpName;
    }

    void Save()
    {
        // 임시로 리터럴 값 넣음
        /*
        */
        player.Name = "p1";
        player.Lv = 2;
        player.Hp = 100;
        player.Atk = 0;
        player.Def = 0;
        player.Exp = 0;
        player.Job = "Scientist";

        string json = JsonUtility.ToJson(player);
        string fileName = "playerStat";
        string path = Application.dataPath + "/Resources/Data/Json/" + fileName + ".json";

        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(json);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }
}
