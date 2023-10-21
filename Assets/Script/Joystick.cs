using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour
{

    public static Joystick instance;

    // Image는 컴포넌트이다!! 게임오브젝트 아님!!!!
    public Image Lever;
    // 이렇게 RectTransform 타입으로 설정하면 아래에서 transform 안거치고 position 접근 가능
    //public RectTransform Lever;

    Vector3 startPos;
    public Vector3 dir { get; set; }
    float radius;
    public RectTransform rcTr;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        // 나중에 계산하기 위해서 처음 스틱 위치 보관
        startPos = Lever.transform.position;
        // 큰 원의 반지름 구해옴 
        // x 대신에 width, hight 로 가져올 수도 있음
        radius = rcTr.sizeDelta.x * 0.5f;

        dir = Vector3.zero;
    }

    void Update()
    {

    }

    public void OnPointerDown(BaseEventData _eventData)
    {
        // Down 위치를 알고자 한다면
        PointerEventData eventData = (PointerEventData)_eventData;
        Debug.Log("누른 위치 : " + eventData.position);

        Lever.transform.position = eventData.position;

        // RectTransform 으로 정의한 경우 대입법
        //Lever.position = eventData.position;
    }

    public void OnPointerUp(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        //Lever.transform.position = transform.position;
        Lever.transform.position = startPos;

        // 손 떼면 0되도록
        dir = Vector3.zero;
    }

    // 겉보기에는 포인터 엔터랑 다를 바 없어보이지만... 내부적으로 다르다고 한다.
    public void OnBeginDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        Lever.transform.position = eventData.position;
    }

    public void OnDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        // vector2 로 형변환 시키면 z축이 잘려나감
        dir = eventData.position - (Vector2)startPos;
        
        // distance로 사용하는 경우도 있음
        float distance = Vector3.Distance(startPos, eventData.position);

        // dir.sqrMagnitude : 루트씌운 값
        // 드래그한 목적지가 반지름보다 큰 상황

        // distance로 사용할 수 있음
        if(distance > radius)
        //if(dir.magnitude > radius)    // sqrMagnitude? magnitude?
        {
            Lever.transform.position = startPos + dir.normalized * radius;
        }
        else
        {
            // 작동이 좀 이상함.. 무조건 외곽으로만 감.. distance사용하자...
            //Lever.transform.position = eventData.position;

            // distance 사용할 때는 이렇게
            Lever.transform.position = startPos + dir.normalized * distance;
        }
    }

    public void OnEndDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        Lever.transform.position = startPos;

        // 손 떼면 0되도록
        dir = Vector3.zero;
    }
}
