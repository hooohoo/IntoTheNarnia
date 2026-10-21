using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InputManager : Singleton<InputManager>
{
    private InputPolicy policy;
    private Vector2 joystick;
    private object joystickOwner;
    private bool dragging;
    private bool cameraGesture;
    public Vector2 Movement { get; private set; }
    public Vector2 CameraDelta { get; private set; }
    public bool AllowsInteraction => policy != null && policy.AllowsInteraction;
    public void SetPolicy(InputPolicy value) { policy = value; ResetInput(); }
    public void SetJoystick(object owner, Vector2 value, bool isDragging)
    {
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
        Movement = Vector2.zero;
        CameraDelta = Vector2.zero;
        if (policy == null) return;
        if (policy.AllowsMovement)
        {
            var keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Movement = dragging ? joystick : Vector2.ClampMagnitude(keyboard, 1);
        }
        if (Input.GetMouseButtonDown(0))
            cameraGesture = EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        if (!Input.GetMouseButton(0)) cameraGesture = false;
        if (policy.AllowsCamera && cameraGesture && !dragging)
            CameraDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
    public float GetCameraAxis(string axis)
    {
        if (axis == "Mouse X") return CameraDelta.x;
        if (axis == "Mouse Y") return CameraDelta.y;
        return 0;
    }
    public void ResetInput()
    {
        joystick = Vector2.zero;
        Movement = Vector2.zero;
        CameraDelta = Vector2.zero;
        dragging = false;
        cameraGesture = false;
    }
    public override void Release() { ResetInput(); joystickOwner = null; policy = null; }
}
