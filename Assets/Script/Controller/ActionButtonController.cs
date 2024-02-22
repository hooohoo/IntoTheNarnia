using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// 액션 버튼(상호작용 버튼) 컨트롤러
// 태그로 구분하여 Structure 이면 상호작용, Monster 이면 공격으로 전환하는 컨트롤러
public class ActionButtonController : MonoBehaviour
{
    // 오브젝트가 구조물인지 몬스터인지 구분하기 위한 변수
    public TagName _verifyTag;
    // 현재 플레이어가 접근한 문
    public DoorController _thisDoor;

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // 태그에 따라 상호작용 혹은 공격하는 함수
    public void DoActionOrDoAttack()
    {
        //
        switch(_verifyTag)
        {
            // 구조물일 경우, 현재는 문
            case TagName.Structure:
                // 문열기, 잠겨있으면 잠긴 애니메이션
                if(_thisDoor != null)
                {
                    _thisDoor.OpenAndCloseDoor();
                }
                // 끝났으면 태그 비워주기
                _verifyTag = TagName.None;
                break;
            // 몬스터일 경우, 공격
            case TagName.Monster:
                // todo
                // 끝났으면 태그 비워주기
                _verifyTag = TagName.None;
                break;
            // _verifyTag가 없으면 아무것도 하지 않음
            default:
                break;
        }
    }
}
