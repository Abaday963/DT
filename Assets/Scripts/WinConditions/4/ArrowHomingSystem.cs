using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHomingSystem : MonoBehaviour
{
    [Header("Настройки определения")]
    [SerializeField] private string molotovTag = "Molotov"; // Тег для коктейля Молотова
    [SerializeField] private LayerMask detectionLayer; // Слой для обнаружения
    [SerializeField] private Transform detectionZone; // Зона обнаружения/триггер

    [Header("Настройки стрелы")]
    [SerializeField] private GameObject arrowPrefab; // Префаб стрелы
    [SerializeField] private Transform arrowSpawnPoint; // Точка появления стрелы
    [SerializeField] private float arrowSpeed = 15f; // Скорость стрелы
    [SerializeField] private float shootDelay = 0.2f; // Задержка перед выстрелом для визуального эффекта

    [Header("Настройки самонаведения")]
    [SerializeField] private float predictionFactor = 0.5f; // Фактор предсказания для упреждения движения

    private List<GameObject> detectedMolotovs = new List<GameObject>();
    private bool canShoot = true;

    void Start()
    {
        if (detectionZone == null)
            detectionZone = transform;
    }

    void Update()
    {
        // Удаляем несуществующие объекты Молотова из списка
        detectedMolotovs.RemoveAll(item => item == null);
    }

    // Отрисовка зоны детекции в редакторе Unity
    void OnDrawGizmos()
    {
        if (detectionZone != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(detectionZone.position, detectionZone.localScale.x / 2);
        }
    }

    // Обнаружение входящего коктейля Молотова
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(molotovTag))
        {
            detectedMolotovs.Add(other.gameObject);

            if (canShoot)
            {
                StartCoroutine(ShootArrowAtMolotov(other.gameObject));
            }
        }
    }

    // Удаление коктейля из списка при выходе из зоны
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(molotovTag))
        {
            detectedMolotovs.Remove(other.gameObject);
        }
    }

    // Корутина для выстрела стрелой с небольшой задержкой
    IEnumerator ShootArrowAtMolotov(GameObject targetMolotov)
    {
        canShoot = false;

        // Небольшая задержка перед выстрелом
        yield return new WaitForSeconds(shootDelay);

        // Проверяем, что цель все еще существует
        if (targetMolotov != null)
        {
            Debug.Log("letim bla");
            FireArrow(targetMolotov);
        }

        canShoot = true;

        // Если в зоне есть другие коктейли, стреляем в следующий
        if (detectedMolotovs.Count > 0 && detectedMolotovs[0] != null && detectedMolotovs[0] != targetMolotov)
        {
            StartCoroutine(ShootArrowAtMolotov(detectedMolotovs[0]));
        }
    }

    // Метод для выстрела стрелой в коктейль Молотова
    void FireArrow(GameObject targetMolotov)
    {
        if (arrowPrefab == null || arrowSpawnPoint == null || targetMolotov == null)
            return;

        // Создаем стрелу
        //GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        GameObject arrow = arrowPrefab;

        // Получаем компоненты
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        Rigidbody2D targetRb = targetMolotov.GetComponent<Rigidbody2D>();

        if (arrowRb != null && targetRb != null)
        {
            // Рассчитываем вектор движения с учетом предсказания
            Vector2 targetPos = targetMolotov.transform.position;
            Vector2 targetVel = targetRb.linearVelocity;

            // Предсказываем, где будет коктейль через некоторое время
            Vector2 predictedPos = targetPos + (targetVel * predictionFactor);

            // Направление к предсказанной позиции
            Vector2 direction = (predictedPos - (Vector2)arrowSpawnPoint.position).normalized;

            // Устанавливаем скорость и направление стрелы
            arrowRb.linearVelocity = direction * arrowSpeed;

            // Поворачиваем стрелу в направлении движения
            //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Добавляем скрипт для самонаведения, если нужно отслеживать цель во время полета
            ArrowHoming homingComponent = arrow.AddComponent<ArrowHoming>();
            homingComponent.SetTarget(targetMolotov.transform);
        }
    }
}

