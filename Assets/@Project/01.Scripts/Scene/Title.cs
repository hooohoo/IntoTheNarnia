public sealed class Title : RuntimeBehaviour
{
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    protected override void Initialize() { states.Change(GameStateType.Title); }
    public void StartButton() { scenes.LoadScene(Define.SceneName.Professor_House); }
    // These inspector entry points remain placeholders, as in the original title.
    public void ContinueButton() { }
    public void SettingButton() { }
}
