using UnityEngine;

// ������� ����� ��� ������� ������
public abstract class WinCondition : MonoBehaviour
{
    [SerializeField] protected string conditionName;
    [SerializeField] protected string conditionDescription;
    [SerializeField] protected int starsAwarded = 1;

    public string Name => conditionName;
    public string Description => conditionDescription;
    public int Stars => starsAwarded;

    public abstract bool IsConditionMet();
    public abstract void ResetCondition();
}

// ������� ����� ��� ������� ���������
public abstract class LoseCondition : MonoBehaviour
{
    [SerializeField] protected string conditionName;
    [SerializeField] protected string conditionDescription;

    public string Name => conditionName;
    public string Description => conditionDescription;

    public abstract bool IsConditionMet();
    public abstract void ResetCondition();
}
    
