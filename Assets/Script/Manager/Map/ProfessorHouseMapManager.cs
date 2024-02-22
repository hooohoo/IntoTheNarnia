using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Professor_House 씬 관련 Manager
public class ProfessorHouseMapManager : MonoBehaviour
{
    // 씬 내부에 있는 문 리스트
    private DoorController[] _doorList;

    // 옷장있는 방 문(오브젝트 : "TheWardrobeRoomDoor")
    private DoorController _theWardRobeRoomDoor;
    // 사형제 방 문(오브젝트 : "TheSiblingsRoomDoor")
    private DoorController _theSiblingsRoomDoor;

    // 포탈
    // SiblingsRoom 으로 이동하는 포탈
    public NextMapController _theSiblingsRoomPortal;
    // WardrobeRoom 으로 이동하는 포탈
    public NextMapController _theWardrobeRoomPortal;

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
        // Find 함수 사용하지 않는 것이 좋지만.. 씬 내부에 있는 모든 door 가져오기 위해 사용
        _doorList = FindObjectsOfType<DoorController>();
        
        // 주요 오브젝트들 따로 저장하기
        CollectMainObject();

        // TheWardrobeRoom, TheSiblingsRoom 열어두기
        _theWardRobeRoomDoor._openOrLocked = true;
        _theSiblingsRoomDoor._openOrLocked = true;

        // 포탈에 씬 연결해두기
        _theSiblingsRoomPortal._linkedScene = SceneName.TheSiblingsRoom;
        _theWardrobeRoomPortal._linkedScene = SceneName.TheWardrobeRoom;
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
