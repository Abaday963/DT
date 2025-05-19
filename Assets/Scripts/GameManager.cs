using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("���������")]
    [SerializeField] private bool useDebugMenu = true;
    [SerializeField] private float restartDelay = 2f;

    [Header("������ �� UI � ������� ��������")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Text starsText;

    // ������ �� UI �������� ��� ���������
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject mainMenuButton;

    // ������ �� ���������� UI �������
    private UIGameplayRootBinder uiRootBinder;
    private GameplayEntryPoint gameplayEntryPoint;

    // ������ �� ������ �����
    [SerializeField] private GameObject[] starIcons;

    [SerializeField] private LevelManager levelManager;

    [Header("���������� ����")]
    private int totalStarsEarned = 0;
    private int currentLevelIndex = 0;
    private bool isPaused = false;

    public LevelManager LevelManager => levelManager;

    private void Awake()
    {
        // �������� �������
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
        // ����� ������ �������������� �����
        InitializeScene();
    }

    private void OnEnable()
    {
        // ������������� �� ������� �������� �����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ������������ �� ������� �������� �����
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // ������������ �� ������� LevelManager, ���� �� ����������
        UnsubscribeFromLevelManager();

        // ������������ �� ������� UIGameplayRootBinder
        UnsubscribeFromUIRootBinder();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLevelIndex = scene.buildIndex;

        // �������������� ����� �����
        StartCoroutine(InitializeSceneAfterFrame());
    }

    private IEnumerator InitializeSceneAfterFrame()
    {
        // ���� ���� ����, ����� ��� ������� ������������������
        yield return null;

        // �������������� �����
        InitializeScene();
    }

    private void InitializeScene()
    {
        // ������������ �� ������ �������, ���� ����
        UnsubscribeFromLevelManager();
        UnsubscribeFromUIRootBinder();

        // ������� ����������� ������� �������� ����� FindObjectOfType
        FindReferences();

        // ������������� �� ������� LevelManager
        SubscribeToLevelManager();

        // ������������� �� ������� UIGameplayRootBinder
        SubscribeToUIRootBinder();

        // ���������� ��������� ����
        ResetGameState();
    }

    private void FindReferences()
    {
        // ������� LevelManager
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<LevelManager>();
        }

        // ������� UIGameplayRootBinder � GameplayEntryPoint
        uiRootBinder = FindObjectOfType<UIGameplayRootBinder>();
        gameplayEntryPoint = FindObjectOfType<GameplayEntryPoint>();

        // ������� UI �������� �������� ����� GetComponentsInChildren ��� FindObjectsOfType
        // ������ ������������� �����

        if (winPanel == null)
        {
            // ������ ������ ������ �� �����
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

        // ������� ������ ���������, ���� ��� �� ��������� ����� ���������
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

        // ������� ������ �����
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

        // ������� ����� ��� �����
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

        Debug.Log($"GameManager: ������� ������ - WinPanel: {winPanel != null}, LosePanel: {losePanel != null}, NextLevelButton: {nextLevelButton != null}, MainMenuButton: {mainMenuButton != null}, ������: {starIcons?.Length ?? 0}");
    }

    private void SubscribeToLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon += HandleLevelWon;
            levelManager.OnLevelLost += HandleLevelLost;
            Debug.Log("GameManager: �������� �� ������� LevelManager ���������");
        }
        else
        {
            Debug.LogWarning("GameManager: LevelManager �� ������, �������� �� ������� ����������");
        }
    }

    private void UnsubscribeFromLevelManager()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelWon -= HandleLevelWon;
            levelManager.OnLevelLost -= HandleLevelLost;
            Debug.Log("GameManager: ������� �� ������� LevelManager ���������");
        }
    }

    private void SubscribeToUIRootBinder()
    {
        if (uiRootBinder != null)
        {
            uiRootBinder.GoToMainMenuButtonClicked += LoadMainMenu;
            uiRootBinder.NextLevelButtonClicked += LoadNextLevel;
            uiRootBinder.RestartLevelButtonClicked += RestartLevel;
            Debug.Log("GameManager: �������� �� ������� UIGameplayRootBinder ���������");
        }
        else
        {
            Debug.LogWarning("GameManager: UIGameplayRootBinder �� ������, �������� �� ������� ����������");
        }

        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested += LoadMainMenu;
            gameplayEntryPoint.GoToNextLevelRequested += LoadNextLevel;
            gameplayEntryPoint.RestartLevelRequested += RestartLevel;
            Debug.Log("GameManager: �������� �� ������� GameplayEntryPoint ���������");
        }
        else
        {
            Debug.LogWarning("GameManager: GameplayEntryPoint �� ������, �������� �� ������� ����������");
        }
    }

    private void UnsubscribeFromUIRootBinder()
    {
        if (uiRootBinder != null)
        {
            uiRootBinder.GoToMainMenuButtonClicked -= LoadMainMenu;
            uiRootBinder.NextLevelButtonClicked -= LoadNextLevel;
            uiRootBinder.RestartLevelButtonClicked -= RestartLevel;
            Debug.Log("GameManager: ������� �� ������� UIGameplayRootBinder ���������");
        }

        if (gameplayEntryPoint != null)
        {
            gameplayEntryPoint.GoToMainMainMenuRequested -= LoadMainMenu;
            gameplayEntryPoint.GoToNextLevelRequested -= LoadNextLevel;
            gameplayEntryPoint.RestartLevelRequested -= RestartLevel;
            Debug.Log("GameManager: ������� �� ������� GameplayEntryPoint ���������");
        }
    }

    public void ResetGameState()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // �������� ������ ��������� ��� ������
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);

        // �������� ��� ������ ��� ������
        HideAllStars();

        if (starsText != null) starsText.text = "0";

        // ���������, ���������� �� AmmunitionManager
        AmmunitionManager ammunitionManager = FindObjectOfType<AmmunitionManager>();
        if (ammunitionManager != null)
        {
            ammunitionManager.ResetAmmunition();
        }
    }

    // �������� ��� ������ �����
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

    // ���������� ������ ���������� �����
    private void ShowStars(int starsCount)
    {
        if (starIcons == null || starIcons.Length == 0)
            return;

        // ������� �������� ��� ������
        HideAllStars();

        // ���������� ������ ���������� �����
        for (int i = 0; i < Mathf.Min(starsCount, starIcons.Length); i++)
        {
            if (starIcons[i] != null)
                starIcons[i].SetActive(true);
        }

        // ��������� ����� �����, ���� ����
        if (starsText != null)
            starsText.text = starsCount.ToString();
    }

    private void HandleLevelWon(int stars, List<string> completedConditions)
    {
        totalStarsEarned += stars;

        Debug.Log($"������� �������! ����� ����������: {stars}");
        Debug.Log($"����������� �������: {string.Join(", ", completedConditions)}");

        // ���������� ������ ������
        if (winPanel != null)
            winPanel.SetActive(true);
        else
            Debug.LogWarning("GameManager: WinPanel �� ������");

        // ���������� ������ �����������
        if (restartButton != null)
            restartButton.SetActive(true);
        else
            Debug.LogWarning("GameManager: RestartButton �� ������");

        // ���������, ���� �� ��������� �������
        bool hasNextLevel = HasNextLevel();

        // ���������� ������ ��������� ��� ������
        if (nextLevelButton != null)
        {
            // ���������� ������ ���������� ������ ������ ���� ���� ��������� �������
            nextLevelButton.SetActive(hasNextLevel);
            if (hasNextLevel)
                Debug.Log("GameManager: NextLevelButton �����������");
            else
                Debug.Log("GameManager: NextLevelButton ����� (��� ���������� ������)");
        }
        else
            Debug.LogWarning("GameManager: NextLevelButton �� ������");

        // ������ �������� ���� ������������ ������
        if (mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
            Debug.Log("GameManager: MainMenuButton �����������");
        }
        else
            Debug.LogWarning("GameManager: MainMenuButton �� ������");

        // ���������� ������ ����� UIRootBinder, ���� �� ��������
        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
            {
                // ���������� ������ ���������� ������ ������ ���� ���� ��������� �������
                uiRootBinder._nextLevelButton.SetActive(hasNextLevel);
                if (hasNextLevel)
                    Debug.Log("GameManager: UIRootBinder NextLevelButton �����������");
                else
                    Debug.Log("GameManager: UIRootBinder NextLevelButton ����� (��� ���������� ������)");
            }

            if (uiRootBinder._mainMenuButton != null)
            {
                uiRootBinder._mainMenuButton.SetActive(true);
                Debug.Log("GameManager: UIRootBinder MainMenuButton �����������");
            }
        }

        // ���������� ������������ ������
        ShowStars(stars);

        // ���� ��� ��������� ������� � ������ ������ "��������� �������",
        // ������������� ��������� ������� ���� ����� ��������� �����
        if (!hasNextLevel)
        {
            Debug.Log("��� ��������� �������. ��� ������� '��������� �������' ����� ������� � ������� ����.");
        }
    }

    private void HandleLevelLost(int stars, List<string> loseReasons)
    {
        //Debug.Log($"������� ��������! �������: {string.Join(", ", loseReasons)}");

        // ���������� ������ ���������
        if (losePanel != null)
            losePanel.SetActive(true);
        else
            Debug.LogWarning("GameManager: LosePanel �� ������");

        // ���������� ������ �����������
        //if (restartButton != null)
        //    restartButton.SetActive(true);
        //else
        //    Debug.LogWarning("GameManager: RestartButton �� ������");

        // ������ ������ ��������� ��� ���������
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);

        // ������������ ������ ����� UIRootBinder, ���� �� ��������
        if (uiRootBinder != null)
        {
            if (uiRootBinder._nextLevelButton != null)
                uiRootBinder._nextLevelButton.SetActive(false);

            if (uiRootBinder._mainMenuButton != null)
                uiRootBinder._mainMenuButton.SetActive(false);
        }

        // �������� ��� ������ ��� ���������
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
        Debug.Log($"���� {(isPaused ? "��������������" : "������������")}");
    }

    public void RestartLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Debug.Log($"���������� ������ {currentLevelIndex}");
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"�������� ���������� ������ {nextLevelIndex}");
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            Debug.Log("�������� �������� ���� (��� ������ �������)");
            SceneManager.LoadScene(0); // ������� ����
        }
    }

    // ���������, ���� �� ��������� �������
    public bool HasNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        return nextLevelIndex < SceneManager.sceneCountInBuildSettings;
    }

    public void LoadMainMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Debug.Log("�������� �������� ����");
        SceneManager.LoadScene(0); // ������� ����
    }

    // ���������� ���� (������ � ���������)
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!useDebugMenu) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 300));
        GUILayout.Label("���������� ����");

        if (GUILayout.Button("������������� �������"))
        {
            RestartLevel();
        }

        if (GUILayout.Button("������� � ���������� ������"))
        {
            LoadNextLevel();
        }

        if (GUILayout.Button("�����/����������"))
        {
            TogglePause();
        }

        GUILayout.Label($"����� �����: {totalStarsEarned}");
        GUILayout.Label($"������� �������: {currentLevelIndex}");
        GUILayout.Label($"������� �����: {starIcons?.Length ?? 0}");
        GUILayout.Label($"WinPanel: {winPanel != null}");
        GUILayout.Label($"LosePanel: {losePanel != null}");
        GUILayout.Label($"NextLevelButton: {nextLevelButton != null}");
        GUILayout.Label($"MainMenuButton: {mainMenuButton != null}");
        GUILayout.Label($"UIRootBinder: {uiRootBinder != null}");

        GUILayout.EndArea();
    }
#endif
}