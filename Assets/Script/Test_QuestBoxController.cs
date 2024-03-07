using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_QuestBoxController : MonoBehaviour
{
    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // Test Code
    public void OnTriggerEnter(Collider collider)
    {
        // 플레이어 콜라이더와 충돌했을 때
        if(collider.CompareTag(Define.TagName.Player.ToString()))
        {
            Debug.Log("들어옴");
            // #1
            // 형제방 잠금 해제
            GameManager.Obj._theSiblingsRoomDoor._openOrLocked = true;
            // 성공했을 때 탈출하도록
            while(!GameManager.Quest._successOrNot)
            {
                Debug.Log("퀘스트 진행중");
                // 형제방(TheSiblingsRoom) 문 열리면 퀘스트 성공
                if(GameManager.Obj._theSiblingsRoomDoor._openOrClose)
                {
                    // 성공메세지 UI로 바꿀 것
                    Debug.Log("첫 번째 퀘스트 성공!");
                    GameManager.Quest._successOrNot = true;
                }
            }
            // 다음 퀘스트를 위해 false로 재설정
            GameManager.Quest._successOrNot = false;
        }
    }
}
