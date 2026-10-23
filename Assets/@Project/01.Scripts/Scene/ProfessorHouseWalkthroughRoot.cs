using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

// A directly playable authoring scene using the same input, camera and asset ownership as the game.
public sealed class ProfessorHouseWalkthroughRoot : SceneRoot
{
    public static readonly string ScenePath = "Assets/@Project/00.Scenes/Professor_House_Connected.unity";
    public static readonly float CharacterHeight = 1.20f;
    public static readonly float CharacterRadius = .22f;
    [Singleton(typeof(FactoryManager))] private FactoryManager factory;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(CameraManager))] private CameraManager cameras;
    [Singleton(typeof(UIManager))] private UIManager ui;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private CinemachineFreeLook cameraRig;
    [SerializeField] private GameObject miniMapBounds;
    [SerializeField] private TraversalRoute[] routes;

    public TraversalRoute[] Routes => routes;
    public Camera SceneCamera => sceneCamera;

    [Serializable]
    public sealed class TraversalRoute
    {
        [SerializeField] private string key;
        [SerializeField] private string title;
        [SerializeField] private Vector3[] points;

        public string Key => key;
        public string Title => title;
        public Vector3[] Points => points;
    }

    public void Configure(Camera camera, CinemachineFreeLook rig, GameObject bounds, TraversalRoute[] paths)
    {
        sceneCamera = camera;
        cameraRig = rig;
        miniMapBounds = bounds;
        routes = paths;
    }

    protected override async UniTask BuildAsync(CancellationToken token)
    {
        if (sceneCamera == null || cameraRig == null || miniMapBounds == null)
            throw new InvalidOperationException("Walkthrough scene bindings are incomplete.");
        if (!await resources.PreloadAsync<GameObject>(AddressableLabels.ProfessorHouse, this, token))
            throw new InvalidOperationException("ProfessorHouse walkthrough preload failed.");
        token.ThrowIfCancellationRequested();
        objects.SetSceneRoot(this, transform);
        cameras.Bind(this, sceneCamera, cameraRig);
        var player = factory.CreatePlayer(transform.position, AssetKeys.Player, gameObject.scene);
        ConfigureReviewCharacter(player);
        cameras.FollowPlayer();
        ui.SetSceneUIRoot(this, transform);
        var field = ui.ShowSceneUI<UI_Field>(AssetKeys.UI.Field);
        field.BindWorld(miniMapBounds);
        ui.Bind(this, new FieldScreen(field));
        states.Change(GameStateType.Exploration);
    }

    private void ConfigureReviewCharacter(PlayerController player)
    {
        var agent = objects.PlayerAgent;
        if (agent == null)
            throw new InvalidOperationException("Walkthrough player requires a NavMeshAgent.");
        var renderers = player.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            throw new InvalidOperationException("Walkthrough player has no visible model.");
        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y < .1f)
            throw new InvalidOperationException("Cannot measure the player's reference height.");
        agent.enabled = false;
        player.transform.localScale *= CharacterHeight / bounds.size.y;
        agent.height = CharacterHeight;
        agent.radius = CharacterRadius;
        agent.baseOffset = 0;
        agent.areaMask = NavMesh.AllAreas;
        // Navigation owns movement here; old duplicate avatar capsules must not obstruct the review camera.
        var colliders = player.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
        agent.enabled = true;
        if (!NavMesh.SamplePosition(transform.position, out var hit, .4f, NavMesh.AllAreas) || !agent.Warp(hit.position))
            throw new InvalidOperationException("The approved entrance spawn is outside the baked navigation data.");
    }

    protected override void ReleaseScene()
    {
        ui.Unbind(this);
        ui.ReleaseSceneUIs(this);
        cameras.Unbind(this);
        objects.ReleaseScene(this);
    }
}
