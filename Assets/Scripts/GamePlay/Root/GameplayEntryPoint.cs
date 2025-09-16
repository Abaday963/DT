using UnityEngine;
using System;

public class GameplayEntryPoint : MonoBehaviour
{
    public event System.Action GoToMainMainMenuRequested;
    public event System.Action GoToNextLevelRequested;
    public event System.Action RestartLevelRequested;

    [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;

    private StarManager starManager;
    private LevelLockManager lockManager;

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
        // Проверяем доступность следующего уровня
        if (starManager != null)
        {
            int currentLevel = starManager.GetCurrentLevelIndex();
            int nextLevel = currentLevel + 1;

            if (starManager.IsLevelAvailable(nextLevel))
            {
                GoToNextLevelRequested?.Invoke();
            }
            else
            {
                Debug.Log($"[GameplayEntryPoint] Следующий уровень {nextLevel + 1} недоступен");

                // Можно показать уведомление или вернуться в меню
                RequestGoToMainMenu();
            }
        }
        else
        {
            GoToNextLevelRequested?.Invoke();
        }
    }

    public void RequestRestartLevel()
    {
        RestartLevelRequested?.Invoke();
    }
}