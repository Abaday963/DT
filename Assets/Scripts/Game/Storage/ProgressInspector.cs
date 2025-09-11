using UnityEngine;
using UnityEngine.UI;

public class ProgressInspector : MonoBehaviour
{
    [Header("Developer Tools")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private Button unlockRegularButton;
    [SerializeField] private Button unlockAllButton;
    [SerializeField] private Button maxStarsButton;
    [SerializeField] private Button testFirstSpecialButton;

    [Header("Level Mapping")]
    [SerializeField] private int firstLevelSceneIndex = 3; // С какой сцены начинаются уровни
    [SerializeField] private int levelsPerLine = 6;

    private StarManager starManager;

    private void Start()
    {
        starManager = StarManager.Instance;

        if (starManager == null)
        {
            Debug.LogError("[ProgressInspector] StarManager не найден!");
            return;
        }

        SetupLevelMapping();
        SetupButtons();
    }

    private void SetupLevelMapping()
    {
        Debug.Log($"[ProgressInspector] Уровни начинаются с Build Index {firstLevelSceneIndex}");
        Debug.Log($"[ProgressInspector] ЛИНИЯ 1: Уровни 1-5 (обычные), Уровень 6 (специальный, нужно 15 звезд)");
        Debug.Log($"[ProgressInspector] ЛИНИЯ 2: Уровни 7-11 (обычные), Уровень 12 (специальный, нужно 33 звезды)");
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

    public int GetLineForLevel(int levelIndex)
    {
        return levelIndex / levelsPerLine;
    }

    public int GetPositionInLine(int levelIndex)
    {
        return levelIndex % levelsPerLine;
    }

    public bool IsSpecialLevel(int levelIndex)
    {
        return GetPositionInLine(levelIndex) == levelsPerLine - 1;
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

        if (unlockRegularButton != null)
        {
            unlockRegularButton.onClick.AddListener(() =>
            {
                if (starManager != null)
                {
                    starManager.UnlockAllRegularLevels();
                    Debug.Log("[ProgressInspector] Все обычные уровни разблокированы!");
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
                    Debug.Log("[ProgressInspector] ВСЕ уровни разблокированы (включая специальные)!");
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

        if (testFirstSpecialButton != null)
        {
            testFirstSpecialButton.onClick.AddListener(() =>
            {
                if (starManager != null)
                {
                    starManager.GiveStarsForFirstSpecialTest();
                    Debug.Log("[ProgressInspector] Дано 14 звезд для тестирования первого специального уровня!");
                }
            });
        }
    }

    // Методы для показа информации о конкретном уровне
    [ContextMenu("Показать конкретный уровень")]
    public void ShowSpecificLevel(int levelIndex)
    {
        if (starManager == null) return;

        int stars = starManager.GetLevelStars(levelIndex);
        bool unlocked = starManager.IsLevelUnlocked(levelIndex);
        int sceneIndex = GetSceneIndexForLevel(levelIndex);
        int line = GetLineForLevel(levelIndex);
        int positionInLine = GetPositionInLine(levelIndex);
        bool isSpecial = IsSpecialLevel(levelIndex);

        string levelInfo = $"Уровень {levelIndex + 1} (Сцена {sceneIndex}, Линия {line + 1}, Позиция {positionInLine + 1})";

        if (isSpecial)
        {
            int requiredStars = starManager.GetStarsRequiredForSpecialLevel(levelIndex);
            bool canUnlock = starManager.CanUnlockSpecialLevel(levelIndex);
            levelInfo += $" [СПЕЦИАЛЬНЫЙ - нужно {requiredStars} звезд, можно разблокировать: {canUnlock}]";
        }

        levelInfo += $": {stars} звезд, {(unlocked ? "разблокирован" : "заблокирован")}";

        Debug.Log(levelInfo);
    }

    // Показать информацию по индексу сцены
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

    // Подробная статистика прогресса
    public void LogCurrentProgress()
    {
        if (starManager == null) return;

        GameProgress progress = starManager.GetGameProgress();
        Debug.Log($"=== ОБЩИЙ ПРОГРЕСС ===");
        Debug.Log($"Всего звезд: {progress.totalStars} из {progress.levels.Count * 3} возможных");

        // Статистика по линиям
        for (int line = 0; line < 2; line++)
        {
            int lineStars = 0;
            int completedInLine = 0;
            int perfectInLine = 0;
            int unlockedInLine = 0;
            int regularUnlockedInLine = 0;
            bool specialUnlocked = false;

            Debug.Log($"=== ЛИНИЯ {line + 1} ===");

            for (int pos = 0; pos < levelsPerLine; pos++)
            {
                int levelIndex = line * levelsPerLine + pos;
                if (levelIndex < progress.levels.Count)
                {
                    var level = progress.levels[levelIndex];
                    lineStars += level.stars;

                    if (level.isUnlocked)
                    {
                        unlockedInLine++;
                        if (!level.isSpecialLevel) regularUnlockedInLine++;
                    }
                    if (level.stars > 0) completedInLine++;
                    if (level.stars == 3) perfectInLine++;
                    if (level.isSpecialLevel && level.isUnlocked) specialUnlocked = true;
                }
            }

            Debug.Log($"Звезд в линии: {lineStars}/{levelsPerLine * 3}");
            Debug.Log($"Разблокировано: {unlockedInLine}/{levelsPerLine} (обычных: {regularUnlockedInLine}/5, специальный: {specialUnlocked})");
            Debug.Log($"Пройдено: {completedInLine}/{levelsPerLine}, Идеально: {perfectInLine}/{levelsPerLine}");

            // Информация о специальном уровне
            int specialLevelIndex = line * levelsPerLine + (levelsPerLine - 1);
            if (specialLevelIndex < progress.levels.Count)
            {
                int requiredStars = starManager.GetStarsRequiredForSpecialLevel(specialLevelIndex);
                bool canUnlock = starManager.CanUnlockSpecialLevel(specialLevelIndex);
                Debug.Log($"Специальный уровень {specialLevelIndex + 1}: нужно {requiredStars} звезд, можно разблокировать: {canUnlock}");
            }
        }
    }

    // Методы для работы с текущим уровнем
    public void ShowCurrentLevelProgress()
    {
        if (starManager == null) return;

        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
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

    // Методы для тестирования уровней
    public void GiveStarsToLevel(int levelIndex, int stars)
    {
        if (starManager == null) return;

        stars = Mathf.Clamp(stars, 0, 3);
        starManager.SetLevelStars(levelIndex, stars);

        int sceneIndex = GetSceneIndexForLevel(levelIndex);
        int line = GetLineForLevel(levelIndex);
        string levelType = IsSpecialLevel(levelIndex) ? "специальному" : "обычному";

        Debug.Log($"[ProgressInspector] {levelType} уровню {levelIndex + 1} (Линия {line + 1}, Сцена {sceneIndex}) присвоено {stars} звезд");
    }

    // Методы быстрого тестирования для текущего уровня
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

    // Методы для тестирования специальных уровней
    [ContextMenu("Дать 15 звезд (разблокировать первый спец уровень)")]
    public void Give15StarsForFirstSpecial()
    {
        if (starManager == null) return;

        // Сбрасываем прогресс и даем ровно 15 звезд
        starManager.ResetProgress();

        // Распределяем 15 звезд по первым 5 уровням
        for (int i = 0; i < 5; i++)
        {
            starManager.SetLevelStars(i, 3);
        }

        Debug.Log("[ProgressInspector] Дано 15 звезд для разблокировки первого специального уровня");
    }

    [ContextMenu("Дать 33 звезды (разблокировать второй спец уровень)")]
    public void Give33StarsForSecondSpecial()
    {
        if (starManager == null) return;

        // Даем максимальные звезды всем обычным уровням
        starManager.ResetProgress();
        for (int i = 0; i < 12; i++)
        {
            if (!IsSpecialLevel(i)) // если не специальный уровень
            {
                starManager.SetLevelStars(i, 3);
            }
        }

        Debug.Log("[ProgressInspector] Дано 33 звезды (максимум) для разблокировки второго специального уровня");
    }

    // Дополнительные методы для отладки
    [ContextMenu("Показать текущий уровень")]
    public void ShowCurrentLevel()
    {
        ShowCurrentLevelProgress();
    }

    [ContextMenu("Показать общий прогресс")]
    public void ShowGeneralProgress()
    {
        LogCurrentProgress();
    }

    [ContextMenu("Показать статус специальных уровней")]
    public void ShowSpecialLevelsStatus()
    {
        if (starManager == null) return;

        Debug.Log("=== СТАТУС СПЕЦИАЛЬНЫХ УРОВНЕЙ ===");

        // Первый специальный уровень (индекс 5)
        int firstSpecial = 5;
        ShowSpecificLevel(firstSpecial);

        // Второй специальный уровень (индекс 11)
        int secondSpecial = 11;
        ShowSpecificLevel(secondSpecial);

        int totalStars = starManager.GetTotalStars();
        Debug.Log($"Общее количество звезд: {totalStars}");
        Debug.Log($"До первого специального: {Mathf.Max(0, 15 - totalStars)} звезд");
        Debug.Log($"До второго специального: {Mathf.Max(0, 33 - totalStars)} звезд");
    }

    // Методы для тестирования конкретных линий
    [ContextMenu("Пройти первую линию идеально")]
    public void CompleteFirstLinePerfectly()
    {
        if (starManager == null) return;

        for (int i = 0; i < levelsPerLine; i++)
        {
            if (!IsSpecialLevel(i)) // Обычные уровни первой линии
            {
                starManager.SetLevelStars(i, 3);
            }
        }

        Debug.Log("[ProgressInspector] Первая линия (уровни 1-5) пройдена идеально");
    }

    [ContextMenu("Пройти вторую линию идеально")]
    public void CompleteSecondLinePerfectly()
    {
        if (starManager == null) return;

        for (int i = levelsPerLine; i < levelsPerLine * 2; i++)
        {
            if (!IsSpecialLevel(i)) // Обычные уровни второй линии
            {
                starManager.SetLevelStars(i, 3);
            }
        }

        Debug.Log("[ProgressInspector] Вторая линия (уровни 7-11) пройдена идеально");
    }

    // Методы для работы с маппингом
    [ContextMenu("Тест маппинга индексов")]
    public void TestIndexMapping()
    {
        Debug.Log("=== ТЕСТ МАППИНГА ИНДЕКСОВ ===");

        for (int levelIndex = 0; levelIndex < 12; levelIndex++)
        {
            int sceneIndex = GetSceneIndexForLevel(levelIndex);
            int backToLevel = GetLevelIndexForScene(sceneIndex);
            int line = GetLineForLevel(levelIndex);
            int position = GetPositionInLine(levelIndex);
            bool isSpecial = IsSpecialLevel(levelIndex);

            string specialText = isSpecial ? " [СПЕЦИАЛЬНЫЙ]" : "";
            Debug.Log($"Уровень {levelIndex + 1} -> Сцена {sceneIndex} -> Обратно уровень {backToLevel + 1}, " +
                     $"Линия {line + 1}, Позиция {position + 1}{specialText}");
        }

        int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevel = GetLevelIndexForScene(currentScene);
        Debug.Log($"Текущая сцена: {currentScene}, соответствует уровню: {currentLevel + 1}");
    }

    // Дополнительные удобные методы
    public void LogLevelProgress(int levelIndex)
    {
        ShowSpecificLevel(levelIndex);
    }

    public void LogLevelProgressBySceneIndex(int sceneIndex)
    {
        ShowSpecificLevelBySceneIndex(sceneIndex);
    }

    // Методы для продвинутого тестирования
    [ContextMenu("Симуляция прохождения игры")]
    public void SimulateGameProgress()
    {
        if (starManager == null) return;

        starManager.ResetProgress();

        Debug.Log("=== СИМУЛЯЦИЯ ПРОХОЖДЕНИЯ ИГРЫ ===");

        // Проходим первую линию с разными результатами
        starManager.SetLevelStars(0, 2); // 2 звезды
        starManager.SetLevelStars(1, 3); // 3 звезды  
        starManager.SetLevelStars(2, 1); // 1 звезда
        starManager.SetLevelStars(3, 3); // 3 звезды
        starManager.SetLevelStars(4, 3); // 3 звезды

        Debug.Log($"После прохождения первой линии: {starManager.GetTotalStars()} звезд");
        Debug.Log($"Первый специальный уровень разблокирован: {starManager.IsLevelUnlocked(5)}");

        // Добираем звезды для разблокировки первого специального
        starManager.SetLevelStars(2, 3); // Улучшаем результат на 3-м уровне

        Debug.Log($"После улучшения: {starManager.GetTotalStars()} звезд");
        Debug.Log($"Первый специальный уровень разблокирован: {starManager.IsLevelUnlocked(5)}");

        // Проходим первый специальный уровень
        if (starManager.IsLevelUnlocked(5))
        {
            starManager.SetLevelStars(5, 2);
            Debug.Log("Первый специальный уровень пройден на 2 звезды!");
        }

        ShowGeneralProgress();
    }
}