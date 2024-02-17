using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// �� ���� ����Ǿ� �־ �� ��ȯ�� ������ ����Ǵ� �ڵ�
public class FieldManager : MonoBehaviour
{
    // ���� ���� �ִ� @___ ������Ʈ��(Root)
    // @UI ������Ʈ(canvas)
    public GameObject _UiRoot;
    // @Camera ������Ʈ
    public GameObject _CameraRoot;
    
    // ���� ���� �ִ� UI�� ���� List
    private List<GameObject> _uiList = new List<GameObject>();
    // ī�޶�� ���� List
    private List<GameObject> _cameraList = new List<GameObject>();

    // �÷��̾� ���� ��ġ
    private Vector3 _startPos;

    void Awake()
    {
        // FieldManager ����� ��ġ�� ������ġ
        _startPos = transform.position;

        // ���� �÷��� ������Ʈ�� �����ͼ� ����Ʈ�� ���
        // UI
        GetRootThisScene(_UiRoot, _uiList);
        // ī�޶�
        GetRootThisScene(_CameraRoot, _cameraList);

        // ������ ������ �Ŵ��� ��������� �� �Ҵ�
        PutUIObjectToUiManager();
        PutCamObjectToCameraManager();

        // �÷��̾� ����
        CreatePlayer();

        // �ӽ� �ڵ�
        ManagerInit();
    }

    void Start(){}

    void Update(){}

    // �Ŵ����� Init()
    private void ManagerInit()
    {
        // Ui �ҷ���
        GameManager.Ui.Init();
        GameManager.Camera.Init();
    }// end ManagerInit()

    // UI �Ŵ����� ������Ʈ �Ѱ��ִ� �Լ�
    private void PutUIObjectToUiManager()
    {
        // ���̽�ƽ
        GameManager.Ui._joyStick = GameManager.Obj.GetObjectByName(_uiList, UIName.Joystick.ToString());
        // ���� ��ư
        GameManager.Ui._settingButton = GameManager.Obj.GetObjectByName(_uiList, UIName.SettingButton.ToString());
        // �׼� ��ư
        GameManager.Ui._actionButton = GameManager.Obj.GetObjectByName(_uiList, UIName.ActionButton.ToString());
        // �̴ϸ�
        GameManager.Ui._miniMap = GameManager.Obj.GetObjectByName(_uiList, UIName.MiniMap.ToString());
    }// end PutUIObjectToUiManager()

    // Camera �Ŵ����� ������Ʈ �Ѱ��ִ� �Լ�
    private void PutCamObjectToCameraManager()
    {
        // �ó׸ӽ� ī�޶�
        GameManager.Camera._cmCam = GameManager.Obj.GetObjectByName(_cameraList, CameraName.CM_Camera.ToString());
    }

    // �� ���� �÷����ִ� @___ ��Ʈ ������Ʈ ���� �ִ� �Լ�
    private void GetRootThisScene(GameObject rootObject, List<GameObject> objList)
    {
        // �ڽ� ������Ʈ�� ����
        int cnt = rootObject.transform.childCount;
        for(int i = 0; i < cnt; i++)
        {
            // ����Ʈ�� �߰�
            objList.Add(rootObject.transform.GetChild(i).gameObject);
        }
    }// end GetCameraThisScene()

    // �÷��̾� �����Լ�
    private void CreatePlayer()
    {
        // �ϴ� Lucy�� ����, ���� ��� ����(�Ű����� �Ѱܼ� ���� ��)
        GameManager.Obj._player = GameManager.Create.CreatePlayer(_startPos, CharacterName.Lucy.ToString());
    }
}
