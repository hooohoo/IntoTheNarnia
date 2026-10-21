using UnityEngine;
using UnityEngine.Serialization;

public sealed class CameraController : RuntimeBehaviour, ILateFrameTick
{
    [FormerlySerializedAs("_Player"), SerializeField] private GameObject player;
    private Vector3 offset;
    protected override void Initialize() { if (player != null) offset = transform.position - player.transform.position; }
    public void LateTick(float deltaTime) { if (player != null) transform.position = player.transform.position + offset; }
}
