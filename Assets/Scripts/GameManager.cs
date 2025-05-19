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

    // Ссылки на UI элементы для навигации
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject mainMenuButton;

    // Ссылки на компоненты UI системы
    private UIGameplayRootBinder uiRootBinder;
    private GameplayEntryPoint gameplayEntryPoint;

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

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex-1;
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

        // Отписываемся от событий UIGameplayRootBinder
        UnsubscribeFromUIRootBinder();
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
        UnsubscribeFromUIRootBinder();

        // Находим необходимые объекты напрямую через FindObjectOfType
        FindReferences();

        // Подписываемся на события LevelManager
        SubscribeToLevelManager();

        // Подписываемся на события UIGameplayRootBinder
        SubscribeToUIRootBinder();

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

        // Находим UIGameplayRootBinder и GameplayEntryPoint
        uiRootBinder = FindObjectOfType<UIGameplayRootBinder>();
        gameplayEntryPoint = FindObjectOfType<GameplayEntryPoint>();

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

        // Находим кнопки навигации, если они не назначены через инспектор
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

        if (mainMenuButton == null)
        {
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button b in allButtons)
            {
                if (b.gameObject.name.Contains("MainMenuButton"))
                {
                    mainMenuButton = b.gameObject;
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

        Debug.Log($"GameManager: Найдены ссылки - WinPanel: {winPanel != null}, LosePanel: {losePanel != null}, NextLevelButton: {nextLevelButton != null}, MainMenuButton: {mainMenuButton != null}, Звезды: {starIcons?.Length ?? 0}");
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

    private void SubscribeToUIRootBinder()
    {
        if (uiRootBinder != null)
        {
            uiRootBinder.GoToMainMenuButtonClicked += LoadMainMenu;
            uiRootBinder.NextLevelButtonClicked += LoadNextLevel;
            uiRootBinder.RestartLevelButtonClicked += RestartLevel;
            Debug.Log("GameManager: Подписка на события UIGameplayRootBinder выполнена");
        }
        else
        {
            Debug.LogWarning("GameManager: UIGameplayRootBinder не найден, подписка на события невозможна");
        }

        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested += LoadMainMenu;
            gameplayEntryPoint.GoToNextLevelRequested += LoadNextLevel;
            gameplayEntryPoint.RestartLevelRequested += RestartLevel;
            Debug.Log("GameManager: Подписка на события GameplayEntryPoint выполнена");
        }
        else
        {
            Debug.LogWarning("GameManager: GameplayEntryPoint не найден, подписка на события невозможна");
        }
    }

    private void UnsubscribeFromUIRootBinder()
    {
        if (uiRootBinder != null)
        {
            uiRootBinder.GoToMainMenuButtonClicked -= LoadMainMenu;
            uiRootBinder.NextLevelButtonClicked -= LoadNextLevel;
            uiRootBinder.RestartLevelButtonClicked -= RestartLevel;
            Debug.Log("GameManager: Отписка от событий UIGameplayRootBinder выполнена");
        }

        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested -= LoadMainMenu;
            gameplayEntryPoint.GoToNextLevelRequested -= LoadNextLevel;
            gameplayEntryPoint.RestartLevelRequested -= RestartLevel;
            Debug.Log("GameManager: Отписка от событий GameplayEntryPoint выполнена");
        }
    }

    public void ResetGameState()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // Скрываем кнопки навигации при сбросе
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);

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

        // Проверяем, есть ли следующий уровень
        bool hasNextLevel = HasNextLevel();

        // Показываем кнопки навигации при победе
        if (nextLevelButton != null)
        {
            // Показываем кнопку следующего уровня только если есть следующий уровень
            nextLevelButton.SetActive(hasNextLevel);
            if (hasNextLevel)
                Debug.Log("GameManager: NextLevelButton активирован");
            else
                Debug.Log("GameManager: NextLevelButton скрыт (нет следующего уровня)");
        }
        else
            Debug.LogWarning("GameManager: NextLevelButton не найден");

        // Кнопка главного меню показывается всегда
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
            Debug.Log("GameManager: MainMenuButton активирован");
        }
        else
            Debug.LogWarning("GameManager: MainMenuButton не найден");

        // Активируем кнопки через UIRootBinder, если он доступен
        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
            {
                // Показываем кнопку следующего уровня только если есть следующий уровень
                uiRootBinder._nextLevelButton.SetActive(hasNextLevel);
                if (hasNextLevel)
                    Debug.Log("GameManager: UIRootBinder NextLevelButton активирован");
                else
                    Debug.Log("GameManager: UIRootBinder NextLevelButton скрыт (нет следующего уровня)");
            }

            if (uiRootBinder._mainMenuButton != null)
            {
                uiRootBinder._mainMenuButton.SetActive(true);
                Debug.Log("GameManager: UIRootBinder MainMenuButton активирован");
            }
        }

        // Отображаем заработанные звезды
        ShowStars(stars);

        // Если это последний уровень и нажата кнопка "Следующий уровень",
        // автоматически загружаем главное меню через некоторое время
        if (!hasNextLevel)
        {
            Debug.Log("Это последний уровень. При нажатии 'Следующий уровень' будет переход в главное меню.");
        }
    }

    private void HandleLevelLost(int stars, List<string> loseReasons)
    {
        //Debug.Log($"Уровень проигран! Причины: {string.Join(", ", loseReasons)}");

        // Показываем панель проигрыша
        if (losePanel != null)
            losePanel.SetActive(true);
        else
            Debug.LogWarning("GameManager: LosePanel не найден");

        // Показываем кнопку перезапуска
        //if (restartButton != null)
        //    restartButton.SetActive(true);
        //else
        //    Debug.LogWarning("GameManager: RestartButton не найден");

        // Прячем кнопки навигации при проигрыше
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);

        // Деактивируем кнопки через UIRootBinder, если он доступен
        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
                uiRootBinder._nextLevelButton.SetActive(false);

            if (uiRootBinder._mainMenuButton != null)
                uiRootBinder._mainMenuButton.SetActive(false);
        }

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

    // Проверяет, есть ли следующий уровень
    public bool HasNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        return nextLevelIndex < SceneManager.sceneCountInBuildSettings;
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
        GUILayout.Label($"NextLevelButton: {nextLevelButton != null}");
        GUILayout.Label($"MainMenuButton: {mainMenuButton != null}");
        GUILayout.Label($"UIRootBinder: {uiRootBinder != null}");

        GUILayout.EndArea();
    }
#endif
}