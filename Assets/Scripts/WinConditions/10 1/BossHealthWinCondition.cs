using UnityEngine;

public class BossHealthWinCondition : WinCondition
{
    [Header("Настройки условия победы")]
    [SerializeField] private string bossTag = "Boss"; // Тег босса
    [SerializeField] private bool findByName = false; // Искать по имени вместо тега
    [SerializeField] private string bossName = "CannonBoss"; // Имя объекта босса

    [Header("Ссылка на Health Manager")]
    [SerializeField] private HealthManager healthManager; // Ссылка на менеджер здоровья

    private CannonBoss targetBoss;
    private bool bossDefeated = false;

    void Start()
    {
        // Настройка описания условия
        conditionName = "Победить босса";
        conditionDescription = "Уничтожьте босса-пушку, чтобы выиграть. Больше здоровья = больше звезд";

        // Найти Health Manager, если не задан вручную
        if (healthManager == null)
        {
            healthManager = FindObjectOfType<HealthManager>();
            if (healthManager != null)
                Debug.Log("HealthManager найден автоматически");
            else
                Debug.LogWarning("HealthManager не найден!");
        }

        // Поиск босса на сцене
        FindBoss();
    }

    void FindBoss()
    {
        if (findByName)
        {
            // Поиск по имени объекта
            GameObject bossObject = GameObject.Find(bossName);
            if (bossObject != null)
            {
                targetBoss = bossObject.GetComponent<CannonBoss>();
            }
        }
        else
        {
            // Поиск по тегу
            GameObject bossObject = GameObject.FindGameObjectWithTag(bossTag);
            if (bossObject != null)
            {
                targetBoss = bossObject.GetComponent<CannonBoss>();
            }
        }

        if (targetBoss == null)
        {
            Debug.LogWarning($"BossHealthWinCondition: Босс не найден! Проверьте тег '{bossTag}' или имя '{bossName}'");
        }
        else
        {
            Debug.Log("BossHealthWinCondition: Босс найден и отслеживается");
        }
    }

    // Уведомляем GameManager о выполнении условия
    private void NotifyGameManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
        {
            // Устанавливаем количество звезд в LevelManager
            LevelManager levelManager = GameManager.Instance.LevelManager;
            levelManager.SetWeightedStarValue(starsAwarded);
            levelManager.CheckWinConditions();
            Debug.Log($"Уведомление GameManager отправлено. Звезд: {starsAwarded}");
        }
        else
        {
            Debug.LogWarning("GameManager.Instance или LevelManager не найдены!");
        }
    }

    void Update()
    {
        // Проверяем, что босс существует и не был побежден ранее
        if (targetBoss == null && !bossDefeated)
        {
            // Если босс исчез (был уничтожен), значит он побежден
            bossDefeated = true;
            CalculateStarsBasedOnHealth();
            Debug.Log($"BossHealthWinCondition: Босс побежден! Условие победы выполнено. Звезд получено: {starsAwarded}");
            NotifyGameManager();
        }
    }

    void CalculateStarsBasedOnHealth()
    {
        if (healthManager != null)
        {
            int currentHealth = healthManager.GetCurrentHearts();
            int maxHealth = healthManager.GetMaxHearts();

            // Если здоровье не потеряно (полное здоровье)
            if (currentHealth == maxHealth)
            {
                starsAwarded = 3;
                Debug.Log($"Полное здоровье ({currentHealth}/{maxHealth}) - 3 звезды");
            }
            // Если осталось 2 или больше единиц здоровья (но не полное)
            else if (currentHealth >= 2)
            {
                starsAwarded = 2;
                Debug.Log($"Хорошее здоровье ({currentHealth}/{maxHealth}) - 2 звезды");
            }
            // Если осталось только 1 единица здоровья
            else if (currentHealth == 1)
            {
                starsAwarded = 1;
                Debug.Log($"Критическое здоровье ({currentHealth}/{maxHealth}) - 1 звезда");
            }
            // Если здоровье закончилось (хотя этого не должно случиться)
            else
            {
                starsAwarded = 0;
                Debug.Log($"Здоровье закончилось ({currentHealth}/{maxHealth}) - 0 звезд");
            }
        }
        else
        {
            Debug.LogWarning("HealthManager не найден, возвращаем 1 звезду по умолчанию");
            starsAwarded = 1; // Базовая награда, если менеджер здоровья не найден
        }
    }

    public override bool IsConditionMet()
    {
        // Условие выполнено, если босс побежден (объект уничтожен)
        return bossDefeated || targetBoss == null;
    }

    public override void ResetCondition()
    {
        bossDefeated = false;
        starsAwarded = 1; // Сброс звезд до базового значения
        FindBoss(); // Повторно ищем босса при сбросе
        Debug.Log("BossHealthWinCondition: Условие сброшено");
    }

    void OnDisable()
    {
        // Очистка ссылок при отключении компонента
        targetBoss = null;
        healthManager = null;
    }
}