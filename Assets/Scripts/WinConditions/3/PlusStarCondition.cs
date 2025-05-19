using UnityEngine;

public class PlusStarCondition : WinCondition
{
    [SerializeField] private Building1 targetBuilding;
    private bool conditionCompleted = false;
    private float checkTimer = 0f;
    private const float CHECK_DURATION = 100.0f; // Время проверки в секундах

    private void Awake()
    {
        starsAwarded = 1; // Всегда устанавливаем 1 звезду
        conditionName = "Delaesh";
        conditionDescription = "Suda star";
        if (targetBuilding == null)
        {
            targetBuilding = FindAnyObjectByType<Building1>();
        }
    }

    private void Update()
    {
        // Увеличиваем таймер
        checkTimer += Time.deltaTime;

        // Проверяем, прошло ли нужное время
        if (checkTimer <= CHECK_DURATION)
        {
            if (targetBuilding != null && targetBuilding.IsOnFire())
            {
                conditionCompleted = true;
                if (GameManager.Instance != null)
                {
                    //Debug.Log("Стрела попала в молотов, но здание ЗАГОРЕЛОС И ТРОЙНОЙ МАТЬ ЕГО УДАР");
                    GameManager.Instance.LevelManager.CheckWinConditions();
                }
            }
        }
    }

    public override bool IsConditionMet()
    {
        return conditionCompleted;
    }

    public override void ResetCondition()
    {
        conditionCompleted = false;
        checkTimer = 0f; // Сбрасываем и таймер тоже
    }
}