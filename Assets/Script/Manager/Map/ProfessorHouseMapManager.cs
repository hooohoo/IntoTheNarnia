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
    private DoorController _theWardrobeRoomDoor;
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

        // 퀘스트 여기서 하면 안됨... 위치 찾아야, Save랑 연동하기
        //Quest_1();
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

        // TheWardrobeRoom, TheSiblingsRoom 잠가두기, 퀘스트 진행할 때 열 것
        _theWardrobeRoomDoor._openOrLocked = false;
        _theSiblingsRoomDoor._openOrLocked = false;

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
                _theWardrobeRoomDoor = one;
            }
            // 사형제 방 문 저장
            if(one.name.Equals(ObjectName.TheSiblingsRoomDoor.ToString()))
            {
                _theSiblingsRoomDoor = one;
            }
        }
    }// end CollectMainObject()

    // Professor_House 에서 진행할 퀘스트
    // 1. 형제방(TheSiblingsRoom) 찾아가기
    // 2. 숨바꼭질하면서 옷장있는 방(TheWardrobeRoom) 들어가기
    private void Quest_1()
    {
        // #1
        // 형제방 잠금 해제
        _theSiblingsRoomDoor._openOrLocked = true;
        // 성공했을 때 탈출하도록
        while(!GameManager.Quest._successOrNot)
        {
            // 형제방(TheSiblingsRoom) 문 열리면 퀘스트 성공
            if(_theSiblingsRoomDoor._openOrClose)
            {
                // 성공메세지 UI로 바꿀 것
                Debug.Log("첫 번째 퀘스트 성공!");
                GameManager.Quest._successOrNot = true;
            }
        }
        // 다음 퀘스트를 위해 false로 재설정
        GameManager.Quest._successOrNot = false;
    }

    private void Quest_2()
    {
        // #2
        // 옷장 방 잠금 해제
        _theWardrobeRoomDoor._openOrLocked = true;
        // 성공했을 때 탈출하도록
        while(!GameManager.Quest._successOrNot)
        {
            // 옷장 방(TheWardRobeRoom) 문 열리면 퀘스트 성공
            if(_theWardrobeRoomDoor._openOrClose)
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
