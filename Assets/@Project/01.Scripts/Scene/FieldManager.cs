using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-500)]
public sealed class FieldManager : RuntimeBehaviour
{
    [FormerlySerializedAs("_UiRoot"), SerializeField] private GameObject uiRoot;
    [FormerlySerializedAs("_CameraRoot"), SerializeField] private GameObject cameraRoot;
    [Singleton(typeof(FactoryManager))] private FactoryManager factory;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(CameraManager))] private CameraManager cameras;
    [Singleton(typeof(UIManager))] private UIManager ui;
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    private PlayerController player;
    protected override void Initialize()
    {
        var camera = SceneBinding.MainCamera(gameObject.scene);
        var rig = cameraRoot == null ? null : cameraRoot.GetComponentInChildren<CinemachineFreeLook>(true);
        cameras.Bind(this, camera, rig);
        player = factory.CreatePlayer(transform.position, nameof(Define.CharacterName.Lucy), gameObject.scene);
        cameras.FollowPlayer();
        if (uiRoot != null) ui.Bind(this, new FieldScreen(uiRoot));
        states.Change(GameStateType.Exploration);
    }
    protected override void Release()
    {
        ui.Unbind(this);
        cameras.Unbind(this);
        if (objects.Player == player) objects.ClearScene();
        if (player != null) Destroy(player.gameObject);
        player = null;
    }
}
