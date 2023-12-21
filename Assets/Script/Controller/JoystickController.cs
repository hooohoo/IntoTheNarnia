using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

// ���̽�ƽ ��Ʈ�ѷ�
// ī�޶� �ٶ󺸴� ������ ��. ī�޶�� ĳ���Ͱ� ���ֺ��� �ִٸ� ��������� ������ ĳ���Ͱ� ī�޶� ������ �ٰ���
public class JoystickController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ������ġ ������
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    // ������ġ ������
    [SerializeField, Range(10, 150)]
    private float leverRange;
    // ���� ���� Ȯ�ο�
    public JoystickState _joystickState;
    // ���� ������
    private float leverRadius;
    // ��ǲ ����
    public Vector2 inputDirection;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        _joystickState = JoystickState.InputFalse;
        leverRadius = rectTransform.sizeDelta.x * 0.5f;
    }

    private void Update()
    {
        if (_joystickState == JoystickState.InputTrue)
        {
            InputControlVector();
        }
    }
    // ���� �巡�� ����
    public void OnBeginDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        _joystickState = JoystickState.InputTrue;
    }
    // ���� �巡�� ��
    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        //Debug.Log("OnDrag-eventData.Position : " + eventData.position);
        _joystickState = JoystickState.InputTrue;
    }
    // ���� �巡�� ��
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 inputPos = Vector2.zero;
        Vector2 inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        lever.anchoredPosition = Vector2.zero;
        inputDirection = inputVector / leverRange;
        _joystickState = JoystickState.InputFalse;
    }
    // ���̽�ƽ �����̴� �Լ�
    public void ControlJoystickLever(PointerEventData eventData)
    {
        Vector2 inputPos = eventData.position;
        Vector2 inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        Debug.Log("inputVector : " + inputVector);
        lever.anchoredPosition = inputVector;
        inputDirection = inputVector / leverRange;
    }
    // ���̽�ƽ�� ���� ���
    public Vector2 InputControlVector()
    {
        _joystickState = JoystickState.InputTrue;
        //Debug.Log("���̽�ƽ : " + inputDirection);
        return inputDirection;
    }
}