using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка выстрела
    public float fireRate = 5f; // Стрельба каждые 5 секунд

    [Header("Эффекты")]
    public GameObject fireEffectPrefab; // Префаб эффекта выстрела
    public float effectDuration = 1f; // Время жизни эффекта (если нужно принудительно удалить)

    [Header("Настройки здоровья")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Анимация")]
    public Animator animator;

    [Header("Поворот дула")]
    public Transform cannonBarrel; // Дуло пушки для поворота
    public float aimingDelay = 1f; // Задержка между прицеливанием и выстрелом

    private float nextFireTime = 0f;
    private Transform targetBase; // Цель для стрельбы
    private BulletPattern nextBulletPattern; // Следующий паттерн пули

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

        // Если дуло не назначено, используем firePoint
        if (cannonBarrel == null)
        {
            cannonBarrel = firePoint;
        }
    }

    void Update()
    {
        // Проверяем, можем ли стрелять
        if (Time.time >= nextFireTime && targetBase != null)
        {
            StartAiming();
            nextFireTime = Time.time + fireRate;
        }
    }

    void StartAiming()
    {
        // Определяем случайный паттерн пули заранее
        BulletPattern[] availablePatterns = { BulletPattern.Straight, BulletPattern.Arc, BulletPattern.ZigzagArc };
        nextBulletPattern = availablePatterns[Random.Range(0, availablePatterns.Length)];

        // Определяем нужный угол поворота дула в зависимости от паттерна
        float targetRotationZ = GetBarrelRotationForPattern(nextBulletPattern);

        // Сразу поворачиваем дуло (без анимации)
        cannonBarrel.localEulerAngles = new Vector3(0, 0, targetRotationZ);

        // Запускаем корутину для задержки перед выстрелом
        StartCoroutine(DelayedFire());
    }

    float GetBarrelRotationForPattern(BulletPattern pattern)
    {
        switch (pattern)
        {
            case BulletPattern.Straight:
                return 0f; // Прямо
            case BulletPattern.Arc:
                return -70f; // Вверх
            case BulletPattern.ZigzagArc:
                return -40f; // Зигзаг
            default:
                return 0f;
        }
    }

    System.Collections.IEnumerator DelayedFire()
    {
        // Задержка перед выстрелом (время для игрока чтобы понять куда будет выстрел)
        yield return new WaitForSeconds(aimingDelay);

        // Стреляем
        Fire();
    }

    void Fire()
    {
        // Запускаем анимацию выстрела
        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }

        // Создаем эффект выстрела
        CreateFireEffect();

        // Создаем пулю
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Передаем цель пуле и устанавливаем паттерн
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetBase);
                bulletScript.SetBulletPattern(nextBulletPattern);
            }
        }
    }

    void CreateFireEffect()
    {
        if (fireEffectPrefab != null && firePoint != null)
        {
            // Создаем эффект как дочерний объект firePoint
            GameObject effect = Instantiate(fireEffectPrefab, firePoint.position, firePoint.rotation, firePoint);

            // Проверяем, есть ли у эффекта аниматор для автоматического удаления
            Animator effectAnimator = effect.GetComponent<Animator>();
            if (effectAnimator != null)
            {
                // Получаем длительность анимации
                AnimationClip[] clips = effectAnimator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    float animationLength = clips[0].length;
                    Destroy(effect, animationLength);
                }
                else
                {
                    // Если не удалось получить длительность, удаляем через заданное время
                    Destroy(effect, effectDuration);
                }
            }
            else
            {
                // Если нет аниматора, удаляем через заданное время
                Destroy(effect, effectDuration);
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