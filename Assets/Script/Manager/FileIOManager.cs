using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

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
        List<string> messagePackage = new List<string>();
        messagePackage.Add("test1 : abc");
        messagePackage.Add("test2 : def");
        messagePackage.Add("test3 : ghi");
        messagePackage.Add("test4 : jkl");
        
        string json = JsonUtility.ToJson(messagePackage);
        string fileName = "test";
        // °æ·Î
        string path = Application.dataPath + "/Data/Message/" + fileName + ".json";

        Debug.Log("path : " + path);

        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(json);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }
}
