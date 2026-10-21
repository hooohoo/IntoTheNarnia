using UnityEngine;

public sealed class ExplorationInputHandler : InputHandlerBase
{
    private Vector2 movement;
    private Vector2 cameraDelta;
    private int? cameraPointer;
    public override bool AllowsInteraction => true;
    public override Vector2 Movement => movement;
    public override Vector2 CameraDelta => cameraDelta;
    public override void OnKeyMove(Vector2 value) { movement = Vector2.ClampMagnitude(value, 1); cameraDelta = Vector2.zero; }
    public override void OnDragStart(Vector2 position, int pointerId)
    {
        if (!cameraPointer.HasValue && RaycastUI(position) == null) cameraPointer = pointerId;
    }
    public override void OnDragging(Vector2 delta, int pointerId)
    {
        if (cameraPointer == pointerId) cameraDelta = delta;
    }
    public override void OnDragEnd(int pointerId)
    {
        if (cameraPointer == pointerId) { cameraPointer = null; cameraDelta = Vector2.zero; }
    }
    public override void OnClear() { movement = Vector2.zero; cameraDelta = Vector2.zero; cameraPointer = null; }
}
