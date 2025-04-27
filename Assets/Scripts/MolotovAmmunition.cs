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
    [SerializeField] public GameObject ammoPrefab; // Префаб этого же боеприпаса
    [SerializeField] public Transform spawnPoint; // Точка появления нового боеприпаса

    // Компоненты взрыва
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDuration = 3f;
    [SerializeField] private float explosionDamage = 10f; // Урон от взрыва

    private void Start()
    {
        ammoRigidbody = GetComponent<Rigidbody2D>();

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
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
        isPressed = false;
        ammoRigidbody.bodyType = RigidbodyType2D.Dynamic;
        StartCoroutine(Release());
    }

    private void OnMouseDown()
    {
        isPressed = true;
        ammoRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnMouseUp()
    {
        if (isPressed)
        {
            isPressed = false;
            ammoRigidbody.bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(Release());
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

        this.enabled = false;

        yield return new WaitForSeconds(2);

        // Создаем новый снаряд
        SpawnNewAmmo();

        Destroy(gameObject, 5);
    }

    private void SpawnNewAmmo()
    {
        if (ammoPrefab != null && spawnPoint != null)
        {
            GameObject newAmmo = Instantiate(ammoPrefab, spawnPoint.position, Quaternion.identity);

            // Убедимся, что компоненты активны
            MolotovAmmunition newMolotov = newAmmo.GetComponent<MolotovAmmunition>();
            if (newMolotov != null)
            {
                newMolotov.enabled = true;
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
        if (!isPressed)
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
        this.enabled = false;
    }

    // Реализация интерфейса IAmmunition
    public void OnImpact()
    {
        // Создаем взрыв при столкновении с объектами
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDuration);

            // Обнаруживаем объекты в радиусе взрыва
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (Collider2D nearbyObject in colliders)
            {
                // Наносим урон объектам с интерфейсом IDamageable--------------------------------------------
                IDamageable damageable = nearbyObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosionDamage);
                }

                // Добавляем силу, чтобы оттолкнуть объекты от взрыва
                Rigidbody2D rb = nearbyObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = (nearbyObject.transform.position - transform.position).normalized;
                    rb.AddForce(direction * 10f, ForceMode2D.Impulse);
                }
            }

            // Отключаем коллизии после взрыва
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }
        }
    }
}
