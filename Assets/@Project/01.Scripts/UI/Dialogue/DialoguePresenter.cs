using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class DialoguePresenter
{
    private readonly DialogueModel model = new DialogueModel();
    private readonly MessageBoxController view;
    private readonly GameStateManager states;
    private readonly EventManager events;
    private readonly CancellationToken lifetime;
    private CancellationTokenSource typing;
    private bool showing;
    public DialoguePresenter(MessageBoxController view, GameStateManager states, EventManager events, CancellationToken lifetime)
    {
        this.view = view;
        this.states = states;
        this.events = events;
        this.lifetime = lifetime;
    }
    public void Show(string speaker, string[] lines)
    {
        model.Reset(lines);
        view.SetNameBoxText(speaker);
        showing = true;
        states.Push(GameStateType.Dialogue);
        Next();
    }
    public void Next()
    {
        if (!showing) return;
        StopTyping();
        if (!model.TryNext(out var line))
        {
            showing = false;
            string scene = view.gameObject.scene.name;
            states.Pop(GameStateType.Dialogue);
            view.Hide();
            events.Publish(new DialogueCompleted(scene));
            return;
        }
        typing = CancellationTokenSource.CreateLinkedTokenSource(lifetime);
        TypeAsync(line, typing.Token).Forget();
    }
    private async UniTask TypeAsync(string line, CancellationToken token)
    {
        try
        {
            view.SetContentsText(string.Empty);
            for (int i = 1; i <= line.Length; i++)
            {
                view.SetContentsText(line.Substring(0, i));
                await UniTask.Delay(10, ignoreTimeScale: true, cancellationToken: token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
    private void StopTyping() { typing?.Cancel(); typing?.Dispose(); typing = null; }
    public void Release()
    {
        StopTyping();
        model.Clear();
        if (showing) states.Pop(GameStateType.Dialogue);
        showing = false;
    }
}
