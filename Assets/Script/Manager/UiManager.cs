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

    public void Init()
    {
        //_joyStick = new GameObject();
        _joyStickController = _joyStick.GetComponent<JoystickController>();
    }
}
