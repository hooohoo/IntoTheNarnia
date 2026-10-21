using System.Threading;
using Cysharp.Threading.Tasks;

// Existing room scenes are content placeholders. Their bootstrap still has an explicit Ready boundary.
public sealed class RoomScene : SceneRoot
{
    protected override UniTask BuildAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        states.Change(GameStateType.Exploration);
        return UniTask.CompletedTask;
    }
    protected override void ReleaseScene() { }
}
