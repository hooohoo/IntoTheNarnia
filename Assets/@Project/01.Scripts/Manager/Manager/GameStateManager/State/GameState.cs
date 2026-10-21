public sealed class GameState : IGameState
{
    private readonly InputManager input;
    private readonly InputHandlerBase handler;
    public GameStateType Type { get; }
    public GameState(GameStateType type, InputManager inputManager)
    {
        Type = type;
        input = inputManager;
        handler = type == GameStateType.Exploration ? (InputHandlerBase)new ExplorationInputHandler() : new BlockedInputHandler();
    }
    public void Enter() { input.SetHandler(handler); }
    public void Exit() { input.ClearHandler(handler); }
}
