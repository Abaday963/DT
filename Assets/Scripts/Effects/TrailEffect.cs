using UnityEngine;

public class TrailEffect : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int maxPoints = 20;       // Максимум точек в следе
    public float minDistance = 0.1f;  // Минимальная дистанция для новой точки

    private Vector3 lastPosition;
    private int currentPointCount = 0;

    void Start()
    {
        lineRenderer.positionCount = 0;  // Начинаем с пустого следа
        lastPosition = transform.position;
    }

    void Update()
    {
        // Добавляем точку, если объект сдвинулся достаточно далеко
        if (Vector2.Distance(transform.position, lastPosition) > minDistance)
        {
            AddPoint(transform.position);
            lastPosition = transform.position;
        }
    }

    void AddPoint(Vector3 newPoint)
    {
        // Увеличиваем количество точек (но не больше maxPoints)
        if (currentPointCount < maxPoints)
        {
            lineRenderer.positionCount = currentPointCount + 1;
            lineRenderer.SetPosition(currentPointCount, newPoint);
            currentPointCount++;
        }
        else
        {
            // Если точек уже maxPoints, сдвигаем старые точки
            for (int i = 1; i < maxPoints; i++)
            {
                lineRenderer.SetPosition(i - 1, lineRenderer.GetPosition(i));
            }
            lineRenderer.SetPosition(maxPoints - 1, newPoint);
        }
    }
}