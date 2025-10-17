using UnityEngine;

public enum BulletPattern
{
    Straight,        // Прямая линия
    Arc,            // Дуга
    ZigzagArc,      // Зигзаг по дуге
    ZigzagStraight, // Зигзаг прямо (новый паттерн для босса)
    Random          // Случайный выбор
}

public class EnemyBulletBoss : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 8f; // Базовая скорость (будет изменяться динамически)
    public float lifetime = 15f;

    [Header("Паттерн полета")]
    public BulletPattern bulletPattern = BulletPattern.Straight;

    [Header("Настройки дуги")]
    public float arcHeight = 2f; // Высота дуги (может изменяться)

    [Header("Настройки зигзага")]
    public float zigzagAmplitude = 1f; // Амплитуда зигзага (может изменяться)
    public float zigzagFrequency = 2f; // Частота зигзага (может изменяться)

    [Header("Настройки поворота")]
    public bool rotateTowardsDirection = true;
    public Transform bulletFront;

    [Header("Отладка")]
    public bool showCurrentPattern = true;

    private Transform target;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentTime;
    private Rigidbody2D rb;

    // Для поворота
    private Vector3 previousPosition;

    // Текущий активный паттерн
    private BulletPattern activePattern;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        previousPosition = transform.position;

        // Если выбран случайный паттерн, выбираем один из доступных
        if (bulletPattern == BulletPattern.Random)
        {
            BulletPattern[] availablePatterns = {
                BulletPattern.Straight,
                BulletPattern.Arc,
                BulletPattern.ZigzagArc,
                BulletPattern.ZigzagStraight
            };
            activePattern = availablePatterns[Random.Range(0, availablePatterns.Length)];
        }
        else
        {
            activePattern = bulletPattern;
        }

        Debug.Log($"Пуля босса создана с паттерном: {activePattern}, скорость: {speed}");

        // Уничтожаем пулю через определенное время
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            targetPosition = target.position;
        }

        currentTime += Time.deltaTime;

        switch (activePattern)
        {
            case BulletPattern.Straight:
                MoveStraight();
                break;
            case BulletPattern.Arc:
                MoveArc();
                break;
            case BulletPattern.ZigzagArc:
                MoveZigzagArc();
                break;
            case BulletPattern.ZigzagStraight:
                MoveZigzagStraight();
                break;
        }

        // Поворачиваем пулю лицом к направлению движения
        if (rotateTowardsDirection)
        {
            RotateTowardsDirection();
        }

        previousPosition = transform.position;
    }

    private void MoveStraight()
    {
        if (target != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void MoveArc()
    {
        if (target == null) return;

        // Вычисляем общее время полета
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float totalTime = totalDistance / speed;

        // Вычисляем текущий прогресс (0 to 1)
        float progress = currentTime / totalTime;

        if (progress >= 1f)
        {
            transform.position = targetPosition;
            return;
        }

        // Линейная интерполяция между начальной и конечной точкой
        Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, progress);

        // Добавляем высоту дуги (параболическая кривая)
        float arcOffset = arcHeight * 4f * progress * (1f - progress);
        Vector3 arcPosition = linearPosition + Vector3.up * arcOffset;

        transform.position = arcPosition;
    }

    private void MoveZigzagArc()
    {
        if (target == null) return;

        // Вычисляем общее время полета
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float totalTime = totalDistance / speed;

        // Вычисляем текущий прогресс (0 to 1)
        float progress = currentTime / totalTime;

        if (progress >= 1f)
        {
            transform.position = targetPosition;
            return;
        }

        // Линейная интерполяция между начальной и конечной точкой
        Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, progress);

        // Добавляем высоту дуги (параболическая кривая)
        float arcOffset = arcHeight * 4f * progress * (1f - progress);
        Vector3 arcPosition = linearPosition + Vector3.up * arcOffset;

        // Добавляем зигзагообразное отклонение
        float zigzagValue = Mathf.Sin(currentTime * zigzagFrequency) * zigzagAmplitude;

        // Вычисляем перпендикулярный вектор для зигзага
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
        Vector3 zigzagOffset = perpendicular * zigzagValue;

        transform.position = arcPosition + zigzagOffset;
    }

    private void MoveZigzagStraight()
    {
        if (target == null) return;

        // Основное движение к цели
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 movement = direction * speed * Time.deltaTime;

        // Добавляем зигзагообразное отклонение
        float zigzagValue = Mathf.Sin(currentTime * zigzagFrequency) * zigzagAmplitude;

        // Вычисляем перпендикулярный вектор для зигзага
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
        Vector3 zigzagMovement = perpendicular * zigzagValue * Time.deltaTime;

        transform.Translate(movement + zigzagMovement);
    }

    private void RotateTowardsDirection()
    {
        Vector3 movementDirection = transform.position - previousPosition;

        if (movementDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            Transform rotationTarget = bulletFront != null ? bulletFront : transform;
            rotationTarget.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPosition = target.position;
        }
    }

    public void SetBulletPattern(BulletPattern pattern)
    {
        bulletPattern = pattern;
        activePattern = pattern;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetZigzagSettings(float amplitude, float frequency)
    {
        zigzagAmplitude = amplitude;
        zigzagFrequency = frequency;
    }

    public void SetArcHeight(float height)
    {
        arcHeight = height;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Пуля босса столкнулась с объектом: {other.name}, тег: {other.tag}");

        if (other.CompareTag("PlayerBase"))
        {
            Debug.Log($"Попадание в цель: {other.name}");
            Kobok kobok = other.GetComponent<Kobok>();
            if (kobok != null)
            {
                Debug.Log("Вызываем TakeHit()");
                kobok.TakeHit();
            }
            else
            {
                Debug.LogError($"На объекте {other.name} нет компонента Kobok!");
            }
            Destroy(gameObject);
        }

        if (other.CompareTag("Molotov"))
        {
            Debug.Log("Пуля босса попала в стену/препятствие");
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBase"))
        {
            Kobok kobok = collision.gameObject.GetComponent<Kobok>();
            if (kobok != null)
            {
                kobok.TakeHit();
            }
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Molotov"))
        {
            Debug.Log("Пуля босса попала в стену/препятствие");
            Destroy(gameObject);
        }
    }
}