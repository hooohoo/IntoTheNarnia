using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 맵 마다 적용되어 있어서 씬 전환할 때마다 실행되는 코드
public class FieldManager : MonoBehaviour
{
    // 현재 씬에 있는 @UI 오브젝트(canvas)
    public GameObject _UiRoot;
    // 임시 코드
    public GameObject _joyStick;

    void Awake()
    {
        int childCount = _UiRoot.transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            string name = _UiRoot.transform.GetChild(i).name;
            Debug.Log("childe name : " + name);
        }
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
