public sealed class UI_Title : UI_Scene
{
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    public override void Init() { InjectUtil.InjectSingleton(this); }
    public void StartButton() { scenes.LoadScene(Define.SceneName.Professor_House); }
    public void ContinueButton() { }
    public void SettingButton() { }
    public override void Release() { }
}
