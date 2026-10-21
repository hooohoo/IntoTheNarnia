using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class FieldManager : SceneRoot
{
    [FormerlySerializedAs("_UiRoot"), SerializeField] private GameObject uiRoot;
    [FormerlySerializedAs("_CameraRoot"), SerializeField] private GameObject cameraRoot;
    [Singleton(typeof(FactoryManager))] private FactoryManager factory;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(CameraManager))] private CameraManager cameras;
    [Singleton(typeof(UIManager))] private UIManager ui;
    [Singleton(typeof(DataManager))] private DataManager data;
    [SerializeField] private GameObject miniMapGround;
    private PlayerController player;
    private ProfessorHouseMapManager map;
    protected override async UniTask BuildAsync(CancellationToken token)
    {
        if (!await resources.PreloadAsync<GameObject>(AddressableLabels.ProfessorHouse, this, token))
            throw new InvalidOperationException("ProfessorHouse preload failed.");
        await data.LoadDialogueAsync(this, token);
        token.ThrowIfCancellationRequested();
        objects.SetSceneRoot(this, transform);
        var camera = SceneBinding.MainCamera(gameObject.scene);
        var rig = cameraRoot == null ? null : cameraRoot.GetComponentInChildren<CinemachineFreeLook>(true);
        cameras.Bind(this, camera, rig);
        player = factory.CreatePlayer(transform.position, AssetKeys.Player, gameObject.scene);
        cameras.FollowPlayer();
        ui.SetSceneUIRoot(this, transform);
        var field = ui.ShowSceneUI<UI_Field>(AssetKeys.UI.Field);
        field.BindWorld(miniMapGround);
        ui.Bind(this, new FieldScreen(field));
        states.Change(GameStateType.Exploration);
        map = SceneBinding.FirstInScene<ProfessorHouseMapManager>(gameObject.scene);
        if (map != null) map.Build();
    }
    protected override void ReleaseScene()
    {
        if (map != null) map.Clear();
        map = null;
        ui.Unbind(this);
        ui.ReleaseSceneUIs(this);
        cameras.Unbind(this);
        objects.ReleaseScene(this);
        data.ReleaseDialogue();
        player = null;
    }
}
