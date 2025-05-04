using System;
using UnityEngine;


public class GameplayEntryPoint : MonoBehaviour
{
    public Action GoToMainMainMenuRequested;

    [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;

    public void Run(UIRootView uiRoot)
    {
        var uiScene = Instantiate(_sceneUIRootPrefab);
        uiRoot.AttachSceneUI(uiScene.gameObject);

        uiScene.GoToMainMenuButtonClicked += () =>
        {
            GoToMainMainMenuRequested?.Invoke();
        };
    }
}
