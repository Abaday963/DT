using UnityEngine;

public class TimerWinCondition : WinCondition
{
    [Header("Настройки времени")]
    [SerializeField] private float winTime = 20f; // Время для победы в секундах

    [Header("Настройки таймера")]
    [SerializeField] private TimerSlider levelTimer; // Ссылка на таймер уровня

    [Header("Ссылка на Health Manager")]
    [SerializeField] private HealthManager healthManager; // Ссылка на менеджер здоровья

    private float currentTime = 0f;
    private bool isWinAvailable = true; // Флаг доступности победы
    private bool conditionCompleted = false;
    private int starsEarned = 0;

    private void Awake()
    {
        conditionName = "Выживание";
        conditionDescription = $"Выживите в течение {winTime} секунд. Больше здоровья = больше звезд";

        Debug.Log("TimerWinCondition инициализирован");
    }

    private void Start()
    {
        // Найти таймер, если не задан вручную
        if (levelTimer == null)
        {
            levelTimer = FindObjectOfType<TimerSlider>();
            if (levelTimer != null)
                Debug.Log("Таймер уровня найден автоматически");
            else
                Debug.LogWarning("Таймер уровня не найден!");
        }

        // Найти Health Manager, если не задан вручную
        if (healthManager == null)
        {
            healthManager = FindObjectOfType<HealthManager>();
            if (healthManager != null)
                Debug.Log("HealthManager найден автоматически");
            else
                Debug.LogWarning("HealthManager не найден!");
        }
    }

    private void Update()
    {
        // Проверяем, не проиграл ли игрок
        //if (healthManager != null && healthManager.IsGameOver())
        //{
        //    isWinAvailable = false;
        //    Debug.Log("Победа недоступна - игрок проиграл");
        //    return;
        //}

        // Обновляем время только если победа еще возможна и условие не выполнено
        if (isWinAvailable && !conditionCompleted)
        {
            currentTime += Time.deltaTime;

            // Проверяем условие победы
            if (currentTime >= winTime)
            {
                OnTimerCompleted();
            }
        }
    }

    private void OnTimerCompleted()
    {
        if (!conditionCompleted)
        {
            starsEarned = CalculateStarsBasedOnHealth();
            starsAwarded = starsEarned;
            conditionCompleted = true;

            Debug.Log($"Условие победы выполнено! Игрок выжил! Получено {starsEarned} звезд");
            NotifyGameManager();
        }
    }

    private int CalculateStarsBasedOnHealth()
    {
        if (healthManager != null)
        {
            int currentHealth = healthManager.GetCurrentHearts();
            int maxHealth = healthManager.GetMaxHearts();

            // Если здоровье не потеряно (полное здоровье)
            if (currentHealth == maxHealth)
            {
                Debug.Log($"Полное здоровье ({currentHealth}/{maxHealth}) - 3 звезды");
                return 3;
            }
            // Если осталось 2 или больше единиц здоровья (но не полное)
            else if (currentHealth >= 2)
            {
                Debug.Log($"Хорошее здоровье ({currentHealth}/{maxHealth}) - 2 звезды");
                return 2;
            }
            // Если осталось только 1 единица здоровья
            else if (currentHealth == 1)
            {
                Debug.Log($"Критическое здоровье ({currentHealth}/{maxHealth}) - 1 звезда");
                return 1;
            }
            // Если здоровье закончилось (хотя этого не должно случиться, так как игра закончится)
            else
            {
                Debug.Log($"Здоровье закончилось ({currentHealth}/{maxHealth}) - 0 звезд");
                return 0;
            }
        }

        Debug.LogWarning("HealthManager не найден, возвращаем 1 звезду по умолчанию");
        return 1; // Базовая награда, если менеджер здоровья не найден
    }

    // Уведомляем GameManager о выполнении условия
    private void NotifyGameManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
        {
            // Устанавливаем количество звезд в LevelManager для режима ByTimer (или как у вас настроено)
            LevelManager levelManager = GameManager.Instance.LevelManager;
            levelManager.SetWeightedStarValue(starsEarned);
            levelManager.CheckWinConditions();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance или LevelManager не найдены!");
        }
    }

    public override bool IsConditionMet()
    {
        return conditionCompleted;
    }

    public override void ResetCondition()
    {
        Debug.Log("Сброс условия TimerWinCondition");
        currentTime = 0f;
        isWinAvailable = true;
        conditionCompleted = false;
        starsEarned = 0;
    }

    // Метод для получения прогресса (для UI)
    public float GetProgress()
    {
        return Mathf.Clamp01(currentTime / winTime);
    }

    // Метод для получения оставшегося времени
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, winTime - currentTime);
    }

    // Получить количество заработанных звезд
    public int GetStarsEarned()
    {
        return starsEarned;
    }
}