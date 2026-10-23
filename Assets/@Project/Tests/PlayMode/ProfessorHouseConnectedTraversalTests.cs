using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public sealed class ProfessorHouseConnectedTraversalTests
{
    [UnityTest, Timeout(360000)]
    public IEnumerator ApprovedRoutesSupportRealPlayerInputAcrossThreeFloors()
    {
        return TraverseAsync().ToCoroutine();
    }

    private async UniTask TraverseAsync()
    {
        var root = await LoadAsync();
        var facade = FacadeManager.Instance;
        var objects = facade.Get<ObjectManager>();
        var input = facade.Get<InputManager>();
        var camera = root.SceneCamera;
        var lines = new List<string>();
        try
        {
            Assert.AreEqual(GameStateType.Exploration, facade.Get<GameStateManager>().Current);
            Assert.IsTrue(objects.PlayerAgent.isOnNavMesh);
            for (int r = 0; r < root.Routes.Length; r++)
            {
                var route = root.Routes[r];
                Assert.IsTrue(NavMesh.SamplePosition(route.Points[0],out var start,.35f,NavMesh.AllAreas));
                Assert.IsTrue(objects.PlayerAgent.Warp(start.position), "Initial route setup: " + route.Key);
                await UniTask.Delay(200);
                for (int i = 1; i < route.Points.Length; i++)
                    await WalkAsync(route.Points[i], objects, input, camera, route.Key + " / " + i);
                var final = route.Points[route.Points.Length-1];
                Assert.Less(Vector3.Distance(final,objects.Player.transform.position),.45f,route.Key + " endpoint");
                Assert.AreEqual(final.y,objects.Player.transform.position.y,.22f,route.Key + " floor height");
                await UniTask.Delay(350);
                Assert.Greater(Vector3.Dot(camera.transform.up,Vector3.up),.65f,"Camera must stay upright.");
                Assert.IsFalse(Physics.CheckSphere(camera.transform.position,.04f,~0,QueryTriggerInteraction.Ignore),route.Key + " camera inside solid geometry");
                lines.Add("PASS " + route.Key + " walked all " + route.Points.Length + " ordered waypoints; final=" + objects.Player.transform.position.ToString("F3"));
            }
            Directory.CreateDirectory("Docs/Validation/ProfessorHousePublicFinish");
            File.WriteAllText("Docs/Validation/ProfessorHousePublicFinish/PlayerTraversal.txt",string.Join("\n",lines)+"\n");
        }
        finally
        {
            input.ClearJoystick(this);
            await SceneManager.LoadSceneAsync("Title");
            await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        }
    }

    private async UniTask WalkAsync(Vector3 target, ObjectManager objects, InputManager input, Camera camera, string label)
    {
        Assert.IsTrue(NavMesh.SamplePosition(target,out var hit,.35f,NavMesh.AllAreas),label);
        var path = new NavMeshPath();
        Assert.IsTrue(objects.PlayerAgent.CalculatePath(hit.position,path),label);
        Assert.AreEqual(NavMeshPathStatus.PathComplete,path.status,label);
        for (int i = 1; i < path.corners.Length; i++)
        {
            var corner = path.corners[i];
            float started = Time.realtimeSinceStartup;
            while (true)
            {
                var delta = corner - objects.Player.transform.position;
                delta.y = 0;
                if (delta.sqrMagnitude < .0144f) break;
                Assert.Less(Time.realtimeSinceStartup-started,15,label + " movement stalled at " + objects.Player.transform.position + "; target=" + corner);
                Assert.IsTrue(objects.PlayerAgent.isOnNavMesh,label);
                var forward = camera.transform.forward;
                forward.y = 0;
                forward.Normalize();
                var right = Vector3.Cross(Vector3.up,forward);
                float speed = Mathf.Min(.7f,delta.magnitude*2.5f);
                input.SetJoystick(this,new Vector2(Vector3.Dot(delta.normalized,right),Vector3.Dot(delta.normalized,forward))*speed,true);
                await UniTask.Yield();
            }
        }
        input.ClearJoystick(this);
        await UniTask.Yield();
    }

    [UnityTest]
    public IEnumerator LeavingAndReenteringRestoresCameraAndAssetOwnership()
    {
        return ReentryAsync().ToCoroutine();
    }

    private async UniTask ReentryAsync()
    {
        await SceneManager.LoadSceneAsync("Title");
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        var previousAxis = CinemachineCore.GetInputAxis;
        var facade = FacadeManager.Instance;
        for (int i = 0; i < 2; i++)
        {
            var root = await LoadAsync();
            Assert.IsNotNull(facade.Get<ObjectManager>().Player);
            Assert.AreSame(root.SceneCamera,facade.Get<CameraManager>().Camera);
            Assert.AreEqual(1,facade.Get<AddressableManager>().OwnedBatchCount,"Only walkthrough prefab label is needed.");
            await SceneManager.LoadSceneAsync("Title");
            await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
            Assert.IsNull(facade.Get<ObjectManager>().Player);
            Assert.IsNull(facade.Get<CameraManager>().Camera);
            Assert.AreEqual(previousAxis,CinemachineCore.GetInputAxis);
            Assert.AreEqual(1,facade.Get<AddressableManager>().OwnedBatchCount,"Only Title ownership remains.");
        }
    }

    private static async UniTask<ProfessorHouseWalkthroughRoot> LoadAsync()
    {
#if UNITY_EDITOR
        await EditorSceneManager.LoadSceneAsyncInPlayMode(ProfessorHouseWalkthroughRoot.ScenePath,new LoadSceneParameters(LoadSceneMode.Single));
#else
        await SceneManager.LoadSceneAsync("Professor_House_Connected");
#endif
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        var root = SceneBinding.FirstInScene<ProfessorHouseWalkthroughRoot>(SceneManager.GetActiveScene());
        Assert.IsNotNull(root);
        return root;
    }
}
