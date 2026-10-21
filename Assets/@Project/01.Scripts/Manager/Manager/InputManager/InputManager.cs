using UnityEngine;

public sealed class InputManager : Singleton<InputManager>
{
    private static readonly string HorizontalAxis = "Horizontal";
    private static readonly string VerticalAxis = "Vertical";
    private static readonly string MouseX = "Mouse X";
    private static readonly string MouseY = "Mouse Y";
    private static readonly int MousePointer = -1;
    private static readonly float TouchSensitivity = 0.05f;
    private InputHandlerBase handler;
    private Vector2 joystick;
    private object joystickOwner;
    private bool dragging;
    public Vector2 Movement => handler == null ? Vector2.zero : handler.Movement;
    public Vector2 CameraDelta => handler == null ? Vector2.zero : handler.CameraDelta;
    public bool AllowsInteraction => handler != null && handler.AllowsInteraction;
    public void SetHandler(InputHandlerBase value) { ResetInput(); handler = value; }
    public void ClearHandler(InputHandlerBase owner)
    {
        if (ReferenceEquals(handler, owner)) { ResetInput(); handler = null; }
    }
    public void SetJoystick(object owner, Vector2 value, bool isDragging)
    {
        if (joystickOwner != null && !ReferenceEquals(joystickOwner, owner)) return;
        joystickOwner = owner;
        joystick = Vector2.ClampMagnitude(value, 1);
        dragging = isDragging;
    }
    public void ClearJoystick(object owner)
    {
        if (!ReferenceEquals(joystickOwner, owner)) return;
        joystickOwner = null;
        joystick = Vector2.zero;
        dragging = false;
    }
    public void ReadFrame()
    {
        if (handler == null) return;
        var keyboard = new Vector2(Input.GetAxisRaw(HorizontalAxis), Input.GetAxisRaw(VerticalAxis));
        handler.OnKeyMove(dragging ? joystick : keyboard);
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began) handler.OnDragStart(touch.position, touch.fingerId);
                if (touch.phase == TouchPhase.Moved) handler.OnDragging(touch.deltaPosition * TouchSensitivity, touch.fingerId);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) handler.OnDragEnd(touch.fingerId);
            }
            return;
        }
        if (Input.GetMouseButtonDown(0)) handler.OnDragStart(Input.mousePosition, MousePointer);
        if (Input.GetMouseButton(0)) handler.OnDragging(new Vector2(Input.GetAxis(MouseX), Input.GetAxis(MouseY)), MousePointer);
        else handler.OnDragEnd(MousePointer);
    }
    public float GetCameraAxis(string axis)
    {
        if (axis == MouseX) return CameraDelta.x;
        if (axis == MouseY) return CameraDelta.y;
        return 0;
    }
    public void ResetInput()
    {
        handler?.OnClear();
        joystick = Vector2.zero;
        joystickOwner = null;
        dragging = false;
    }
    public override void Release() { ResetInput(); handler = null; }
}
