using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// @UI�� �־ ��ư �Լ��鿡 ����� �Ŵ��� Ŭ����
public class ButtonManager : MonoBehaviour
{
    void Start(){}

    void Update(){}

    // ����â On
    public void SettingWindowOn()
    {
        GameManager.Ui._settingWindow.SetActive(true);
    }

    // ����â Off
    public void SettingWindowOff()
    {
        GameManager.Ui._settingWindow.SetActive(false);
    }

    // �׼� ��ư
    public void ActionButton()
    {
        //
    }
}
