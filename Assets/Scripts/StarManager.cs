using UnityEngine;
using System.Collections.Generic;
using YG;

[System.Serializable]
public class LevelProgress
{
    public int levelIndex;
    public int stars;
    public bool isUnlocked;
    public int lineIndex; // 0 - первая линия, 1 - вторая линия
    public bool isSpecialLevel; // является ли уровень специальным (последним в линии)

    public LevelProgress(int index, int starsEarned, bool unlocked, int line = 0, bool special = false)
    {
        levelIndex = index;
        stars = starsEarned;
        isUnlocked = unlocked;
        lineIndex = line;
        isSpecialLevel = special;
    }
}

[System.Serializable]
public class GameProgress
{
    public List<LevelProgress> levels = new List<LevelProgress>();
    public int totalStars = 0;
}

public class StarManager : MonoBehaviour
{
    public static StarManager Instance { get; private set; }

    [SerializeField] private int totalLevelsCount = 12;
    [SerializeField] private int levelsPerLine = 6;
    [SerializeField] private int starsRequiredForFirstSpecial = 15;
    [SerializeField] private int starsRequiredForSecondSpecial = 33; // все звезды
    [SerializeField] private bool debugMode = true;

    private GameProgress gameProgress;
    private const string SAVE_KEY = "GameProgress";
    private bool isInitialized = false;

    public System.Action<int, int> OnLevelStarsUpdated;
    public System.Action<int> OnTotalStarsUpdated;
    public System.Action<int> OnStarsChanged;
    public System.Action<int> OnLevelUnlocked;
    public System.Action<int> OnSpecialLevelUnlocked;
    public System.Action OnProgressLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        YG2.onGetSDKData += LoadProgress;
        LoadProgress();
    }

    private void OnDestroy()
    {
        YG2.onGetSDKData -= LoadProgress;
    }

    private void InitializeProgress()
    {
        gameProgress = new GameProgress();

        for (int i = 0; i < totalLevelsCount; i++)
        {
            int lineIndex = i / levelsPerLine; // 0 или 1
            int positionInLine = i % levelsPerLine; // 0-5
            bool isSpecialLevel = (positionInLine == levelsPerLine - 1); // последний в линии
            bool isUnlocked = !isSpecialLevel; // все обычные уровни разблокированы, специальные - нет

            gameProgress.levels.Add(new LevelProgress(i, 0, isUnlocked, lineIndex, isSpecialLevel));
        }

        isInitialized = true;

        if (debugMode) Debug.Log("[StarManager] Прогресс инициализирован с двумя линиями");
    }

    public void LoadProgress()
    {
        if (YG2.saves == null)
        {
            if (debugMode) Debug.Log("[StarManager] SDK еще не готов");
            return;
        }

        string savedData = "";

        try
        {
            var savesType = YG2.saves.GetType();
            var fields = savesType.GetFields();

            foreach (var field in fields)
            {
                if (field.Name == SAVE_KEY && field.FieldType == typeof(string))
                {
                    savedData = (string)field.GetValue(YG2.saves);
                    break;
                }
            }

            if (string.IsNullOrEmpty(savedData))
            {
                var props = savesType.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.Name == SAVE_KEY && prop.PropertyType == typeof(string))
                    {
                        savedData = (string)prop.GetValue(YG2.saves);
                        break;
                    }
                }
            }
        }
        catch { }

        if (!string.IsNullOrEmpty(savedData))
        {
            try
            {
                gameProgress = JsonUtility.FromJson<GameProgress>(savedData);
                if (gameProgress.levels.Count < totalLevelsCount)
                {
                    for (int i = gameProgress.levels.Count; i < totalLevelsCount; i++)
                    {
                        int lineIndex = i / levelsPerLine;
                        int positionInLine = i % levelsPerLine;
                        bool isSpecialLevel = (positionInLine == levelsPerLine - 1);
                        bool isUnlocked = !isSpecialLevel;

                        gameProgress.levels.Add(new LevelProgress(i, 0, isUnlocked, lineIndex, isSpecialLevel));
                    }
                }

                RecalculateTotalStars();
                CheckSpecialLevelsUnlock(); // Проверяем разблокировку специальных уровней
                isInitialized = true;
                OnProgressLoaded?.Invoke();

                if (debugMode) Debug.Log($"[StarManager] Прогресс загружен: {gameProgress.totalStars} звезд");
            }
            catch
            {
                InitializeProgress();
            }
        }
        else
        {
            InitializeProgress();
        }
    }

    public void SaveProgress()
    {
        if (!isInitialized)
        {
            if (debugMode) Debug.LogWarning("[StarManager] Попытка сохранения до инициализации");
            return;
        }

        if (YG2.saves == null)
        {
            Debug.LogWarning("[StarManager] SDK не готов");
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(gameProgress);
            var savesType = YG2.saves.GetType();
            bool saved = false;

            foreach (var field in savesType.GetFields())
            {
                if (field.Name == SAVE_KEY && field.FieldType == typeof(string))
                {
                    field.SetValue(YG2.saves, jsonData);
                    saved = true;
                    break;
                }
            }

            if (!saved)
            {
                foreach (var prop in savesType.GetProperties())
                {
                    if (prop.Name == SAVE_KEY && prop.PropertyType == typeof(string) && prop.CanWrite)
                    {
                        prop.SetValue(YG2.saves, jsonData);
                        saved = true;
                        break;
                    }
                }
            }

            if (saved)
            {
                YG2.SaveProgress();
                if (debugMode) Debug.Log($"[StarManager] Прогресс сохранен: {gameProgress.totalStars} звезд");
            }
        }
        catch (System.Exception e)
        {
            if (debugMode) Debug.LogError($"[StarManager] Ошибка сохранения: {e.Message}");
        }
    }

    public void SetLevelStars(int levelIndex, int stars)
    {
        if (!isInitialized)
        {
            if (debugMode) Debug.LogWarning("[StarManager] Попытка установки звезд до инициализации");
            return;
        }

        if (levelIndex < 0 || levelIndex >= totalLevelsCount)
        {
            Debug.LogError($"[StarManager] Некорректный индекс: {levelIndex}. Допустимый диапазон: 0-{totalLevelsCount - 1}");
            return;
        }

        if (levelIndex >= gameProgress.levels.Count)
        {
            Debug.LogError($"[StarManager] Индекс {levelIndex} превышает размер списка уровней ({gameProgress.levels.Count})");
            return;
        }

        stars = Mathf.Clamp(stars, 0, 3);
        LevelProgress level = gameProgress.levels[levelIndex];
        int oldStars = level.stars;

        if (debugMode)
        {
            string levelType = level.isSpecialLevel ? "специальный" : "обычный";
            Debug.Log($"[StarManager] Установка звезд для {levelType} уровня {levelIndex + 1} (линия {level.lineIndex + 1}): {oldStars} -> {stars}");
        }

        if (stars != level.stars)
        {
            level.stars = stars;
            level.isUnlocked = true;

            RecalculateTotalStars();
            CheckSpecialLevelsUnlock(); // Проверяем разблокировку специальных уровней после изменения звезд
            SaveProgress();

            // Вызываем события ПОСЛЕ сохранения
            OnLevelStarsUpdated?.Invoke(levelIndex, stars);
            OnTotalStarsUpdated?.Invoke(gameProgress.totalStars);
            OnStarsChanged?.Invoke(gameProgress.totalStars);

            if (debugMode)
                Debug.Log($"[StarManager] События вызваны для уровня {levelIndex + 1}: {stars} звезд");
        }
        else
        {
            if (debugMode)
                Debug.Log($"[StarManager] Звезды не изменились для уровня {levelIndex + 1}");
        }
    }

    private void CheckSpecialLevelsUnlock()
    {
        // Проверяем первый специальный уровень (индекс 5 - последний в первой линии)
        int firstSpecialIndex = levelsPerLine - 1; // 5
        if (firstSpecialIndex < gameProgress.levels.Count)
        {
            LevelProgress firstSpecial = gameProgress.levels[firstSpecialIndex];
            if (!firstSpecial.isUnlocked && gameProgress.totalStars >= starsRequiredForFirstSpecial)
            {
                firstSpecial.isUnlocked = true;
                OnSpecialLevelUnlocked?.Invoke(firstSpecialIndex);
                OnLevelUnlocked?.Invoke(firstSpecialIndex);

                if (debugMode)
                    Debug.Log($"[StarManager] Разблокирован первый специальный уровень ({firstSpecialIndex + 1}) за {starsRequiredForFirstSpecial} звезд!");
            }
        }

        // Проверяем второй специальный уровень (индекс 11 - последний во второй линии)
        int secondSpecialIndex = (levelsPerLine * 2) - 1; // 11
        if (secondSpecialIndex < gameProgress.levels.Count)
        {
            LevelProgress secondSpecial = gameProgress.levels[secondSpecialIndex];
            if (!secondSpecial.isUnlocked && gameProgress.totalStars >= starsRequiredForSecondSpecial)
            {
                secondSpecial.isUnlocked = true;
                OnSpecialLevelUnlocked?.Invoke(secondSpecialIndex);
                OnLevelUnlocked?.Invoke(secondSpecialIndex);

                if (debugMode)
                    Debug.Log($"[StarManager] Разблокирован второй специальный уровень ({secondSpecialIndex + 1}) за {starsRequiredForSecondSpecial} звезд!");
            }
        }
    }

    // Новые методы для работы с линиями
    public bool IsSpecialLevel(int levelIndex)
    {
        if (!IsValid(levelIndex)) return false;
        return gameProgress.levels[levelIndex].isSpecialLevel;
    }

    public int GetLevelLine(int levelIndex)
    {
        if (!IsValid(levelIndex)) return -1;
        return gameProgress.levels[levelIndex].lineIndex;
    }

    public int GetStarsRequiredForSpecialLevel(int levelIndex)
    {
        if (!IsValid(levelIndex) || !gameProgress.levels[levelIndex].isSpecialLevel) return -1;

        int lineIndex = gameProgress.levels[levelIndex].lineIndex;
        return lineIndex == 0 ? starsRequiredForFirstSpecial : starsRequiredForSecondSpecial;
    }

    public bool CanUnlockSpecialLevel(int levelIndex)
    {
        if (!IsValid(levelIndex) || !gameProgress.levels[levelIndex].isSpecialLevel) return false;

        int requiredStars = GetStarsRequiredForSpecialLevel(levelIndex);
        return gameProgress.totalStars >= requiredStars;
    }

    // Остальные методы остаются без изменений
    public int GetLevelStars(int levelIndex)
    {
        if (!isInitialized) return 0;
        return IsValid(levelIndex) ? gameProgress.levels[levelIndex].stars : 0;
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (!isInitialized) return false;
        return IsValid(levelIndex) && gameProgress.levels[levelIndex].isUnlocked;
    }

    public int GetTotalStars()
    {
        if (!isInitialized) return 0;
        return gameProgress.totalStars;
    }

    public GameProgress GetGameProgress() => gameProgress;

    public int GetCurrentLevelIndex()
    {
        var identifier = FindObjectOfType<LevelIdentifier>();
        if (identifier != null)
            return identifier.logicalLevelIndex;

        Debug.LogWarning("[StarManager] LevelIdentifier не найден на сцене.");
        return -1;
    }

    private bool IsValid(int index) => index >= 0 && index < gameProgress.levels.Count;

    private void RecalculateTotalStars()
    {
        gameProgress.totalStars = 0;
        foreach (var level in gameProgress.levels)
            gameProgress.totalStars += level.stars;
    }
    // Добавьте эти методы в конец класса StarManager (перед закрывающей скобкой класса)

    /// <summary>
    /// Проверяет доступность уровня с учетом админ-блокировок
    /// Использует LevelLockManager если он доступен
    /// </summary>
    public bool IsLevelAvailable(int levelIndex)
    {
        // Сначала проверяем стандартную логику
        bool standardUnlock = IsLevelUnlocked(levelIndex);

        // Если есть LevelLockManager, учитываем его блокировки
        if (LevelLockManager.Instance != null)
        {
            return LevelLockManager.Instance.IsLevelAvailable(levelIndex);
        }

        // Если LevelLockManager нет, используем стандартную логику
        return standardUnlock;
    }

    /// <summary>
    /// Проверяет, заблокирован ли уровень администратором
    /// </summary>
    public bool IsLevelLockedByAdmin(int levelIndex)
    {
        if (LevelLockManager.Instance != null)
        {
            return LevelLockManager.Instance.IsLevelLockedByAdmin(levelIndex);
        }

        return false; // Если LevelLockManager нет, админ-блокировок тоже нет
    }
    // Методы для принудительного обновления UI
    public void ForceUpdateAllDisplays()
    {
        if (!isInitialized) return;

        for (int i = 0; i < gameProgress.levels.Count; i++)
        {
            OnLevelStarsUpdated?.Invoke(i, gameProgress.levels[i].stars);
        }
        OnTotalStarsUpdated?.Invoke(gameProgress.totalStars);
        OnStarsChanged?.Invoke(gameProgress.totalStars);

        if (debugMode) Debug.Log("[StarManager] Принудительное обновление всех дисплеев");
    }

    [ContextMenu("Сбросить прогресс")]
    public void ResetProgress()
    {
        InitializeProgress();
        SaveProgress();
        OnTotalStarsUpdated?.Invoke(0);
        OnStarsChanged?.Invoke(0);
        ForceUpdateAllDisplays();

        if (debugMode) Debug.Log("[StarManager] Прогресс сброшен");
    }

    [ContextMenu("Разблокировать все обычные уровни")]
    public void UnlockAllRegularLevels()
    {
        if (!isInitialized) return;

        foreach (var level in gameProgress.levels)
        {
            if (!level.isSpecialLevel)
                level.isUnlocked = true;
        }

        SaveProgress();
        ForceUpdateAllDisplays();

        if (debugMode) Debug.Log("[StarManager] Все обычные уровни разблокированы");
    }

    [ContextMenu("Разблокировать ВСЕ уровни (включая специальные)")]
    public void UnlockAllLevels()
    {
        if (!isInitialized) return;

        foreach (var level in gameProgress.levels)
            level.isUnlocked = true;

        SaveProgress();
        ForceUpdateAllDisplays();

        if (debugMode) Debug.Log("[StarManager] ВСЕ уровни разблокированы (включая специальные)");
    }

    [ContextMenu("Дать 3 звезды всем уровням")]
    public void GiveMaxStarsToAll()
    {
        if (!isInitialized) return;

        for (int i = 0; i < gameProgress.levels.Count; i++)
            SetLevelStars(i, 3);

        if (debugMode) Debug.Log("[StarManager] Максимальные звезды выданы всем уровням");
    }

    // Методы для отладки
    [ContextMenu("Показать текущий прогресс")]
    public void DebugShowProgress()
    {
        if (!isInitialized)
        {
            Debug.Log("[StarManager] Не инициализирован");
            return;
        }

        Debug.Log($"[StarManager] Общий прогресс: {gameProgress.totalStars} звезд");

        // Показываем по линиям
        for (int line = 0; line < 2; line++)
        {
            Debug.Log($"=== ЛИНИЯ {line + 1} ===");
            for (int i = 0; i < levelsPerLine; i++)
            {
                int levelIndex = line * levelsPerLine + i;
                if (levelIndex < gameProgress.levels.Count)
                {
                    var level = gameProgress.levels[levelIndex];
                    string levelType = level.isSpecialLevel ? " (СПЕЦИАЛЬНЫЙ)" : "";
                    string unlockStatus = level.isUnlocked ? "разблокирован" : "заблокирован";

                    if (level.isSpecialLevel && !level.isUnlocked)
                    {
                        int requiredStars = GetStarsRequiredForSpecialLevel(levelIndex);
                        unlockStatus += $" (нужно {requiredStars} звезд, есть {gameProgress.totalStars})";
                    }

                    Debug.Log($"Уровень {levelIndex + 1}{levelType}: {level.stars} звезд, {unlockStatus}");
                }
            }
        }
    }

    [ContextMenu("Дать 14 звезд для тестирования первого спец уровня")]
    public void GiveStarsForFirstSpecialTest()
    {
        if (!isInitialized) return;

        // Даем звезды первым 5 уровням (кроме специального)
        int starsPerLevel = 14 / 5; // 2 звезды на уровень
        int remainingStars = 14 % 5; // 4 оставшиеся звезды

        for (int i = 0; i < levelsPerLine - 1; i++) // первые 5 уровней
        {
            int stars = starsPerLevel;
            if (remainingStars > 0)
            {
                stars++;
                remainingStars--;
            }
            SetLevelStars(i, stars);
        }

        Debug.Log("[StarManager] Дано 14 звезд для тестирования первого специального уровня");
    }
}