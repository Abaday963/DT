using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка выстрела
    public float fireRate = 5f; // Стрельба каждые 5 секунд

    [Header("Настройки здоровья")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Анимация")]
    public Animator animator;

    private float nextFireTime = 0f;
    private Transform targetBase; // Цель для стрельбы

    void Start()
    {
        currentHealth = maxHealth;

        // Находим базу игрока по тегу
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        if (playerBase != null)
        {
            targetBase = playerBase.transform;
        }

        // Если нет аниматора, пытаемся найти его
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Проверяем, можем ли стрелять
        if (Time.time >= nextFireTime && targetBase != null)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        // Запускаем анимацию выстрела
        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }

        // Создаем пулю
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Передаем цель пуле
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetBase);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Логика смерти пушки
        Destroy(gameObject);
    }

    // Метод для анимационного события (вызывается из анимации)
    public void OnFireAnimationComplete()
    {
        // Здесь можно добавить дополнительную логику после завершения анимации
        Debug.Log("Анимация выстрела завершена");
    }
}