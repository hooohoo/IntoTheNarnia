public sealed class UI_Settings : UI_Popup
{
    [Singleton(typeof(UIManager))] private UIManager ui;
    public override void Init() { InjectUtil.InjectSingleton(this); }
    public void SettingWindowOff() { ui.HideSettings(); }
    public override void Release() { }
}
