using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Loading : UI_Scene, IFrameTick
{
    [SerializeField] private Image background;
    [SerializeField] private Image progress;
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    private FrameLoop loop;
    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        loop = FacadeManager.Instance.Loop;
        loop.Register(this);
    }
    public void SetBackground(Sprite sprite) { background.sprite = sprite; }
    public void Tick(float deltaTime) { progress.fillAmount = scenes.Progress; }
    public override void Release() { loop.Unregister(this); loop = null; background.sprite = null; }
}
