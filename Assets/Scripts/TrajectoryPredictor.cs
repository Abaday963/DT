using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [SerializeField] private int pointsCount = 20; // Количество точек для отображения
    [SerializeField] private float timeStep = 0.05f; // Временной шаг для симуляции
    [SerializeField] private GameObject pointPrefab; // Префаб точки траектории
    [SerializeField] private Transform shootPoint; // Точка выстрела (рогатка)
    [SerializeField] private float dotScale = 0.5f; // Размер точек
    [SerializeField] private bool useGradient = true; // Использовать градиент цвета
    [SerializeField] private Gradient trajectoryGradient; // Градиент для точек траектории
    [SerializeField] private float dotAlpha = 0.7f; // Прозрачность точек
    [SerializeField] private float dotPulseSpeed = 1.5f; // Скорость пульсации точек
    [SerializeField] private float dotPulseAmount = 0.2f; // Величина пульсации

    private List<GameObject> trajectoryPoints = new List<GameObject>();
    private Rigidbody2D ammoRigidbody;
    private bool isAiming = false;
    private float pulseTimer = 0f;

    private void Start()
    {
        ammoRigidbody = GetComponent<Rigidbody2D>();

        // Если нет ссылки на точку выстрела, попробуем найти её
        if (shootPoint == null)
        {
            GameObject slingshot = GameObject.FindGameObjectWithTag("Slingshot");
            if (slingshot != null)
            {
                shootPoint = slingshot.transform;
            }
        }

        // Создаем точки для траектории
        CreateTrajectoryPoints();

        // По умолчанию скрываем точки
        HideTrajectory();

        // Настройка градиента по умолчанию, если он не задан
        if (trajectoryGradient.colorKeys.Length == 0)
        {
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0].color = Color.white;
            colorKeys[0].time = 0.0f;
            colorKeys[1].color = Color.red;
            colorKeys[1].time = 1.0f;

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = dotAlpha;
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 0.2f;
            alphaKeys[1].time = 1.0f;

            trajectoryGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    private void Update()
    {
        // Если мы прицеливаемся и активирован режим пульсации
        if (isAiming && dotPulseAmount > 0)
        {
            pulseTimer += Time.deltaTime * dotPulseSpeed;
            float pulseFactor = 1f + Mathf.Sin(pulseTimer) * dotPulseAmount;

            // Применяем пульсацию к точкам
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                if (trajectoryPoints[i].activeSelf)
                {
                    trajectoryPoints[i].transform.localScale = Vector3.one * dotScale * pulseFactor;
                }
            }
        }
    }

    private void CreateTrajectoryPoints()
    {
        // Если нет префаба точки, создаем простой спрайт
        if (pointPrefab == null)
        {
            // Создаем временный префаб точки
            GameObject tempPoint = new GameObject("TrajectoryPoint");
            SpriteRenderer renderer = tempPoint.AddComponent<SpriteRenderer>();

            // Создаем круглый спрайт программно
            Texture2D texture = new Texture2D(32, 32);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(texture.width / 2, texture.height / 2));
                    if (distance < texture.width / 2)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(1, 1, 1, 0));
                    }
                }
            }
            texture.Apply();

            Sprite circleSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            renderer.sprite = circleSprite;

            pointPrefab = tempPoint;
            tempPoint.SetActive(false);
        }

        // Создаем точки
        for (int i = 0; i < pointsCount; i++)
        {
            GameObject point = Instantiate(pointPrefab);
            point.transform.localScale = Vector3.one * dotScale;
            point.SetActive(false);

            SpriteRenderer renderer = point.GetComponent<SpriteRenderer>();
            if (renderer != null && useGradient)
            {
                float gradientPosition = (float)i / pointsCount;
                renderer.color = trajectoryGradient.Evaluate(gradientPosition);
            }

            trajectoryPoints.Add(point);
        }
    }

    public void ShowTrajectory()
    {
        isAiming = true;
    }

    public void HideTrajectory()
    {
        isAiming = false;
        foreach (GameObject point in trajectoryPoints)
        {
            point.SetActive(false);
        }
    }

    // Метод для вызова из скриптов боеприпасов
    public void UpdateTrajectory(Vector2 startPosition, Vector2 startVelocity)
    {
        if (!isAiming || ammoRigidbody == null || trajectoryPoints.Count == 0 || shootPoint == null)
            return;

        // Если скорость не указана, рассчитываем её на основе расстояния до точки выстрела
        Vector2 velocity = startVelocity;
        if (velocity == Vector2.zero)
        {
            velocity = (shootPoint.position - transform.position).normalized *
                      Vector2.Distance(transform.position, shootPoint.position) *
                      ammoRigidbody.mass * Physics2D.gravity.magnitude * 0.5f;

            velocity = -velocity; // Инвертируем, поскольку оттягиваем в противоположную сторону
        }

        // Обновляем положение точек на основе физики
        Vector2 currentPosition = startPosition;
        Vector2 currentVelocity = velocity;

        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            // Активируем точку
            trajectoryPoints[i].SetActive(true);

            // Симулируем физику для текущего шага времени
            float timeOffset = timeStep * i;
            Vector2 gravityEffect = Physics2D.gravity * ammoRigidbody.gravityScale * timeOffset * timeOffset * 0.5f;

            // Рассчитываем позицию на основе начальной скорости и гравитации
            Vector2 predictedPosition = currentPosition + currentVelocity * timeOffset + gravityEffect;

            // Устанавливаем позицию точки
            trajectoryPoints[i].transform.position = predictedPosition;

            // Проверяем столкновение с окружением
            RaycastHit2D hit = Physics2D.Linecast(currentPosition, predictedPosition);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                // Если обнаружено столкновение, прерываем траекторию
                trajectoryPoints[i].transform.position = hit.point;

                // Делаем невидимыми все последующие точки
                for (int j = i + 1; j < trajectoryPoints.Count; j++)
                {
                    trajectoryPoints[j].SetActive(false);
                }

                break;
            }

            currentPosition = predictedPosition;
        }
    }
}