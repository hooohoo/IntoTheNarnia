using UnityEngine;
using UnityEngine.Serialization;

public sealed class ActionButtonController : RuntimeBehaviour
{
    // Serialized prototype fields retained for compatibility; targeting is scene-owned.
    [FormerlySerializedAs("_verifyTag"), SerializeField] private Define.TagName verifyTag;
    [FormerlySerializedAs("_thisDoor"), SerializeField] private DoorController targetDoor;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(InputManager))] private InputManager input;
    public void DoActionOrDoAttack()
    {
        if (input.AllowsInteraction && objects.InteractionDoor != null) objects.InteractionDoor.OpenAndCloseDoor();
    }
}
