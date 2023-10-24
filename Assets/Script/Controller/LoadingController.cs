using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    // �ε��� ��� �̹���
    [SerializeField]
    Image _BackgroundIMG;
    
    // �ε��� progress �̹���
    [SerializeField]
    Image _LoadingProgressIMG;

    // �ε� ����
    AsyncOperation _Operation;

    void Start()
    {
        int randomCode = Random.Range(0, 3);
        Sprite backgroundSprite;
        switch(randomCode)
        {
            // 0�̸� ���Ͼ� ǳ��
            case 0:
                backgroundSprite = GameManager.Resource.GetImage("NarniaLandscape");
                _BackgroundIMG.sprite = backgroundSprite;
                break;
            // 1�̸� ����
            case 1:
                backgroundSprite = GameManager.Resource.GetImage("TheWhiteWitch");
                _BackgroundIMG.sprite = backgroundSprite;
                break;
            // 2�̸� ����
            case 2:
                backgroundSprite = GameManager.Resource.GetImage("TheWardrobe");
                _BackgroundIMG.sprite = backgroundSprite;
                break;
        }
        // �ε� �ڷ�ƾ ����
        StartCoroutine("LoadSceneProcess");
    }

    // �ε�ȭ�� �����ָ鼭 �ڿ� �ε��� �� �غ� �ڷ�ƾ
    IEnumerator LoadSceneProcess()
    {
        // Ÿ�̸� ����
        float timer = 0f;
        // �ε��� ���� �񵿱� ������� �ε��� �� ȣ�� ���� ����ش�.
        _Operation = SceneManager.LoadSceneAsync(GameManager.Scene._LoadSceneName);
        // �� Ȱ��ȭ false
        _Operation.allowSceneActivation = false;

        // �� ȣ�� �Ϸ� �ȵƴٸ� �ݺ�
        while(!_Operation.isDone)
        {
            yield return null;
            // ȣ�� ���� 0.9f �̸��� ��
            if(_Operation.progress < 0.9f)
            {
                // ���� ������� filling���� ������
                _LoadingProgressIMG.fillAmount = _Operation.progress;
            }
            else
            {
                // �ð��� deltaTime ����
                timer += Time.unscaledDeltaTime;
                // ���� ������� 0.9f ~ 1f ���� �����ֱ� ���� Lerp �Լ� ���
                _LoadingProgressIMG.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                // �ε��� �� á����
                if(_LoadingProgressIMG.fillAmount >= 1f)
                {
                    // �� Ȱ��ȭ
                    _Operation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
