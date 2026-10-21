using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class Title : SceneRoot
{
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    [Singleton(typeof(UIManager))] private UIManager ui;
    protected override async UniTask BuildAsync(CancellationToken token)
    {
        if (!await resources.PreloadAsync<UnityEngine.GameObject>(AddressableLabels.Title, this, token))
            throw new InvalidOperationException("Title preload failed.");
        token.ThrowIfCancellationRequested();
        ui.SetSceneUIRoot(this, transform);
        ui.ShowSceneUI<UI_Title>(AssetKeys.UI.Title);
        states.Change(GameStateType.Title);
    }
    public void StartButton() { scenes.LoadScene(Define.SceneName.Professor_House); }
    public void ContinueButton() { }
    public void SettingButton() { }
    protected override void ReleaseScene() { ui.ReleaseSceneUIs(this); }
}
