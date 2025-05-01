using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowAmmunition : MonoBehaviour, IAmmunition
{
    [SerializeField] private bool isPressed = false;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float launchForce = 10f; // Множитель силы
    [SerializeField] private Rigidbody2D shootRigid; // Ссылка на рогатку
    [SerializeField] public GameObject ammoPrefab; // Префаб этого же боеприпаса
    [SerializeField] public Transform spawnPoint; // Точка появления нового боеприпаса

    private Vector2 startPosition;
    private Vector2 releaseDirection;
    private Rigidbody2D arrowRigidbody;

    private void Start()
    {
        arrowRigidbody = GetComponent<Rigidbody2D>();

        // Сохраняем начальную позицию
        startPosition = transform.position;

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
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
        if (Input.GetMouseButtonDown(0) && !isPressed)
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

            if (touch.phase == TouchPhase.Began && !isPressed)
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

        if (hit != null && hit.gameObject == gameObject)
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

        // Вызываем метод интерфейса IAmmunition
        Launch(releaseDirection * forceMagnitude);
    }

    private IEnumerator LaunchWithForce(float force)
    {
        // Удаляем пружинное соединение
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
            Destroy(springJoint);
        }

        // Применяем силу - БЕЗ знака минус!
        arrowRigidbody.AddForce(releaseDirection * force, ForceMode2D.Impulse);

        this.enabled = false;

        yield return new WaitForSeconds(2);

        // Создаем новый снаряд
        SpawnNewAmmo();

        Destroy(gameObject, 5);
    }

    private void OnMouseDown()
    {
        isPressed = true;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
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
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, является ли столкнувшийся объект боеприпасом
        IAmmunition ammunitionComponent = collision.gameObject.GetComponent<IAmmunition>();

        // Если объект - боеприпас, то игнорируем столкновение
        if (ammunitionComponent != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            return;
        }

        // Вызываем метод интерфейса IAmmunition
        OnImpact();

    }

    // Реализация интерфейса IAmmunition
    public void Launch(Vector2 force)
    {
        // Удаляем пружинное соединение
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
            Destroy(springJoint);
        }

        // Применяем силу
        arrowRigidbody.AddForce(force, ForceMode2D.Impulse);
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

        // Прикрепляем стрелу к объекту, в который она попала
        if (transform.parent == null) // Только если еще не прикреплена
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
            foreach (Collider2D col in colliders)
            {
                if (col.gameObject != gameObject)
                {
                    transform.parent = col.transform;
                    break;
                }
            }
        }
    }
}