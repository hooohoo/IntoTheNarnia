using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*public static GameManager instance;
    public Transform playerParent;
    public Transform mobParent;*/

    // static ��ü
    public static GameManager _instance;

    // getter
    public static GameManager Instance
    {
        get { return _instance; }
    }

    // �Ŵ��� ����
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();

    // �Ŵ��� �ν��Ͻ� getter
    public static ResourceManager Resource
    {
        get { return _instance._resource; }
    }
    public static SceneManagerEx Scene
    {
        get { return _instance._scene; }
    }


    void Awake()
    {
        /*instance = this;
        InstanceMananger.instance.Initialize();
        InstanceMananger.instance.CreatePlayer("Player", playerParent);
        InstanceMananger.instance.CreateMonster("Monster", mobParent);*/

        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        // ���ҽ� �Ŵ��� Init()
        GameManager.Resource.Init();
    }
}
