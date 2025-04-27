using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Отслеживание состояния игры
    private bool hasBuilding1Caught = false;
    private bool allShardsThrown = false;
    private int currentStars = 0;
    private bool lastMolotovThrown = false;
    private float lastMolotovWaitTime = 3.0f; // Время ожидания в секундах для последнего молотова
    private float lastMolotovTimer = 0f;

    // Ссылки на UI и игровые элементы
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Text starsText;

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            // Удалите DontDestroyOnLoad отсюда
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Инициализация состояния игры
        ResetGameState();
    }
    private void Update()
    {
        // Если последний молотов был брошен, но здание еще не загорелось
        if (lastMolotovThrown && !hasBuilding1Caught)
        {
            lastMolotovTimer += Time.deltaTime;

            // Если прошло достаточно времени, а здание так и не загорелось
            if (lastMolotovTimer >= lastMolotovWaitTime)
            {
                // Значит молотов не попал или не сработал
                lastMolotovThrown = false;
                OnAllShardsThrown(); // Теперь можно считать, что все молотовы были брошены и не попали
            }
        }
    }
    // Новый метод для сброса состояния игры
    public void ResetGameState()
    {
        hasBuilding1Caught = false;
        allShardsThrown = false;
        currentStars = 0;
        lastMolotovThrown = false; // Сбрасываем флаг последнего молотова

        // Скрываем все панели
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // Сбросить состояние патронов
        //if (AmmunitionManager.Instance != null)
        //{
        //    AmmunitionManager.Instance.ResetAmmunition();
        //}
    }

    public void OnBuilding1Caught()
    {
        hasBuilding1Caught = true;
        currentStars = 3;
        HandleGameWin();
    }

    public void OnAllShardsThrown()
    {
        allShardsThrown = true;
        CheckGameOutcome();
    }

    private void CheckGameOutcome()
    {
        if (allShardsThrown)
        {
            if (!hasBuilding1Caught)
            {
                if (restartButton != null)
                {
                    restartButton.SetActive(true);
                }

                if (losePanel != null)
                {
                    losePanel.SetActive(true);
                }
            }
        }
    }

    private void HandleGameWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (starsText != null)
        {
            starsText.text = $"Звезды: {currentStars}";
        }
    }

    public void RestartGame()
    {
        // Загружаем сцену заново
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Добавим метод, который будет вызываться после загрузки сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameState();
    }

    public void OnLastMolotovThrown()
    {
        lastMolotovThrown = true;
        lastMolotovTimer = 0f; // Сбрасываем таймер
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
    }

}