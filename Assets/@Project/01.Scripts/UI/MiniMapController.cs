using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class MiniMapController : RuntimeBehaviour, IFrameTick
{
    [FormerlySerializedAs("_worldSize"), SerializeField] private Vector3 worldSize;
    [FormerlySerializedAs("_scrollRect"), SerializeField] private ScrollRect scrollRect;
    [FormerlySerializedAs("_playerIcon"), SerializeField] private GameObject playerIcon;
    [FormerlySerializedAs("_plane"), SerializeField] private GameObject plane;
    [FormerlySerializedAs("_playerObject"), SerializeField] private GameObject playerObject;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    private RectTransform icon;
    protected override void Initialize()
    {
        if (playerIcon != null) icon = playerIcon.GetComponent<RectTransform>();
        if (plane != null && plane.TryGetComponent<Collider>(out var collider)) worldSize = collider.bounds.size;
        if (scrollRect != null) scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }
    public void BindWorld(GameObject ground)
    {
        plane = ground;
        if (plane != null && plane.TryGetComponent<Collider>(out var collider)) worldSize = collider.bounds.size;
    }
    public void Tick(float deltaTime)
    {
        var player = objects.Player;
        if (player == null || plane == null || scrollRect == null) return;
        var relative = player.transform.position - plane.transform.position;
        scrollRect.normalizedPosition = new Vector2(Mathf.InverseLerp(-worldSize.x * 0.5f, worldSize.x * 0.5f, relative.x), Mathf.InverseLerp(-worldSize.z * 0.5f, worldSize.z * 0.5f, relative.z));
        if (icon != null) icon.rotation = Quaternion.Euler(0, 0, -player.transform.eulerAngles.y);
    }
}
