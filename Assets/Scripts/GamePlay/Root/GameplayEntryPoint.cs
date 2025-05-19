using UnityEngine;
using System;

public class GameplayEntryPoint : MonoBehaviour
{
    public event System.Action GoToMainMainMenuRequested;
    public event System.Action GoToNextLevelRequested;
    public event System.Action RestartLevelRequested;

    [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;

    public void Run(UIRootView uiRoot)
    {
        var uiScene = Instantiate(_sceneUIRootPrefab);
        uiRoot.AttachSceneUI(uiScene.gameObject);

        // Настраиваем существующие обработчики
        uiScene.GoToMainMenuButtonClicked += () =>
        {
            GoToMainMainMenuRequested?.Invoke();
        };

        // Добавляем обработчики для новых событий
        uiScene.NextLevelButtonClicked += () =>
        {
            GoToNextLevelRequested?.Invoke();
        };

        uiScene.RestartLevelButtonClicked += () =>
        {
            RestartLevelRequested?.Invoke();
        };
    }

    // Методы для вызова событий из кода
    public void RequestGoToMainMenu()
    {
        GoToMainMainMenuRequested?.Invoke();
    }

    public void RequestNextLevel()
    {
        GoToNextLevelRequested?.Invoke();
    }

    public void RequestRestartLevel()
    {
        RestartLevelRequested?.Invoke();
    }
}