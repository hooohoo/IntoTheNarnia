using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class Joystick : RuntimeBehaviour
{
    [FormerlySerializedAs("Lever"), SerializeField] private Image lever;
    [SerializeField] private RectTransform rcTr;
    [Singleton(typeof(InputManager))] private InputManager input;
    private Vector3 startPosition;
    private float radius;
    protected override void Initialize()
    {
        if (lever != null) startPosition = lever.transform.position;
        radius = rcTr == null ? 1 : Mathf.Max(1, rcTr.rect.width * 0.5f);
    }
    public void OnPointerDown(BaseEventData eventData) { OnDrag(eventData); }
    public void OnPointerUp(BaseEventData eventData) { ResetLever(); }
    public void OnBeginDrag(BaseEventData eventData) { OnDrag(eventData); }
    public void OnDrag(BaseEventData eventData)
    {
        if (!(eventData is PointerEventData pointer) || lever == null) return;
        Vector2 direction = Vector2.ClampMagnitude(pointer.position - (Vector2)startPosition, radius);
        lever.transform.position = startPosition + (Vector3)direction;
        input.SetJoystick(this, direction / radius, true);
    }
    public void OnEndDrag(BaseEventData eventData) { ResetLever(); }
    private void ResetLever()
    {
        if (lever != null) lever.transform.position = startPosition;
        input.ClearJoystick(this);
    }
    protected override void Release() { ResetLever(); }
}
