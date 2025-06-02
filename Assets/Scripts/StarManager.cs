using UnityEngine;
using System.Collections.Generic;
using YG;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private int maxLevelsCount = 50; // Максимальное количество уровней в игре

    // События для уведомления UI
    public System.Action<int> OnStarsChanged;
    public System.Action<int> OnLevelUnlocked;
    public System.Action OnProgressLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Подписываемся на событие получения данных SDK
        YG2.onGetSDKData += OnDataLoaded;

        // Если данные уже загружены, инициализируем сразу
        if (YG2.saves != null)
        {
            OnDataLoaded();
        }
    }

    private void OnDestroy()
    {
        YG2.onGetSDKData -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        InitializeLevelStarsIfNeeded();

        if (enableDebugLogs)
        {
            Debug.Log($"[StarManager] Прогресс загружен: {GetTotalStars()} звёзд, {GetUnlockedLevelsCount()} уровней разблокировано");
        }

        OnProgressLoaded?.Invoke();
        OnStarsChanged?.Invoke(GetTotalStars());
    }

    private void InitializeLevelStarsIfNeeded()
    {
        if (YG2.saves.levelStars == null)
        {
            YG2.saves.levelStars = new List<int>();
        }

        // Расширяем список до нужного размера
        while (YG2.saves.levelStars.Count < maxLevelsCount)
        {
            YG2.saves.levelStars.Add(0);
        }
    }

    /// <summary>
    /// Устанавливает количество звёзд для уровня
    /// </summary>
    /// <param name="levelIndex">Индекс уровня (начиная с 0)</param>
    /// <param name="stars">Количество звёзд (0-3)</param>
    public void SetLevelStars(int levelIndex, int stars)
    {
        if (YG2.saves == null)
        {
            Debug.LogWarning("[StarManager] Сохранения ещё не загружены!");
            return;
        }

        InitializeLevelStarsIfNeeded();

        stars = Mathf.Clamp(stars, 0, 3);
        levelIndex = Mathf.Clamp(levelIndex, 0, maxLevelsCount - 1);

        // Если это новый результат или лучше предыдущего
        if (YG2.saves.levelStars[levelIndex] < stars)
        {
            // Вычитаем старые звёзды
            YG2.saves.totalStars -= YG2.saves.levelStars[levelIndex];

            // Устанавливаем новые звёзды
            YG2.saves.levelStars[levelIndex] = stars;
            YG2.saves.totalStars += stars;

            // Разблокируем следующий уровень если получили хотя бы 1 звезду
            if (stars > 0)
            {
                int nextLevel = levelIndex + 1;
                if (nextLevel > YG2.saves.unlockedLevels)
                {
                    YG2.saves.unlockedLevels = nextLevel;
                    OnLevelUnlocked?.Invoke(nextLevel);

                    if (enableDebugLogs)
                        Debug.Log($"[StarManager] Разблокирован уровень: {nextLevel}");
                }
            }

            // Сохраняем время последнего сохранения
            YG2.saves.lastSaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            OnStarsChanged?.Invoke(YG2.saves.totalStars);

            // Сохраняем прогресс в облако/локально
            SaveProgress();

            if (enableDebugLogs)
            {
                Debug.Log($"[StarManager] Уровень {levelIndex}: {stars} звёзд. Общий счёт: {YG2.saves.totalStars}");
            }
        }
    }

    /// <summary>
    /// Получает количество звёзд для уровня
    /// </summary>
    public int GetLevelStars(int levelIndex)
    {
        if (YG2.saves?.levelStars == null || levelIndex < 0 || levelIndex >= YG2.saves.levelStars.Count)
            return 0;

        return YG2.saves.levelStars[levelIndex];
    }

    /// <summary>
    /// Получает общее количество звёзд
    /// </summary>
    public int GetTotalStars()
    {
        return YG2.saves?.totalStars ?? 0;
    }

    /// <summary>
    /// Проверяет, разблокирован ли уровень
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (YG2.saves == null) return levelIndex == 0; // Первый уровень всегда доступен
        return levelIndex < YG2.saves.unlockedLevels;
    }

    /// <summary>
    /// Получает количество разблокированных уровней
    /// </summary>
    public int GetUnlockedLevelsCount()
    {
        return YG2.saves?.unlockedLevels ?? 1;
    }

    /// <summary>
    /// Принудительно разблокирует уровень
    /// </summary>
    public void UnlockLevel(int levelIndex)
    {
        if (YG2.saves == null) return;

        if (levelIndex > YG2.saves.unlockedLevels)
        {
            YG2.saves.unlockedLevels = levelIndex;
            OnLevelUnlocked?.Invoke(levelIndex);
            SaveProgress();

            if (enableDebugLogs)
                Debug.Log($"[StarManager] Принудительно разблокирован уровень: {levelIndex}");
        }
    }

    /// <summary>
    /// Сохраняет прогресс используя PluginYG
    /// </summary>
    public void SaveProgress()
    {
        YG2.SaveProgress();

        if (enableDebugLogs)
            Debug.Log($"[StarManager] Прогресс сохранён: {GetTotalStars()} звёзд");
    }

    /// <summary>
    /// Получает статистику по всем уровням
    /// </summary>
    public Dictionary<int, int> GetAllLevelStats()
    {
        var stats = new Dictionary<int, int>();

        if (YG2.saves?.levelStars != null)
        {
            for (int i = 0; i < YG2.saves.levelStars.Count; i++)
            {
                if (YG2.saves.levelStars[i] > 0)
                {
                    stats[i] = YG2.saves.levelStars[i];
                }
            }
        }

        return stats;
    }

    /// <summary>
    /// Сбрасывает весь прогресс (для тестирования)
    /// </summary>
    [ContextMenu("Сбросить прогресс")]
    public void ResetProgress()
    {
        if (YG2.saves != null)
        {
            YG2.saves.levelStars = new List<int>();
            YG2.saves.totalStars = 0;
            YG2.saves.unlockedLevels = 1;
            YG2.saves.lastSaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            InitializeLevelStarsIfNeeded();
            SaveProgress();

            OnStarsChanged?.Invoke(0);

            if (enableDebugLogs)
                Debug.Log("[StarManager] Прогресс сброшен");
        }
    }

    /// <summary>
    /// Разблокирует все уровни (для тестирования)
    /// </summary>
    [ContextMenu("Разблокировать все уровни")]
    public void UnlockAllLevels()
    {
        if (YG2.saves != null)
        {
            YG2.saves.unlockedLevels = maxLevelsCount;
            SaveProgress();

            if (enableDebugLogs)
                Debug.Log("[StarManager] Все уровни разблокированы");
        }
    }

    /// <summary>
    /// Добавляет звёзды к общему счёту (бонусы, награды и т.д.)
    /// </summary>
    public void AddBonusStars(int bonusStars)
    {
        if (YG2.saves != null && bonusStars > 0)
        {
            YG2.saves.totalStars += bonusStars;
            OnStarsChanged?.Invoke(YG2.saves.totalStars);
            SaveProgress();

            if (enableDebugLogs)
                Debug.Log($"[StarManager] Добавлено {bonusStars} бонусных звёзд");
        }
    }

    // Автосохранение при сворачивании/закрытии игры
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveProgress();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveProgress();
    }

#if UNITY_EDITOR
    [ContextMenu("Показать статистику")]
    private void ShowStats()
    {
        if (YG2.saves != null)
        {
            Debug.Log($"=== СТАТИСТИКА STARMANAGER ===");
            Debug.Log($"Общих звёзд: {YG2.saves.totalStars}");
            Debug.Log($"Разблокированных уровней: {YG2.saves.unlockedLevels}");
            Debug.Log($"Последнее сохранение: {YG2.saves.lastSaveTime}");

            var completedLevels = 0;
            for (int i = 0; i < YG2.saves.levelStars.Count; i++)
            {
                if (YG2.saves.levelStars[i] > 0)
                {
                    completedLevels++;
                    Debug.Log($"Уровень {i}: {YG2.saves.levelStars[i]} звёзд");
                }
            }
            Debug.Log($"Пройденных уровней: {completedLevels}");
        }
    }
#endif
}