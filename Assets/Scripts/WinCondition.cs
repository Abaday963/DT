using UnityEngine;

public abstract class WinCondition : MonoBehaviour
{
    [SerializeField] protected int starsAwarded = 1;
    protected string conditionName;
    protected string conditionDescription;

    public string Name => conditionName;
    public string Description => conditionDescription;
    public int Stars => starsAwarded;

    public abstract bool IsConditionMet();
    public abstract void ResetCondition();
}

// Базовый класс для условий проигрыша
public abstract class LoseCondition : MonoBehaviour
{
    [SerializeField] protected string conditionName;
    [SerializeField] protected string conditionDescription;

    public string Name => conditionName;
    public string Description => conditionDescription;

    public abstract bool IsConditionMet();
    public abstract void ResetCondition();
}
    
