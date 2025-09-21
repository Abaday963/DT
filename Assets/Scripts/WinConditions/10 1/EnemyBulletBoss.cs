using UnityEngine;
using System.Collections.Generic;

public class EnemyBulletBoss : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 5f;
    public float towardSpeed = 8f;
    public float lifetime = 10f;

    [Header("Паттерн полета")]
    public BulletPattern bulletPattern = BulletPattern.Straight;

    [Header("Настройки дуги")]
    public float arcHeight = 2f; // Высота дуги

    [Header("Настройки зигзага")]
    public float zigzagAmplitude = 1f; // Амплитуда зигзага
    public float zigzagFrequency = 2f; // Частота зигзага
    public float zigzacArcHeight = 2f; // Высота дуги

    [Header("Настройки поворота")]
    public bool rotateTowardsDirection = true; // Поворачивать лицом к направлению движения
    public Transform bulletFront; // Перед пули (если null, используется сам объект)

    [Header("Отладка")]
    public bool showCurrentPattern = true; // Показать текущий паттерн в инспекторе

    private Transform target;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelTime;
    private float currentTime;
    private Rigidbody2D rb;

    // Для зигзага
    private Vector3 lastPosition;
    private float zigzagOffset;

    // Для поворота
    private Vector3 previousPosition;

    // Текущий активный паттерн (для отладки)
    private BulletPattern activePattern;

    // Модификаторы для босса
    private float speedMultiplier = 1.0f;
    private float zigzagIntensityMultiplier = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        previousPosition = transform.position;

        // Если выбран случайный паттерн, выбираем один из доступных
        if (bulletPattern == BulletPattern.Random)
        {
            BulletPattern[] availablePatterns = { BulletPattern.Straight, BulletPattern.Arc, BulletPattern.ZigzagArc };
            activePattern = availablePatterns[Random.Range(0, availablePatterns.Length)];
        }
        else
        {
            activePattern = bulletPattern;
        }

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
            float currentSpeed = towardSpeed * speedMultiplier;
            transform.Translate(direction * currentSpeed * Time.deltaTime);
        }
    }

    private void MoveArc()
    {
        if (target == null) return;

        // Вычисляем общее время полета с учетом модификатора скорости
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float effectiveSpeed = speed * speedMultiplier;
        float totalTime = totalDistance / effectiveSpeed;

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

        // Вычисляем общее время полета с учетом модификатора скорости
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float effectiveSpeed = speed * speedMultiplier;
        float totalTime = totalDistance / effectiveSpeed;

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
        float arcOffset = zigzacArcHeight * 4f * progress * (1f - progress);
        Vector3 arcPosition = linearPosition + Vector3.up * arcOffset;

        // Добавляем зигзагообразное отклонение с учетом интенсивности
        float effectiveZigzagAmplitude = zigzagAmplitude * zigzagIntensityMultiplier;
        float effectiveZigzagFrequency = zigzagFrequency * zigzagIntensityMultiplier;
        float zigzagValue = Mathf.Sin(currentTime * effectiveZigzagFrequency) * effectiveZigzagAmplitude;

        // Вычисляем перпендикулярный вектор для зигзага
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
        Vector3 zigzagOffset = perpendicular * zigzagValue;

        transform.position = arcPosition + zigzagOffset;
    }

    private void RotateTowardsDirection()
    {
        Vector3 movementDirection = transform.position - previousPosition;

        if (movementDirection.magnitude > 0.01f) // Избегаем поворота при очень маленьком движении
        {
            // Вычисляем угол поворота
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;

            // Применяем поворот к объекту (или к bulletFront, если он задан)
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

            // Для дуги вычисляем время полета
            if (bulletPattern == BulletPattern.Arc)
            {
                float distance = Vector3.Distance(startPosition, targetPosition);
                float effectiveSpeed = speed * speedMultiplier;
                travelTime = distance / effectiveSpeed;
            }
        }
    }

    public void SetBulletPattern(BulletPattern pattern)
    {
        bulletPattern = pattern;
        activePattern = pattern;
    }

    // Новые методы для босса
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;

        // Если цель уже установлена и это дуга, пересчитываем время полета
        if (target != null && (bulletPattern == BulletPattern.Arc || activePattern == BulletPattern.Arc))
        {
            float distance = Vector3.Distance(startPosition, targetPosition);
            float effectiveSpeed = speed * speedMultiplier;
            travelTime = distance / effectiveSpeed;
        }
    }

    public void SetZigzagIntensity(float intensity)
    {
        zigzagIntensityMultiplier = intensity;
    }

    // Дополнительные методы для более тонкой настройки
    public void SetCustomSpeed(float customSpeed)
    {
        speed = customSpeed;
        towardSpeed = customSpeed;
    }

    public void SetZigzagParameters(float amplitude, float frequency)
    {
        zigzagAmplitude = amplitude;
        zigzagFrequency = frequency;
    }

    // Метод для получения текущих параметров (для отладки)
    public void GetBulletInfo()
    {
        Debug.Log($"Bullet Info - Pattern: {activePattern}, Speed Multiplier: {speedMultiplier}, Zigzag Intensity: {zigzagIntensityMultiplier}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Пуля столкнулась с объектом: {other.name}, тег: {other.tag}");

        if (other.CompareTag("PlayerBase"))
        {
            Debug.Log($"Попадание в кобока: {other.name}");
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
            Debug.Log("Пуля попала в стену/препятствие");
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
            Debug.Log("Пуля попала в стену/препятствие");
            Destroy(gameObject);
        }
    }
}