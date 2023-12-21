using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

// 조이스틱 컨트롤러
// 카메라가 바라보는 방향이 앞. 카메라와 캐릭터가 마주보고 있다면 ↓방향으로 눌러야 캐릭터가 카메라 쪽으로 다가옴
public class JoystickController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 레버위치 구현용
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    // 레버위치 조정용
    [SerializeField, Range(10, 150)]
    private float leverRange;
    // 레버 상태 확인용
    public JoystickState _joystickState;
    // 인풋 방향
    public Vector2 inputDirection;
    // 조이스틱 캔버스
    private Canvas _canvas;
    private void Awake()
    {
        // Pan ract
        rectTransform = GetComponent<RectTransform>();
        // Joystick 오브젝트
        parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        // 레버 상태 초기값 false
        _joystickState = JoystickState.InputFalse;
        // 조이스틱 캔버스에 값 담기, 현재 transform은 Pan
        _canvas = parentRectTransform.GetComponent<Canvas>();
    }

    private void Update()
    {
        if (_joystickState == JoystickState.InputTrue)
        {
            InputControlVector();
        }
    }
    // 레버 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        _joystickState = JoystickState.InputTrue;
    }
    // 레버 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        //Debug.Log("OnDrag-eventData.Position : " + eventData.position);
        _joystickState = JoystickState.InputTrue;
    }
    // 레버 드래그 끝
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 inputPos = Vector2.zero;
        Vector2 inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        lever.anchoredPosition = Vector2.zero;
        inputDirection = inputVector / leverRange;
        _joystickState = JoystickState.InputFalse;
    }
    // 조이스틱 움직이는 함수
    public void ControlJoystickLever(PointerEventData eventData)
    {
        // 클릭(터치) 지점 임시 저장, 스크린 좌표
        Vector2 inputPos = eventData.position;
        // 스크린 좌표에서 로컬 값으로 변경해서 담을 변수
        Vector2 inputVector;
        // 스크린 좌표(원점이 좌하단)에서 로컬 rect 값으로 out 값 담는 부분
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPos, _canvas.worldCamera, out inputVector);
        // 레버 범위 나갔을 때 범위 내 최댓값으로 벡터 설정
        if(inputVector.magnitude > leverRange)
            inputVector = inputVector.normalized * leverRange;
        // 레버 위치 동기화
        lever.anchoredPosition = inputVector;
        inputDirection = inputVector / leverRange;
    }
    // 조이스틱의 방향 계산
    public Vector2 InputControlVector()
    {
        _joystickState = JoystickState.InputTrue;
        //Debug.Log("조이스틱 : " + inputDirection);
        return inputDirection;
    }
}