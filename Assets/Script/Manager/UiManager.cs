using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager
{
    // ¡∂¿ÃΩ∫∆Ω
    public JoystickController _joyStickController;
    public GameObject _joyStick;

    public void Init()
    {
        //_joyStick = new GameObject();
        _joyStickController = _joyStick.GetComponentInChildren<JoystickController>();
    }
}
