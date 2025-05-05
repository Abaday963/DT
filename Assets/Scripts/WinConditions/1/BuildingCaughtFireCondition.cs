using UnityEngine;
public class BuildingCaughtFireCondition : WinCondition
{
    [SerializeField] private Building1 targetBuilding;
    private bool conditionCompleted = false;

    private void Awake()
    {
        starsAwarded = 1; // Всегда устанавливаем 1 звезду
        conditionName = "Поджигание здания";
        conditionDescription = "Поджечь здание любым количеством боеприпаса";
        if (targetBuilding == null)
        {
            targetBuilding = FindAnyObjectByType<Building1>();
        }
    }

    private void Update()
    {
        // Проверяем загорелось ли здание и не было ли уже зарегистрировано выполнение условия
        if (!conditionCompleted && targetBuilding != null && targetBuilding.IsOnFire())
        {
            conditionCompleted = true;

            // Оповещаем менеджер уровня о выполнении условия
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelManager.CheckWinConditions();
            }

            Debug.Log("Условие 'Поджигание здания' выполнено");
        }
    }

    public override bool IsConditionMet()
    {
        return conditionCompleted;
    }

    public override void ResetCondition()
    {
        conditionCompleted = false;
    }
}