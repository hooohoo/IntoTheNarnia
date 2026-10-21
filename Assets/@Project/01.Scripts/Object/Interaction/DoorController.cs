using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class DoorController : RuntimeBehaviour
{
    private static readonly int StateParameter = Animator.StringToHash("State");
    [FormerlySerializedAs("_doorHandle"), SerializeField] private GameObject doorHandle;
    [FormerlySerializedAs("_openOrLocked"), SerializeField] private bool isUnlocked;
    [FormerlySerializedAs("_openOrClose"), SerializeField] private bool isOpen;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(EventManager))] private EventManager events;
    [FindComponents, SerializeField] private Animator animator;
    private Animator handleAnimator;
    private CancellationTokenSource handleAnimation;
    public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
    public bool IsOpen => isOpen;
    protected override void Initialize()
    {
        if (doorHandle != null) handleAnimator = doorHandle.GetComponent<Animator>();
        animator.SetInteger(StateParameter, isOpen ? 1 : 0);
    }
    public void OpenAndCloseDoor()
    {
        EnsureInitialized();
        if (isOpen) { CloseDoor(); return; }
        if (isUnlocked) ShowDoorOpen();
        else ShowDoorLocked();
    }
    public void CloseDoor()
    {
        EnsureInitialized();
        isOpen = false;
        animator.SetInteger(StateParameter, 0);
        events.PostNotification(DoorEvent.Changed, new DoorChanged(this, false));
    }
    public void ShowDoorLocked() { AnimateHandle(1); }
    public void ShowDoorOpen()
    {
        EnsureInitialized();
        isOpen = true;
        animator.SetInteger(StateParameter, 1);
        AnimateHandle(2);
        events.PostNotification(DoorEvent.Changed, new DoorChanged(this, true));
    }
    private void AnimateHandle(int state)
    {
        EnsureInitialized();
        StopHandle();
        if (handleAnimator == null) return;
        handleAnimator.SetInteger(StateParameter, state);
        handleAnimation = CancellationTokenSource.CreateLinkedTokenSource(Lifetime);
        ResetHandleAsync(handleAnimation.Token).Forget();
    }
    private async UniTask ResetHandleAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            if (handleAnimator != null) handleAnimator.SetInteger(StateParameter, 0);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) objects.SetInteraction(this);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) objects.ClearInteraction(this);
    }
    private void StopHandle()
    {
        handleAnimation?.Cancel();
        handleAnimation?.Dispose();
        handleAnimation = null;
    }
    protected override void Release()
    {
        StopHandle();
        objects.ClearInteraction(this);
        if (handleAnimator != null) handleAnimator.SetInteger(StateParameter, 0);
    }
}
