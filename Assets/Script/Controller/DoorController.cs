using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 문 열림/닫힘, 문 손잡이 컨트롤러
// 액션버튼으로 컨트롤
public class DoorController : MonoBehaviour
{
    // 문 손잡이 오브젝트
    public GameObject _doorHandle;

    // 문 애니메이터
    private Animator _doorAnimator;
    // 문 손잡이 애니메이터
    private Animator _doorHandleAnimator;

    // 애니메이터 파라미터
    private string _animParameter;

    // 문 열림 or 닫힘
    private bool _openOrNot;

    void Start()
    {
        Init();
    }

    void Update()
    {
        // 임시코드
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ShowDoorLocked();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            ShowDoorOpen();
        }
    }

    // 초기화
    private void Init()
    {
        // 디폴트는 잠김상태
        _openOrNot = false;
        // 문 손잡이 애니메이터 넣어주기
        _doorHandleAnimator = _doorHandle.GetComponent<Animator>();
        // 파라미터 변수 넣어주기, "State"
        _animParameter = Define.Parameter.State.ToString();
    }

    // 문열기
    // 잠겨있으면 잠겨있다는 표시
    public void OpenDoor()
    {
        // 열려있는지 확인
        if(_openOrNot)
        {
            // 문열기
            ShowDoorOpen();
        }
        else
        {
            // 닫혀있습니다
            ShowDoorLocked();
        }
    }

    // 문잠김 상태 애니메이션 플레이(손잡이)
    public void ShowDoorLocked()
    {
        // 잠김(State, 1)
        _doorHandleAnimator.SetInteger(_animParameter, 1);
        StartCoroutine("ReturnToIdle");
    }

    // 문열림 상태 애니메이션 플레이(문, 손잡이 모두)
    public void ShowDoorOpen()
    {
        // 플레이어 캐릭터 + 시네머신 움직임 삽입할 것인지 추후 결정

        // 열림(State, 2) : 손잡이
        _doorHandleAnimator.SetInteger(_animParameter, 2);
        StartCoroutine("ReturnToIdle");
        // 열림(State, ??) : 문
        //_doorAnimator.SetInteger(_animParameter, );
    }

    // 손잡이 모션 이후 다시 Idle로 상태 전환하기 위한 코루틴
    private IEnumerator ReturnToIdle()
    {
        // 1초 기다리기
        yield return new WaitForSeconds(1f);
        // Idle 상태로 전환
        _doorHandleAnimator.SetInteger(_animParameter, 0);
    }

    // 플레이어와 충돌체크
    private void OnTriggerEnter(Collider collider)
    {
        // 플레이어 콜라이더와 충돌했을 때
        if(collider.CompareTag(Define.TagName.Player.ToString()))
        {
            // 디버깅용
            string msg = _openOrNot ? "열려있습니다." : "문 닫힘.";
            Debug.Log(transform.name + " : " + msg);
            // 액션버튼 태그에 Structure 넣기
            GameManager.Ui._actionButtonController._verifyTag = Define.TagName.Structure;
        }
    }
}
