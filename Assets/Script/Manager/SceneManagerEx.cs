using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx// : MonoBehaviour
{
    // 씬 이름 확인용
    public string _LoadSceneName;
    // 다른 매니저에서 현재 씬 확인하는 용 Enum
    public Define.SceneName _SceneNameEnum;

    // 로딩 씬 불러오는 함수
    // LoadScene(Enum.toString()) -> 이런 형식으로 사용될 것
    public void LoadScene(string sceneName)
    {
        _LoadSceneName = sceneName;
        SceneCheck();
        SceneManager.LoadSceneAsync(Define.SceneName.LoadingScene.ToString());
    }

    // 현재 어느 씬인지 확인하는 함수
    // _SceneNameEnum에 담김
    public void SceneCheck()
    {
        switch(_LoadSceneName)
        {
            case "Title":
                _SceneNameEnum = Define.SceneName.Title;
                break;
        }
    }
}
