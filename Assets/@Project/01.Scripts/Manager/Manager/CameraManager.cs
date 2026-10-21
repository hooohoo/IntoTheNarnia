using Cinemachine;
using UnityEngine;

public sealed class CameraManager : Singleton<CameraManager>
{
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(InputManager))] private InputManager input;
    private CinemachineCore.AxisInputDelegate previousInput;
    private CinemachineCore.AxisInputDelegate axisReader;
    private CinemachineFreeLook freeLook;
    private object owner;
    public Camera Camera { get; private set; }
    public void Bind(object sceneOwner, Camera camera, CinemachineFreeLook cameraRig)
    {
        Clear();
        owner = sceneOwner;
        Camera = camera;
        freeLook = cameraRig;
        previousInput = CinemachineCore.GetInputAxis;
        axisReader = input.GetCameraAxis;
        CinemachineCore.GetInputAxis = axisReader;
        FollowPlayer();
    }
    public void FollowPlayer()
    {
        if (freeLook == null || objects.Player == null) return;
        freeLook.Follow = objects.Player.transform;
        freeLook.LookAt = objects.Player.transform;
    }
    public void Unbind(object sceneOwner) { if (ReferenceEquals(owner, sceneOwner)) Clear(); }
    private void Clear()
    {
        if (axisReader != null && CinemachineCore.GetInputAxis == axisReader) CinemachineCore.GetInputAxis = previousInput;
        if (freeLook != null) { freeLook.Follow = null; freeLook.LookAt = null; }
        previousInput = null;
        axisReader = null;
        owner = null;
        freeLook = null;
        Camera = null;
    }
    public override void Release() { Clear(); }
}
