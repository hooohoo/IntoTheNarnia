using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class MessageBoxController : RuntimeBehaviour
{
    [FormerlySerializedAs("_nameText"), SerializeField] private Text nameText;
    [FormerlySerializedAs("_contentsText"), SerializeField] private Text contentsText;
    [FormerlySerializedAs("_nextButton"), SerializeField] private Button nextButton;
    [FormerlySerializedAs("_isReady"), SerializeField] private bool isReady = true;
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    [Singleton(typeof(EventManager))] private EventManager events;
    private DialoguePresenter presenter;
    private bool bound;
    public bool IsReady => isReady;
    protected override void Initialize()
    {
        presenter = new DialoguePresenter(this, states, events, Lifetime);
        if (nextButton == null) return;
        bool persistent = false;
        for (int i = 0; i < nextButton.onClick.GetPersistentEventCount(); i++)
            if (nextButton.onClick.GetPersistentTarget(i) == this && nextButton.onClick.GetPersistentMethodName(i) == nameof(PlayNextLine)) persistent = true;
        if (!persistent) { nextButton.onClick.AddListener(PlayNextLine); bound = true; }
    }
    public void SetMessageBox(string speaker, string[] lines)
    {
        gameObject.SetActive(true);
        EnsureInitialized();
        isReady = false;
        presenter.Show(speaker, lines);
    }
    public void SetNameBoxText(string value) { if (nameText != null) nameText.text = value; }
    public void SetContentsText(string value) { if (contentsText != null) contentsText.text = value; }
    public void SetContentsBoxText(string[] lines) { SetMessageBox(nameText == null ? string.Empty : nameText.text, lines); }
    public void PlayNextLine() { presenter?.Next(); }
    public void Hide() { isReady = true; gameObject.SetActive(false); }
    protected override void Release()
    {
        if (bound && nextButton != null) nextButton.onClick.RemoveListener(PlayNextLine);
        bound = false;
        presenter?.Release();
        presenter = null;
        isReady = true;
    }
}
