using UnityEngine;
using System.Collections.Generic;
using YG;

[System.Serializable]
public class LevelProgress
{
    public int levelIndex;
    public int stars;
    public bool isUnlocked;

    public LevelProgress(int index, int starsEarned, bool unlocked)
    {
        levelIndex = index;
        stars = starsEarned;
        isUnlocked = unlocked;
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
    [SerializeField] private bool debugMode = true;

    private GameProgress gameProgress;
    private const string SAVE_KEY = "GameProgress";
    private bool isInitialized = false;

    public System.Action<int, int> OnLevelStarsUpdated;
    public System.Action<int> OnTotalStarsUpdated;
    public System.Action<int> OnStarsChanged;
    public System.Action<int> OnLevelUnlocked;
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
            bool isUnlocked = (i == 0);
            gameProgress.levels.Add(new LevelProgress(i, 0, isUnlocked));
        }
        isInitialized = true;

        if (debugMode) Debug.Log("[StarManager] Прогресс инициализирован");
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
                        gameProgress.levels.Add(new LevelProgress(i, 0, false));
                }

                RecalculateTotalStars();
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

        // Исправленная проверка: levelIndex должен быть от 0 до totalLevelsCount-1
        if (levelIndex < 0 || levelIndex >= totalLevelsCount)
        {
            Debug.LogError($"[StarManager] Некорректный индекс: {levelIndex}. Допустимый диапазон: 0-{totalLevelsCount - 1}");
            return;
        }

        // Дополнительная проверка на случай, если список levels меньше totalLevelsCount
        if (levelIndex >= gameProgress.levels.Count)
        {
            Debug.LogError($"[StarManager] Индекс {levelIndex} превышает размер списка уровней ({gameProgress.levels.Count})");
            return;
        }

        stars = Mathf.Clamp(stars, 0, 3);
        LevelProgress level = gameProgress.levels[levelIndex];
        int oldStars = level.stars;

        if (debugMode)
            Debug.Log($"[StarManager] Установка звезд для уровня {levelIndex + 1}: {oldStars} -> {stars}");

        // Обновляем звезды (разрешаем и уменьшение звезд для тестирования)
        if (stars != level.stars)
        {
            level.stars = stars;
            level.isUnlocked = true;

            // Разблокируем следующий уровень, если текущий пройден
            if (stars > 0 && levelIndex + 1 < gameProgress.levels.Count)
            {
                if (!gameProgress.levels[levelIndex + 1].isUnlocked)
                {
                    gameProgress.levels[levelIndex + 1].isUnlocked = true;
                    OnLevelUnlocked?.Invoke(levelIndex + 1);

                    if (debugMode)
                        Debug.Log($"[StarManager] Разблокирован уровень {levelIndex + 2}");
                }
            }

            RecalculateTotalStars();
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

    [ContextMenu("Разблокировать все уровни")]
    public void UnlockAllLevels()
    {
        if (!isInitialized) return;

        foreach (var level in gameProgress.levels)
            level.isUnlocked = true;

        SaveProgress();
        ForceUpdateAllDisplays();

        if (debugMode) Debug.Log("[StarManager] Все уровни разблокированы");
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
        for (int i = 0; i < gameProgress.levels.Count; i++)
        {
            var level = gameProgress.levels[i];
            Debug.Log($"Уровень {i + 1}: {level.stars} звезд, " +
                     (level.isUnlocked ? "разблокирован" : "заблокирован"));
        }
    }
}