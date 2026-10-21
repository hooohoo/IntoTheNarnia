public sealed class ButtonManager : RuntimeBehaviour
{
    [Singleton(typeof(UIManager))] private UIManager ui;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(InputManager))] private InputManager input;
    public void SettingWindowOn() { ui.ShowSettings(); }
    public void SettingWindowOff() { ui.HideSettings(); }
    public void ActionButton()
    {
        if (input.AllowsInteraction && objects.InteractionDoor != null) objects.InteractionDoor.OpenAndCloseDoor();
    }
}
