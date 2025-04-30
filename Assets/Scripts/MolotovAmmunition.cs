using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MolotovAmmunition : MonoBehaviour, IAmmunition
{
    [SerializeField] private Rigidbody2D ammoRigidbody;
    [SerializeField] private bool isPressed = false;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private Rigidbody2D shootRigid; // Ссылка на рогатку

    // Компоненты взрыва
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 3f;

    // Осколки огня
    [SerializeField] private GameObject fireShardPrefab; // Префаб осколка огня
    [SerializeField] private int fireShardCount = 5; // Количество осколков
    [SerializeField] private float fireShardDistance = 1.2f; // Расстояние между осколками
    [SerializeField] private float fireShardForce = 5f; // Сила, с которой осколки полетят вниз
    [SerializeField] private float fireShardLifetime = 3f; // Время жизни осколков

    // Аниматор для переключения анимаций
    [SerializeField] private Animator animator;

    private bool hasExploded = false; // Флаг для отслеживания взрыва
    private bool isLaunched = false; // Флаг для отслеживания запуска

    // Ссылка на менеджер боеприпасов
    private AmmunitionManager ammunitionManager;

    private void Start()
    {
        ammoRigidbody = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }

        // Если не назначена рогатка, найдем её автоматически по тегу FirstSlingshot
        if (shootRigid == null)
        {
            GameObject slingshot = GameObject.FindGameObjectWithTag("FirstSlingshot");
            if (slingshot != null)
            {
                shootRigid = slingshot.GetComponent<Rigidbody2D>();
                if (shootRigid == null)
                {
                    Debug.LogError("На объекте с тегом 'FirstSlingshot' не найден Rigidbody2D!");
                }
            }
            else
            {
                Debug.LogError("Не найден объект с тегом 'FirstSlingshot' в сцене!");
            }
        }

        // Если не назначен менеджер боеприпасов, найдем его
        if (ammunitionManager == null)
        {
            ammunitionManager = AmmunitionManager.Instance;
        }
    }

    // Метод для установки менеджера боеприпасов извне
    public void SetAmmunitionManager(AmmunitionManager manager)
    {
        ammunitionManager = manager;
    }

    private void Update()
    {
        // Если снаряд уже запущен, не обрабатываем пользовательский ввод
        if (isLaunched)
            return;

        // Обработка PC ввода
        if (Input.GetMouseButtonDown(0) && !isPressed)
        {
            CheckTouch(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            ReleaseAmmo();
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
                ReleaseAmmo();
            }
        }

        // Перемещение снаряда при удержании
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
                ammoRigidbody.position = shootRigid.position +
                    (inputPos - shootRigid.position).normalized * maxDistance;
            }
            else
            {
                ammoRigidbody.position = inputPos;
            }
        }
    }

    private void CheckTouch(Vector2 touchPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (hit != null && hit.gameObject == gameObject)
        {
            isPressed = true;
            ammoRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void ReleaseAmmo()
    {
        if (isPressed)
        {
            isPressed = false;
            ammoRigidbody.bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(Release());
        }
    }

    private void OnMouseDown()
    {
        if (!isLaunched)
        {
            isPressed = true;
            ammoRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnMouseUp()
    {
        if (isPressed && !isLaunched)
        {
            ReleaseAmmo();
        }
    }

    private IEnumerator Release()
    {
        yield return new WaitForSeconds(0.1f);

        SpringJoint2D springJoint = gameObject.GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
        }

        isLaunched = true; // Помечаем снаряд как запущенный

        // Сообщаем менеджеру о запуске боеприпаса
        if (ammunitionManager != null)
        {
            ammunitionManager.OnMolotovLaunched();
        }
        else
        {
            Debug.LogWarning("AmmunitionManager не найден!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что бутылка запущена и еще не взорвалась
        if (isLaunched && !isPressed && !hasExploded)
        {
            OnImpact();
        }
    }

    // Реализация интерфейса IAmmunition
    public void Launch(Vector2 force)
    {
        // Отключаем пружинное соединение
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
        }

        // Применяем силу к снаряду
        ammoRigidbody.bodyType = RigidbodyType2D.Dynamic;
        ammoRigidbody.AddForce(force, ForceMode2D.Impulse);
        isLaunched = true; // Помечаем снаряд как запущенный

        // Сообщаем менеджеру о запуске боеприпаса
        if (ammunitionManager != null)
        {
            ammunitionManager.OnMolotovLaunched();
        }
    }

    // Реализация интерфейса IAmmunition
    public void OnImpact()
    {
        // Проверяем, что взрыв еще не произошел
        if (hasExploded)
            return;

        hasExploded = true; // Помечаем, что взрыв произошел

        // Отключаем анимацию Idle и включаем анимацию взрыва
        if (animator != null)
        {
            animator.SetTrigger("Explode"); // Предполагается, что в аниматоре есть триггер "Explode"
        }

        // Создаем взрыв при столкновении с объектами
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDuration);

            Debug.Log("Взрыв молотова");
            // Создаем осколки огня
            SpawnFireShards();

            // Отключаем коллизии после взрыва
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }

            // Делаем объект невидимым, но не уничтожаем сразу,
            // чтобы дать время эффектам взрыва отработать
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            // Уничтожаем объект через небольшую задержку
            Destroy(gameObject, 0.04f);
        }
    }

    // Создание осколков огня
    private void SpawnFireShards()
    {
        if (fireShardPrefab != null)
        {
            // Вычисляем общую ширину линии осколков
            float totalWidth = fireShardDistance * (fireShardCount - 1);

            // Начальная позиция (левый край линии)
            Vector2 startPos = (Vector2)transform.position - new Vector2(totalWidth / 2, 0);

            // Создаем осколки в линию
            for (int i = 0; i < fireShardCount; i++)
            {
                // Вычисляем позицию для текущего осколка
                Vector2 shardPos = startPos + new Vector2(fireShardDistance * i, 0);

                // Создаем осколок
                GameObject shard = Instantiate(fireShardPrefab, shardPos, Quaternion.identity);

                // Добавляем осколку физику, если её нет
                Rigidbody2D shardRb = shard.GetComponent<Rigidbody2D>();
                if (shardRb == null)
                {
                    shardRb = shard.AddComponent<Rigidbody2D>();
                }

                // Прикладываем силу вниз
                shardRb.AddForce(Vector2.down * fireShardForce, ForceMode2D.Impulse);

                // Устанавливаем время жизни осколка
                Destroy(shard, fireShardLifetime);
            }
        }
    }
}