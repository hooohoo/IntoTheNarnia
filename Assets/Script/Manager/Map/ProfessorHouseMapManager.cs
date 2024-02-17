using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Professor_House 씬 관련 Manager
// 플레이어 시작 위치와 동일하게 배치할 것
public class ProfessorHouseMapManager : MonoBehaviour
{
    // 씬 내부에 있는 문 리스트
    private DoorController[] _doorList;

    // 옷장있는 방 문(오브젝트 : "TheWardrobeRoomDoor")
    private DoorController _theWardRobeRoomDoor;
    // 사형제 방 문(오브젝트 : "TheSiblingsRoomDoor")
    private DoorController _theSiblingsRoomDoor;

    // 플레이어 생성될 위치 == 이 오브젝트 위치한 곳
    private Vector3 _startPos;

    void Start()
    {
        // 초기화
        Init();
    }

    void Update()
    {
        //
    }

    // 초기화
    private void Init()
    {
        // 시작위치 담기
        _startPos = transform.position;
        
        // 플레이어 생성
        //CreatePlayer();
        
        // Find 함수 사용하지 않는 것이 좋지만.. 씬 내부에 있는 모든 door 가져오기 위해 사용
        _doorList = FindObjectsOfType<DoorController>();
        
        // 주요 오브젝트들 따로 저장하기
        CollectMainObject();
    }

    // 플레이어 생성 코드
    private void CreatePlayer()
    {
        // 현재 맵에서 주인공은 Lucy
        GameManager.Obj._player = GameManager.Create.CreatePlayer(_startPos, CharacterName.Lucy.ToString());
    }

    // _doorList에서 핵심 오브젝트 빼서 정리하기
    private void CollectMainObject()
    {
        foreach(DoorController one in _doorList)
        {
            // 옷장 방 문 저장
            if(one.name.Equals(ObjectName.TheWardrobeRoomDoor.ToString()))
            {
                _theWardRobeRoomDoor = one;
            }
            // 사형제 방 문 저장
            if(one.name.Equals(ObjectName.TheSiblingsRoomDoor.ToString()))
            {
                _theSiblingsRoomDoor = one;
            }
        }
    }
}
