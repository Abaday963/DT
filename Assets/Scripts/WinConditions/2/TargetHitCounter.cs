using UnityEngine;
using TMPro;

public class TargetHitCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TimerTargetHitCondition targetCondition;

    private void Start()
    {
        // Автоматически найти компоненты, если не заданы
        if (counterText == null)
        {
            counterText = GetComponent<TextMeshProUGUI>();
        }

        if (targetCondition == null)
        {
            targetCondition = FindObjectOfType<TimerTargetHitCondition>();
        }

        UpdateCounter(0, targetCondition != null ? targetCondition.RequiredTargetHits : 0);
    }

    public void UpdateCounter(int current, int required)
    {
        if (counterText != null)
        {
            counterText.text = $"{current}/{required}";
        }
    }
}