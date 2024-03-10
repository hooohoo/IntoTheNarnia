using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 퀘스트 매니저
// 퀘스트 관련 기능들 모아놓은 매니저
public class QuestManager
{
    // 퀘스트 레벨
    // 플레이어 저장 파일에서 가져오기
    private static int _questLevel;

    // 플레이어 네비메시 에이전트 컴포넌트
    private NavMeshAgent _playerNavmeshAgent;
    // 에이전트 Area Mask
    private int _areaMask;

    // 성공여부 확인하는 변수, 성공은 true 실패는 false
    public bool _successOrNot;

    // _questLevel Getter
    public static int QuestLevel
    {
        get { return _questLevel; }
    }
    // 초기화
    public void Init()
    {
        // 임시 코드, 후에 저장 파일에서 불러온 값 넣어야 함
        // 레벨 초기화
        _questLevel = 0;

        // 네비메시 에이전트 넣어줌
        _playerNavmeshAgent = GameManager.Obj._player.GetComponent<NavMeshAgent>();
        // 에이전트 Area Mask 넣어줌
        _areaMask = _playerNavmeshAgent.areaMask;
        // 성공변수에 값 넣기
        _successOrNot = false;
    }

    // 퀘스트 레벨 업 함수
    public void QuestLevelUp()
    {
        _questLevel++;
    }

    // 열려야 하는 문 가져와서 해당 영역 AreaMask에 추가하는 함수
    // string 타입으로 가져옴
    public void OpenDoor(string objName)
    {
        // Area Mask에 더해줌 == 플레이어가 다닐 수 있는 영역으로 설정
        // GetAreaFromName("Everything") 은 모든 영역으로 설정
        _areaMask += 1 << NavMesh.GetAreaFromName(objName);
        _playerNavmeshAgent.areaMask = _areaMask;
    }
}
