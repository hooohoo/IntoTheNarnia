using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour
{

    public static Joystick instance;

    // Image�� ������Ʈ�̴�!! ���ӿ�����Ʈ �ƴ�!!!!
    public Image Lever;
    // �̷��� RectTransform Ÿ������ �����ϸ� �Ʒ����� transform �Ȱ�ġ�� position ���� ����
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
        // ���߿� ����ϱ� ���ؼ� ó�� ��ƽ ��ġ ����
        startPos = Lever.transform.position;
        // ū ���� ������ ���ؿ� 
        // x ��ſ� width, hight �� ������ ���� ����
        radius = rcTr.sizeDelta.x * 0.5f;

        dir = Vector3.zero;
    }

    void Update()
    {

    }

    public void OnPointerDown(BaseEventData _eventData)
    {
        // Down ��ġ�� �˰��� �Ѵٸ�
        PointerEventData eventData = (PointerEventData)_eventData;
        Debug.Log("���� ��ġ : " + eventData.position);

        Lever.transform.position = eventData.position;

        // RectTransform ���� ������ ��� ���Թ�
        //Lever.position = eventData.position;
    }

    public void OnPointerUp(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        //Lever.transform.position = transform.position;
        Lever.transform.position = startPos;

        // �� ���� 0�ǵ���
        dir = Vector3.zero;
    }

    // �Ѻ��⿡�� ������ ���Ͷ� �ٸ� �� ���������... ���������� �ٸ��ٰ� �Ѵ�.
    public void OnBeginDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        Lever.transform.position = eventData.position;
    }

    public void OnDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        // vector2 �� ����ȯ ��Ű�� z���� �߷�����
        dir = eventData.position - (Vector2)startPos;
        
        // distance�� ����ϴ� ��쵵 ����
        float distance = Vector3.Distance(startPos, eventData.position);

        // dir.sqrMagnitude : ��Ʈ���� ��
        // �巡���� �������� ���������� ū ��Ȳ

        // distance�� ����� �� ����
        if(distance > radius)
        //if(dir.magnitude > radius)    // sqrMagnitude? magnitude?
        {
            Lever.transform.position = startPos + dir.normalized * radius;
        }
        else
        {
            // �۵��� �� �̻���.. ������ �ܰ����θ� ��.. distance�������...
            //Lever.transform.position = eventData.position;

            // distance ����� ���� �̷���
            Lever.transform.position = startPos + dir.normalized * distance;
        }
    }

    public void OnEndDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        Lever.transform.position = startPos;

        // �� ���� 0�ǵ���
        dir = Vector3.zero;
    }
}
