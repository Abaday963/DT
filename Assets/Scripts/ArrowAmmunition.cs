using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowAmmunition : MonoBehaviour, IAmmunition, IProjectile
{
    [SerializeField] private bool isPressed = false;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float launchForce = 10f; // Множитель силы
    [SerializeField] private Rigidbody2D shootRigid; // Ссылка на рогатку
    [SerializeField] public GameObject ammoPrefab; // Префаб этого же боеприпаса
    [SerializeField] public Transform spawnPoint; // Точка появления нового боеприпаса
    [SerializeField] private ArrowHitMolotovCondition arrowHitCondition;
    [SerializeField] private LayerMask molotovLayer; // Слой для молотова

    [Header("Анимация падения")]
    [SerializeField] private float tiltAngle = 45f; // Угол наклона задней части
    [SerializeField] private float tiltDuration = 0.3f; // Время наклона
    [SerializeField] private float fallDelay = 0.2f; // Задержка перед падением
    [SerializeField] private float fallGravity = 2f; // Гравитация при падении

    [Header("Аудио")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drawSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float drawVolume = 1f;

    private Vector2 startPosition;
    private Vector2 releaseDirection;
    private Rigidbody2D arrowRigidbody;
    private bool wasLaunched = false; // Флаг, указывающий, что стрела была выстрелена
    private Collider2D arrowCollider; // Ссылка на коллайдер стрелы
    private bool isFalling = false; // Флаг анимации падения
    private Quaternion originalRotation; // Исходный поворот стрелы

    private void Start()
    {
        arrowRigidbody = GetComponent<Rigidbody2D>();

        // Сохраняем начальную позицию
        startPosition = transform.position;

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        // Получаем ссылку на коллайдер
        arrowCollider = GetComponent<Collider2D>();

        // Установим триггер для коллайдера, пока стрела не выстрелена
        // Это позволит ей проходить сквозь другие объекты
        if (arrowCollider != null)
        {
            arrowCollider.isTrigger = true;
        }

        // Если не назначена рогатка, найдем её автоматически
        if (shootRigid == null)
        {
            GameObject slingshot = GameObject.FindGameObjectWithTag("Slingshot");
            if (slingshot != null)
            {
                shootRigid = slingshot.GetComponent<Rigidbody2D>();
            }
        }

        // Если не назначена точка спавна, попробуем найти её
        if (spawnPoint == null)
        {
            GameObject spawner = GameObject.FindGameObjectWithTag("AmmoSpawner");
            if (spawner != null)
            {
                spawnPoint = spawner.transform;
            }
        }

        // Добавляем SpringJoint2D если его нет
        if (shootRigid != null && !GetComponent<SpringJoint2D>())
        {
            SpringJoint2D springJoint = gameObject.AddComponent<SpringJoint2D>();
            springJoint.connectedBody = shootRigid;
            springJoint.distance = 0.5f;
            springJoint.dampingRatio = 0.1f;
            springJoint.frequency = 2.0f;
        }
    }

    private void Update()
    {
        // Если стрела падает, не обрабатываем ввод
        if (isFalling) return;

        // Обработка PC ввода
        if (Input.GetMouseButtonDown(0) && !isPressed && !wasLaunched)
        {
            CheckTouch(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            ReleaseArrow();
        }

        // Обработка мобильного ввода
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && !isPressed && !wasLaunched)
            {
                CheckTouch(Camera.main.ScreenToWorldPoint(touch.position));
            }

            if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isPressed)
            {
                ReleaseArrow();
            }
        }

        // Перемещение стрелы при удержании
        if (isPressed && shootRigid != null)
        {
            Vector2 inputPos;

            if (Input.touchCount > 0)
            {
                inputPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            else
            {
                inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            // Ограничение дистанции оттягивания
            if (Vector2.Distance(inputPos, shootRigid.position) > maxDistance)
            {
                arrowRigidbody.position = shootRigid.position +
                    (inputPos - shootRigid.position).normalized * maxDistance;
            }
            else
            {
                arrowRigidbody.position = inputPos;
            }

            // Вычисляем направление запуска для использования позже
            // ВАЖНО: направление ОТ стрелы К рогатке для правильного запуска
            releaseDirection = (shootRigid.position - arrowRigidbody.position).normalized;

            // Поворачиваем стрелу в направлении движения
            if ((arrowRigidbody.position - shootRigid.position).sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(releaseDirection.y, releaseDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90); // +90 чтобы острие стрелы смотрело вперед
            }
        }

        // Когда стрела летит, выравниваем её по направлению скорости
        if (!isPressed && arrowRigidbody.bodyType == RigidbodyType2D.Dynamic && arrowRigidbody.linearVelocity.sqrMagnitude > 0.1f && GetComponent<SpringJoint2D>() == null)
        {
            float angle = Mathf.Atan2(arrowRigidbody.linearVelocity.y, arrowRigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);  // -90 чтобы острие стрелы смотрело вперед
        }
    }

    private void CheckTouch(Vector2 touchPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (hit != null && hit.gameObject == gameObject && !wasLaunched)
        {
            isPressed = true;
            arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

            // Проигрываем звук натяжения
            PlayDrawSound();
        }
    }

    private void OnMouseDown()
    {
        if (!wasLaunched)
        {
            isPressed = true;
            arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

            // Проигрываем звук натяжения
            PlayDrawSound();
        }
    }

    private void ReleaseArrow()
    {
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (!isPressed) return;

        isPressed = false;
        arrowRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // Рассчитываем силу на основе расстояния от рогатки
        float distance = Vector2.Distance(arrowRigidbody.position, shootRigid.position);
        float forceMagnitude = distance * launchForce;

        // Отмечаем, что стрела выпущена до вызова метода Launch
        wasLaunched = true;

        // Вызываем метод интерфейса IAmmunition
        Launch(releaseDirection * forceMagnitude);
    }

    private void OnMouseUp()
    {
        if (isPressed)
        {
            ReleaseArrow();
        }
    }

    private void SpawnNewAmmo()
    {
        if (ammoPrefab != null && spawnPoint != null)
        {
            GameObject newAmmo = Instantiate(ammoPrefab, spawnPoint.position, Quaternion.identity);

            // Убедимся, что компоненты активны
            ArrowAmmunition newArrow = newAmmo.GetComponent<ArrowAmmunition>();
            if (newArrow != null)
            {
                newArrow.enabled = true;
            }

            SpringJoint2D newSpringJoint = newAmmo.GetComponent<SpringJoint2D>();
            if (newSpringJoint != null)
            {
                newSpringJoint.enabled = true;
            }
        }
        else
        {
            StartCoroutine(ReloadScene());
        }
    }

    private IEnumerator ReloadScene()
    {
        Debug.Log("эо ес че арроу");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Если стрела не была выстрелена, игнорируем столкновения
        if (!wasLaunched)
        {
            // Игнорируем это конкретное столкновение
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            return;
        }

        // Проверяем, имеет ли объект компонент IAmmunition
        IAmmunition target = collision.gameObject.GetComponent<IAmmunition>();

        // Проверка на попадание в молотов
        if (((1 << collision.gameObject.layer) & molotovLayer) != 0)
        {
            Debug.Log("Стрела попала в молотов!");

            if (arrowHitCondition != null)
            {
                arrowHitCondition.OnArrowHitMolotov();
            }
        }

        // Если объект не реализует IAmmunition, запускаем анимацию падения
        if (target == null)
        {
            // Запускаем анимацию падения
            StartCoroutine(FallAnimation());
        }
        else
        {
            // Если попали в объект IAmmunition, вызываем OnImpact() без застревания
            OnImpact();
        }
    }

    // Корутина для анимации падения стрелы
    private IEnumerator FallAnimation()
    {
        if (isFalling) yield break; // Если уже падаем, не запускаем повторно

        isFalling = true;

        // Останавливаем движение стрелы
        OnImpact();

        yield return new WaitForSeconds(0.2f);

        // Сохраняем текущий поворот
        originalRotation = transform.rotation;

        // Фаза 1: Наклон задней части (с перьями) вниз
        float elapsedTime = 0f;
        Vector3 startEuler = originalRotation.eulerAngles;
        Vector3 targetEuler = startEuler;
        targetEuler.z += tiltAngle; // Наклоняем вниз

        while (elapsedTime < tiltDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / tiltDuration;

            // Плавная интерполяция поворота
            Vector3 currentEuler = Vector3.Lerp(startEuler, targetEuler, t);
            transform.rotation = Quaternion.Euler(currentEuler);

            yield return null;
        }

        // Небольшая пауза перед падением
        yield return new WaitForSeconds(fallDelay);

        // Фаза 2: Падение всей стрелы
        arrowRigidbody.bodyType = RigidbodyType2D.Dynamic;
        arrowRigidbody.gravityScale = fallGravity;

        // Через некоторое время стрела может быть уничтожена или остановлена
        yield return new WaitForSeconds(2f);

        // Останавливаем падение
        arrowRigidbody.linearVelocity = Vector2.zero;
        arrowRigidbody.angularVelocity = 0f;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Launch(Vector2 force)
    {
        // Удаляем пружинное соединение
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
            Destroy(springJoint);
        }

        // Отмечаем, что стрела выстрелена
        wasLaunched = true;

        // Отключаем триггер, чтобы стрела могла нормально сталкиваться
        if (arrowCollider != null)
        {
            arrowCollider.isTrigger = false;
        }

        // Применяем силу
        arrowRigidbody.AddForce(force, ForceMode2D.Impulse);

        // После запуска отключаем скрипт для взаимодействия
        this.enabled = false;

        // Запускаем корутину для создания нового снаряда
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnNewAmmo();
        Destroy(gameObject, 5);
    }

    // Реализация интерфейса IAmmunition
    public void OnImpact()
    {
        // Остановка движения стрелы при столкновении
        arrowRigidbody.linearVelocity = Vector2.zero;
        arrowRigidbody.angularVelocity = 0f;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // Примечание: Мы не прикрепляем стрелу к объекту IAmmunition
    }

    private void PlayDrawSound()
    {
        if (drawSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(drawSound);
        }
    }

    // Реализация метода из интерфейса IProjectile
    public Transform GetTransform()
    {
        return transform;
    }

    // Реализация нового метода из интерфейса IProjectile
    public bool IsLaunched()
    {
        return wasLaunched;
    }

    // Метод для доступа к состоянию нажатия стрелы
    public bool IsPressed()
    {
        return isPressed;
    }
}