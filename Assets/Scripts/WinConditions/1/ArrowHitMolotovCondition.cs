public class ArrowHitMolotovCondition : WinCondition
{
    private bool arrowHitMolotov = false;

    private void Awake()
    {
        starsAwarded = 3;
        conditionName = "Тройной удар";
        conditionDescription = "Попасть стрелой в молотов";
    }

    // Этот метод будет вызываться из скрипта Arrow (который вы можете создать отдельно)
    public void OnArrowHitMolotov()
    {
        arrowHitMolotov = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelManager.CheckWinConditions();
        }
    }

    public override bool IsConditionMet()
    {
        return arrowHitMolotov;
    }

    public override void ResetCondition()
    {
        arrowHitMolotov = false;
    }
}