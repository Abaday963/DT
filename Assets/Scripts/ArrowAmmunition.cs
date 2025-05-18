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

    private Vector2 startPosition;
    private Vector2 releaseDirection;
    private Rigidbody2D arrowRigidbody;
    private bool wasLaunched = false; // Флаг, указывающий, что стрела была выстрелена
    private Collider2D arrowCollider; // Ссылка на коллайдер стрелы

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
        }
    }

    private void ReleaseArrow()
    {
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

    private void OnMouseDown()
    {
        if (!wasLaunched)
        {
            isPressed = true;
            arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
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

        Debug.Log("Стрела попала !!!!!!!!!");

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

        // Если объект не реализует IAmmunition, застреваем в нем
        if (target == null)
        {
            // Застреваем в объекте
            StickToObject(collision);
        }
        else
        {
            // Если попали в объект IAmmunition, вызываем OnImpact() без застревания
            OnImpact();
        }
    }

    // Новый метод для застревания стрелы в объектах
    private void StickToObject(Collision2D collision)
    {
        // Остановка движения стрелы при столкновении
        arrowRigidbody.linearVelocity = Vector2.zero;
        arrowRigidbody.angularVelocity = 0f;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

        Debug.Log("Стрела застряла в объекте: " + collision.gameObject.name);
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
        yield return new WaitForSeconds(2);
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