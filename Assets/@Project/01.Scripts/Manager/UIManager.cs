public sealed class UIManager : Singleton<UIManager>, IEventListener<StateChanged>
{
    [Singleton(typeof(EventManager))] private EventManager events;
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    private FieldScreen screen;
    private object owner;
    public override void Init() { events.Subscribe<StateChanged>(this); }
    public void Bind(object sceneOwner, FieldScreen value)
    {
        screen?.Release();
        owner = sceneOwner;
        screen = value;
        screen.Show(states.Current);
    }
    public void Unbind(object sceneOwner)
    {
        if (!ReferenceEquals(owner, sceneOwner)) return;
        var previous = screen;
        screen = null;
        owner = null;
        previous?.Release();
    }
    public void OnEvent(StateChanged message) { screen?.Show(message.Current); }
    public void ShowSettings() { states.Push(GameStateType.Settings); }
    public void HideSettings() { states.Pop(GameStateType.Settings); }
    public void ShowDialogue(string speaker, string[] lines) { screen?.ShowDialogue(speaker, lines); }
    public override void Release()
    {
        events.Unsubscribe<StateChanged>(this);
        var previous = screen;
        screen = null;
        owner = null;
        previous?.Release();
    }
}
