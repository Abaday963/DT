using UnityEngine;
public class ArrowHitMolotovCondition : WinCondition
{
    [SerializeField] private Building1 targetBuilding;
    private bool arrowHitMolotov = false;
    private bool conditionCompleted = false;
    private float checkTimer = 0f;
    private const float CHECK_DURATION = 2.0f; // Время проверки в секундах

    private void Awake()
    {
        starsAwarded = 1; // Меняем с 3 на 1 звезду
        conditionName = "Тройной удар";
        conditionDescription = "Попасть стрелой в молотов и поджечь здание";
        if (targetBuilding == null)
        {
            targetBuilding = FindAnyObjectByType<Building1>();
        }
    }

    private void Update()
    {
        // Если стрела попала в молотов, но условие еще не выполнено
        if (arrowHitMolotov && !conditionCompleted)
        {
            checkTimer += Time.deltaTime;
            // Проверяем в течение определенного времени, загорелось ли здание
            if (checkTimer <= CHECK_DURATION)
            {
                if (targetBuilding != null && targetBuilding.IsOnFire())
                {
                    conditionCompleted = true;
                    if (GameManager.Instance != null)
                    {
                        Debug.Log("Стрела попала в молотов, но здание ЗАГОРЕЛОС И ТРОЙНОЙ МАТЬ ЕГО УДАР");
                        GameManager.Instance.LevelManager.CheckWinConditions();
                    }
                }
            }
            else
            {
                // Если прошло CHECK_DURATION секунд, а здание не загорелось - сбрасываем попадание
                arrowHitMolotov = false;
                checkTimer = 0f;
                Debug.Log("Стрела попала в молотов, но здание не загорелось в течение указанного времени");
            }
        }
    }

    // Этот метод будет вызываться из скрипта Arrow при попадании в молотов
    public void OnArrowHitMolotov()
    {
        if (!conditionCompleted)
        {
            arrowHitMolotov = true;
            checkTimer = 0f; // Начинаем отсчет времени
            Debug.Log("Стрела попала в молотов. Проверяем загорится ли здание...");
        }
    }

    public override bool IsConditionMet()
    {
        return conditionCompleted;
    }

    public override void ResetCondition()
    {
        arrowHitMolotov = false;
        conditionCompleted = false;
        checkTimer = 0f;
    }
}