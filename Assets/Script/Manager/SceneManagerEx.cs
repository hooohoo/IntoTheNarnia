using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    // �� �̸� Ȯ�ο�
    public string _LoadSceneName;
    // �ٸ� �Ŵ������� ���� �� Ȯ���ϴ� �� Enum
    public Define.SceneName _SceneNameEnum;

    // �ε� �� �ҷ����� �Լ�
    // LoadScene(Enum.toString()) -> �̷� �������� ���� ��
    public void LoadScene(string sceneName)
    {
        _LoadSceneName = sceneName;
        SceneCheck();
        SceneManager.LoadSceneAsync(Define.SceneName.LoadingScene.ToString());
    }

    // ���� ��� ������ Ȯ���ϴ� �Լ�
    // _SceneNameEnum�� ���
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
