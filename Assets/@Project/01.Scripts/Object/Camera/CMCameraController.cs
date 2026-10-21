using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public sealed class CMCameraController : RuntimeBehaviour, IPointerClickHandler
{
    [FormerlySerializedAs("_camera"), SerializeField] private CinemachineFreeLook cameraRig;
    [FormerlySerializedAs("_hiddenBarrier"), SerializeField] private GameObject hiddenBarrier;
    [FormerlySerializedAs("viewChange"), SerializeField] private bool allowViewChange = true;
    [Singleton(typeof(InputManager))] private InputManager input;
    protected override void Initialize() { if (cameraRig == null) cameraRig = GetComponent<CinemachineFreeLook>(); }
    public void Init() { EnsureInitialized(); }
    public float ClickControl(string axis) { return allowViewChange ? input.GetCameraAxis(axis) : 0; }
    public void OnPointerClick(PointerEventData eventData) { }
}
