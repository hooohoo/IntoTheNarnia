using UnityEngine;

public sealed class UI_Dialogue : UI_Popup
{
    [SerializeField] private MessageBoxController view;
    public override void Init() { }
    public void Show(string speaker, string[] lines) { view.SetMessageBox(speaker, lines); }
    public override void Release() { view.Hide(); }
}
