using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상수 정의
public class Define
{
    public enum SceneName
    {
        Title,
        Tutorial,
        Professor_House,
        LoadingScene
    }

    public enum CreatureState
    {
        Idle,
        Move,
        // Walk = 2
        // Run = 3
        Jump,
        Attack,
        Dead,
        None,
    }

    public enum JoystickState
    {
        InputFalse,
        InputTrue,
    }

    public enum UIName
    {
        Joystick,
        ActionButton,
        SettingButton,
        SettingWindow,
        MiniMap,
    }
}
