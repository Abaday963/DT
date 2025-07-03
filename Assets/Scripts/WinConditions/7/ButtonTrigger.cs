using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private string targetTag = "Player"; // Тег объекта, который активирует кнопку
    [SerializeField] private MovingWall targetWall; // Ссылка на стену
    [SerializeField] private float cooldownTime = 5f; // Время кулдауна в секундах
    [SerializeField] private bool isPressed = false;
    private bool isOnCooldown = false;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color pressedColor = Color.red;
    private Color originalColor;

    private void Start()
    {
        // Сохраняем изначальный цвет кнопки
        if (buttonSprite != null)
        {
            originalColor = buttonSprite.color;
        }

        // Автоматически находим стену, если не назначена
        if (targetWall == null)
        {
            targetWall = FindObjectOfType<MovingWall>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это нужный объект, кнопка не нажата и не на кулдауне
        if (other.CompareTag(targetTag) && !isPressed && !isOnCooldown)
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        isPressed = true;
        isOnCooldown = true;

        // Визуальная обратная связь
        if (buttonSprite != null)
        {
            buttonSprite.color = pressedColor;
        }

        // Активируем стену
        if (targetWall != null)
        {
            targetWall.ActivateWall();
        }

        Debug.Log("Кнопка нажата! Кулдаун: " + cooldownTime + " секунд");

        // Сбрасываем визуальное состояние кнопки через небольшую задержку
        Invoke(nameof(ResetButtonVisual), 0.5f);

        // Включаем кулдаун
        Invoke(nameof(ResetCooldown), cooldownTime);
    }

    private void ResetButtonVisual()
    {
        isPressed = false;

        // Возвращаем изначальный цвет
        if (buttonSprite != null)
        {
            buttonSprite.color = originalColor;
        }
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
        Debug.Log("Кнопка снова готова к использованию!");
    }
}