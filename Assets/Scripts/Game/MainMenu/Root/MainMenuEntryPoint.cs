using UnityEngine;
using System;

public class MainMenuEntryPoint : MonoBehaviour
{
    public event Action GoToGameplaySceneRequested;

    [SerializeField] private UIMainMenuRootBinder _sceneUIRootPrefab;

    public void Run(UIRootView uiRoot)
    {
        var uiScene = Instantiate(_sceneUIRootPrefab);
        uiRoot.AttachSceneUI(uiScene.gameObject);

        uiScene.GoToGameplayButtonClicked += () =>
        {
            GoToGameplaySceneRequested?.Invoke();
        };
    }
}
