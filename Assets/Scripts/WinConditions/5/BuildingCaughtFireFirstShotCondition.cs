using UnityEngine;
public class BuildingCaughtFireFirstShotCondition : WinCondition
{
    [SerializeField] private Building1 targetBuilding;
    [SerializeField] private AmmunitionManager ammunitionManager;
    private bool conditionChecked = false;

    private void Awake()
    {
        starsAwarded = 1; // Всегда устанавливаем 1 звезду
        conditionName = "Меткий стрелок";
        conditionDescription = "Поджечь здание первым боеприпасом";
    }

    private void Update()
    {
        // Проверяем условие только один раз когда здание загорелось
        if (!conditionChecked && targetBuilding != null && targetBuilding.IsOnFire())
        {
            conditionChecked = true;

            // Проверяем, сколько боеприпасов было использовано
            int usedAmmunition = ammunitionManager.GetMaxAmmunition() - ammunitionManager.GetCurrentAmmunition();

            // Если использован только 1 боеприпас, условие выполнено
            if (usedAmmunition == 1 && GameManager.Instance != null)
            {
                GameManager.Instance.LevelManager.CheckWinConditions();
            }
        }
    }

    public override bool IsConditionMet()
    {
        if (targetBuilding == null || ammunitionManager == null) return false;

        if (targetBuilding.IsOnFire())
        {
            int usedAmmunition = ammunitionManager.GetMaxAmmunition() - ammunitionManager.GetCurrentAmmunition();
            return usedAmmunition == 1;
        }

        return false;
    }

    public override void ResetCondition()
    {
        conditionChecked = false;
    }
}