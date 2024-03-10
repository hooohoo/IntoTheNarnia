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
    // ���� ��ư
    public GameObject _settingButton;
    public GameObject _settingWindow;
    // �׼� ��ư
    public GameObject _actionButton;
    public ActionButtonController _actionButtonController;
    // �̴ϸ�
    public GameObject _miniMap;
    public MiniMapController _miniMapController;
    // ��ȭâ
    public GameObject _messageBox;
    public MessageBoxController _messageController;

    public void Init()
    {
        // ���̽�ƽ ��Ʈ�ѷ� ��ũ��Ʈ
        _joyStickController = _joyStick.GetComponent<JoystickController>();
        // ���� â
        _settingWindow = _settingButton.transform.Find(Define.UIName.SettingWindow.ToString()).gameObject;
        // ���� â �ε��� �� ��Ȱ��ȭ
        _settingWindow.SetActive(false);
        // �׼� ��ư ��Ʈ�ѷ� ��ũ��Ʈ
        _actionButtonController = _actionButton.GetComponent<ActionButtonController>();
        // �̴ϸ� ��Ʈ�ѷ� ��ũ��Ʈ
        _miniMapController = _miniMap.GetComponent<MiniMapController>();
        // ��ȭâ ��Ʈ�ѷ� ��ũ��Ʈ
        _messageController = _messageBox.GetComponent<MessageBoxController>();
        // ��ȭâ �ε��� �� ��Ȱ��ȭ
        _messageBox.SetActive(false);
    }

    // ��� UI ����
    public void AllUIOff()
    {
        _joyStick.SetActive(false);
        _settingButton.SetActive(false);
        _actionButton.SetActive(false);
        _miniMap.SetActive(false);
    }

    // ��� UI �ѱ�
    public void AllUIOn()
    {
        _joyStick.SetActive(true);
        _settingButton.SetActive(true);
        _actionButton.SetActive(true);
        _miniMap.SetActive(true);
    }
}
