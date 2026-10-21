using UnityEngine;

public sealed class FieldScreen
{
    private readonly GameObject joystick;
    private readonly GameObject action;
    private readonly GameObject settings;
    private readonly GameObject settingWindow;
    private readonly GameObject miniMap;
    private readonly MessageBoxController dialogue;
    public FieldScreen(GameObject root)
    {
        joystick = SceneBinding.NamedChild(root, nameof(Define.UIName.Joystick));
        action = SceneBinding.NamedChild(root, nameof(Define.UIName.ActionButton));
        settings = SceneBinding.NamedChild(root, nameof(Define.UIName.SettingButton));
        settingWindow = SceneBinding.NamedChild(settings, nameof(Define.UIName.SettingWindow));
        miniMap = SceneBinding.NamedChild(root, nameof(Define.UIName.MiniMap));
        dialogue = SceneBinding.NamedChild(root, nameof(Define.UIName.MessageBox)).GetComponent<MessageBoxController>();
        if (dialogue == null) throw new MissingComponentException("MessageBox requires MessageBoxController.");
        settingWindow.SetActive(false);
        dialogue.gameObject.SetActive(false);
    }
    public void Show(GameStateType state)
    {
        bool exploration = state == GameStateType.Exploration;
        joystick.SetActive(exploration);
        action.SetActive(exploration);
        miniMap.SetActive(exploration || state == GameStateType.Settings);
        // Keep the parent active while the close button in its child popup is needed.
        settings.SetActive(exploration || state == GameStateType.Settings);
        settingWindow.SetActive(state == GameStateType.Settings);
    }
    public void ShowDialogue(string speaker, string[] lines) { dialogue.SetMessageBox(speaker, lines); }
    public void Release()
    {
        if (dialogue != null) dialogue.gameObject.SetActive(false);
        if (settingWindow != null) settingWindow.SetActive(false);
    }
}
