using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��� ����
public class Define
{
    public enum SceneName
    {
        Title,
        Tutorial,
        LoadingScene
    }

    public enum CreatureState
    {
        Idle,
        Move,
        AutoMove,
        Skill,
        Skill2,
        Skill3,
        Rolling,
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
