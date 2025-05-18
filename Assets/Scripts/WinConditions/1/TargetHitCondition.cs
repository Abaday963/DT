using UnityEngine;

public class TargetHitCondition : WinCondition
{
    [Header("Настройки мишени")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Collider2D innerCollider;   // Центральный коллайдер (3 звезды)
    [SerializeField] private Collider2D middleCollider;  // Средний коллайдер (2 звезды)
    [SerializeField] private Collider2D outerCollider;   // Внешний коллайдер (1 звезда)

    [Header("Визуальные эффекты")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioSource hitSound;

    private bool conditionCompleted = false;
    private int starsEarned = 0;

    private void Awake()
    {
        conditionName = "Меткий стрелок";
        conditionDescription = "Попасть в мишень. Центр - 3 звезды, среднее кольцо - 2 звезды, внешнее кольцо - 1 звезда";

        // Находим коллайдеры, если они не назначены
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        if (innerCollider == null || middleCollider == null || outerCollider == null)
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            if (colliders.Length >= 3)
            {
                innerCollider = colliders[0];
                middleCollider = colliders[1];
                outerCollider = colliders[2];
            }
            else
            {
                Debug.LogError("TargetHitCondition: Не найдено достаточно коллайдеров на мишени!");
            }
        }
    }

    // Этот метод будет вызываться при попадании в центральный коллайдер
    public void OnInnerHit(Vector2 hitPosition)
    {
        if (!conditionCompleted)
        {
            starsEarned = 3;
            starsAwarded = 3;
            conditionCompleted = true;
            PlayHitEffects(hitPosition);
            Debug.Log("Попадание в центр мишени! Получено 3 звезды");
            NotifyGameManager();
        }
    }

    // Этот метод будет вызываться при попадании в средний коллайдер
    public void OnMiddleHit(Vector2 hitPosition)
    {
        if (!conditionCompleted)
        {
            starsEarned = 2;
            starsAwarded = 2;
            conditionCompleted = true;
            PlayHitEffects(hitPosition);
            Debug.Log("Попадание в среднее кольцо мишени! Получено 2 звезды");
            NotifyGameManager();
        }
    }

    // Этот метод будет вызываться при попадании во внешний коллайдер
    public void OnOuterHit(Vector2 hitPosition)
    {
        if (!conditionCompleted)
        {
            starsEarned = 1;
            starsAwarded = 1;
            conditionCompleted = true;
            PlayHitEffects(hitPosition);
            Debug.Log("Попадание во внешнее кольцо мишени! Получена 1 звезда");
            NotifyGameManager();
        }
    }

    // Воспроизводим эффекты при попадании
    private void PlayHitEffects(Vector2 hitPosition)
    {
        if (hitEffect != null)
        {
            hitEffect.transform.position = hitPosition;
            hitEffect.Play();
        }

        if (hitSound != null)
        {
            hitSound.Play();
        }
    }

    // Уведомляем GameManager о выполнении условия
    private void NotifyGameManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
        {
            // Устанавливаем количество звезд в LevelManager для режима ByTargetHit
            LevelManager levelManager = GameManager.Instance.LevelManager;
            if (levelManager.GetStarCountMode() == LevelManager.StarCountMode.ByTargetHit)
            {
                levelManager.SetWeightedStarValue(starsEarned);
            }

            levelManager.CheckWinConditions();
        }
    }

    public override bool IsConditionMet()
    {
        return conditionCompleted;
    }

    public override void ResetCondition()
    {
        conditionCompleted = false;
        starsEarned = 0;
    }

    // Получить количество заработанных звезд
    public int GetStarsEarned()
    {
        return starsEarned;
    }
}