using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class SceneManagerEx// : MonoBehaviour
{
    // 씬 이름 확인용
    public string _LoadSceneName;
    // 다른 매니저에서 현재 씬 확인하는 용 Enum
    public SceneName _SceneNameEnum;

    // 로딩 씬 불러오는 함수
    // LoadScene(Define.SceneName) -> 이런 형식으로 사용될 것
    public void LoadScene(SceneName sceneName)
    {
        // 로드할 씬 이름 enum에서 string으로 변환
        _LoadSceneName = sceneName.ToString();
        SceneCheck();
        // 로딩중 씬 시작
        SceneManager.LoadSceneAsync(SceneName.LoadingScene.ToString());
    }

    // 아마 필요 없을 것 같은...
    // 현재 어느 씬인지 확인하는 함수
    // _SceneNameEnum에 담김
    public void SceneCheck()
    {
        switch(_LoadSceneName)
        {
            case "Title":
                _SceneNameEnum = SceneName.Title;
                break;
            case "Professor_House":
                _SceneNameEnum = SceneName.Professor_House;
                break;
            case "TheSiblingsRoom":
                _SceneNameEnum = SceneName.TheSiblingsRoom;
                break;
            case "TheWardrobeRoom":
                _SceneNameEnum = SceneName.TheWardrobeRoom;
                break;
        }
    }
}
