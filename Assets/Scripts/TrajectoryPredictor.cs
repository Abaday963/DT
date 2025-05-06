using System.Collections.Generic;
using UnityEngine;

// Интерфейс для любого типа боеприпаса
public interface IProjectile
{
    Transform GetTransform();
    bool IsLaunched(); // Добавляем метод для проверки был ли выпущен снаряд
}

// Универсальный предиктор траектории
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int linePoints = 25;
    [SerializeField] private float timeStep = 0.05f;
    [SerializeField] private float maxTime = 5f;
    [SerializeField] private bool showOnlyWhenDragging = true;

    [Header("References")]
    [SerializeField] private GameObject projectileObject;  // Ссылка на объект боеприпаса
    [SerializeField] private Rigidbody2D slingshotRigidbody;
    [SerializeField] private float forceMultiplier = 5f;

    // Ссылка на боеприпас через интерфейс
    private IProjectile projectile;
    private Transform projectileTransform;

    // Физический движок для симуляции
    private PhysicsScene2D physicsScene;
    private GameObject dummyObject;
    private Rigidbody2D dummyRigidbody;

    // Флаг для отслеживания активности траектории
    private bool trajectoryActive = false;

    // Флаг, был ли уже выпущен боеприпас
    private bool projectileLaunched = false;

    private void Start()
    {
        // Инициализация LineRenderer, если он не назначен
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }
        }
        else
        {
            SetupLineRenderer();
        }

        // Инициализация проектила
        InitializeProjectile();

        // Инициализация рогатки, если она не назначена
        if (slingshotRigidbody == null)
        {
            GameObject slingshot = GameObject.FindGameObjectWithTag("FirstSlingshot");
            if (slingshot != null)
            {
                slingshotRigidbody = slingshot.GetComponent<Rigidbody2D>();
            }
        }

        // По умолчанию скрываем траекторию
        lineRenderer.enabled = false;

        // Создаем фиктивный объект для симуляции
        CreateDummyObject();

        // Получаем текущую физическую сцену
        physicsScene = Physics2D.defaultPhysicsScene;
    }

    private void InitializeProjectile()
    {
        // Проверяем наличие объекта боеприпаса
        if (projectileObject == null)
        {
            projectileObject = gameObject;
        }

        // Получаем компонент, реализующий интерфейс IProjectile
        projectile = projectileObject.GetComponent<IProjectile>();

        // Если компонент не реализует интерфейс, пробуем получить Transform напрямую
        if (projectile == null)
        {
            projectileTransform = projectileObject.transform;
            projectileLaunched = false; // По умолчанию считаем, что не выпущен
        }
        else
        {
            projectileTransform = projectile.GetTransform();
            // Проверяем, был ли уже выпущен боеприпас
            projectileLaunched = projectile.IsLaunched();
        }
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = linePoints;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = true;
    }

    private void CreateDummyObject()
    {
        // Создаем невидимый объект для симуляции траектории
        dummyObject = new GameObject("TrajectoryDummy");
        dummyObject.hideFlags = HideFlags.HideInHierarchy;

        // Добавляем компонент Rigidbody2D
        dummyRigidbody = dummyObject.AddComponent<Rigidbody2D>();
        dummyRigidbody.gravityScale = 1f;
        dummyRigidbody.linearDamping = 0f;

        // Добавляем небольшой коллайдер для обнаружения столкновений
        CircleCollider2D collider = dummyObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.1f;
        collider.isTrigger = true;
    }

    private void Update()
    {
        // Проверяем, что необходимые компоненты существуют и боеприпас не выпущен
        if (projectileTransform == null || slingshotRigidbody == null)
            return;

        // Обновляем статус снаряда, если у нас есть интерфейс IProjectile
        if (projectile != null)
        {
            projectileLaunched = projectile.IsLaunched();
        }

        // Если боеприпас уже выпущен, скрываем траекторию и выходим
        if (projectileLaunched)
        {
            lineRenderer.enabled = false;
            trajectoryActive = false;
            return;
        }

        // Проверяем, нажата ли кнопка мыши или есть касание
        bool isPressed = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended && Input.GetTouch(0).phase != TouchPhase.Canceled);

        // Определяем, является ли текущий боеприпас активным (нажатым)
        bool isCurrentProjectileActive = false;

        if (projectile is MonoBehaviour monoBehaviour)
        {
            MolotovAmmunition molotov = monoBehaviour as MolotovAmmunition;
            ArrowAmmunition arrow = monoBehaviour as ArrowAmmunition;
            if (arrow != null)
            {
                isCurrentProjectileActive = arrow.IsPressed();
            }
            if (molotov != null)
            {
                isCurrentProjectileActive = molotov.IsPressed();
            }
        }

        // Показываем траекторию только если текущий боеприпас активен и нажат
        if (isPressed && isCurrentProjectileActive)
        {
            // Получаем направление и силу броска
            Vector2 direction = (Vector2)slingshotRigidbody.position - (Vector2)projectileTransform.position;
            Vector2 force = direction * forceMultiplier;

            // Показываем траекторию
            lineRenderer.enabled = true;
            trajectoryActive = true;
            PredictTrajectory(projectileTransform.position, force);
        }
        else if (showOnlyWhenDragging || !isCurrentProjectileActive)
        {
            // Скрываем траекторию, если не натягиваем текущий боеприпас
            lineRenderer.enabled = false;
            trajectoryActive = false;
        }
    }

    private void PredictTrajectory(Vector3 startPos, Vector2 force)
    {
        // Устанавливаем начальную позицию фиктивного объекта
        dummyObject.transform.position = startPos;
        dummyRigidbody.linearVelocity = Vector2.zero;

        // Симулируем применение силы
        Vector2 velocity = force / dummyRigidbody.mass;

        // Заполняем первую точку траектории
        lineRenderer.SetPosition(0, startPos);

        // Моделируем траекторию по времени
        float timeElapsed = 0f;
        Vector2 currentPosition = startPos;

        for (int i = 1; i < linePoints; i++)
        {
            // Увеличиваем время
            timeElapsed += timeStep;

            if (timeElapsed > maxTime)
                break;

            // Вычисляем новую позицию с учетом гравитации и начальной скорости
            // s = ut + 0.5 * a * t^2
            currentPosition = (Vector2)startPos + velocity * timeElapsed + 0.5f * Physics2D.gravity * timeElapsed * timeElapsed;

            // Задаем позицию в LineRenderer
            lineRenderer.SetPosition(i, currentPosition);
        }

        // Если точек осталось меньше максимального количества, устанавливаем оставшиеся в последнюю позицию
        for (int i = Mathf.CeilToInt(timeElapsed / timeStep) + 1; i < linePoints; i++)
        {
            lineRenderer.SetPosition(i, currentPosition);
        }
    }

    // Метод для обновления ссылки на боеприпас
    public void SetProjectile(GameObject newProjectile)
    {
        projectileObject = newProjectile;
        InitializeProjectile();
    }

    // Метод для внешней проверки активности траектории
    public bool IsTrajectoryActive()
    {
        return trajectoryActive;
    }

    private void OnDestroy()
    {
        // Очистка ресурсов
        if (dummyObject != null)
        {
            Destroy(dummyObject);
        }
    }
}