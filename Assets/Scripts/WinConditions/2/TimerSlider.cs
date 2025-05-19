using UnityEngine;
using UnityEngine.UI;
public class TimerSlider : MonoBehaviour
{
    [Header("Настройки таймера")]
    [SerializeField] private float totalTime = 12f;
    [SerializeField] private float greenDuration = 5f;
    [SerializeField] private float yellowDuration = 4f;

    [Header("UI элементы")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image fillImage; // fill объекта слайдера
    [SerializeField] private Text timerText;

    [Header("Цвета")]
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color redColor = Color.red;

    private float currentTime;
    private TimerState currentState = TimerState.Green;
    private bool isTimerRunning = true;

    private void Start()
    {
        currentTime = totalTime;

        if (timerSlider != null)
        {
            timerSlider.maxValue = totalTime;
            timerSlider.value = totalTime;
        }

        UpdateVisuals();
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, totalTime);

        UpdateTimerState();
        UpdateVisuals();

        if (currentTime <= 0f && currentState != TimerState.Expired)
        {
            currentState = TimerState.Expired;
            isTimerRunning = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelManager.CheckLoseConditions();
            }
        }
    }

    private void UpdateTimerState()
    {
        float elapsed = totalTime - currentTime;

        if (elapsed < greenDuration)
            currentState = TimerState.Green;
        else if (elapsed < greenDuration + yellowDuration)
            currentState = TimerState.Yellow;
        else if (currentTime > 0f)
            currentState = TimerState.Red;
    }

    private void UpdateVisuals()
    {
        if (timerSlider != null)
        {
            timerSlider.value = currentTime;

            if (fillImage != null)
            {
                switch (currentState)
                {
                    case TimerState.Green:
                        fillImage.color = greenColor;
                        break;
                    case TimerState.Yellow:
                        fillImage.color = yellowColor;
                        break;
                    default:
                        fillImage.color = redColor;
                        break;
                }
            }
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    // Методы доступа для других скриптов
    public TimerState GetCurrentState() => currentState;
    public float GetRemainingTime() => currentTime;

    public void PauseTimer() => isTimerRunning = false;
    public void ResumeTimer() => isTimerRunning = true;

    public void ResetTimer()
    {
        currentTime = totalTime;
        currentState = TimerState.Green;
        isTimerRunning = true;
        UpdateVisuals();
    }
}
