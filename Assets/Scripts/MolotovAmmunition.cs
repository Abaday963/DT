using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MolotovAmmunition : MonoBehaviour, IAmmunition, IProjectile
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
    //[SerializeField] private Animator animator;

    [Header("Аудио")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip releaseSound; // Звук при отпускании
    [SerializeField] private AudioClip flyingSound; // Звук через полсекунды после запуска
    public AudioMixerGroup masterMixerGroup;


    private bool hasExploded = false; // Флаг для отслеживания взрыва
    private bool isLaunched = false; // Флаг для отслеживания запуска
    private MolotovSpawner molotovSpawner;

    // Ссылка на менеджер боеприпасов
    private AmmunitionManager ammunitionManager;

    private void Start()
    {
        ammoRigidbody = GetComponent<Rigidbody2D>();

        // Получаем или добавляем компонент AudioSource
        audioSource = GetComponent<AudioSource>();

        //if (animator == null)
        //{
        //    animator = GetComponent<Animator>();
        //}

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

        isLaunched = true;

        // Звук при отпускании
        if (releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }

        // Звук полета через 0.5 секунды
        if (flyingSound != null)
        {
            StartCoroutine(PlayFlyingSoundDelayed(0.4f));
        }

        // Сообщаем менеджеру (оригинальная система)
        if (ammunitionManager != null)
        {
            ammunitionManager.OnMolotovLaunched();
        }

        // Сообщаем спавнеру (новая система)
        if (molotovSpawner != null)
        {
            molotovSpawner.OnMolotovLaunched();
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
        if (hasExploded)
            return;

        hasExploded = true;

        // Проигрываем взрывной звук на отдельном объекте, чтобы не обрывался
        if (explosionSound != null)
        {
            GameObject audioObj = new GameObject("ExplosionSound");
            AudioSource tempAudio = audioObj.AddComponent<AudioSource>();
            tempAudio.clip = explosionSound;
            tempAudio.outputAudioMixerGroup = masterMixerGroup; // <-- Устанавливаем группу
            tempAudio.Play();
            Destroy(audioObj, explosionSound.length);
        }

        // Создаем визуальный эффект взрыва
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDuration);

            // Создаем осколки огня
            SpawnFireShards();

            // Отключаем физику и коллайдер
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }

            // Выключаем визуал объекта
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            // Уничтожаем объект (можно быстро, т.к. звук живет отдельно)
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
    private IEnumerator PlayFlyingSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isLaunched && !hasExploded && flyingSound != null)
        {
            audioSource.PlayOneShot(flyingSound);
        }
    }
    public void SetSpawner(MolotovSpawner spawner)
    {
        molotovSpawner = spawner;
    }
    public Transform GetTransform()
    {
        return transform;
    }
    public bool IsLaunched()
    {
        return isLaunched;
    }
    public bool IsPressed()
    {
        return isPressed;
    }
}