using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Компонент для управления блокировкой/разблокировкой уровней
/// Работает с существующим StarManager без изменения его логики
/// </summary>
public class LevelLockManager : MonoBehaviour
{
    public static LevelLockManager Instance { get; private set; }

    [Header("Настройки блокировки")]
    [SerializeField] private bool debugMode = true;

    // Словарь для хранения дополнительных блокировок уровней
    // true = заблокирован администратором, false = разблокирован
    private Dictionary<int, bool> adminLockedLevels = new Dictionary<int, bool>();

    private StarManager starManager;

    public System.Action<int, bool> OnLevelLockChanged;

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
            return;
        }
    }

    private void Start()
    {
        // Получаем ссылку на StarManager
        starManager = StarManager.Instance;
        if (starManager == null)
        {
            Debug.LogError("[LevelLockManager] StarManager не найден!");
        }

        // Подписываемся на событие загрузки прогресса
        if (starManager != null)
        {
            starManager.OnProgressLoaded += LoadLockData;
        }

        LoadLockData();
    }

    private void OnDestroy()
    {
        if (starManager != null)
        {
            starManager.OnProgressLoaded -= LoadLockData;
        }
    }

    /// <summary>
    /// Блокирует уровень администратором (принудительная блокировка)
    /// </summary>
    public void LockLevel(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return;

        adminLockedLevels[levelIndex] = true;
        SaveLockData();

        OnLevelLockChanged?.Invoke(levelIndex, true);

        if (debugMode)
        {
            Debug.Log($"[LevelLockManager] Уровень {levelIndex + 1} заблокирован администратором");
        }
    }

    /// <summary>
    /// Разблокирует уровень администратором
    /// </summary>
    public void UnlockLevel(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return;

        adminLockedLevels[levelIndex] = false;
        SaveLockData();

        OnLevelLockChanged?.Invoke(levelIndex, false);

        if (debugMode)
        {
            Debug.Log($"[LevelLockManager] Уровень {levelIndex + 1} разблокирован администратором");
        }
    }

    /// <summary>
    /// Переключает состояние блокировки уровня
    /// </summary>
    public void ToggleLevelLock(int levelIndex)
    {
        if (IsLevelLockedByAdmin(levelIndex))
        {
            UnlockLevel(levelIndex);
        }
        else
        {
            LockLevel(levelIndex);
        }
    }

    /// <summary>
    /// Проверяет, заблокирован ли уровень администратором
    /// </summary>
    public bool IsLevelLockedByAdmin(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return false;

        return adminLockedLevels.ContainsKey(levelIndex) && adminLockedLevels[levelIndex];
    }

    /// <summary>
    /// Проверяет, доступен ли уровень для игры (учитывает все типы блокировок)
    /// </summary>
    public bool IsLevelAvailable(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex)) return false;

        // Если заблокирован администратором - недоступен
        if (IsLevelLockedByAdmin(levelIndex))
        {
            return false;
        }

        // Иначе используем стандартную логику StarManager
        return starManager != null ? starManager.IsLevelUnlocked(levelIndex) : false;
    }

    /// <summary>
    /// Блокирует все уровни
    /// </summary>
    public void LockAllLevels()
    {
        if (starManager == null) return;

        var gameProgress = starManager.GetGameProgress();
        if (gameProgress != null)
        {
            for (int i = 0; i < gameProgress.levels.Count; i++)
            {
                adminLockedLevels[i] = true;
            }

            SaveLockData();

            if (debugMode)
            {
                Debug.Log("[LevelLockManager] Все уровни заблокированы администратором");
            }

            // Уведомляем об изменениях
            for (int i = 0; i < gameProgress.levels.Count; i++)
            {
                OnLevelLockChanged?.Invoke(i, true);
            }
        }
    }

    /// <summary>
    /// Разблокирует все уровни (убирает админ-блокировки)
    /// </summary>
    public void UnlockAllLevels()
    {
        if (starManager == null) return;

        var gameProgress = starManager.GetGameProgress();
        if (gameProgress != null)
        {
            List<int> changedLevels = new List<int>();

            for (int i = 0; i < gameProgress.levels.Count; i++)
            {
                if (adminLockedLevels.ContainsKey(i) && adminLockedLevels[i])
                {
                    adminLockedLevels[i] = false;
                    changedLevels.Add(i);
                }
            }

            SaveLockData();

            if (debugMode)
            {
                Debug.Log("[LevelLockManager] Все админ-блокировки уровней сняты");
            }

            // Уведомляем об изменениях только измененных уровней
            foreach (int levelIndex in changedLevels)
            {
                OnLevelLockChanged?.Invoke(levelIndex, false);
            }
        }
    }

    /// <summary>
    /// Блокирует диапазон уровней
    /// </summary>
    public void LockLevelRange(int startLevel, int endLevel)
    {
        startLevel = Mathf.Max(0, startLevel);
        endLevel = Mathf.Min(endLevel, GetTotalLevelsCount() - 1);

        for (int i = startLevel; i <= endLevel; i++)
        {
            adminLockedLevels[i] = true;
        }

        SaveLockData();

        if (debugMode)
        {
            Debug.Log($"[LevelLockManager] Заблокированы уровни с {startLevel + 1} по {endLevel + 1}");
        }

        // Уведомляем об изменениях
        for (int i = startLevel; i <= endLevel; i++)
        {
            OnLevelLockChanged?.Invoke(i, true);
        }
    }

    /// <summary>
    /// Получает список всех заблокированных администратором уровней
    /// </summary>
    public List<int> GetAdminLockedLevels()
    {
        List<int> lockedLevels = new List<int>();

        foreach (var kvp in adminLockedLevels)
        {
            if (kvp.Value) // если заблокирован
            {
                lockedLevels.Add(kvp.Key);
            }
        }

        return lockedLevels;
    }

    /// <summary>
    /// Получает информацию о состоянии блокировки уровня
    /// </summary>
    public string GetLevelLockInfo(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
        {
            return "Некорректный индекс уровня";
        }

        bool adminLocked = IsLevelLockedByAdmin(levelIndex);
        bool starManagerUnlocked = starManager != null ? starManager.IsLevelUnlocked(levelIndex) : false;
        bool available = IsLevelAvailable(levelIndex);

        string info = $"Уровень {levelIndex + 1}:\n";
        info += $"• Админ-блокировка: {(adminLocked ? "Заблокирован" : "Не заблокирован")}\n";
        info += $"• StarManager: {(starManagerUnlocked ? "Разблокирован" : "Заблокирован")}\n";
        info += $"• Итоговый статус: {(available ? "Доступен" : "Недоступен")}";

        return info;
    }

    private bool IsValidLevelIndex(int levelIndex)
    {
        if (starManager == null) return false;

        var gameProgress = starManager.GetGameProgress();
        return gameProgress != null && levelIndex >= 0 && levelIndex < gameProgress.levels.Count;
    }

    private int GetTotalLevelsCount()
    {
        if (starManager == null) return 0;

        var gameProgress = starManager.GetGameProgress();
        return gameProgress != null ? gameProgress.levels.Count : 0;
    }

    private void LoadLockData()
    {
        // Простое сохранение в PlayerPrefs
        string lockDataKey = "AdminLockedLevels";

        if (PlayerPrefs.HasKey(lockDataKey))
        {
            try
            {
                string jsonData = PlayerPrefs.GetString(lockDataKey);
                var lockData = JsonUtility.FromJson<SerializableDictionary>(jsonData);

                adminLockedLevels.Clear();
                if (lockData?.items != null)
                {
                    foreach (var item in lockData.items)
                    {
                        adminLockedLevels[item.key] = item.value;
                    }
                }

                if (debugMode)
                {
                    Debug.Log($"[LevelLockManager] Загружены данные блокировок: {adminLockedLevels.Count} записей");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LevelLockManager] Ошибка загрузки данных блокировок: {e.Message}");
                adminLockedLevels.Clear();
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("[LevelLockManager] Данные блокировок не найдены, начинаем с чистого листа");
            }
        }
    }

    private void SaveLockData()
    {
        try
        {
            var lockData = new SerializableDictionary();
            lockData.items = new List<SerializableDictionary.DictItem>();

            foreach (var kvp in adminLockedLevels)
            {
                lockData.items.Add(new SerializableDictionary.DictItem
                {
                    key = kvp.Key,
                    value = kvp.Value
                });
            }

            string jsonData = JsonUtility.ToJson(lockData);
            PlayerPrefs.SetString("AdminLockedLevels", jsonData);
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log($"[LevelLockManager] Сохранены данные блокировок: {adminLockedLevels.Count} записей");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LevelLockManager] Ошибка сохранения данных блокировок: {e.Message}");
        }
    }

    // Класс для сериализации словаря
    [System.Serializable]
    private class SerializableDictionary
    {
        public List<DictItem> items = new List<DictItem>();

        [System.Serializable]
        public class DictItem
        {
            public int key;
            public bool value;
        }
    }

    #region Debug методы (ContextMenu)

    [ContextMenu("Показать состояние всех блокировок")]
    public void DebugShowAllLockStates()
    {
        if (starManager == null)
        {
            Debug.Log("[LevelLockManager] StarManager не найден");
            return;
        }

        var gameProgress = starManager.GetGameProgress();
        if (gameProgress == null)
        {
            Debug.Log("[LevelLockManager] GameProgress не найден");
            return;
        }

        Debug.Log("=== СОСТОЯНИЕ БЛОКИРОВОК УРОВНЕЙ ===");

        for (int i = 0; i < gameProgress.levels.Count; i++)
        {
            Debug.Log(GetLevelLockInfo(i));
            Debug.Log("---");
        }

        Debug.Log($"Всего админ-блокировок: {GetAdminLockedLevels().Count}");
    }

    [ContextMenu("Заблокировать первые 3 уровня")]
    public void DebugLockFirst3Levels()
    {
        LockLevelRange(0, 2);
    }

    [ContextMenu("Разблокировать все уровни")]
    public void DebugUnlockAllLevels()
    {
        UnlockAllLevels();
    }

    [ContextMenu("Заблокировать все уровни")]
    public void DebugLockAllLevels()
    {
        LockAllLevels();
    }

    [ContextMenu("Очистить все админ-блокировки")]
    public void DebugClearAllAdminLocks()
    {
        adminLockedLevels.Clear();
        SaveLockData();
        Debug.Log("[LevelLockManager] Все админ-блокировки очищены");
    }

    #endregion
}