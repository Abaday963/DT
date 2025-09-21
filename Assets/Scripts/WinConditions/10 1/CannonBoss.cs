using UnityEngine;
using System.Collections;

public class CannonBoss : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка выстрела

    [Header("Первая фаза")]
    public float phase1FireRate = 1.5f; // Быстрая стрельба в первой фазе
    public float phase1BigDelayRate = 5f; // Большая задержка после каждого 3-го выстрела

    [Header("Вторая фаза")]
    public float phase2AttackCooldown = 3f; // Время между атаками во второй фазе
    public float phase2PlayerAttackWindow = 2f; // Окно для атак игрока

    [Header("Эффекты")]
    public GameObject fireEffectPrefab; // Префаб эффекта выстрела
    public float effectDuration = 1f; // Время жизни эффекта

    [Header("Звук")]
    public AudioSource audioSource; // Аудио источник
    public AudioClip fireSound; // Звук выстрела

    [Header("Настройки здоровья")]
    public int maxHealth = 16; // 16 попаданий для уничтожения
    private int currentHealth;
    private bool isPhase2 = false;

    [Header("Анимация")]
    public Animator animator;

    [Header("Поворот дула")]
    public Transform cannonBarrel; // Дуло пушки для поворота
    public float aimingDelay = 0.5f; // Уменьшенная задержка для быстрых атак

    private float nextFireTime = 0f;
    private Transform targetBase; // Цель для стрельбы
    private BulletPattern nextBulletPattern; // Следующий паттерн пули

    // Переменные для первой фазы
    private int shotsInCurrentBurst = 0;

    // Переменные для второй фазы
    private bool isExecutingAttackPattern = false;

    // Enum для новых паттернов второй фазы
    public enum Phase2AttackPattern
    {
        QuickArcQuick,        // Быстрая -> дуговая -> быстрая
        CirclingCombination,  // Кружащая -> быстрая -> кружащая  
        ComplexSequence       // Медленная дребезжащая -> прямая дребезжащая -> быстрая дуговая
    }

    void Start()
    {
        currentHealth = maxHealth;
        // Находим базу игрока по тегу
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        if (playerBase != null)
        {
            targetBase = playerBase.transform;
        }

        // Инициализация компонентов
        InitializeComponents();
    }

    void InitializeComponents()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (cannonBarrel == null)
            cannonBarrel = firePoint;
    }

    void Update()
    {
        if (targetBase == null) return;

        if (!isPhase2)
        {
            HandlePhase1();
        }
        else
        {
            HandlePhase2();
        }
    }

    void HandlePhase1()
    {
        if (Time.time >= nextFireTime && !isExecutingAttackPattern)
        {
            StartCoroutine(Phase1Attack());
        }
    }

    void HandlePhase2()
    {
        if (Time.time >= nextFireTime && !isExecutingAttackPattern)
        {
            StartCoroutine(ExecutePhase2Attack());
        }
    }

    IEnumerator Phase1Attack()
    {
        isExecutingAttackPattern = true;
        shotsInCurrentBurst++;

        // Определяем паттерн для первой фазы
        BulletPattern[] phase1Patterns = { BulletPattern.Straight, BulletPattern.Arc, BulletPattern.ZigzagArc };
        nextBulletPattern = phase1Patterns[Random.Range(0, phase1Patterns.Length)];

        // Быстрое прицеливание и выстрел
        yield return StartCoroutine(AimAndFire(0.3f)); // Быстрое прицеливание

        // Проверяем, нужна ли большая задержка после каждого 3-го выстрела
        if (shotsInCurrentBurst >= 3)
        {
            shotsInCurrentBurst = 0;
            nextFireTime = Time.time + phase1BigDelayRate; // 5-секундная задержка
        }
        else
        {
            nextFireTime = Time.time + phase1FireRate; // Обычная задержка
        }

        isExecutingAttackPattern = false;
    }

    IEnumerator ExecutePhase2Attack()
    {
        isExecutingAttackPattern = true;

        // Выбираем случайный паттерн атаки для второй фазы
        Phase2AttackPattern attackPattern = (Phase2AttackPattern)Random.Range(0, 3);

        switch (attackPattern)
        {
            case Phase2AttackPattern.QuickArcQuick:
                yield return StartCoroutine(QuickArcQuickAttack());
                break;
            case Phase2AttackPattern.CirclingCombination:
                yield return StartCoroutine(CirclingCombinationAttack());
                break;
            case Phase2AttackPattern.ComplexSequence:
                yield return StartCoroutine(ComplexSequenceAttack());
                break;
        }

        // Окно для атак игрока
        yield return new WaitForSeconds(phase2PlayerAttackWindow);

        nextFireTime = Time.time + phase2AttackCooldown;
        isExecutingAttackPattern = false;
    }

    // Паттерн 1: Быстрая -> длинная дуговая -> быстрая
    IEnumerator QuickArcQuickAttack()
    {
        // Быстрая атака
        nextBulletPattern = BulletPattern.Straight;
        yield return StartCoroutine(AimAndFire(0.2f, 1.5f)); // Быстрая пуля

        yield return new WaitForSeconds(0.3f);

        // Длинная дуговая
        nextBulletPattern = BulletPattern.Arc;
        yield return StartCoroutine(AimAndFire(0.5f, 0.8f)); // Медленная дуговая

        yield return new WaitForSeconds(0.3f);

        // Вторая быстрая
        nextBulletPattern = BulletPattern.Straight;
        yield return StartCoroutine(AimAndFire(0.2f, 1.5f));
    }

    // Паттерн 2: Кружащая -> быстрая -> кружащая
    IEnumerator CirclingCombinationAttack()
    {
        // Кружащая средняя с сильным дребезжанием
        nextBulletPattern = BulletPattern.ZigzagArc;
        yield return StartCoroutine(AimAndFire(0.4f, 1.0f, 2.0f)); // Среднее время, сильное дребезжание

        yield return new WaitForSeconds(0.8f); // Средное ожидание

        // Быстрая атака
        nextBulletPattern = BulletPattern.Straight;
        yield return StartCoroutine(AimAndFire(0.2f, 1.5f));

        yield return new WaitForSeconds(0.3f);

        // Еще одна кружащая
        nextBulletPattern = BulletPattern.ZigzagArc;
        yield return StartCoroutine(AimAndFire(0.4f, 1.0f, 2.0f));
    }

    // Паттерн 3: Медленная дребезжащая -> прямая дребезжащая -> быстрая дуговая
    IEnumerator ComplexSequenceAttack()
    {
        // Медленная дребезжащаяся средняя
        nextBulletPattern = BulletPattern.ZigzagArc;
        yield return StartCoroutine(AimAndFire(0.6f, 0.7f, 1.5f)); // Медленная с дребезжанием

        yield return new WaitForSeconds(0.4f);

        // Прямая дребезжащаяся (чуть медленнее обычной)
        nextBulletPattern = BulletPattern.Straight; // Будем использовать зигзаг но с прямым углом
        yield return StartCoroutine(AimAndFireZigzagStraight(0.5f, 0.9f, 1.2f));

        yield return new WaitForSeconds(0.2f);

        // Высокая дуговая очень быстрая
        nextBulletPattern = BulletPattern.Arc;
        yield return StartCoroutine(AimAndFire(0.1f, 2.0f)); // Очень быстрая
    }

    IEnumerator AimAndFire(float aimTime, float bulletSpeedMultiplier = 1.0f, float zigzagIntensity = 1.0f)
    {
        // Поворачиваем дуло
        float targetRotationZ = GetBarrelRotationForPattern(nextBulletPattern);
        cannonBarrel.localEulerAngles = new Vector3(0, 0, targetRotationZ);

        // Задержка прицеливания
        yield return new WaitForSeconds(aimTime);

        // Стреляем
        Fire(bulletSpeedMultiplier, zigzagIntensity);
    }

    // Специальный метод для прямого зигзага
    IEnumerator AimAndFireZigzagStraight(float aimTime, float bulletSpeedMultiplier, float zigzagIntensity)
    {
        // Поворачиваем дуло прямо (0 градусов)
        cannonBarrel.localEulerAngles = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(aimTime);

        // Стреляем зигзагом но прямо
        nextBulletPattern = BulletPattern.ZigzagArc;
        Fire(bulletSpeedMultiplier, zigzagIntensity);
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

    void Fire(float speedMultiplier = 1.0f, float zigzagIntensity = 1.0f)
    {
        // Воспроизводим звук выстрела
        PlayFireSound();

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
            EnemyBulletBoss bulletScript = bullet.GetComponent<EnemyBulletBoss>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetBase);
                bulletScript.SetBulletPattern(nextBulletPattern);

                // Устанавливаем модификаторы скорости и интенсивности зигзага
                bulletScript.SetSpeedMultiplier(speedMultiplier);
                bulletScript.SetZigzagIntensity(zigzagIntensity);
            }
        }
    }

    void PlayFireSound()
    {
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    void CreateFireEffect()
    {
        if (fireEffectPrefab != null && firePoint != null)
        {
            GameObject effect = Instantiate(fireEffectPrefab, firePoint.position, firePoint.rotation, firePoint);

            Animator effectAnimator = effect.GetComponent<Animator>();
            if (effectAnimator != null)
            {
                AnimationClip[] clips = effectAnimator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    float animationLength = clips[0].length;
                    Destroy(effect, animationLength);
                }
                else
                {
                    Destroy(effect, effectDuration);
                }
            }
            else
            {
                Destroy(effect, effectDuration);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire shard"))
        {
            TakeDamage(1);
            Destroy(other.gameObject); // Уничтожаем снаряд игрока
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Проверяем переход во вторую фазу (ровно половина здоровья)
        if (currentHealth == maxHealth / 2 && !isPhase2)
        {
            EnterPhase2();
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log($"Boss Health: {currentHealth}/{maxHealth}");
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("Босс переходит во вторую фазу!");

        // Сбрасываем текущие атаки
        StopAllCoroutines();
        isExecutingAttackPattern = false;
        nextFireTime = Time.time + 1f; // Небольшая задержка перед началом второй фазы

        // Можно добавить визуальные/звуковые эффекты перехода фазы
        if (animator != null)
        {
            animator.SetTrigger("Phase2Start");
        }
    }

    void Die()
    {
        Debug.Log("Босс побежден!");
        // Логика смерти босса - можно добавить эффекты, награды и т.д.
        Destroy(gameObject);
    }

    // Метод для анимационного события
    public void OnFireAnimationComplete()
    {
        Debug.Log("Анимация выстрела завершена");
    }

    // Дополнительные методы для отладки
    void OnDrawGizmosSelected()
    {
        // Показываем здоровье босса в Scene view
        if (Application.isPlaying)
        {
            Gizmos.color = isPhase2 ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.5f);
        }
    }
}