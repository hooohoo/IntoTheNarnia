using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ProfessorHouseTraversalTests
{
    [UnityTest]
    public IEnumerator PlayerCanClimbTheStairsAndReachBothDoors()
    {
        return TraverseAsync().ToCoroutine();
    }

    private async UniTask TraverseAsync()
    {
        await SceneManager.LoadSceneAsync("Professor_House");
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        var facade = FacadeManager.Instance;
        var objects = facade.Get<ObjectManager>();
        var states = facade.Get<GameStateManager>();
        var input = facade.Get<InputManager>();
        var camera = facade.Get<CameraManager>().Camera;
        var dialogue = SceneBinding.FirstInScene<MessageBoxController>(SceneManager.GetActiveScene());
        for (int i = 0; i < 12 && states.Current == GameStateType.Dialogue; i++)
            dialogue.PlayNextLine();
        Assert.IsTrue(objects.PlayerAgent.isOnNavMesh, "Spawn must land on the new navigation data.");
        try
        {
            await WalkAsync(new Vector3(2.9f, 3.7f, 4.8f), objects, input, camera);
            Assert.Greater(objects.Player.transform.position.y, 3.4f, "Player must climb, not teleport to the gallery.");
            await WalkAsync(new Vector3(-3.3f, 3.7f, 6.05f), objects, input, camera);
            await UniTask.Delay(100);
            Assert.IsNotNull(objects.SiblingsDoor);
            Assert.IsNotNull(objects.WardrobeDoor);
            Assert.AreSame(objects.SiblingsDoor, objects.InteractionDoor, "Walking into the doorway must expose its interaction.");
            var agent = objects.PlayerAgent;
            var blocked = new NavMeshPath();
            NavMesh.CalculatePath(agent.transform.position, new Vector3(-3.3f, 3.68f, 8.3f), 7, blocked);
            Assert.AreNotEqual(NavMeshPathStatus.PathComplete, blocked.status, "A closed quest gate must prevent entry.");
            objects.SiblingsDoor.IsUnlocked = true;
            objects.InteractionDoor.OpenAndCloseDoor();
            await UniTask.Delay(500);
            var leaf = objects.SiblingsDoor.transform.Find("Leaf");
            Assert.Less(Mathf.DeltaAngle(0, leaf.localEulerAngles.y), -85, "The visible leaf must actually open.");
            var unlocked = new NavMeshPath();
            NavMesh.CalculatePath(agent.transform.position, new Vector3(-3.3f, 3.68f, 8.3f), agent.areaMask, unlocked);
            Assert.AreEqual(NavMeshPathStatus.PathComplete, unlocked.status);
            await WalkAsync(new Vector3(0, 3.7f, 6.05f), objects, input, camera);
            await UniTask.Delay(100);
            Assert.AreSame(objects.WardrobeDoor, objects.InteractionDoor);
        }
        finally { input.ClearJoystick(this); }
        await SceneManager.LoadSceneAsync("Title");
    }

    private async UniTask WalkAsync(Vector3 target, ObjectManager objects, InputManager input, Camera camera)
    {
        Assert.IsTrue(NavMesh.SamplePosition(target, out var hit, .5f, NavMesh.AllAreas));
        var path = new NavMeshPath();
        Assert.IsTrue(objects.PlayerAgent.CalculatePath(hit.position, path));
        Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status);
        foreach (var corner in path.corners)
        {
            float started = Time.realtimeSinceStartup;
            while (Vector3.Distance(objects.Player.transform.position, corner) > .22f)
            {
                Assert.Less(Time.realtimeSinceStartup - started, 20, "Movement stalled near " + corner + "; player=" + objects.Player.transform.position);
                var delta = corner - objects.Player.transform.position;
                delta.y = 0;
                if (delta.sqrMagnitude < .015f)
                    break;
                var forward = camera.transform.forward;
                forward.y = 0;
                forward.Normalize();
                var right = Vector3.Cross(Vector3.up, forward);
                var axes = new Vector2(Vector3.Dot(delta.normalized, right), Vector3.Dot(delta.normalized, forward)) * .65f;
                input.SetJoystick(this, axes, true);
                await UniTask.Yield();
            }
        }
        input.ClearJoystick(this);
        await UniTask.Yield();
    }
}
