using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [SerializeField]
    // �ε��� ��� �̹���
    Image _BackgroundIMG;

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
    }

    IEnumerator LoadSceneProgress()
    {
        float timer = 0f;
        yield return null;
    }
}
