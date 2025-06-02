using UnityEngine;
using UnityEngine.UI;

public class WarningIndicator : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject contentToHide; // Контент который нужно скрыть в начале

    [Header("Timing Settings")]
    [SerializeField] private float appearDelay = 1f; // Задержка появления объекта
    [SerializeField] private float buttonAppearDelay = 6f; // Задержка появления кнопки

    private bool isInitialized = false;

    // События для внешних скриптов
    public System.Action OnIndicatorDestroyed;

    void Start()
    {
        InitializeIndicator();
    }

    private void InitializeIndicator()
    {
        if (isInitialized) return;

        // Скрываем контент, но объект остается активным
        if (contentToHide != null)
        {
            contentToHide.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
            closeButton.onClick.AddListener(DestroyIndicator);
        }

        // Запускаем последовательность показа
        StartCoroutine(ShowSequence());

        isInitialized = true;
        Debug.Log("Индикатор инициализирован");
    }

    private System.Collections.IEnumerator ShowSequence()
    {
        // Ждем 1 секунду перед появлением контента
        yield return new WaitForSeconds(appearDelay);

        // Показываем контент
        if (contentToHide != null)
        {
            contentToHide.SetActive(true);
        }
        Debug.Log("Контент появился");

        // Ждем еще 6 секунд перед появлением кнопки
        yield return new WaitForSeconds(buttonAppearDelay);

        // Показываем кнопку
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
            Debug.Log("Кнопка закрытия появилась");
        }
    }

    public void DestroyIndicator()
    {
        Debug.Log("Уничтожение индикатора");

        // Уведомляем внешние скрипты о уничтожении
        OnIndicatorDestroyed?.Invoke();

        // Уничтожаем объект
        Destroy(gameObject);
    }

    // Метод для принудительного закрытия
    public void ForceDestroy()
    {
        DestroyIndicator();
    }

    // Публичные свойства для проверки состояния
    public bool IsInitialized => isInitialized;
    public bool IsButtonVisible => closeButton != null && closeButton.gameObject.activeInHierarchy;

    void OnDestroy()
    {
        // Отписываемся от события кнопки
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(DestroyIndicator);
        }
    }
}