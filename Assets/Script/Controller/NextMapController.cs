using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 다음 맵으로 이동할 때 이용되는 컨트롤러
// BoxCollider, Rigidbody 적용된 오브젝트에 부착되어 사용
public class NextMapController : MonoBehaviour
{
    // 연결된 씬 이름
    public Define.SceneName _linkedScene;

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // 현재 어떤 씬에 있었는지 체크하는 함수
    private void RecentSceneCheck()
    {
        //
    }

    // 플레이어 콜라이더 부딪히면 다음 맵으로 넘어가기
    private void OnTriggerEnter(Collider other)
    {
        // 태그가 Player 이면
        if(other.CompareTag(Define.TagName.Player.ToString()))
        {
            // 씬 로드
            GameManager.Scene.LoadScene(_linkedScene);
        }
    }
}
