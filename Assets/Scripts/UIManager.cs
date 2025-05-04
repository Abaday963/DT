using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Элементы")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject restartButton;
    public GameObject Ammunition;
    public Text starsText;
    public GameObject[] starIcons;

    private void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager: Создан и переведен в DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("UIManager: Уже существует, уничтожаю дубликат");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindUIReferences();
        AssignMainCameraToCanvases();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignMainCameraToCanvases();
    }

    private void FindUIReferences()
    {
        // Ищем основные UI элементы, если они не заданы
        if (winPanel == null)
            winPanel = transform.Find("WinPanel")?.gameObject;

        if (losePanel == null)
            losePanel = transform.Find("LosePanel")?.gameObject;

        if (restartButton == null)
            restartButton = transform.Find("RestartButton")?.gameObject;

        if (Ammunition == null)
            Ammunition = transform.Find("Ammunition")?.gameObject;

        if (starsText == null)
            starsText = GetComponentInChildren<Text>(true);

        // Собираем все иконки звезд
        if (starIcons == null || starIcons.Length == 0)
        {
            Transform starsParent = transform.Find("Stars");
            if (starsParent != null)
            {
                starIcons = new GameObject[starsParent.childCount];
                for (int i = 0; i < starsParent.childCount; i++)
                {
                    starIcons[i] = starsParent.GetChild(i).gameObject;
                }
            }
        }

        Debug.Log($"UIManager: Найдены UI элементы - WinPanel: {winPanel != null}, " +
                 $"LosePanel: {losePanel != null}, Stars: {starIcons?.Length ?? 0}");
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("UIManager: Показана панель победы");
        }
        else
        {
            Debug.LogWarning("UIManager: Панель победы не найдена");
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Debug.Log("UIManager: Показана панель поражения");
        }
        else
        {
            Debug.LogWarning("UIManager: Панель поражения не найдена");
        }
    }

    public void ShowRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    public void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        HideAllStars();
    }

    public void ShowStars(int count)
    {
        if (starIcons == null || starIcons.Length == 0)
        {
            Debug.LogWarning("UIManager: Иконки звезд не найдены");
            return;
        }

        // Сначала скрываем все звезды
        HideAllStars();

        // Показываем нужное количество звезд
        for (int i = 0; i < Mathf.Min(count, starIcons.Length); i++)
        {
            if (starIcons[i] != null)
                starIcons[i].SetActive(true);
        }

        // Обновляем текст звезд, если есть
        if (starsText != null)
            starsText.text = count.ToString();

        Debug.Log($"UIManager: Показано звезд: {count}");
    }

    public void HideAllStars()
    {
        if (starIcons != null && starIcons.Length > 0)
        {
            foreach (var star in starIcons)
            {
                if (star != null)
                    star.SetActive(false);
            }
        }

        if (starsText != null)
            starsText.text = "0";
    }
    private void AssignMainCameraToCanvases()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("UIManager: MainCamera не найдена");
            return;
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                canvas.worldCamera = mainCam;
                Debug.Log("UIManager: Назначена MainCamera для Canvas: " + canvas.name);
            }
        }
    }

}