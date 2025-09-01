using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;
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

        // ВАЖНО: Сначала сбрасываем состояние игры (включая звезды)
        ResetGameState();

        // Только ПОСЛЕ сброса загружаем прогресс, но только если это не переход между уровнями
        // Проверяем, был ли уровень уже пройден ранее
        LoadCurrentLevelProgress();
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

        HideAllStars();
        if (starsText != null) starsText.text = "0";

        AmmunitionManager ammunitionManager = FindObjectOfType<AmmunitionManager>();
        if (ammunitionManager != null)
        {
            ammunitionManager.ResetAmmunition();
        }
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
        if (starIcons != null && starIcons.Length > 0)
        {
            foreach (var starIcon in starIcons)
            {
                if (starIcon != null)
                    starIcon.SetActive(false);
            }
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