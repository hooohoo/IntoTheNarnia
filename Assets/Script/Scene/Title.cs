using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    // 시작 버튼
    public void StartButton()
    {
        GameManager.Scene.LoadScene(Define.SceneName.Professor_House);
    }

    // Continue 버튼
    public void ContinueButton()
    {
        // todo
    }

    // 설정 버튼
    public void SettingButton()
    {
        // todo
    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }


}
