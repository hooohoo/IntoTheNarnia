using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public sealed class PlayerController : OBJ_Base, IFrameTick
{
    private static readonly int StateParameter = Animator.StringToHash("State");
    [FormerlySerializedAs("_camera"), SerializeField] private GameObject cameraObject;
    [FormerlySerializedAs("_creatureState"), SerializeField] private Define.CreatureState creatureState;
    [FormerlySerializedAs("_characterName"), SerializeField] private string characterName;
    [FormerlySerializedAs("_moveSpeed"), SerializeField] private float moveSpeed = 5;
    [FormerlySerializedAs("_rotationSpeed"), SerializeField] private float rotationSpeed = 10;
    [FormerlySerializedAs("_walkOrRun"), SerializeField] private bool walkOrRun = true;
    [Singleton(typeof(InputManager))] private InputManager input;
    [Singleton(typeof(CameraManager))] private CameraManager cameras;
    [FindComponents, SerializeField] private Animator animator;
    private NavMeshAgent agent;
    public string CharacterName { get => characterName; set => characterName = value; }
    protected override void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        if (moveSpeed <= 0) moveSpeed = 5;
        if (rotationSpeed <= 0) rotationSpeed = 10;
        creatureState = Define.CreatureState.Idle;
    }
    public void Tick(float deltaTime)
    {
        if (creatureState == Define.CreatureState.Dead || creatureState == Define.CreatureState.Attack) return;
        Vector2 movement = input.Movement;
        if (movement.sqrMagnitude < 0.0001f)
        {
            creatureState = Define.CreatureState.Idle;
            animator.SetInteger(StateParameter, 0);
            return;
        }
        creatureState = Define.CreatureState.Move;
        walkOrRun = movement.magnitude < 0.5f;
        animator.SetInteger(StateParameter, walkOrRun ? 2 : 3);
        var camera = cameras.Camera;
        var forward = camera != null ? camera.transform.forward : Vector3.forward;
        forward.y = 0;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();
        Vector3 direction = forward * movement.y + Vector3.Cross(Vector3.up, forward) * movement.x;
        Vector3 step = direction * (deltaTime * moveSpeed);
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.Move(step);
        else transform.position += step;
        Vector3 facing = Vector3.RotateTowards(transform.forward, direction, deltaTime * rotationSpeed, 0);
        if (facing.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(facing.normalized);
    }
    public override void Init() { EnsureInitialized(); }
}
