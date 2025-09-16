using System;
using UnityEngine;

/// <summary>
/// Обновленный биндер главного меню с поддержкой системы блокировки
/// </summary>
public class UIMainMenuRootBinder : MonoBehaviour
{
    // Событие для перехода на геймплей без указания уровня (для обратной совместимости)
    public event Action GoToGameplayButtonClicked;
    // Новое событие с параметром для указания конкретного уровня
    public event Action<string> GoToLevelRequested;

    [Header("Кнопки уровней (для визуального обновления)")]
    [SerializeField] private UnityEngine.UI.Button[] levelButtons;
    [SerializeField] private GameObject[] lockIcons; // Иконки замочков для заблокированных уровней

    private StarManager starManager;
    private LevelLockManager lockManager;

    private void Start()
    {
        // Получаем ссылки на менеджеры
        starManager = StarManager.Instance;
        lockManager = LevelLockManager.Instance;

        // Подписываемся на события изменения блокировок
        if (lockManager != null)
        {
            lockManager.OnLevelLockChanged += OnLevelLockChanged;
        }

        if (starManager != null)
        {
            starManager.OnLevelUnlocked += OnLevelUnlocked;
            starManager.OnProgressLoaded += UpdateAllLevelButtons;
        }

        // Обновляем состояние кнопок
        UpdateAllLevelButtons();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (lockManager != null)
        {
            lockManager.OnLevelLockChanged -= OnLevelLockChanged;
        }

        if (starManager != null)
        {
            starManager.OnLevelUnlocked -= OnLevelUnlocked;
            starManager.OnProgressLoaded -= UpdateAllLevelButtons;
        }
    }

    private void OnLevelLockChanged(int levelIndex, bool isLocked)
    {
        UpdateLevelButton(levelIndex);
    }

    private void OnLevelUnlocked(int levelIndex)
    {
        UpdateLevelButton(levelIndex);
    }

    /// <summary>
    /// Обновляет состояние всех кнопок уровней
    /// </summary>
    private void UpdateAllLevelButtons()
    {
        if (starManager == null) return;

        var gameProgress = starManager.GetGameProgress();
        if (gameProgress == null) return;

        for (int i = 0; i < gameProgress.levels.Count; i++)
        {
            UpdateLevelButton(i);
        }
    }

    /// <summary>
    /// Обновляет состояние конкретной кнопки уровня
    /// </summary>
    private void UpdateLevelButton(int levelIndex)
    {
        // Обновляем кнопку, если она есть в массиве
        if (levelButtons != null && levelIndex < levelButtons.Length && levelButtons[levelIndex] != null)
        {
            bool isAvailable = starManager != null ? starManager.IsLevelAvailable(levelIndex) : false;
            levelButtons[levelIndex].interactable = isAvailable;

            // Обновляем визуал (например, цвет)
            var colors = levelButtons[levelIndex].colors;
            colors.normalColor = isAvailable ? Color.white : Color.gray;
            levelButtons[levelIndex].colors = colors;
        }

        // Обновляем иконку замочка
        if (lockIcons != null && levelIndex < lockIcons.Length && lockIcons[levelIndex] != null)
        {
            bool showLock = starManager != null ? !starManager.IsLevelAvailable(levelIndex) : true;
            lockIcons[levelIndex].SetActive(showLock);
        }
    }

    /// <summary>
    /// Проверяет доступность уровня перед переходом
    /// </summary>
    private bool CanGoToLevel(int levelIndex)
    {
        if (starManager == null) return true; // Если нет менеджера, разрешаем переход

        bool available = starManager.IsLevelAvailable(levelIndex);

        if (!available)
        {
            // Показываем сообщение пользователю
            string reason = "";
            if (lockManager != null && lockManager.IsLevelLockedByAdmin(levelIndex))
            {
                reason = "Уровень временно недоступен";
            }
            else
            {
                reason = "Уровень еще не разблокирован";
            }

            Debug.Log($"[UIMainMenuRootBinder] Переход на уровень {levelIndex + 1} заблокирован: {reason}");

            // Здесь можно показать UI уведомление пользователю
            // ShowLevelLockedNotification(reason);
        }

        return available;
    }

    // Методы для кнопок (с проверкой доступности)
    public void HandleGoToGamePlayButtonClick()
    {
        GoToGameplayButtonClicked?.Invoke();
    }

    public void HandleGoToLevelClick(string levelName)
    {
        GoToLevelRequested?.Invoke(levelName);
    }

    // Обновленные методы для конкретных уровней с проверкой доступности
    public void HandleGoToLevel1Click()
    {
        if (CanGoToLevel(0))
            GoToLevelRequested?.Invoke(Scenes.LEVEL1);
    }

    public void HandleGoToLevel2Click()
    {
        if (CanGoToLevel(1))
            GoToLevelRequested?.Invoke(Scenes.LEVEL2);
    }

    public void HandleGoToLevel3Click()
    {
        if (CanGoToLevel(2))
            GoToLevelRequested?.Invoke(Scenes.LEVEL3);
    }

    public void HandleGoToLevel4Click()
    {
        if (CanGoToLevel(3))
            GoToLevelRequested?.Invoke(Scenes.LEVEL4);
    }

    public void HandleGoToLevel5Click()
    {
        if (CanGoToLevel(4))
            GoToLevelRequested?.Invoke(Scenes.LEVEL5);
    }

    public void HandleGoToLevel5_1Click()
    {
        if (CanGoToLevel(5)) // Специальный уровень
            GoToLevelRequested?.Invoke(Scenes.LEVEL5_1);
    }

    public void HandleGoToLevel6Click()
    {
        if (CanGoToLevel(5))
            GoToLevelRequested?.Invoke(Scenes.LEVEL6);
    }

    public void HandleGoToLevel7Click()
    {
        if (CanGoToLevel(6))
            GoToLevelRequested?.Invoke(Scenes.LEVEL7);
    }

    public void HandleGoToLevel8Click()
    {
        if (CanGoToLevel(7))
            GoToLevelRequested?.Invoke(Scenes.LEVEL8);
    }

    public void HandleGoToLevel9Click()
    {
        if (CanGoToLevel(8))
            GoToLevelRequested?.Invoke(Scenes.LEVEL9);
    }

    public void HandleGoToLevel10Click()
    {
        if (CanGoToLevel(9))
            GoToLevelRequested?.Invoke(Scenes.LEVEL10);
    }

    public void HandleGoToLevel10_1Click()
    {
        if (CanGoToLevel(11)) // Специальный уровень
            GoToLevelRequested?.Invoke(Scenes.LEVEL10_1);
    }

    #region Debug методы

    [ContextMenu("Заблокировать уровень 1")]
    public void DebugLockLevel1()
    {
        if (lockManager != null)
            lockManager.LockLevel(0);
    }

    [ContextMenu("Разблокировать уровень 1")]
    public void DebugUnlockLevel1()
    {
        if (lockManager != null)
            lockManager.UnlockLevel(0);
    }

    [ContextMenu("Показать состояние всех уровней")]
    public void DebugShowAllLevels()
    {
        if (starManager == null || lockManager == null) return;

        var gameProgress = starManager.GetGameProgress();
        if (gameProgress == null) return;

        for (int i = 0; i < gameProgress.levels.Count; i++)
        {
            bool available = starManager.IsLevelAvailable(i);
            bool adminLocked = lockManager.IsLevelLockedByAdmin(i);
            bool starUnlocked = starManager.IsLevelUnlocked(i);

            Debug.Log($"Уровень {i + 1}: Доступен={available}, АдминБлок={adminLocked}, StarManager={starUnlocked}");
        }
    }

    #endregion
}
