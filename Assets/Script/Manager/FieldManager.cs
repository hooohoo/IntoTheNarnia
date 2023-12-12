using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    // 임시 코드
    public GameObject _joyStick;

    void Awake()
    {
        // 임시 코드
        GameManager.Ui._joyStick = _joyStick;

        // 임시 코드
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
        // Ui 불러옴
        GameManager.Ui.Init();
    }
}
