using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ���� ����Ǿ� �־ �� ��ȯ�� ������ ����Ǵ� �ڵ�
public class FieldManager : MonoBehaviour
{
    // ���� ���� �ִ� @UI ������Ʈ(canvas)
    public GameObject _UiRoot;
    // ���� ���� �ִ� UI�� ���� List
    private List<GameObject> _uiList = new List<GameObject>();

    void Awake()
    {
        // ���� �÷��� UI�� �����ͼ� ����Ʈ�� ���
        GetUIThisScene();

        // ������ ������ UI �Ŵ��� ��������� �� �Ҵ�
        PutUIObjectToUiManager();

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
    }// end ManagerInit()

    // UI �Ŵ����� ������Ʈ �Ѱ��ִ� �Լ�
    private void PutUIObjectToUiManager()
    {
        // ���̽�ƽ
        GameManager.Ui._joyStick = GameManager.Obj.GetObjectByName(_uiList, Define.UIName.Joystick.ToString());
        // ���� ��ư
        GameManager.Ui._settingButton = GameManager.Obj.GetObjectByName(_uiList, Define.UIName.SettingButton.ToString());
    }

    // �� ���� �÷����ִ� UI ���ӿ�����Ʈ Ÿ������ ���� �ִ� �Լ�
    private void GetUIThisScene()
    {
        // @UI �ڽ� ������Ʈ�� ����
        int cnt = _UiRoot.transform.childCount;
        for(int i = 0; i < cnt; i++)
        {
            // ����Ʈ�� ui �߰�
            _uiList.Add(_UiRoot.transform.GetChild(i).gameObject);
        }
    }// end GetUIThisScene()
}
