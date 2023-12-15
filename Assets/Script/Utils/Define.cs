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
        Walk,
        Run,
        Attack,
        Dead,
        None,
    }

    public enum JoystickState
    {
        InputFalse,
        InputTrue,
    }
}
