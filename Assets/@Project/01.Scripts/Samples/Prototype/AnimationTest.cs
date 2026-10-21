using UnityEngine;

public sealed class AnimationTest : RuntimeBehaviour
{
    [FindComponents] private Animator animator;
    protected override void Initialize() { animator.SetInteger("State", 0); }
}
