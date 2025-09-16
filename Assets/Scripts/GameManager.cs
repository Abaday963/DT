using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool isRestarting = false;

    [Header("Настройки индексации")]
    [SerializeField] private int firstLevelBuildIndex = 2; // Индекс первого уровня в Build Settings

    [Header("Настройки")]
    [SerializeField] private bool useDebugMenu = true;
    [SerializeField] private float restartDelay = 2f;

    [Header("Ссылки на UI и игровые элементы")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Text starsText;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject menuButton; // ДОБАВЛЕНО: кнопка меню

    private UIGameplayRootBinder uiRootBinder;
    private GameplayEntryPoint gameplayEntryPoint;

    [SerializeField] private GameObject[] starIcons;
    [SerializeField] private LevelManager levelManager;

    [Header("Статистика игры")]
    private int totalStarsEarned = 0;
    private int currentLevelIndex = 0; // Логический индекс уровня (0, 1, 2...)
    private int currentSceneBuildIndex = 0; // Индекс сцены в Build Settings
    private bool isPaused = false;

    [Header("Аудио")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private float soundVolume = 1.0f;
    [SerializeField] private AudioSource audioSource;

    private StarManager starManager;

    public LevelManager LevelManager => levelManager;

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

        // ИСПРАВЛЕНИЕ: Правильная индексация
        currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        currentLevelIndex = GetLogicalLevelIndex(currentSceneBuildIndex);

        Debug.Log($"[GameManager] Сцена: {currentSceneBuildIndex}, Логический уровень: {currentLevelIndex + 1}");
    }

    /// <summary>
    /// Конвертирует индекс сцены в логический индекс уровня
    /// </summary>
    private int GetLogicalLevelIndex(int sceneBuildIndex)
    {
        if (sceneBuildIndex < firstLevelBuildIndex)
        {
            Debug.LogWarning($"[GameManager] Сцена {sceneBuildIndex} не является уровнем!");
            return -1;
        }
        return sceneBuildIndex - firstLevelBuildIndex;
    }

    /// <summary>
    /// Конвертирует логический индекс уровня в индекс сцены
    /// </summary>
    private int GetSceneBuildIndex(int logicalLevelIndex)
    {
        return logicalLevelIndex + firstLevelBuildIndex;
    }

    /// <summary>
    /// Проверяет, является ли текущая сцена уровнем
    /// </summary>
    private bool IsCurrentSceneLevel()
    {
        return currentSceneBuildIndex >= firstLevelBuildIndex;
    }

    private void Start()
    {
        InitializeScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromLevelManager();
        UnsubscribeFromUIRootBinder();
        ForceHideAllUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ИСПРАВЛЕНИЕ: Правильное обновление индексов
        currentSceneBuildIndex = scene.buildIndex;
        currentLevelIndex = GetLogicalLevelIndex(currentSceneBuildIndex);

        Debug.Log($"[GameManager] Загружена сцена {currentSceneBuildIndex}, логический уровень: {currentLevelIndex + 1}");

        if (isRestarting)
        {
            Debug.Log("[GameManager] Сцена загружена после перезапуска");
        }

        // Инициализируем только если это уровень
        if (IsCurrentSceneLevel())
        {
            StartCoroutine(InitializeSceneAfterFrame());
        }
    }

    private IEnumerator InitializeSceneAfterFrame()
    {
        yield return null;
        InitializeScene();
    }

    private void InitializeScene()
    {
        // Проверяем, что мы на уровне
        if (!IsCurrentSceneLevel())
        {
            Debug.Log($"[GameManager] Сцена {currentSceneBuildIndex} не является уровнем, пропускаем инициализацию");
            return;
        }

        UnsubscribeFromLevelManager();
        UnsubscribeFromUIRootBinder();
        FindReferences();

        // Получаем ссылку на StarManager
        starManager = StarManager.Instance;
        if (starManager == null)
        {
            Debug.LogWarning("[GameManager] StarManager не найден! Создаем временный.");
            GameObject starManagerGO = new GameObject("StarManager");
            starManager = starManagerGO.AddComponent<StarManager>();
        }

        SubscribeToLevelManager();
        SubscribeToUIRootBinder();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ResetGameState();

        if (!isRestarting)
        {
            LoadCurrentLevelProgress();
        }
        else
        {
            isRestarting = false;
            Debug.Log("[GameManager] Перезапуск завершен - прогресс не загружался");
        }
    }

    private void FindReferences()
    {
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        uiRootBinder = FindObjectOfType<UIGameplayRootBinder>();
        gameplayEntryPoint = FindObjectOfType<GameplayEntryPoint>();

        if (winPanel == null)
        {
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.name.Contains("WinPanel"))
                {
                    winPanel = t.gameObject;
                    break;
                }
            }
        }

        if (losePanel == null)
        {
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.name.Contains("LosePanel"))
                {
                    losePanel = t.gameObject;
                    break;
                }
            }
        }

        if (restartButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("RestartButton"))
                {
                    restartButton = b.gameObject;
                    break;
                }
            }
        }

        if (nextLevelButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("NextLevelButton"))
                {
                    nextLevelButton = b.gameObject;
                    break;
                }
            }
        }

        // ДОБАВЛЕНО: Поиск кнопки меню
        if (menuButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("MenuButton"))
                {
                    menuButton = b.gameObject;
                    break;
                }
            }
        }

        List<GameObject> foundStars = new List<GameObject>();
        Transform[] transforms = FindObjectsOfType<Transform>();
        foreach (Transform t in transforms)
        {
            if (t.gameObject.name.Contains("Star") && t.gameObject.name.Contains("Icon"))
            {
                foundStars.Add(t.gameObject);
            }
        }

        if (foundStars.Count > 0)
        {
            starIcons = foundStars.ToArray();
        }

        if (starsText == null)
        {
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text t in allTexts)
            {
                if (t.gameObject.name.Contains("StarsText"))
                {
                    starsText = t;
                    break;
                }
            }
        }
    }

    private void SubscribeToLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon += HandleLevelWon;
            levelManager.OnLevelLost += HandleLevelLost;
        }
    }

    private void UnsubscribeFromLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon -= HandleLevelWon;
            levelManager.OnLevelLost -= HandleLevelLost;
        }
    }

    private void SubscribeToUIRootBinder()
    {
        if (uiRootBinder != null)
        {
            uiRootBinder.GoToMainMenuButtonClicked += () =>
            {
                if (gameplayEntryPoint != null)
                    gameplayEntryPoint.RequestGoToMainMenu();
                else
                    LoadMainMenu();
            };

            uiRootBinder.NextLevelButtonClicked += () =>
            {
                if (gameplayEntryPoint != null)
                    gameplayEntryPoint.RequestNextLevel();
                else
                    LoadNextLevel();
            };

            uiRootBinder.RestartLevelButtonClicked += () =>
            {
                if (gameplayEntryPoint != null)
                    gameplayEntryPoint.RequestRestartLevel();
                else
                    RestartLevel();
            };
        }

        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested += () => LoadMainMenu();
            gameplayEntryPoint.GoToNextLevelRequested += () => LoadNextLevel();
            gameplayEntryPoint.RestartLevelRequested += () => RestartLevel();
        }
    }

    private void UnsubscribeFromUIRootBinder()
    {
        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested -= LoadMainMenu;
            gameplayEntryPoint.GoToNextLevelRequested -= LoadNextLevel;
            gameplayEntryPoint.RestartLevelRequested -= RestartLevel;
        }
    }

    public void ResetGameState()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false); // ДОБАВЛЕНО: скрытие кнопки меню

        HideAllStars();
        if (starsText != null) starsText.text = "0";

        StartCoroutine(ForceUpdateUI());

        AmmunitionManager ammunitionManager = FindObjectOfType<AmmunitionManager>();
        if (ammunitionManager != null)
        {
            ammunitionManager.ResetAmmunition();
        }

        Debug.Log("[GameManager] ResetGameState выполнен");
    }

    private void LoadCurrentLevelProgress()
    {
        if (starManager != null && currentLevelIndex >= 0)
        {
            int savedStars = starManager.GetLevelStars(currentLevelIndex);
            if (savedStars > 0)
            {
                ShowStars(savedStars);
                Debug.Log($"[GameManager] Загружен прогресс уровня {currentLevelIndex + 1}: {savedStars} звезд");
            }
        }
    }

    private void HideAllStars()
    {
        Debug.Log("[GameManager] Скрываем все звезды...");

        if (starIcons != null && starIcons.Length > 0)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(false);
                    Debug.Log($"[GameManager] Звезда {i} ({starIcons[i].name}) скрыта");
                }
            }
        }

        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.name.Contains("Star") && obj.name.Contains("Icon") && obj.scene.isLoaded)
            {
                obj.SetActive(false);
                Debug.Log($"[GameManager] Дополнительно скрыта звезда: {obj.name}");
            }
        }
    }

    private void ForceHideAllUI()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false); // ДОБАВЛЕНО: скрытие кнопки меню

        HideAllStars();

        if (starsText != null) starsText.text = "0";

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            canvas.enabled = false;
            canvas.enabled = true;
        }
    }

    private void ShowStars(int starsCount)
    {
        if (starIcons == null || starIcons.Length == 0)
            return;

        HideAllStars();

        for (int i = 0; i < Mathf.Min(starsCount, starIcons.Length); i++)
        {
            if (starIcons[i] != null)
                starIcons[i].SetActive(true);
        }

        if (starsText != null)
            starsText.text = starsCount.ToString();
    }

    private void HandleLevelWon(int stars, List<string> completedConditions)
    {
        if (winSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(winSound, soundVolume);
        }

        // ИСПРАВЛЕНИЕ: Используем правильный логический индекс
        if (starManager != null)
        {
            int previousStars = starManager.GetLevelStars(currentLevelIndex);
            starManager.SetLevelStars(currentLevelIndex, stars);

            if (stars > previousStars)
            {
                Debug.Log($"[GameManager] Новый рекорд! Уровень {currentLevelIndex + 1}: {stars} звезд (было {previousStars})");
            }
            else
            {
                Debug.Log($"[GameManager] Уровень {currentLevelIndex + 1} пройден на {stars} звезд (рекорд: {previousStars})");
            }
        }

        totalStarsEarned += stars;

        if (winPanel != null) winPanel.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);

        bool hasNextLevel = HasNextLevel();
        bool nextLevelUnlocked = starManager != null ? starManager.IsLevelUnlocked(currentLevelIndex + 1) : true;

        // ДОБАВЛЕНО: Логика показа кнопок
        bool showNextButton = hasNextLevel && nextLevelUnlocked;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(showNextButton);

        // Показываем кнопку меню, если кнопка следующего уровня не показывается
        if (menuButton != null)
            menuButton.SetActive(!showNextButton);

        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
                uiRootBinder._nextLevelButton.SetActive(showNextButton);

            if (uiRootBinder._mainMenuButton != null)
                uiRootBinder._mainMenuButton.SetActive(!showNextButton); // ИЗМЕНЕНО: показываем только если нет кнопки следующего уровня
        }

        ShowStars(stars);
    }

    private void HandleLevelLost(int stars, List<string> loseReasons)
    {
        if (loseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(loseSound, soundVolume);
        }

        if (losePanel != null)
            losePanel.SetActive(true);

        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false); // ДОБАВЛЕНО: скрытие при поражении

        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
                uiRootBinder._nextLevelButton.SetActive(false);

            if (uiRootBinder._mainMenuButton != null)
                uiRootBinder._mainMenuButton.SetActive(false);
        }

        HideAllStars();
        StartCoroutine(RestartLevelAfterDelay(restartDelay));

        Debug.Log($"[GameManager] Уровень {currentLevelIndex + 1} провален. Причины: {string.Join(", ", loseReasons)}");
    }

    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartLevel();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;

        isRestarting = true;
        ResetGameState();

        if (levelManager != null)
        {
            levelManager.StopAllLevelProcesses();
        }

        Debug.Log($"[GameManager] Перезапуск уровня {currentLevelIndex + 1} (сцена {currentSceneBuildIndex})");

        // ИСПРАВЛЕНИЕ: Перезапускаем текущую сцену
        SceneManager.LoadScene(currentSceneBuildIndex);
    }

    public void LoadNextLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;

        int nextLogicalLevel = currentLevelIndex + 1;
        int nextSceneBuildIndex = GetSceneBuildIndex(nextLogicalLevel);

        // Проверяем существование сцены
        if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[GameManager] Переход на уровень {nextLogicalLevel + 1} (сцена {nextSceneBuildIndex})");
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.Log("[GameManager] Следующего уровня нет, возвращаемся в главное меню");
            SceneManager.LoadScene(1); // главное меню
        }

        if (starManager != null && !starManager.IsLevelAvailable(nextLogicalLevel))
        {
            string reason = "";
            if (LevelLockManager.Instance != null && LevelLockManager.Instance.IsLevelLockedByAdmin(nextLogicalLevel))
            {
                reason = " (заблокирован администратором)";
            }
            else
            {
                reason = " (еще не разблокирован)";
            }

            Debug.LogWarning($"[GameManager] Уровень {nextLogicalLevel + 1} недоступен{reason}");

            // Возвращаемся в главное меню вместо перехода на недоступный уровень
            LoadMainMenu();
            return;
        }
    }

    public bool HasNextLevel()
    {
        int nextLogicalLevel = currentLevelIndex + 1;
        int nextSceneBuildIndex = GetSceneBuildIndex(nextLogicalLevel);
        return nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings;
    }

    public bool IsNextLevelUnlocked()
    {
        if (starManager == null) return true;

        int nextLogicalLevel = currentLevelIndex + 1;

        // Используем новый метод IsLevelAvailable вместо IsLevelUnlocked
        return starManager.IsLevelAvailable(nextLogicalLevel);
    }

    public void LoadMainMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        SceneManager.LoadScene(1); // главное меню
    }

    private IEnumerator ForceUpdateUI()
    {
        yield return null;
        HideAllStars();
        Canvas.ForceUpdateCanvases();
        Debug.Log("[GameManager] UI принудительно обновлен");
    }

    // Публичные методы для использования в других скриптах
    public int GetCurrentLevelStars()
    {
        return starManager != null && currentLevelIndex >= 0 ? starManager.GetLevelStars(currentLevelIndex) : 0;
    }

    public int GetTotalStars()
    {
        return starManager != null ? starManager.GetTotalStars() : 0;
    }

    public bool IsCurrentLevelCompleted()
    {
        return GetCurrentLevelStars() > 0;
    }

    public int GetCurrentLogicalLevelIndex() => currentLevelIndex;
    public int GetCurrentSceneBuildIndex() => currentSceneBuildIndex;

    // Методы для дебага и проверки прогресса
    [ContextMenu("Показать текущие индексы")]
    public void DebugShowCurrentIndexes()
    {
        Debug.Log($"[GameManager] Текущая сцена: {currentSceneBuildIndex}");
        Debug.Log($"[GameManager] Логический уровень: {currentLevelIndex + 1}");
        Debug.Log($"[GameManager] Является ли сцена уровнем: {IsCurrentSceneLevel()}");
        Debug.Log($"[GameManager] Первый уровень в Build Settings: {firstLevelBuildIndex}");
    }

    [ContextMenu("Показать прогресс текущего уровня")]
    public void ShowCurrentLevelProgress()
    {
        if (starManager != null && currentLevelIndex >= 0)
        {
            int stars = starManager.GetLevelStars(currentLevelIndex);
            bool unlocked = starManager.IsLevelUnlocked(currentLevelIndex);
            Debug.Log($"Уровень {currentLevelIndex + 1}: {stars} звезд, " + (unlocked ? "разблокирован" : "заблокирован"));
        }
    }

    [ContextMenu("Показать общий прогресс")]
    public void ShowTotalProgress()
    {
        if (starManager != null)
        {
            int totalStars = starManager.GetTotalStars();
            Debug.Log($"Общий прогресс: {totalStars} звезд");

            GameProgress progress = starManager.GetGameProgress();
            for (int i = 0; i < progress.levels.Count; i++)
            {
                LevelProgress level = progress.levels[i];
                string status = level.isUnlocked ? "🔓" : "🔒";
                Debug.Log($"Уровень {i + 1}: {level.stars} звезд {status}");
            }
        }
    }
}