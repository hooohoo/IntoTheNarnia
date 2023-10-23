using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [SerializeField]
    // 로딩씬 배경 이미지
    Image _BackgroundIMG;

    void Start()
    {
        int randomCode = Random.Range(0, 3);
        Sprite backgroundSprite;
        switch(randomCode)
        {
            // 0이면 나니아 풍경
            case 0:
                backgroundSprite = GameManager.Resource.GetImage("NarniaLandscape");
                _BackgroundIMG.sprite = backgroundSprite;
                break;
            // 1이면 마녀
            case 1:
                backgroundSprite = GameManager.Resource.GetImage("TheWhiteWitch");
                _BackgroundIMG.sprite = backgroundSprite;
                break;
            // 2이면 옷장
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
