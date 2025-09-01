using UnityEngine;
using UnityEngine.UI;

public class ProgressInspector : MonoBehaviour
{
    [Header("Developer Tools")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button unlockAllButton;
    [SerializeField] private Button maxStarsButton;

    [Header("Level Mapping")]
    [SerializeField] private int firstLevelSceneIndex = 3; // С какой сцены начинаются уровни

    private StarManager starManager;

    private void Start()
    {
        starManager = StarManager.Instance;

        if (starManager == null)
        {
            Debug.LogError("[ProgressInspector] StarManager не найден!");
            return;
        }

        // Создаем маппинги
        SetupLevelMapping();
        SetupButtons();
    }

    private void SetupLevelMapping()
    {
        Debug.Log($"[ProgressInspector] Уровни начинаются с Build Index {firstLevelSceneIndex}");
        Debug.Log($"[ProgressInspector] Уровень 1 = Сцена {firstLevelSceneIndex}, Уровень 2 = Сцена {firstLevelSceneIndex + 1}, и т.д.");
    }

    // Публичные методы для конвертации индексов
    public int GetSceneIndexForLevel(int levelIndex)
    {
        return firstLevelSceneIndex + levelIndex;
    }

    public int GetLevelIndexForScene(int sceneIndex)
    {
        return sceneIndex - firstLevelSceneIndex;
    }

    private void SetupButtons()
    {
        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.AddListener(() =>
            {
                if (starManager != null)
                {
                    starManager.ResetProgress();
                    Debug.Log("[ProgressInspector] Прогресс сброшен!");
                }
            });
        }

        if (unlockAllButton != null)
        {
            unlockAllButton.onClick.AddListener(() =>
            {
                if (starManager != null)
                {
                    starManager.UnlockAllLevels();
                    Debug.Log("[ProgressInspector] Все уровни разблокированы!");
                }
            });
        }

        if (maxStarsButton != null)
        {
            maxStarsButton.onClick.AddListener(() =>
            {
                if (starManager != null)
                {
                    starManager.GiveMaxStarsToAll();
                    Debug.Log("[ProgressInspector] Максимальные звезды выданы всем уровням!");
                }
            });
        }
    }

    // Методы для вызова из Inspector'а или других скриптов
    [ContextMenu("Показать конкретный уровень")]
    public void ShowSpecificLevel(int levelIndex)
    {
        if (starManager == null) return;

        int stars = starManager.GetLevelStars(levelIndex);
        bool unlocked = starManager.IsLevelUnlocked(levelIndex);
        int sceneIndex = GetSceneIndexForLevel(levelIndex);

        Debug.Log($"Уровень {levelIndex + 1} (Сцена {sceneIndex}): {stars} звезд, " +
                 (unlocked ? "разблокирован" : "заблокирован"));
    }

    // Методы для работы с индексами сцен
    public void ShowSpecificLevelBySceneIndex(int sceneIndex)
    {
        int levelIndex = GetLevelIndexForScene(sceneIndex);
        if (levelIndex >= 0)
        {
            ShowSpecificLevel(levelIndex);
        }
        else
        {
            Debug.LogWarning($"Сцена {sceneIndex} не соответствует ни одному уровню");
        }
    }

    // Публичные методы для использования в других скриптах
    public void LogCurrentProgress()
    {
        if (starManager == null) return;

        GameProgress progress = starManager.GetGameProgress();
        Debug.Log($"Текущий прогресс: {progress.totalStars} звезд из {progress.levels.Count * 3} возможных");

        // Подробная статистика
        int completedLevels = 0;
        int perfectLevels = 0;
        int unlockedLevels = 0;

        foreach (var level in progress.levels)
        {
            if (level.isUnlocked) unlockedLevels++;
            if (level.stars > 0) completedLevels++;
            if (level.stars == 3) perfectLevels++;
        }

        Debug.Log($"Разблокировано: {unlockedLevels}/{progress.levels.Count}, " +
                 $"Пройдено: {completedLevels}/{progress.levels.Count}, " +
                 $"Идеально: {perfectLevels}/{progress.levels.Count}");
    }

    public void LogLevelProgress(int levelIndex)
    {
        ShowSpecificLevel(levelIndex);
    }

    // Исправленный метод для логирования по индексу сцены
    public void LogLevelProgressBySceneIndex(int sceneIndex)
    {
        ShowSpecificLevelBySceneIndex(sceneIndex);
    }

    // Дополнительные методы для работы с прогрессом
    [ContextMenu("Показать общий прогресс")]
    public void ShowGeneralProgress()
    {
        LogCurrentProgress();
    }

    // Исправленный метод для работы с текущим уровнем
    public void ShowCurrentLevelProgress()
    {
        if (starManager == null) return;

        // Получаем текущий индекс сцены
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        // Конвертируем индекс сцены в индекс уровня
        int currentLevelIndex = GetLevelIndexForScene(currentSceneIndex);

        if (currentLevelIndex >= 0)
        {
            ShowSpecificLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log($"[ProgressInspector] Текущая сцена {currentSceneIndex} не является игровым уровнем");
        }
    }

    // Методы для тестирования конкретных уровней
    public void GiveStarsToLevel(int levelIndex, int stars)
    {
        if (starManager == null) return;

        stars = Mathf.Clamp(stars, 0, 3);
        starManager.SetLevelStars(levelIndex, stars);
        int sceneIndex = GetSceneIndexForLevel(levelIndex);
        Debug.Log($"[ProgressInspector] Уровню {levelIndex + 1} (Сцена {sceneIndex}) присвоено {stars} звезд");
    }

    public void UnlockSpecificLevel(int levelIndex)
    {
        if (starManager == null) return;

        GameProgress progress = starManager.GetGameProgress();
        if (levelIndex >= 0 && levelIndex < progress.levels.Count)
        {
            progress.levels[levelIndex].isUnlocked = true;
            int sceneIndex = GetSceneIndexForLevel(levelIndex);
            Debug.Log($"[ProgressInspector] Уровень {levelIndex + 1} (Сцена {sceneIndex}) разблокирован");
        }
    }

    // Исправленные методы для быстрого тестирования текущего уровня
    [ContextMenu("Дать 1 звезду текущему уровню")]
    public void GiveOneStarToCurrent()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelIndex = GetLevelIndexForScene(currentSceneIndex);

        if (currentLevelIndex >= 0)
        {
            GiveStarsToLevel(currentLevelIndex, 1);
        }
        else
        {
            Debug.LogWarning($"[ProgressInspector] Текущая сцена {currentSceneIndex} не является игровым уровнем");
        }
    }

    [ContextMenu("Дать 2 звезды текущему уровню")]
    public void GiveTwoStarsToCurrent()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelIndex = GetLevelIndexForScene(currentSceneIndex);

        if (currentLevelIndex >= 0)
        {
            GiveStarsToLevel(currentLevelIndex, 2);
        }
        else
        {
            Debug.LogWarning($"[ProgressInspector] Текущая сцена {currentSceneIndex} не является игровым уровнем");
        }
    }

    [ContextMenu("Дать 3 звезды текущему уровню")]
    public void GiveThreeStarsToCurrent()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelIndex = GetLevelIndexForScene(currentSceneIndex);

        if (currentLevelIndex >= 0)
        {
            GiveStarsToLevel(currentLevelIndex, 3);
        }
        else
        {
            Debug.LogWarning($"[ProgressInspector] Текущая сцена {currentSceneIndex} не является игровым уровнем");
        }
    }

    // Дополнительные удобные методы для отладки
    [ContextMenu("Показать текущий уровень")]
    public void ShowCurrentLevel()
    {
        ShowCurrentLevelProgress();
    }

    // Метод для тестирования маппинга
    [ContextMenu("Тест маппинга индексов")]
    public void TestIndexMapping()
    {
        Debug.Log("=== ТЕСТ МАППИНГА ИНДЕКСОВ ===");

        for (int levelIndex = 0; levelIndex < 5; levelIndex++)
        {
            int sceneIndex = GetSceneIndexForLevel(levelIndex);
            int backToLevel = GetLevelIndexForScene(sceneIndex);
            Debug.Log($"Уровень {levelIndex + 1} -> Сцена {sceneIndex} -> Обратно уровень {backToLevel + 1}");
        }

        int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevel = GetLevelIndexForScene(currentScene);
        Debug.Log($"Текущая сцена: {currentScene}, соответствует уровню: {currentLevel + 1}");
    }
}