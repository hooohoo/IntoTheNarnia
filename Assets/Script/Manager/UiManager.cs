using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager
{
    // ���̽�ƽ
    public JoystickController _joyStickController;
    public GameObject _joyStick;
    // �ó׸ӽ�
    public CMCameraController _cmCamController;

    public void Init()
    {
        //_joyStick = new GameObject();
        _joyStickController = _joyStick.GetComponent<JoystickController>();
    }
}
