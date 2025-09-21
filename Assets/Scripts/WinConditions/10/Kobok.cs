using UnityEngine;

public class Kobok : MonoBehaviour
{
    [Header("Настройки здоровья")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Визуальные эффекты")]
    public GameObject hitEffect; // Эффект попадания
    public AudioClip hitSound; // Звук попадания

    [Header("Тряска камеры")]
    public float shakeDuration = 0.3f; // Длительность тряски
    public float shakeMagnitude = 0.1f; // Сила тряски

    private HealthManager healthManager;
    private AudioSource audioSource;
    private CameraShake cameraShake; // Ссылка на скрипт тряски камеры

    void Start()
    {
        currentHealth = maxHealth;

        // Находим менеджер здоровья
        healthManager = FindObjectOfType<HealthManager>();

        // Находим скрипт тряски камеры
        cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake == null)
        {
            Debug.LogWarning("CameraShake скрипт не найден! Тряска камеры не будет работать.");
        }

        // Получаем AudioSource или создаем его
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Регистрируем кобока в менеджере здоровья
        if (healthManager != null)
        {
            healthManager.RegisterKobok(this);
        }
    }

    public void TakeHit()
    {
        Debug.Log($"Кобок {gameObject.name} получил попадание!");
        currentHealth--;

        // Визуальные и звуковые эффекты
        ShowHitEffect();
        PlayHitSound();

        // ДОБАВЛЯЕМ ТРЯСКУ КАМЕРЫ
        if (cameraShake != null)
        {
            cameraShake.StartShake(shakeDuration, shakeMagnitude);
        }

        // Уведомляем менеджер здоровья о попадании
        if (healthManager != null)
        {
            Debug.Log("Уведомляем менеджер здоровья о попадании");
            healthManager.OnKobokHit(this);
        }
        else
        {
            Debug.LogError("HealthManager не найден!");
        }

        // Проверяем, жив ли еще кобок
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Можно добавить анимацию получения урона
            Debug.Log($"Кобок {gameObject.name} получил урон! Осталось здоровья: {currentHealth}");
        }
    }

    void ShowHitEffect()
    {
        if (hitEffect != null)
        {
            // Создаем эффект попадания
            GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
            // Уничтожаем эффект через 2 секунды
            Destroy(effect, 2f);
        }
    }

    void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    void Die()
    {
        // Более сильная тряска при смерти
        if (cameraShake != null)
        {
            cameraShake.StartShake(shakeDuration * 1.5f, shakeMagnitude * 2f);
        }

        // Уведомляем менеджер здоровья о смерти
        if (healthManager != null)
        {
            healthManager.OnKobokDeath(this);
        }

        Debug.Log($"Кобок {gameObject.name} погиб!");
        // Можно добавить анимацию смерти или эффекты
        // Уничтожаем объект
        Destroy(gameObject);
    }

    // Метод для восстановления здоровья (если нужно)
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Кобок {gameObject.name} восстановил здоровье: {currentHealth}/{maxHealth}");
    }

    // Геттер для текущего здоровья
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Геттер для максимального здоровья
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    void OnDestroy()
    {
        // Убираем кобока из менеджера здоровья при уничтожении
        if (healthManager != null)
        {
            healthManager.UnregisterKobok(this);
        }
    }
}