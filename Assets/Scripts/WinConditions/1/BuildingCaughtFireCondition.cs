using UnityEngine;

public class BuildingCaughtFireCondition : WinCondition
{
    [SerializeField] private Building1 targetBuilding;

    private void Awake()
    {
        starsAwarded = 1;
        conditionName = "Поджигание здания";
        conditionDescription = "Поджечь здание любым количеством боеприпаса";
    }

    private void Update()
    {
        // Постоянно проверяем, загорелось ли здание
        if (IsConditionMet() && GameManager.Instance != null)
        {
            // Оповещаем менеджер уровня о выполнении условия
            GameManager.Instance.LevelManager.CheckWinConditions();
        }
    }

    public override bool IsConditionMet()
    {
        // Используем существующий геттер IsOnFire() из Building1
        return targetBuilding != null && targetBuilding.IsOnFire();
    }

    public override void ResetCondition()
    {
        // Сброс происходит через перезапуск уровня и сброс состояния здания
    }
}