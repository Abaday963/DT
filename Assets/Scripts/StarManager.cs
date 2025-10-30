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
    private bool hasLoadedOnce = false;

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

            if (debugMode) Debug.Log("[StarManager] Instance создан");
        }
        else
        {
            if (debugMode) Debug.Log("[StarManager] Уничтожаем дубликат");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (debugMode) Debug.Log($"[StarManager] Start. SDK enabled: {YG2.isSDKEnabled}, Saves null: {YG2.saves == null}");

        // Если SDK уже готов (например, после перезагрузки сцены)
        if (YG2.isSDKEnabled && YG2.saves != null && !hasLoadedOnce)
        {
            if (debugMode) Debug.Log("[StarManager] SDK уже готов в Start, загружаем");
            OnSDKReady();
        }
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении
        YG2.onGetSDKData -= OnSDKReady;

        if (debugMode) Debug.Log("[StarManager] Отписались от onGetSDKData");
    }
    private void OnEnable()
    {
        // Подписываемся при включении объекта
        YG2.onGetSDKData += OnSDKReady;

        if (debugMode) Debug.Log("[StarManager] Подписались на onGetSDKData");
    }
    private void OnSDKReady()
    {
        if (hasLoadedOnce)
        {
            if (debugMode) Debug.Log("[StarManager] SDK ready вызван повторно, пропускаем");
            return;
        }

        if (debugMode) Debug.Log("[StarManager] ========== SDK READY! ==========");

        hasLoadedOnce = true;
        LoadProgress();
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
        if (debugMode) Debug.Log("[StarManager] >>> LoadProgress вызван");

        // КРИТИЧНО: Проверяем готовность SDK
        if (YG2.saves == null)
        {
            if (debugMode) Debug.LogWarning("[StarManager] YG2.saves == null! SDK не готов");

            // В билде пробуем подождать и повторить
#if !UNITY_EDITOR
        if (debugMode) Debug.Log("[StarManager] Пробуем повторить через 0.5 сек...");
        Invoke(nameof(LoadProgress), 0.5f);
#endif

            return;
        }

        if (debugMode) Debug.Log($"[StarManager] SDK готов! isSDKEnabled: {YG2.isSDKEnabled}");

        string savedData = YG2.saves.GameProgress;

        if (debugMode)
        {
            if (string.IsNullOrEmpty(savedData))
                Debug.Log("[StarManager] Сохраненных данных нет (первый запуск)");
            else
                Debug.Log($"[StarManager] Найдены данные: {savedData.Length} символов");
        }

        if (!string.IsNullOrEmpty(savedData))
        {
            try
            {
                gameProgress = JsonUtility.FromJson<GameProgress>(savedData);

                if (debugMode)
                    Debug.Log($"[StarManager] ✓ Десериализация OK. Уровней: {gameProgress.levels.Count}, Звезд: {gameProgress.totalStars}");

                // Добавляем новые уровни если надо
                while (gameProgress.levels.Count < totalLevelsCount)
                {
                    int i = gameProgress.levels.Count;
                    int lineIndex = i / levelsPerLine;
                    int positionInLine = i % levelsPerLine;
                    bool isSpecialLevel = (positionInLine == levelsPerLine - 1);
                    bool isUnlocked = !isSpecialLevel;

                    gameProgress.levels.Add(new LevelProgress(i, 0, isUnlocked, lineIndex, isSpecialLevel));
                }

                RecalculateTotalStars();
                CheckSpecialLevelsUnlock();
                isInitialized = true;

                if (debugMode)
                    Debug.Log($"[StarManager] ✅ Прогресс загружен! Звезд: {gameProgress.totalStars}");

                OnProgressLoaded?.Invoke();

                // Обновляем UI через небольшую задержку
                StartCoroutine(DelayedUIUpdate());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StarManager] ❌ Ошибка загрузки: {e.Message}");
                InitializeProgress();
                SaveProgress();
            }
        }
        else
        {
            if (debugMode) Debug.Log("[StarManager] Первый запуск - создаем новый прогресс");

            InitializeProgress();
            SaveProgress();
        }
    }
    private System.Collections.IEnumerator DelayedUIUpdate()
    {
        yield return new WaitForSeconds(0.2f);
        ForceUpdateAllDisplays();

        if (debugMode) Debug.Log("[StarManager] UI обновлен");
    }
    public void SaveProgress()
    {
        if (!isInitialized)
        {
            if (debugMode) Debug.LogWarning("[StarManager] Пропускаем сохранение - не инициализирован");
            return;
        }

        if (YG2.saves == null)
        {
            Debug.LogWarning("[StarManager] Пропускаем сохранение - YG2.saves == null");
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(gameProgress);

            if (debugMode)
            {
                Debug.Log($"[StarManager] Сохраняем: {gameProgress.totalStars} звезд");
                Debug.Log($"[StarManager] JSON длина: {jsonData.Length} символов");
            }

            YG2.saves.GameProgress = jsonData;
            YG2.saves.lastSaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // КРИТИЧНО: Вызываем сохранение SDK
            YG2.SaveProgress();

            if (debugMode)
                Debug.Log($"[StarManager] ✅ YG2.SaveProgress() вызван! Время: {YG2.saves.lastSaveTime}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StarManager] ❌ Ошибка сохранения: {e.Message}");
        }
    }
    private bool IsSDKReady()
    {
        bool ready = YG2.isSDKEnabled && YG2.saves != null;

        if (!ready && debugMode)
            Debug.LogWarning("[StarManager] SDK не готов. isSDKEnabled: " + YG2.isSDKEnabled);

        return ready;
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

        // ⭐ КРИТИЧНО: Сохраняем только если новый результат ЛУЧШЕ
        if (stars <= oldStars)
        {
            if (debugMode)
            {
                string levelType = level.isSpecialLevel ? "специальный" : "обычный";
                Debug.Log($"[StarManager] {levelType} уровень {levelIndex + 1}: результат {stars} не лучше текущего {oldStars}, пропускаем");
            }
            return; // Не перезаписываем лучший результат!
        }

        if (debugMode)
        {
            string levelType = level.isSpecialLevel ? "специальный" : "обычный";
            Debug.Log($"[StarManager] Улучшен результат {levelType} уровня {levelIndex + 1} (линия {level.lineIndex + 1}): {oldStars} -> {stars}");
        }

        level.stars = stars;
        level.isUnlocked = true;

        RecalculateTotalStars();
        CheckSpecialLevelsUnlock();
        SaveProgress();

        // Вызываем события ПОСЛЕ сохранения
        OnLevelStarsUpdated?.Invoke(levelIndex, stars);
        OnTotalStarsUpdated?.Invoke(gameProgress.totalStars);
        OnStarsChanged?.Invoke(gameProgress.totalStars);

        if (debugMode)
            Debug.Log($"[StarManager] События вызваны для уровня {levelIndex + 1}: {stars} звезд");
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
    [ContextMenu("🔍 Проверить статус SDK")]
    public void CheckSDKStatus()
    {
        Debug.Log("=== СТАТУС SDK ===");
        Debug.Log($"SDK включен: {YG2.isSDKEnabled}");
        Debug.Log($"Saves не null: {YG2.saves != null}");
        Debug.Log($"StarManager инициализирован: {isInitialized}");

        if (YG2.saves != null)
        {
            Debug.Log($"Длина сохраненных данных: {YG2.saves.GameProgress.Length} символов");
            Debug.Log($"Последнее сохранение: {YG2.saves.lastSaveTime}");
        }
    }

    [ContextMenu("💾 Принудительное сохранение")]
    public void ForceSave()
    {
        Debug.Log("[TEST] Принудительное сохранение...");
        SaveProgress();
        CheckSDKStatus();
    }

    [ContextMenu("📥 Принудительная загрузка")]
    public void ForceLoad()
    {
        Debug.Log("[TEST] Принудительная загрузка...");
        LoadProgress();
        DebugShowProgress();
    }

    [ContextMenu("🧪 Дать 5 звезд первому уровню и сохранить")]
    public void TestSaveStars()
    {
        Debug.Log("[TEST] Даем 3 звезды первому уровню...");
        SetLevelStars(0, 3);
        Debug.Log("[TEST] Даем 2 звезды второму уровню...");
        SetLevelStars(1, 2);

        DebugShowProgress();
        Debug.Log("[TEST] Теперь выйдите и перезайдите в игру для проверки!");
    }
}