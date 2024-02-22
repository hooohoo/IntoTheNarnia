using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class SceneManagerEx// : MonoBehaviour
{
    // �� �̸� Ȯ�ο�
    public string _LoadSceneName;
    // �ٸ� �Ŵ������� ���� �� Ȯ���ϴ� �� Enum
    public SceneName _SceneNameEnum;

    // �ε� �� �ҷ����� �Լ�
    // LoadScene(Define.SceneName) -> �̷� �������� ���� ��
    public void LoadScene(SceneName sceneName)
    {
        // �ε��� �� �̸� enum���� string���� ��ȯ
        _LoadSceneName = sceneName.ToString();
        SceneCheck();
        // �ε��� �� ����
        SceneManager.LoadSceneAsync(SceneName.LoadingScene.ToString());
    }

    // �Ƹ� �ʿ� ���� �� ����...
    // ���� ��� ������ Ȯ���ϴ� �Լ�
    // _SceneNameEnum�� ���
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
