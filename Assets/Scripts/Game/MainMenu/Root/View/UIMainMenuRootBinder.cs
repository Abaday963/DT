using System;
using UnityEngine;

public class UIMainMenuRootBinder : MonoBehaviour
{
    // Событие для перехода на геймплей без указания уровня (для обратной совместимости)
    public event Action GoToGameplayButtonClicked;

    // Новое событие с параметром для указания конкретного уровня
    public event Action<string> GoToLevelRequested;

    // Старый метод (для обратной совместимости)
    public void HandleGoToGamePlayButtonClick()
    {
        GoToGameplayButtonClicked?.Invoke();
    }

    // Новый метод для перехода на конкретный уровень
    public void HandleGoToLevelClick(string levelName)
    {
        GoToLevelRequested?.Invoke(levelName);
    }

    // Для кнопок в UI можно создать публичные методы для конкретных уровней
    public void HandleGoToLevel1Click()
    {
        GoToLevelRequested?.Invoke(Scenes.LEVEL1);
    }

    public void HandleGoToLevel2Click()
    {
        GoToLevelRequested?.Invoke(Scenes.LEVEL2);
    }

    public void HandleGoToLevel3Click()
    {
        GoToLevelRequested?.Invoke(Scenes.LEVEL3);
    }

    public void HandleGoToLevel4Click()
    {
        GoToLevelRequested?.Invoke(Scenes.LEVEL4);
    }

    public void HandleGoToLevel5Click()
    {
        GoToLevelRequested?.Invoke(Scenes.LEVEL5);
    }
}