using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ���� ����Ǿ� �־ �� ��ȯ�� ������ ����Ǵ� �ڵ�
public class FieldManager : MonoBehaviour
{
    // ���� ���� �ִ� @UI ������Ʈ(canvas)
    public GameObject _UiRoot;
    // �ӽ� �ڵ�
    public GameObject _joyStick;

    void Awake()
    {
        int childCount = _UiRoot.transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            string name = _UiRoot.transform.GetChild(i).name;
            Debug.Log("childe name : " + name);
        }
        // �ӽ� �ڵ�
        GameManager.Ui._joyStick = _joyStick;
        // �ӽ� �ڵ�
        ManagerInit();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void ManagerInit()
    {
        // Ui �ҷ���
        GameManager.Ui.Init();
    }
}
