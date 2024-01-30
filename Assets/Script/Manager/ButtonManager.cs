using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// @UI에 넣어서 버튼 함수들에 적용될 매니저 클래스
public class ButtonManager : MonoBehaviour
{
    void Start(){}

    void Update(){}

    // 설정창 On
    public void SettingWindowOn()
    {
        GameManager.Ui._settingWindow.SetActive(true);
    }

    // 설정창 Off
    public void SettingWindowOff()
    {
        GameManager.Ui._settingWindow.SetActive(false);
    }

    // 액션 버튼
    public void ActionButton()
    {
        //
    }
}
