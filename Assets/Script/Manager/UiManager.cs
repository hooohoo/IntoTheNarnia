using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager
{
    // 조이스틱
    public JoystickController _joyStickController;
    public GameObject _joyStick;
    // 시네머신
    public CMCameraController _cmCamController;
    // 설정 버튼
    public GameObject _settingButton;
    public GameObject _settingWindow;

    public void Init()
    {
        // 조이스틱 컨트롤러 스크립트
        _joyStickController = _joyStick.GetComponent<JoystickController>();
        // 설정 창
        _settingWindow = _settingButton.transform.Find(Define.UIName.SettingWindow.ToString()).gameObject;
        // 설정 창 로드한 뒤 비활성화
        _settingWindow.SetActive(false);
    }

    // 모든 UI 끄기
    public void AllUIOff()
    {
        _joyStick.SetActive(false);
        _settingButton.SetActive(false);
    }
}
