using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    // �ӽ� �ڵ�
    public GameObject _joyStick;

    void Awake()
    {
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
