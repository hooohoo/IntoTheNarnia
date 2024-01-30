using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 맵 마다 적용되어 있어서 씬 전환할 때마다 실행되는 코드
public class FieldManager : MonoBehaviour
{
    // 현재 씬에 있는 @UI 오브젝트(canvas)
    public GameObject _UiRoot;
    // 현재 씬에 있는 UI들 담은 List
    private List<GameObject> _uiList = new List<GameObject>();

    void Awake()
    {
        // 씬에 올려진 UI들 가져와서 리스트에 담기
        GetUIThisScene();

        // 위에서 가져온 UI 매니저 멤버변수에 값 할당
        PutUIObjectToUiManager();

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
    }// end ManagerInit()

    // UI 매니저에 오브젝트 넘겨주는 함수
    private void PutUIObjectToUiManager()
    {
        // 조이스틱
        GameManager.Ui._joyStick = GameManager.Obj.GetObjectByName(_uiList, Define.UIName.Joystick.ToString());
        // 세팅 버튼
        GameManager.Ui._settingButton = GameManager.Obj.GetObjectByName(_uiList, Define.UIName.SettingButton.ToString());
    }

    // 씬 위에 올려져있는 UI 게임오브젝트 타입으로 갖고 있는 함수
    private void GetUIThisScene()
    {
        // @UI 자식 오브젝트의 개수
        int cnt = _UiRoot.transform.childCount;
        for(int i = 0; i < cnt; i++)
        {
            // 리스트에 ui 추가
            _uiList.Add(_UiRoot.transform.GetChild(i).gameObject);
        }
    }// end GetUIThisScene()
}
