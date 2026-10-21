using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Cinemachine;

public sealed class NarniaPlayModeTests
{
    // IEnumerator is required by Unity Test Framework, never used in gameplay.
    [UnityTest] public IEnumerator TitleToHouseSupportsDialogueSettingsDoorsAndReentry() { return TitleFlowAsync().ToCoroutine(); }
    private async UniTask TitleFlowAsync()
    {
        await SceneManager.LoadSceneAsync("Title");
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        var facade = FacadeManager.Instance;
        var scenes = facade.Get<GameSceneManager>();
        var objects = facade.Get<ObjectManager>();
        var states = facade.Get<GameStateManager>();
        var quests = facade.Get<QuestManager>();
        var previousAxis = CinemachineCore.GetInputAxis;
        Click("StartButton");
        scenes.LoadScene(Define.SceneName.Title); // A second request while loading must be ignored.
        await WaitForSceneAsync("Professor_House");
        Assert.IsNotNull(objects.Player);
        Assert.IsNotNull(facade.Get<CameraManager>().Camera);
        Assert.AreEqual(GameStateType.Dialogue, states.Current);
        Assert.AreEqual(Vector2.zero, facade.Get<InputManager>().Movement);
        var dialogue = Object.FindObjectOfType<MessageBoxController>();
        Assert.IsNotNull(dialogue);
        for (int i = 0; i < 8 && states.Current == GameStateType.Dialogue; i++) Click("PlayNextLine");
        await UniTask.Yield();
        Assert.AreEqual(GameStateType.Exploration, states.Current);
        Assert.AreEqual(1, quests.Level);
        Click("SettingWindowOn");
        Assert.AreEqual(GameStateType.Settings, states.Current);
        Assert.IsFalse(facade.Get<InputManager>().AllowsInteraction);
        Click("SettingWindowOff");
        Assert.AreEqual(GameStateType.Exploration, states.Current);
        Assert.IsTrue(objects.SiblingsDoor.IsUnlocked);
        Assert.IsFalse(objects.WardrobeDoor.IsUnlocked);
        var input = facade.Get<InputManager>();
        input.SetJoystick(this, Vector2.right, true);
        input.ReadFrame();
        Assert.AreEqual(Vector2.right, input.Movement);
        Click("SettingWindowOn");
        input.ReadFrame();
        Assert.AreEqual(Vector2.zero, input.Movement);
        Click("SettingWindowOff");
        input.ClearJoystick(this);
        objects.SetInteraction(objects.WardrobeDoor);
        Click("ActionButton");
        Assert.IsFalse(objects.WardrobeDoor.IsOpen);
        Assert.AreEqual(1, quests.Level);
        objects.SetInteraction(objects.SiblingsDoor);
        Click("ActionButton");
        Assert.AreEqual(2, quests.Level);
        int areaMask = objects.PlayerAgent.areaMask;
        Click("ActionButton");
        Click("ActionButton");
        Assert.AreEqual(areaMask, objects.PlayerAgent.areaMask);
        Assert.AreEqual(2, quests.Level);
        Assert.IsTrue(objects.WardrobeDoor.IsUnlocked);
        objects.SetInteraction(objects.WardrobeDoor);
        Click("ActionButton");
        Assert.AreEqual(3, quests.Level);
        var previousPlayer = objects.Player;
        await scenes.LoadSceneAsync(Define.SceneName.Title);
        Assert.IsNull(objects.Player);
        Assert.AreEqual(1, facade.Get<AddressableManager>().OwnedBatchCount, "Only Title assets should remain owned.");
        Assert.AreEqual(1, facade.Get<AddressableManager>().CachedAssetCount);
        Assert.IsNull(facade.Get<CameraManager>().Camera);
        Assert.AreEqual(previousAxis, CinemachineCore.GetInputAxis);
        await scenes.LoadSceneAsync(Define.SceneName.Professor_House);
        Assert.AreEqual(3, quests.Level);
        Assert.AreEqual(GameStateType.Exploration, states.Current);
        Assert.IsNotNull(objects.Player);
        Assert.AreNotSame(previousPlayer, objects.Player);
        await scenes.LoadSceneAsync(Define.SceneName.Title);
    }
    [UnityTest] public IEnumerator DirectHouseEntryDoesNotRequireTitle() { return DirectEntryAsync().ToCoroutine(); }
    private async UniTask DirectEntryAsync()
    {
        await SceneManager.LoadSceneAsync("Professor_House");
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        Assert.IsNotNull(FacadeManager.Instance.Get<ObjectManager>().Player);
        Assert.IsNotNull(FacadeManager.Instance.Get<CameraManager>().Camera);
        await SceneManager.LoadSceneAsync("Title");
    }
    private static void Click(string method)
    {
        foreach (var button in Object.FindObjectsOfType<Button>(true))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) != method) continue;
                Assert.IsTrue(button.isActiveAndEnabled, "Button must be reachable: " + method);
                button.onClick.Invoke();
                return;
            }
        }
        Assert.Fail("No persistent button binding: " + method);
    }
    private static async UniTask WaitForSceneAsync(string scene)
    {
        float started = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != scene || FacadeManager.Instance.Get<GameSceneManager>().IsLoading)
        {
            Assert.Less(Time.realtimeSinceStartup - started, 45, "Scene load timed out: " + scene);
            await UniTask.Yield();
        }
    }
}
