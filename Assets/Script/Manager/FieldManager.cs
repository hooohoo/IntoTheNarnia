using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// 맵 마다 적용되어 있어서 씬 전환할 때마다 실행되는 코드
public class FieldManager : MonoBehaviour
{
    // 현재 씬에 있는 @___ 오브젝트들(Root)
    // @UI 오브젝트(canvas)
    public GameObject _UiRoot;
    // @Camera 오브젝트
    public GameObject _CameraRoot;
    
    // 현재 씬에 있는 UI들 담은 List
    private List<GameObject> _uiList = new List<GameObject>();
    // 카메라들 담은 List
    private List<GameObject> _cameraList = new List<GameObject>();

    // 플레이어 시작 위치
    private Vector3 _startPos;

    void Awake()
    {
        // FieldManager 적용된 위치가 시작위치
        _startPos = transform.position;

        // 씬에 올려진 오브젝트들 가져와서 리스트에 담기
        // UI
        GetRootThisScene(_UiRoot, _uiList);
        // 카메라
        GetRootThisScene(_CameraRoot, _cameraList);

        // 위에서 가져온 매니저 멤버변수에 값 할당
        PutUIObjectToUiManager();
        PutCamObjectToCameraManager();

        // 플레이어 생성
        CreatePlayer();

        // 임시 코드
        ManagerInit();
    }

    void Start(){}

    void Update(){}

    // 매니저들 Init()
    private void ManagerInit()
    {
        // Ui 불러옴
        GameManager.Ui.Init();
        GameManager.Camera.Init();
    }// end ManagerInit()

    // UI 매니저에 오브젝트 넘겨주는 함수
    private void PutUIObjectToUiManager()
    {
        // 조이스틱
        GameManager.Ui._joyStick = GameManager.Obj.GetObjectByName(_uiList, UIName.Joystick.ToString());
        // 세팅 버튼
        GameManager.Ui._settingButton = GameManager.Obj.GetObjectByName(_uiList, UIName.SettingButton.ToString());
        // 액션 버튼
        GameManager.Ui._actionButton = GameManager.Obj.GetObjectByName(_uiList, UIName.ActionButton.ToString());
        // 미니맵
        GameManager.Ui._miniMap = GameManager.Obj.GetObjectByName(_uiList, UIName.MiniMap.ToString());
    }// end PutUIObjectToUiManager()

    // Camera 매니저에 오브젝트 넘겨주는 함수
    private void PutCamObjectToCameraManager()
    {
        // 시네머신 카메라
        GameManager.Camera._cmCam = GameManager.Obj.GetObjectByName(_cameraList, CameraName.CM_Camera.ToString());
    }

    // 씬 위에 올려져있는 @___ 루트 오브젝트 갖고 있는 함수
    private void GetRootThisScene(GameObject rootObject, List<GameObject> objList)
    {
        // 자식 오브젝트의 개수
        int cnt = rootObject.transform.childCount;
        for(int i = 0; i < cnt; i++)
        {
            // 리스트에 추가
            objList.Add(rootObject.transform.GetChild(i).gameObject);
        }
    }// end GetCameraThisScene()

    // 플레이어 생성함수
    private void CreatePlayer()
    {
        // 일단 Lucy로 시작, 추후 방식 변경(매개변수 넘겨서 오는 등)
        GameManager.Obj._player = GameManager.Create.CreatePlayer(_startPos, CharacterName.Lucy.ToString());
    }
}
