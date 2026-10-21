using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class LoadingController : SceneRoot
{
    [FormerlySerializedAs("_BackgroundIMG"), SerializeField] private Image backgroundImage;
    [FormerlySerializedAs("_LoadingProgressIMG"), SerializeField] private Image progressImage;
    [Singleton(typeof(UIManager))] private UIManager ui;
    protected override async UniTask BuildAsync(CancellationToken token)
    {
        if (!await resources.PreloadAsync<GameObject>(AddressableLabels.Loading, this, token) ||
            !await resources.PreloadAsync<Sprite>(AddressableLabels.Loading, this, token))
            throw new InvalidOperationException("Loading screen preload failed.");
        token.ThrowIfCancellationRequested();
        ui.SetSceneUIRoot(this, transform);
        var loading = ui.ShowSceneUI<UI_Loading>(AssetKeys.UI.Loading);
        loading.SetBackground(resources.Load<Sprite>(AssetKeys.LoadingBackgrounds[UnityEngine.Random.Range(0, AssetKeys.LoadingBackgrounds.Length)]));
    }
    protected override void ReleaseScene() { ui.ReleaseSceneUIs(this); }
}
