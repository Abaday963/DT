using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для управления визуальным состоянием замка в префабе уровня
/// Автоматически обновляет изображение замка в зависимости от состояния уровня
/// </summary>
public class LevelLockVisual : MonoBehaviour
{
    [Header("Настройки уровня")]
    [SerializeField] private int levelIndex = 0; // Индекс уровня (0-11)

    [Header("Изображения замка")]
    [SerializeField] private Image lockImage; // Основной Image компонент для замка
    [SerializeField] private Sprite lockedSprite; // Изображение закрытого замка
    [SerializeField] private Sprite unlockedSprite; // Изображение открытого замка

    [Header("Альтернативный способ (GameObject)")]
    [SerializeField] private GameObject lockedObject; // GameObject с закрытым замком
    [SerializeField] private GameObject unlockedObject; // GameObject с открытым замком

    [Header("Дополнительные настройки")]
    [SerializeField] private bool hideWhenUnlocked = false; // Скрывать замок полностью когда разблокирован
    [SerializeField] private bool useColorTint = false; // Использовать изменение цвета
    [SerializeField] private Color lockedColor = Color.white;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Анимация (опционально)")]
    [SerializeField] private bool useAnimation = false;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Отладка")]
    [SerializeField] private bool debugMode = false;

    private StarManager starManager;
    private LevelLockManager lockManager;
    private bool lastLockState = true; // Предыдущее состояние блокировки

    private void Start()
    {
        InitializeManagers();
        SubscribeToEvents();
        UpdateLockVisual();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeManagers()
    {
        starManager = StarManager.Instance;
        lockManager = LevelLockManager.Instance;

        if (starManager == null && debugMode)
        {
            Debug.LogWarning($"[LevelLockVisual] StarManager не найден для уровня {levelIndex + 1}");
        }

        if (lockManager == null && debugMode)
        {
            Debug.LogWarning($"[LevelLockVisual] LevelLockManager не найден для уровня {levelIndex + 1}");
        }
    }

    private void SubscribeToEvents()
    {
        if (starManager != null)
        {
            starManager.OnLevelUnlocked += OnLevelStateChanged;
            starManager.OnLevelStarsUpdated += OnLevelStarsUpdated;
            starManager.OnProgressLoaded += UpdateLockVisual;
        }

        if (lockManager != null)
        {
            lockManager.OnLevelLockChanged += OnLevelLockChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (starManager != null)
        {
            starManager.OnLevelUnlocked -= OnLevelStateChanged;
            starManager.OnLevelStarsUpdated -= OnLevelStarsUpdated;
            starManager.OnProgressLoaded -= UpdateLockVisual;
        }

        if (lockManager != null)
        {
            lockManager.OnLevelLockChanged -= OnLevelLockChanged;
        }
    }

    private void OnLevelStateChanged(int changedLevelIndex)
    {
        if (changedLevelIndex == levelIndex)
        {
            UpdateLockVisual();
        }
    }

    private void OnLevelStarsUpdated(int changedLevelIndex, int stars)
    {
        if (changedLevelIndex == levelIndex)
        {
            UpdateLockVisual();
        }
    }

    private void OnLevelLockChanged(int changedLevelIndex, bool isLocked)
    {
        if (changedLevelIndex == levelIndex)
        {
            UpdateLockVisual();
        }
    }

    /// <summary>
    /// Обновляет визуальное состояние замка
    /// </summary>
    public void UpdateLockVisual()
    {
        bool isLocked = GetCurrentLockState();

        // Проверяем, изменилось ли состояние
        if (isLocked == lastLockState && Application.isPlaying)
        {
            return; // Состояние не изменилось
        }

        if (debugMode)
        {
            Debug.Log($"[LevelLockVisual] Обновление замка для уровня {levelIndex + 1}: " +
                     $"{(isLocked ? "ЗАБЛОКИРОВАН" : "РАЗБЛОКИРОВАН")}");
        }

        if (useAnimation && Application.isPlaying)
        {
            StartCoroutine(AnimateLockChange(isLocked));
        }
        else
        {
            SetLockVisualImmediate(isLocked);
        }

        lastLockState = isLocked;
    }

    /// <summary>
    /// Получает текущее состояние блокировки уровня
    /// </summary>
    private bool GetCurrentLockState()
    {
        if (starManager == null)
        {
            return true; // По умолчанию заблокирован, если нет StarManager
        }

        // Используем IsLevelAvailable, который учитывает все типы блокировок
        bool isAvailable = starManager.IsLevelAvailable(levelIndex);
        return !isAvailable; // Если недоступен, значит заблокирован
    }

    /// <summary>
    /// Немедленно устанавливает визуальное состояние замка
    /// </summary>
    private void SetLockVisualImmediate(bool isLocked)
    {
        // Метод 1: Использование Image компонента со сменой спрайтов
        if (lockImage != null)
        {
            if (hideWhenUnlocked && !isLocked)
            {
                lockImage.gameObject.SetActive(false);
            }
            else
            {
                lockImage.gameObject.SetActive(true);

                // Меняем спрайт
                if (isLocked && lockedSprite != null)
                {
                    lockImage.sprite = lockedSprite;
                }
                else if (!isLocked && unlockedSprite != null)
                {
                    lockImage.sprite = unlockedSprite;
                }

                // Меняем цвет, если включено
                if (useColorTint)
                {
                    lockImage.color = isLocked ? lockedColor : unlockedColor;
                }
            }
        }

        // Метод 2: Использование разных GameObjects
        if (lockedObject != null)
        {
            lockedObject.SetActive(isLocked);
        }

        if (unlockedObject != null)
        {
            unlockedObject.SetActive(!isLocked && !hideWhenUnlocked);
        }
    }

    /// <summary>
    /// Анимированная смена состояния замка
    /// </summary>
    private System.Collections.IEnumerator AnimateLockChange(bool toLocked)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;

        // Анимация уменьшения
        while (elapsedTime < animationDuration / 2)
        {
            float progress = elapsedTime / (animationDuration / 2);
            float scaleMultiplier = animationCurve.Evaluate(1f - progress * 0.3f); // Уменьшаем на 30%

            transform.localScale = originalScale * scaleMultiplier;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Меняем визуал в середине анимации
        SetLockVisualImmediate(toLocked);

        elapsedTime = 0f;

        // Анимация увеличения обратно
        while (elapsedTime < animationDuration / 2)
        {
            float progress = elapsedTime / (animationDuration / 2);
            float scaleMultiplier = animationCurve.Evaluate(0.7f + progress * 0.3f); // Увеличиваем обратно

            transform.localScale = originalScale * scaleMultiplier;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Возвращаем исходный масштаб
        transform.localScale = originalScale;
    }

    /// <summary>
    /// Устанавливает индекс уровня программно
    /// </summary>
    public void SetLevelIndex(int newLevelIndex)
    {
        if (newLevelIndex != levelIndex)
        {
            levelIndex = newLevelIndex;
            UpdateLockVisual();

            if (debugMode)
            {
                Debug.Log($"[LevelLockVisual] Установлен новый индекс уровня: {levelIndex + 1}");
            }
        }
    }

    /// <summary>
    /// Получает текущий индекс уровня
    /// </summary>
    public int GetLevelIndex()
    {
        return levelIndex;
    }

    /// <summary>
    /// Принудительно обновляет состояние (полезно для отладки)
    /// </summary>
    [ContextMenu("Принудительно обновить")]
    public void ForceUpdate()
    {
        lastLockState = !GetCurrentLockState(); // Инвертируем, чтобы гарантировать обновление
        UpdateLockVisual();
    }

    /// <summary>
    /// Показывает информацию о текущем состоянии
    /// </summary>
    [ContextMenu("Показать информацию")]
    public void ShowDebugInfo()
    {
        bool currentState = GetCurrentLockState();
        string stateText = currentState ? "ЗАБЛОКИРОВАН" : "РАЗБЛОКИРОВАН";

        Debug.Log($"[LevelLockVisual] Уровень {levelIndex + 1}: {stateText}");

        if (starManager != null)
        {
            bool starUnlocked = starManager.IsLevelUnlocked(levelIndex);
            bool available = starManager.IsLevelAvailable(levelIndex);
            int stars = starManager.GetLevelStars(levelIndex);

            Debug.Log($"  • Звезды: {stars}");
            Debug.Log($"  • StarManager разблокирован: {starUnlocked}");
            Debug.Log($"  • Общая доступность: {available}");
        }

        if (lockManager != null)
        {
            bool adminLocked = lockManager.IsLevelLockedByAdmin(levelIndex);
            Debug.Log($"  • Админ-блокировка: {adminLocked}");
        }
    }

    /// <summary>
    /// Тестовое переключение состояния (только для отладки)
    /// </summary>
    [ContextMenu("Тестовое переключение")]
    public void DebugToggleLock()
    {
        if (lockManager != null)
        {
            lockManager.ToggleLevelLock(levelIndex);
            Debug.Log($"[LevelLockVisual] Переключено состояние блокировки уровня {levelIndex + 1}");
        }
        else
        {
            Debug.LogWarning("[LevelLockVisual] LevelLockManager не найден для тестового переключения");
        }
    }

    #region Валидация в редакторе

    private void OnValidate()
    {
        // Проверяем корректность настроек
        if (lockImage == null && lockedObject == null && unlockedObject == null)
        {
            Debug.LogWarning($"[LevelLockVisual] На уровне {levelIndex + 1} не настроены элементы для отображения замка!");
        }

        if (lockImage != null && (lockedSprite == null || unlockedSprite == null))
        {
            Debug.LogWarning($"[LevelLockVisual] На уровне {levelIndex + 1} не настроены спрайты замка!");
        }

        // Обновляем визуал в редакторе
        if (!Application.isPlaying)
        {
            UpdateLockVisual();
        }
    }

    #endregion
}