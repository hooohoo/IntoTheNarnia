using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*public static GameManager instance;
    public Transform playerParent;
    public Transform mobParent;*/

    // static 객체
    public static GameManager _instance;

    // getter
    public static GameManager Instance
    {
        get { return _instance; }
    }

    // 매니저 생성
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    UiManager _ui = new UiManager();
    ObjectManager _obj = new ObjectManager();
    CameraManager _camera = new CameraManager();
    CreateManager _create = new CreateManager();

    // 매니저 인스턴스 getter
    public static ResourceManager Resource
    {
        get { return _instance._resource; }
    }
    public static SceneManagerEx Scene
    {
        get { return _instance._scene; }
    }
    public static UiManager Ui
    {
        get { return _instance._ui; }
    }
    public static ObjectManager Obj
    {
        get { return _instance._obj; }
    }
    public static CameraManager Camera
    {
        get { return _instance._camera; }
    }
    public static CreateManager Create
    {
        get { return _instance._create; }
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

        // 리소스 매니저 Init()
        GameManager.Resource.Init();
    }
}
