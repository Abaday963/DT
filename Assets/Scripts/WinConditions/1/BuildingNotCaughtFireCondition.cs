using UnityEngine;

public class BuildingNotCaughtFireCondition : LoseCondition
{
    [SerializeField] private Building1 targetBuilding;
    [SerializeField] private AmmunitionManager ammunitionManager;
    [SerializeField] private float checkDelay = 2.0f; // Задержка перед проверкой окончательного проигрыша

    private bool isChecking = false;
    private float timer = 0f;

    private void Awake()
    {
        conditionName = "Провал";
        conditionDescription = "Здание не было подожжено и закончились боеприпасы";
    }

    private void Update()
    {
        // Проверяем условия для запуска таймера проигрыша
        if (!isChecking && ammunitionManager != null && !ammunitionManager.HasAmmo() &&
            targetBuilding != null && !targetBuilding.IsOnFire())
        {
            isChecking = true;
            timer = 0f;
        }

        // Если проверка началась, отсчитываем время
        if (isChecking)
        {
            timer += Time.deltaTime;

            // Если в задержке здание загорелось, отменяем проверку
            if (targetBuilding != null && targetBuilding.IsOnFire())
            {
                isChecking = false;
            }
            // Если время вышло и здание не загорелось, игрок проиграл
            else if (timer >= checkDelay)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LevelManager.CheckLoseConditions();
                }
            }
        }
    }

    public override bool IsConditionMet()
    {
        if (targetBuilding == null || ammunitionManager == null) return false;

        // Проиграл, если закончились боеприпасы, здание не горит и прошло достаточно времени
        return !ammunitionManager.HasAmmo() && !targetBuilding.IsOnFire() && isChecking && timer >= checkDelay;
    }

    public override void ResetCondition()
    {
        isChecking = false;
        timer = 0f;
    }
}
    