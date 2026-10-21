using System;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class FacadeManager : MonoBehaviour
{
    private static FacadeManager instance;
    private static bool shuttingDown;
    private ServiceRegistry registry;
    public FrameLoop Loop { get; private set; }
    public static FacadeManager Instance
    {
        get
        {
            if (instance == null && !shuttingDown) Bootstrap();
            return instance;
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; shuttingDown = false; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null || shuttingDown) return;
        new GameObject("@Facade").AddComponent<FacadeManager>();
    }
    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        registry = new ServiceRegistry();
        registry.Initialize(new IManager[]
        {
            new EventManager(), new AddressableManager(), new DataManager(), new ObjectManager(),
            new InputManager(), new CameraManager(), new UIManager(), new QuestManager(),
            new FactoryManager(), new GameStateManager(), new GameSceneManager()
        });
        Loop = new FrameLoop();
        Loop.Initialize(registry.Get<InputManager>());
    }
    public T Get<T>() where T : class, IManager => registry.Get<T>();
    public void Inject(object target) { registry.Inject(target); }
    private void OnApplicationQuit() { shuttingDown = true; Shutdown(); }
    private void OnDestroy() { if (instance == this) { Shutdown(); instance = null; } }
    private void Shutdown()
    {
        Loop?.Release();
        Loop = null;
        if (registry == null) return;
        try { registry.Release(); }
        catch (Exception exception) { LogUtil.LogException(exception); }
        registry = null;
    }
}
