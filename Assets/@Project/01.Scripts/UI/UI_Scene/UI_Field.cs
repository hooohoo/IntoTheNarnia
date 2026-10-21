using UnityEngine;

public sealed class UI_Field : UI_Scene
{
    [SerializeField] private GameObject joystick;
    [SerializeField] private GameObject action;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject miniMap;
    public override void Init() { }
    public void ShowState(GameStateType state)
    {
        bool exploration = state == GameStateType.Exploration;
        joystick.SetActive(exploration);
        action.SetActive(exploration);
        settings.SetActive(exploration || state == GameStateType.Settings);
        miniMap.SetActive(exploration || state == GameStateType.Settings);
    }
    public void BindWorld(GameObject ground)
    {
        var map = miniMap.GetComponent<MiniMapController>();
        if (map != null) map.BindWorld(ground);
    }
    public override void Release() { }
}
