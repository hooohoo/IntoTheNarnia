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
        Professor_House,
        TheSiblingsRoom,
        TheWardrobeRoom,
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
        MessageBox,
    }

    public enum Parameter
    {
        State
    }

    public enum ObjectName
    {
        TheWardrobeRoomDoor,
        TheSiblingsRoomDoor,
        Everything,
    }

    public enum CharacterName
    {
        Lucy,
        Edmund,
        Susan,
        Peter,
        ProfessorDigoryKirke,
        MrsMacready,
        System,
    }

    public enum CameraName
    {
        CM_Camera
    }

    public enum TagName
    {
        Player,
        Structure,
        Monster,
        None,
    }
}
