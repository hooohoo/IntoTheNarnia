using UnityEngine;

public sealed class PlayerWithStick : RuntimeBehaviour, IFrameTick
{
    [Singleton(typeof(InputManager))] private InputManager input;
    public void Tick(float deltaTime)
    {
        var direction = input.Movement;
        transform.position += new Vector3(direction.x, 0, direction.y) * (deltaTime * 3);
    }
}
