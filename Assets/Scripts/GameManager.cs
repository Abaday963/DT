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

    // Ссылки на иконки звезд
    [SerializeField] private GameObject[] starIcons;

    [SerializeField] private LevelManager levelManager;

    [Header("Статистика игры")]
    private int totalStarsEarned = 0;
    private int currentLevelIndex = 0;
    private bool isPaused = false;

    public LevelManager LevelManager => levelManager;

    private void Awake()
    {
        // Синглтон паттерн
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

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void Start()
    {
        // После старта инициализируем сцену
        InitializeScene();
    }

    private void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Отписываемся от события загрузки сцены
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Отписываемся от событий LevelManager, если он существует
        UnsubscribeFromLevelManager();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;

        // Инициализируем новую сцену
        StartCoroutine(InitializeSceneAfterFrame());
    }

    private IEnumerator InitializeSceneAfterFrame()
    {
        // Ждем один кадр, чтобы все объекты инициализировались
        yield return null;

        // Инициализируем сцену
        InitializeScene();
    }

    private void InitializeScene()
    {
        // Отписываемся от старых событий, если были
        UnsubscribeFromLevelManager();

        // Находим необходимые объекты напрямую через FindObjectOfType
        FindReferences();

        // Подписываемся на события LevelManager
        SubscribeToLevelManager();

        // Сбрасываем состояние игры
        ResetGameState();
    }

    private void FindReferences()
    {
        // Находим LevelManager
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<LevelManager>();
        }

        // Находим UI элементы напрямую через GetComponentsInChildren или FindObjectsOfType
        // вместо использования тегов

        if (winPanel == null)
        {
            // Пример поиска панели по имени
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

        // Находим иконки звезд
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

        // Находим текст для звезд
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

        Debug.Log($"GameManager: Найдены ссылки - WinPanel: {winPanel != null}, LosePanel: {losePanel != null}, Звезды: {starIcons?.Length ?? 0}");
    }

    private void SubscribeToLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon += HandleLevelWon;
            levelManager.OnLevelLost += HandleLevelLost;
            Debug.Log("GameManager: Подписка на события LevelManager выполнена");
        }
        else
        {
            Debug.LogWarning("GameManager: LevelManager не найден, подписка на события невозможна");
        }
    }

    private void UnsubscribeFromLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon -= HandleLevelWon;
            levelManager.OnLevelLost -= HandleLevelLost;
            Debug.Log("GameManager: Отписка от событий LevelManager выполнена");
        }
    }

    public void ResetGameState()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // Скрываем все звезды при сбросе
        HideAllStars();

        if (starsText != null) starsText.text = "0";

        // Проверяем, существует ли AmmunitionManager
        AmmunitionManager ammunitionManager = FindObjectOfType<AmmunitionManager>();
        if (ammunitionManager != null)
        {
            ammunitionManager.ResetAmmunition();
        }
    }

    // Скрывает все иконки звезд
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

    // Показывает нужное количество звезд
    private void ShowStars(int starsCount)
    {
        if (starIcons == null || starIcons.Length == 0)
            return;

        // Сначала скрываем все звезды
        HideAllStars();

        // Показываем нужное количество звезд
        for (int i = 0; i < Mathf.Min(starsCount, starIcons.Length); i++)
        {
            if (starIcons[i] != null)
                starIcons[i].SetActive(true);
        }

        // Обновляем текст звезд, если есть
        if (starsText != null)
            starsText.text = starsCount.ToString();
    }

    private void HandleLevelWon(int stars, List<string> completedConditions)
    {
        totalStarsEarned += stars;

        Debug.Log($"Уровень пройден! Звезд заработано: {stars}");
        Debug.Log($"Выполненные условия: {string.Join(", ", completedConditions)}");

        // Показываем панель победы
        if (winPanel != null)
            winPanel.SetActive(true);
        else
            Debug.LogWarning("GameManager: WinPanel не найден");

        // Показываем кнопку перезапуска
        if (restartButton != null)
            restartButton.SetActive(true);
        else
            Debug.LogWarning("GameManager: RestartButton не найден");

        // Отображаем заработанные звезды
        ShowStars(stars);
    }

    private void HandleLevelLost(int stars, List<string> loseReasons)
    {
        Debug.Log($"Уровень проигран! Причины: {string.Join(", ", loseReasons)}");

        // Показываем панель проигрыша
        if (losePanel != null)
            losePanel.SetActive(true);
        else
            Debug.LogWarning("GameManager: LosePanel не найден");

        // Показываем кнопку перезапуска
        if (restartButton != null)
            restartButton.SetActive(true);
        else
            Debug.LogWarning("GameManager: RestartButton не найден");

        // Скрываем все звезды при проигрыше
        HideAllStars();

        StartCoroutine(RestartLevelAfterDelay(restartDelay));
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
        Debug.Log($"Игра {(isPaused ? "приостановлена" : "возобновлена")}");
    }

    public void RestartLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Debug.Log($"Перезапуск уровня {currentLevelIndex}");
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Загрузка следующего уровня {nextLevelIndex}");
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            Debug.Log("Загрузка главного меню (нет больше уровней)");
            SceneManager.LoadScene(0); // главное меню
        }
    }

    public void LoadMainMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Debug.Log("Загрузка главного меню");
        SceneManager.LoadScene(0); // главное меню
    }

    // Отладочное меню (только в редакторе)
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!useDebugMenu) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 300));
        GUILayout.Label("Отладочное меню");

        if (GUILayout.Button("Перезапустить уровень"))
        {
            RestartLevel();
        }

        if (GUILayout.Button("Перейти к следующему уровню"))
        {
            LoadNextLevel();
        }

        if (GUILayout.Button("Пауза/Продолжить"))
        {
            TogglePause();
        }

        GUILayout.Label($"Всего звезд: {totalStarsEarned}");
        GUILayout.Label($"Текущий уровень: {currentLevelIndex}");
        GUILayout.Label($"Найдено звезд: {starIcons?.Length ?? 0}");
        GUILayout.Label($"WinPanel: {winPanel != null}");
        GUILayout.Label($"LosePanel: {losePanel != null}");

        GUILayout.EndArea();
    }
#endif
}