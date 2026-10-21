using UnityEngine;
using UnityEngine.Serialization;

public sealed class ColliderTest : RuntimeBehaviour
{
    [FormerlySerializedAs("_playerCollider"), SerializeField] private Collider playerCollider;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) LogUtil.Log("충돌", this);
    }
}
