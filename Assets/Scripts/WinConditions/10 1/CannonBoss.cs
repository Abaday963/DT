using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CannonBoss : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; // Префаб пули босса
    public Transform firePoint; // Точка выстрела

    [Header("Фаза 1 - настройки")]
    public float phase1FireRate = 1.5f; // Быстрая стрельба в первой фазе
    public float phase1BigDelayEvery = 3; // Каждый третий выстрел
    public float phase1BigDelay = 5f; // Большая задержка после каждого третьего

    [Header("Фаза 2 - настройки")]
    public float phase2DelayBetweenAttacks = 3f; // Задержка между атаками во второй фазе

    [Header("Эффекты")]
    public GameObject fireEffectPrefab; // Префаб эффекта выстрела
    public float effectDuration = 1f; // Время жизни эффекта

    [Header("Звук")]
    public AudioSource audioSource; // Аудио источник
    public AudioClip fireSound; // Звук выстрела

    [Header("Настройки здоровья")]
    public int maxHealth = 16; // 16 попаданий для победы
    private int currentHealth;

    [Header("UI Здоровья")]
    public Slider healthSlider; // Слайдер для отображения HP
    public GameObject healthBarCanvas; // Canvas с полоской HP (опционально)

    [Header("Анимация")]
    public Animator animator;

    [Header("Поворот дула")]
    public Transform cannonBarrel; // Дуло пушки для поворота
    public float aimingDelay = 0.5f; // Задержка между прицеливанием и выстрелом

    [Header("Переход фаз")]
    public float phaseTransitionDelay = 2f; // Задержка при переходе фаз
    public Color phase2Color = Color.red; // Цвет спрайта во второй фазе
    public float colorTransitionDuration = 1f; // Время изменения цвета

    // Ссылки на компоненты
    private CameraShake cameraShake;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // Исходный цвет спрайта

    // Состояние босса
    private enum BossPhase { Phase1, Phase2 }
    private BossPhase currentPhase = BossPhase.Phase1;
    private bool isTransitioningPhase = false; // Флаг перехода фаз

    private float nextFireTime = 0f;
    private Transform targetBase; // Цель для стрельбы
    private BulletPattern nextBulletPattern;

    // Счетчики для первой фазы
    private int shotCounter = 0;

    // Паттерны атак для второй фазы
    private enum Phase2AttackPattern { FastCombo, CirclingCombo, ComplexCombo }

    void Start()
    {
        currentHealth = maxHealth;
        InitializeHealthUI();

        // Находим базу игрока по тегу
        GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");
        if (playerBase != null)
        {
            targetBase = playerBase.transform;
        }

        // Инициализация компонентов
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (cannonBarrel == null)
            cannonBarrel = firePoint;

        // Инициализация новых компонентов
        InitializeNewComponents();

        Debug.Log("Босс запущен! Фаза 1 активна");
    }

    void InitializeNewComponents()
    {
        // Найти компонент CameraShake
        cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake == null)
        {
            Debug.LogWarning("CameraShake компонент не найден! Добавьте его на камеру.");
        }

        // Получить SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer не найден! Изменение цвета не будет работать.");
        }
    }

    void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Показываем UI здоровья при появлении босса
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // Плавное обновление слайдера
            StartCoroutine(SmoothUpdateHealthSlider());
        }
    }

    IEnumerator SmoothUpdateHealthSlider()
    {
        float targetValue = currentHealth; // Используем абсолютное значение
        float currentValue = healthSlider.value;
        float duration = 0.3f; // Время анимации
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            healthSlider.value = Mathf.Lerp(currentValue, targetValue, t);
            yield return null;
        }

        healthSlider.value = targetValue;
    }

    void Update()
    {
        if (targetBase == null || isTransitioningPhase) return;

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                HandlePhase1();
                break;
            case BossPhase.Phase2:
                HandlePhase2();
                break;
        }
    }

    void HandlePhase1()
    {
        if (Time.time >= nextFireTime)
        {
            shotCounter++;

            // Каждый третий выстрел - большая задержка
            if (shotCounter % phase1BigDelayEvery == 0)
            {
                StartCoroutine(Phase1BigDelayAttack());
                nextFireTime = Time.time + phase1BigDelay;
            }
            else
            {
                // Обычная быстрая атака
                StartAimingPhase1();
                nextFireTime = Time.time + phase1FireRate;
            }
        }
    }

    void HandlePhase2()
    {
        if (Time.time >= nextFireTime)
        {
            StartCoroutine(ExecutePhase2Attack());
            nextFireTime = Time.time + phase2DelayBetweenAttacks;
        }
    }

    IEnumerator Phase1BigDelayAttack()
    {
        Debug.Log("Большая задержка после 3-го выстрела!");
        StartAimingPhase1();
        yield return new WaitForSeconds(phase1BigDelay);
    }

    void StartAimingPhase1()
    {
        // В первой фазе используем базовые паттерны
        BulletPattern[] phase1Patterns = { BulletPattern.Straight, BulletPattern.Arc, BulletPattern.ZigzagArc };
        nextBulletPattern = phase1Patterns[Random.Range(0, phase1Patterns.Length)];

        float targetRotationZ = GetBarrelRotationForPattern(nextBulletPattern);
        cannonBarrel.localEulerAngles = new Vector3(0, 0, targetRotationZ);

        StartCoroutine(DelayedFire());
    }

    IEnumerator ExecutePhase2Attack()
    {
        Phase2AttackPattern attackPattern = (Phase2AttackPattern)Random.Range(0, 3);
        Debug.Log($"Фаза 2: Выполняется атака паттерн {attackPattern}");

        switch (attackPattern)
        {
            case Phase2AttackPattern.FastCombo:
                yield return StartCoroutine(FastComboAttack());
                break;
            case Phase2AttackPattern.CirclingCombo:
                yield return StartCoroutine(CirclingComboAttack());
                break;
            case Phase2AttackPattern.ComplexCombo:
                yield return StartCoroutine(ComplexComboAttack());
                break;
        }
    }

    // Паттерн 1: Быстрая прямая + длинная дуга + быстрая прямая + пауза
    IEnumerator FastComboAttack()
    {
        Debug.Log("Фаза 2: Быстрая комбо атака!");

        // Быстрая прямая
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.Straight, 12f, 0.2f));
        yield return new WaitForSeconds(0.3f);

        // Длинная дуговая
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.Arc, 6f, 0.4f));
        yield return new WaitForSeconds(0.5f);

        // Вторая быстрая прямая
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.Straight, 12f, 0.2f));

        Debug.Log("Быстрая комбо завершена - окно для атаки игрока!");
    }

    // Паттерн 2: Кружащая + пауза + быстрая + кружащая
    IEnumerator CirclingComboAttack()
    {
        Debug.Log("Фаза 2: Кружащая комбо атака!");

        // Кружащая средняя с сильным дребезжанием
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.ZigzagArc, 7f, 0.5f, 2f, 4f)); // Увеличенная амплитуда и частота
        yield return new WaitForSeconds(1f); // Среднее ожидание

        // Быстрая атака
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.Straight, 10f, 0.3f));
        yield return new WaitForSeconds(0.4f);

        // Еще одна кружащая
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.ZigzagArc, 7f, 0.5f, 2f, 4f));
    }

    // Паттерн 3: Сложная комбо (средняя медленная дребезжащая + дребезжащая прямая + быстрая дуга)
    IEnumerator ComplexComboAttack()
    {
        Debug.Log("Фаза 2: Сложная комбо атака!");

        // Средняя дребезжащая медленная
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.ZigzagArc, 4f, 0.8f, 1.5f, 3f));
        yield return new WaitForSeconds(0.3f);

        // Дребезжащая прямая чуть медленнее
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.ZigzagStraight, 6f, 0.6f, 1f, 2.5f));
        yield return new WaitForSeconds(0.2f);

        // Высокая дуговая очень быстрая
        yield return StartCoroutine(FireBulletWithPattern(BulletPattern.Arc, 15f, 0.2f, 0f, 0f, 4f)); // Увеличенная высота дуги
    }

    IEnumerator FireBulletWithPattern(BulletPattern pattern, float speed, float delay, float zigzagAmplitude = 1f, float zigzagFreq = 2f, float arcHeight = 2f)
    {
        // Поворачиваем дуло
        float targetRotationZ = GetBarrelRotationForPattern(pattern);
        cannonBarrel.localEulerAngles = new Vector3(0, 0, targetRotationZ);

        yield return new WaitForSeconds(delay);

        // Стреляем
        Fire(pattern, speed, zigzagAmplitude, zigzagFreq, arcHeight);
    }

    float GetBarrelRotationForPattern(BulletPattern pattern)
    {
        switch (pattern)
        {
            case BulletPattern.Straight:
                return 0f;
            case BulletPattern.Arc:
                return -70f;
            case BulletPattern.ZigzagArc:
                return -40f;
            case BulletPattern.ZigzagStraight:
                return 0f;
            default:
                return 0f;
        }
    }

    System.Collections.IEnumerator DelayedFire()
    {
        yield return new WaitForSeconds(aimingDelay);
        Fire();
    }

    void Fire()
    {
        // Обычная стрельба для фазы 1
        PlayFireSound();

        if (animator != null)
            animator.SetTrigger("Fire");

        CreateFireEffect();

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            EnemyBulletBoss bulletScript = bullet.GetComponent<EnemyBulletBoss>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetBase);
                bulletScript.SetBulletPattern(nextBulletPattern);

                // В первой фазе пули быстрее
                if (currentPhase == BossPhase.Phase1)
                {
                    bulletScript.SetSpeed(10f); // Увеличенная скорость для фазы 1
                }
            }
        }
    }

    void Fire(BulletPattern pattern, float speed, float zigzagAmplitude = 1f, float zigzagFreq = 2f, float arcHeight = 2f)
    {
        // Продвинутая стрельба для фазы 2
        PlayFireSound();

        if (animator != null)
            animator.SetTrigger("Fire");

        CreateFireEffect();

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            EnemyBulletBoss bulletScript = bullet.GetComponent<EnemyBulletBoss>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetBase);
                bulletScript.SetBulletPattern(pattern);
                bulletScript.SetSpeed(speed);
                bulletScript.SetZigzagSettings(zigzagAmplitude, zigzagFreq);
                bulletScript.SetArcHeight(arcHeight);
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

    public void TakeDamage(int damage, string damageTag)
    {
        // Принимаем урон только от объектов с тегом "Fire shard"
        if (damageTag == "Fire shard")
        {
            currentHealth -= damage;
            Debug.Log($"Босс получил урон! Здоровье: {currentHealth}/{maxHealth}");

            // Обновляем UI здоровья
            UpdateHealthUI();

            // Переход во вторую фазу при половине здоровья
            if (currentHealth == maxHealth / 2 && currentPhase == BossPhase.Phase1 && !isTransitioningPhase)
            {
                StartCoroutine(StartPhase2Transition());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    IEnumerator StartPhase2Transition()
    {
        isTransitioningPhase = true;

        Debug.Log("НАЧИНАЕТСЯ ПЕРЕХОД ВО ВТОРУЮ ФАЗУ!");

        // Запускаем тряску камеры на все 2 секунды
        if (cameraShake != null)
        {
            cameraShake.StartShake(phaseTransitionDelay, 0.15f);
        }

        // Ждем заданное время
        yield return new WaitForSeconds(phaseTransitionDelay);

        // Переходим во вторую фазу
        currentPhase = BossPhase.Phase2;

        // Изменяем цвет спрайта
        if (spriteRenderer != null)
        {
            StartCoroutine(ChangeColorCoroutine());
        }

        Debug.Log("ПЕРЕХОД ВО ВТОРУЮ ФАЗУ ЗАВЕРШЕН! Босс становится более агрессивным!");

        // Сбрасываем таймер для немедленного начала новых атак
        nextFireTime = Time.time + 1f;

        isTransitioningPhase = false;
    }

    IEnumerator ChangeColorCoroutine()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < colorTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / colorTransitionDuration;
            spriteRenderer.color = Color.Lerp(startColor, phase2Color, t);
            yield return null;
        }

        spriteRenderer.color = phase2Color;
    }

    void Die()
    {
        Debug.Log("Босс побежден!");

        // Скрываем UI здоровья
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        // Можно добавить эффекты смерти, дроп лута и т.д.
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire shard"))
        {
            TakeDamage(1, "Fire shard");
            // Уничтожаем снаряд который попал в босса
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Fire shard"))
        {
            TakeDamage(1, "Fire shard");
            Destroy(collision.gameObject);
        }
    }

    public void OnFireAnimationComplete()
    {
        Debug.Log("Анимация выстрела босса завершена");
    }

    // Публичные методы для доступа к информации о здоровье
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}