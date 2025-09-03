using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool isRestarting = false;

    [Header("Настройки")]
    [SerializeField] private bool useDebugMenu = true;
    [SerializeField] private float restartDelay = 2f;

    [Header("Ссылки на UI и игровые элементы")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Text starsText;

    [SerializeField] private GameObject nextLevelButton;

    private UIGameplayRootBinder uiRootBinder;
    private GameplayEntryPoint gameplayEntryPoint;

    [SerializeField] private GameObject[] starIcons;
    [SerializeField] private LevelManager levelManager;

    [Header("Статистика игры")]
    private int totalStarsEarned = 0;
    private int currentLevelIndex = 0;
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

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex - 1;
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

        // ДОБАВИТЬ: Скрываем UI при деактивации
        ForceHideAllUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;

        // ДОБАВИТЬ: Дополнительная проверка при загрузке сцены
        if (isRestarting)
        {
            Debug.Log("[GameManager] Сцена загружена после перезапуска");
        }

        StartCoroutine(InitializeSceneAfterFrame());
    }

    private IEnumerator InitializeSceneAfterFrame()
    {
        yield return null;
        InitializeScene();
    }

    private void InitializeScene()
    {
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

        // ВАЖНО: Сначала сбрасываем состояние игры
        ResetGameState();

        // ИСПРАВЛЕНИЕ: Загружаем прогресс только если это НЕ перезапуск
        if (!isRestarting)
        {
            LoadCurrentLevelProgress();
        }
        else
        {
            // Сбрасываем флаг перезапуска после инициализации
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

        // Принудительное скрытие звезд
        HideAllStars();
        if (starsText != null) starsText.text = "0";

        // ДОБАВИТЬ: Принудительное обновление Canvas
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
        if (starManager != null)
        {
            int savedStars = starManager.GetLevelStars(currentLevelIndex);
            if (savedStars > 0)
            {
                // Показываем сохраненный прогресс
                ShowStars(savedStars);
                Debug.Log($"[GameManager] Загружен прогресс уровня {currentLevelIndex + 1}: {savedStars} звезд");
            }
        }
    }

    private void HideAllStars()
    {
        Debug.Log("[GameManager] Скрываем все звезды...");

        // Скрываем звезды из массива starIcons
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

        // ДОБАВИТЬ: Дополнительный поиск всех объектов со словом Star
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
        // Скрываем все UI элементы
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        // Скрываем звезды
        HideAllStars();

        // Сбрасываем текст
        if (starsText != null) starsText.text = "0";

        // Принудительно обновляем все Canvas
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

        // Сохраняем прогресс через StarManager
        if (starManager != null)
        {
            int previousStars = starManager.GetLevelStars(currentLevelIndex);
            starManager.SetLevelStars(currentLevelIndex, stars);

            // Логируем результат
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

        if (nextLevelButton != null)
            nextLevelButton.SetActive(hasNextLevel && nextLevelUnlocked);

        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
                uiRootBinder._nextLevelButton.SetActive(hasNextLevel && nextLevelUnlocked);

            if (uiRootBinder._mainMenuButton != null)
                uiRootBinder._mainMenuButton.SetActive(true);
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

        // ДОБАВИТЬ: Устанавливаем флаг перезапуска
        isRestarting = true;

        // Принудительно скрываем все UI и звезды
        ResetGameState();

        // ДОБАВИТЬ: Останавливаем все процессы в LevelManager
        if (levelManager != null)
        {
            levelManager.StopAllLevelProcesses();
        }

        Debug.Log("[GameManager] Перезапуск уровня - UI сброшен");

        SceneManager.LoadScene(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        int nextLevelIndex = currentLevelIndex + 1;

        // Проверяем, разблокирован ли следующий уровень
        if (starManager != null && !starManager.IsLevelUnlocked(nextLevelIndex))
        {
            Debug.LogWarning($"[GameManager] Уровень {nextLevelIndex + 1} еще не разблокирован!");
            return;
        }

        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            SceneManager.LoadScene(0); // главное меню
        }
    }

    public bool HasNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        return nextLevelIndex < SceneManager.sceneCountInBuildSettings;
    }

    public bool IsNextLevelUnlocked()
    {
        if (starManager == null) return true;

        int nextLevelIndex = currentLevelIndex + 1;
        return starManager.IsLevelUnlocked(nextLevelIndex);
    }

    public void LoadMainMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        SceneManager.LoadScene(1); // главное меню
    }

    // Методы для дебага и проверки прогресса
    [ContextMenu("Показать прогресс текущего уровня")]
    public void ShowCurrentLevelProgress()
    {
        if (starManager != null)
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

            // Выводим прогресс по всем уровням
            GameProgress progress = starManager.GetGameProgress();
            for (int i = 0; i < progress.levels.Count; i++)
            {
                LevelProgress level = progress.levels[i];
                string status = level.isUnlocked ? "🔓" : "🔒";
                Debug.Log($"Уровень {i + 1}: {level.stars} звезд {status}");
            }
        }
    }
    private IEnumerator ForceUpdateUI()
    {
        yield return null; // Ждем один кадр

        // Еще раз скрываем звезды
        HideAllStars();

        // Принудительно обновляем Canvas
        Canvas.ForceUpdateCanvases();

        Debug.Log("[GameManager] UI принудительно обновлен");
    }

    // Публичные методы для использования в других скриптах
    public int GetCurrentLevelStars()
    {
        return starManager != null ? starManager.GetLevelStars(currentLevelIndex) : 0;
    }

    public int GetTotalStars()
    {
        return starManager != null ? starManager.GetTotalStars() : 0;
    }

    public bool IsCurrentLevelCompleted()
    {
        return GetCurrentLevelStars() > 0;
    }
}