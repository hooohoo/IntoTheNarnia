using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 플레이어 오브젝트
    public GameObject _Player;
    // 카메라 원래 위치 담을 변수
    Vector3 _CameraPos;
    // 플레이어의 직전 위치
    Vector3 _OldPos;

    void Start()
    {
        // 임시로 쓸 코드
        _CameraPos = transform.position;
        _OldPos = _Player.transform.position;
    }

    void Update()
    {
        FollowingPlayer();
    }

    // 플레이어 따라가는 함수
    void FollowingPlayer()
    {
        Vector3 delta = _Player.transform.position - _OldPos;
        transform.position += delta;
        _OldPos = _Player.transform.position;
    }
}
