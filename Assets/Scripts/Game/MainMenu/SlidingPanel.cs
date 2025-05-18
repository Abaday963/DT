using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlidingPanel : MonoBehaviour
{
    [Tooltip("Начальная позиция панели")]
    public Vector3 startPosition;

    [Tooltip("Конечная позиция панели (куда должна перемещаться)")]
    public Vector3 endPosition;

    [Tooltip("Скорость перемещения панели")]
    public float moveSpeed = 10f;

    [Tooltip("Кнопка для выдвижения панели")]
    public Button openButton;

    [Tooltip("Кнопка для закрытия панели")]
    public Button closeButton;

    private bool isMoving = false;
    private bool isPanelOpen = false;

    void Start()
    {
        // Устанавливаем начальную позицию
        transform.localPosition = startPosition;

        // Добавляем обработчики событий для кнопок
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    // Метод для открытия панели
    public void OpenPanel()
    {
        if (!isMoving && !isPanelOpen)
        {
            StartCoroutine(MovePanel(startPosition, endPosition));
            isPanelOpen = true;
        }
    }

    // Метод для закрытия панели
    public void ClosePanel()
    {
        if (!isMoving && isPanelOpen)
        {
            StartCoroutine(MovePanel(endPosition, startPosition));
            isPanelOpen = false;
        }
    }

    // Корутина для плавного перемещения панели
    private IEnumerator MovePanel(Vector3 from, Vector3 to)
    {
        isMoving = true;

        float elapsedTime = 0;
        float duration = Vector3.Distance(from, to) / moveSpeed;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(from, to, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Убедимся, что панель достигла точной конечной позиции
        transform.localPosition = to;
        isMoving = false;
    }

    // Метод для переключения состояния панели
    public void TogglePanel()
    {
        if (isPanelOpen)
            ClosePanel();
        else
            OpenPanel();
    }
}