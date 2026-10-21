public interface IGameState
{
    GameStateType Type { get; }
    void Enter();
    void Exit();
}
