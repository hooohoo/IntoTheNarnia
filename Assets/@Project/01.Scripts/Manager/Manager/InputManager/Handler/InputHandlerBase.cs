using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InputHandlerBase
{
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointer;
    private EventSystem eventSystem;
    public virtual bool AllowsInteraction => false;
    public virtual Vector2 Movement => Vector2.zero;
    public virtual Vector2 CameraDelta => Vector2.zero;
    public virtual void OnKeyMove(Vector2 value) { }
    public virtual void OnDragStart(Vector2 position, int pointerId) { }
    public virtual void OnDragging(Vector2 delta, int pointerId) { }
    public virtual void OnDragEnd(int pointerId) { }
    public virtual void OnClear() { }
    protected GameObject RaycastUI(Vector2 position)
    {
        if (EventSystem.current == null) return null;
        if (eventSystem != EventSystem.current)
        {
            eventSystem = EventSystem.current;
            pointer = new PointerEventData(eventSystem);
        }
        pointer.Reset();
        pointer.position = position;
        raycastResults.Clear();
        eventSystem.RaycastAll(pointer, raycastResults);
        return raycastResults.Count == 0 ? null : raycastResults[0].gameObject;
    }
}
