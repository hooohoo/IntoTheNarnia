using UnityEngine;
using UnityEngine.Serialization;

public sealed class Player : RuntimeBehaviour, IFrameTick
{
    [FormerlySerializedAs("_Camera"), SerializeField] private GameObject cameraObject;
    [Singleton(typeof(InputManager))] private InputManager input;
    public void Tick(float deltaTime)
    {
        Vector2 movement = input.Movement;
        Vector3 direction = new Vector3(movement.x, 0, movement.y);
        transform.position += direction * (5 * deltaTime);
        if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, direction, deltaTime * 10, 0));
    }
}
