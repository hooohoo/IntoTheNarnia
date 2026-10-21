using UnityEngine;
using UnityEngine.AI;

public sealed class ObjectManager : Singleton<ObjectManager>
{
    public PlayerController Player { get; private set; }
    public NavMeshAgent PlayerAgent { get; private set; }
    public DoorController InteractionDoor { get; private set; }
    public DoorController SiblingsDoor { get; set; }
    public DoorController WardrobeDoor { get; set; }
    public void SetPlayer(PlayerController player)
    {
        Player = player;
        PlayerAgent = player == null ? null : player.GetComponent<NavMeshAgent>();
    }
    public void SetInteraction(DoorController door) { InteractionDoor = door; }
    public void ClearInteraction(DoorController door)
    {
        if (InteractionDoor == door) InteractionDoor = null;
    }
    public void ClearScene()
    {
        Player = null;
        PlayerAgent = null;
        InteractionDoor = null;
        SiblingsDoor = null;
        WardrobeDoor = null;
    }
    public override void Release() { ClearScene(); }
}
