public sealed class FieldScreen
{
    private readonly UI_Field view;
    public FieldScreen(UI_Field view) { this.view = view; }
    public void Show(GameStateType state) { view.ShowState(state); }
    public void Release() { view.Close(); }
}
