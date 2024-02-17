using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 캐릭터 등 오브젝트를 씬에 실제로 만드는 함수(Instantiate() 사용)
public class CreateManager
{
    // 플레이어 캐릭터
    private PlayerController _player;

    // 플레이어 캐릭터 생성
    public PlayerController CreatePlayer(Vector3 origin, string characterName)
    {
        // 위에서 레이를 쏴서 지형 높이에 따른 캐릭터 생성 코드
        origin.y += 100f;
        RaycastHit hit;
        if(Physics.Raycast(origin, -Vector3.up, out hit, Mathf.Infinity))
        {
            GameObject tempObj = GameManager.Resource.GetPlayer(characterName);
            // null 체크 하기
            if(tempObj != null)
            {
                GameObject player = GameObject.Instantiate<GameObject>(tempObj, hit.point, Quaternion.identity);
                // 프리팹에 부착되어있는 PlayerController 스크립트 가져오기
                _player = player.GetComponent<PlayerController>();
                // 이름 설정, 이름은 각 씬에서 넘어옴, 추후 퀘스트매니저로 관리할수도 있음
                _player._characterName = characterName;
            }
        }
        return _player;
    }
}
