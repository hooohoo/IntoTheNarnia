using UnityEngine;

public sealed class CustomCamera : RuntimeBehaviour, ILateFrameTick
{
    [SerializeField] private GameObject player;
    [SerializeField] private Camera customCam;
    private Vector3 offset;
    protected override void Initialize()
    {
        if (customCam == null) customCam = SceneBinding.MainCamera(gameObject.scene);
        if (customCam != null && player != null) offset = customCam.transform.position - player.transform.position;
    }
    public void LateTick(float deltaTime)
    {
        if (customCam != null && player != null) customCam.transform.position = player.transform.position + offset;
    }
}
