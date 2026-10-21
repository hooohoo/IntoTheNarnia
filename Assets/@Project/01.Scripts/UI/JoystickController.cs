using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public sealed class JoystickController : RuntimeBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform lever;
    [SerializeField, Range(10, 150)] private float leverRange = 100;
    [FormerlySerializedAs("_joystickState"), SerializeField] private Define.JoystickState joystickState;
    [FormerlySerializedAs("inputDirection"), SerializeField] private Vector2 direction;
    [FindComponents] private RectTransform rectTransform;
    [Singleton(typeof(InputManager))] private InputManager input;
    private Canvas canvas;
    public Vector2 InputDirection => direction;
    protected override void Initialize()
    {
        canvas = GetComponentInParent<Canvas>();
        ResetLever();
    }
    public void OnBeginDrag(PointerEventData eventData) { ControlJoystickLever(eventData); }
    public void OnDrag(PointerEventData eventData) { ControlJoystickLever(eventData); }
    public void OnEndDrag(PointerEventData eventData) { ResetLever(); }
    public void ControlJoystickLever(PointerEventData eventData)
    {
        if (lever == null || canvas == null || leverRange <= 0) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, canvas.worldCamera, out var point);
        var clamped = Vector2.ClampMagnitude(point, leverRange);
        lever.anchoredPosition = clamped;
        direction = clamped / leverRange;
        joystickState = Define.JoystickState.InputTrue;
        input.SetJoystick(this, direction, true);
    }
    public Vector2 InputControlVector() { return direction; }
    private void ResetLever()
    {
        direction = Vector2.zero;
        joystickState = Define.JoystickState.InputFalse;
        if (lever != null) lever.anchoredPosition = Vector2.zero;
        input.ClearJoystick(this);
    }
    protected override void Release() { ResetLever(); }
}
