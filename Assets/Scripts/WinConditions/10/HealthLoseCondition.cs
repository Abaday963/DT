using UnityEngine;

public class HealthLoseCondition : LoseCondition
{
    [Header("Ссылка на Health Manager")]
    [SerializeField] private HealthManager healthManager;

    private void Awake()
    {
        conditionName = "Здоровье исчерпано";
        conditionDescription = "Вы потеряли все сердца";

        Debug.Log("HealthLoseCondition инициализирован");
    }

    private void Start()
    {
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
        // Проверяем условие проигрыша каждый кадр
        if (IsConditionMet())
        {
            Debug.LogWarning("Cheliy");

            GameManager.Instance.LevelManager.CheckLoseConditions();

            //if (GameManager.Instance != null)
            //{
            //    GameManager.Instance.LevelManager.CheckLoseConditions();
            //    Debug.LogWarning("Cheliy");

            //}
            //else
            //{
            //    Debug.LogWarning("GameManager.Instance или LevelManager не найдены!");
            //}
        }
    }

    public override bool IsConditionMet()
    {
        if (healthManager != null)
        {
            bool isGameOver = healthManager.IsGameOver();
            // Дополнительная проверка через количество сердец
            bool noHealthLeft = healthManager.GetCurrentHearts() <= 0;

            return isGameOver || noHealthLeft;
        }

        Debug.LogWarning("HealthManager не найден! Условие проигрыша не может быть проверено.");
        return false;
    }

    public override void ResetCondition()
    {
        Debug.Log("Сброс условия HealthLoseCondition");
        // Сброс не требуется, так как состояние полностью зависит от HealthManager
        // При необходимости можно сбросить здоровье через healthManager.ResetHealth()
    }

    // Метод для получения текущего количества сердец (для UI)
    public int GetCurrentHearts()
    {
        if (healthManager != null)
        {
            return healthManager.GetCurrentHearts();
        }
        return 0;
    }

    // Метод для получения максимального количества сердец (для UI)
    public int GetMaxHearts()
    {
        if (healthManager != null)
        {
            return healthManager.GetMaxHearts();
        }
        return 0;
    }
}