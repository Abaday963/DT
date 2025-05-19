using UnityEngine;
using System;

public class MainMenuEntryPoint : MonoBehaviour
{
    public event System.Action<string> GoToGameplaySceneRequested;
    [SerializeField] private UIMainMenuRootBinder _sceneUIRootPrefab;

    public void Run(UIRootView uiRoot)
    {
        var uiScene = Instantiate(_sceneUIRootPrefab);
        uiRoot.AttachSceneUI(uiScene.gameObject);

        // Изменяем здесь, чтобы передавать аргумент
        uiScene.GoToGameplayButtonClicked += () =>
        {
            // Вызываем с null или с первым уровнем напрямую
            GoToGameplaySceneRequested?.Invoke(Scenes.LEVEL1);
        };

        // Подписываемся на событие выбора конкретного уровня
        uiScene.GoToLevelRequested += (levelName) =>
        {
            GoToGameplaySceneRequested?.Invoke(levelName);
        };
    }

    // Метод для запуска конкретного уровня
    public void RequestStartLevel(string levelName)
    {
        GoToGameplaySceneRequested?.Invoke(levelName);
    }

    // Метод для запуска первого уровня
    public void RequestStartGame()
    {
        GoToGameplaySceneRequested?.Invoke(Scenes.LEVEL1);
    }
}
