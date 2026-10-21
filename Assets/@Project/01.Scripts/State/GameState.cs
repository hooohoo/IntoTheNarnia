public sealed class GameState : IGameState
{
    private readonly InputManager input;
    private readonly InputPolicy policy;
    public GameStateType Type { get; }
    public GameState(GameStateType type, InputManager inputManager)
    {
        Type = type;
        input = inputManager;
        bool exploration = type == GameStateType.Exploration;
        policy = new InputPolicy(exploration, exploration, exploration);
    }
    public void Enter() { input.SetPolicy(policy); }
    public void Exit() { input.ResetInput(); }
}
