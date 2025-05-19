
// 3. Условие проигрыша по истечению времени
using UnityEngine;

public class TimeExpiredCondition : LoseCondition
{
    [SerializeField] private TimerSlider levelTimer;

    private void Awake()
    {
        conditionName = "Время истекло";
        conditionDescription = "Вы не успели поразить все цели за отведенное время";
    }

    private void Start()
    {
        if (levelTimer == null)
        {
            levelTimer = FindObjectOfType<TimerSlider>();
        }
    }

    public override bool IsConditionMet()
    {
        if (levelTimer != null)
        {
            return levelTimer.GetCurrentState() == TimerState.Expired;
        }
        return false;
    }

    public override void ResetCondition()
    {
        // Сброс не требуется, так как состояние полностью зависит от таймера
    }
}
// 4. Компонент для мишени, который регистрирует попадания
