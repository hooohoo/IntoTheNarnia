using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 미니맵 컨트롤러
public class MiniMapController : MonoBehaviour
{
    // 맵 범위 크기의 Plane 오브젝트 크기
    public Vector3 _worldSize;
    // 미니맵 ScorllRect
    public ScrollRect _scrollRect;
    // 플레이어 아이콘(현재 원모양)
    public GameObject _playerIcon;
    // 맵 범위 크기의 Plane 오브젝트
    public GameObject _plane;
    // 플레이어 오브젝트
    public GameObject _playerObject;

    void Start()
    {
        // 시작하자 마자 맵 중앙으로 표시, but 의미 없음
        _scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
        // Plane의 사이즈 가져옴
        _worldSize = _plane.transform.GetComponent<MeshCollider>().bounds.size;
        // 플레이어 오브젝트 값 넣어줌
        _playerObject = GameManager.Obj._player.gameObject;
    }

    void Update()
    {
        // 맵 위치 실시간 변경
        TryCalculate();
        // 플레이어 아이콘 회전
        RotatePlayerIcon();
    }

    // 맵 위치 계산하는 함수
    void TryCalculate()
    {
        // Player의 Plane에 상대적인 위치값
        // Plane(부모) - Player(자식) 관계라면 localPosition으로 가져올 수 있지만 캐릭터 스케일 값이 바뀌어서
        // 이렇게 상대위치 구해줌
        Vector3 relativePos = _playerObject.transform.position - _plane.transform.position;
        
        // ScrollRect의 Pivot이 좌하단이기 때문에 위치 값에 Plane의 가로 세로 절반을 각각 더해줌
        relativePos.x += _worldSize.x * 0.5f;
        relativePos.z += _worldSize.z * 0.5f;

        // 현재 플레이어의 위치를 비율로 알기 위하여 InverseLerp 함수 사용(0 ~ 1 사이값)
        float playerX = Mathf.InverseLerp(0, _worldSize.x, relativePos.x);
        float playerY = Mathf.InverseLerp(0, _worldSize.z, relativePos.z);
        // 맵의 위치 설정
        _scrollRect.normalizedPosition = new Vector2(playerX, playerY);
    }

    // 플레이어 아이콘 회전시키는 함수
    void RotatePlayerIcon()
    {
        // 플레이어 오브젝트 회전 y값 카메라가 월드 기준 반대 방향을 보고 있기 때문에 360에서 빼준 값
        float rotateValue = 360f - _playerObject.transform.rotation.eulerAngles.y;
        // 플레이어 아이콘 RectTransform 가져옴
        RectTransform iconRect = _playerIcon.GetComponent<RectTransform>();
        // 아이콘 rotation z값 바꿔줌
        iconRect.rotation = Quaternion.Euler(0, 0, rotateValue);
    }
}
