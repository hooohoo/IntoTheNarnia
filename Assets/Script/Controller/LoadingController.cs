using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    // 로딩씬 배경 이미지
    [SerializeField]
    Image _BackgroundIMG;
    
    // 로딩바 progress 이미지
    [SerializeField]
    Image _LoadingProgressIMG;

    // 로딩 변수
    AsyncOperation _Operation;

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
        // 로딩 코루틴 시작
        StartCoroutine("LoadSceneProcess");
    }

    // 로딩화면 보여주면서 뒤에 로드할 씬 준비 코루틴
    IEnumerator LoadSceneProcess()
    {
        // 타이머 세팅
        float timer = 0f;
        // 로딩씬 이후 비동기 방식으로 로드할 씬 호출 상태 담아준다.
        _Operation = SceneManager.LoadSceneAsync(GameManager.Scene._LoadSceneName);
        // 씬 활성화 false
        _Operation.allowSceneActivation = false;

        // 씬 호출 완료 안됐다면 반복
        while(!_Operation.isDone)
        {
            yield return null;
            // 호출 상태 0.9f 미만일 때
            if(_Operation.progress < 0.9f)
            {
                // 현재 진행상태 filling으로 보여줌
                _LoadingProgressIMG.fillAmount = _Operation.progress;
            }
            else
            {
                // 시간에 deltaTime 누적
                timer += Time.unscaledDeltaTime;
                // 현재 진행상태 0.9f ~ 1f 사이 보여주기 위해 Lerp 함수 사용
                _LoadingProgressIMG.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                // 로딩바 다 찼으면
                if(_LoadingProgressIMG.fillAmount >= 1f)
                {
                    // 씬 활성화
                    _Operation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
