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

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // 태그에 따라 상호작용 혹은 공격하는 함수
    private void DoActionOrDoAttack(TagName tag)
    {
        //
        switch(tag)
        {
            case TagName.Structure:
                break;
            case TagName.Monster:
                break;
            default:
                break;
        }
    }
}
