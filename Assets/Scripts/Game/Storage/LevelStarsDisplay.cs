using UnityEngine;
using UnityEngine.UI;

public class LevelStarsDisplay : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelIndex; // Индекс уровня (0, 1, 2...)

    [Header("Star UI Elements")]
    [SerializeField] private Image[] starImages = new Image[3]; // Массив из 3 звезд

    [Header("Star Sprites")]
    [SerializeField] private Sprite fullStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("Settings")]
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool debugMode = true;

    private StarManager starManager;
    private int currentStars = -1; // Для отслеживания изменений
    private float lastUpdateTime;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        starManager = StarManager.Instance;

        if (starManager == null)
        {
            if (debugMode) Debug.LogError($"[LevelStarsDisplay] StarManager не найден! Уровень {levelIndex + 1}");
            // Попробуем еще раз через короткое время
            Invoke(nameof(InitializeDisplay), 0.1f);
            return;
        }

        // Проверяем, что у нас есть все необходимые элементы
        if (!ValidateSetup()) return;

        // Подписываемся на события
        starManager.OnLevelStarsUpdated += OnLevelStarsUpdated;
        starManager.OnProgressLoaded += OnProgressLoaded;

        isInitialized = true;

        // Первоначальное обновление
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LevelStarsDisplay] Инициализирован для уровня {levelIndex + 1}");
    }

    private bool ValidateSetup()
    {
        bool isValid = true;

        // Проверяем массив звезд
        if (starImages == null || starImages.Length != 3)
        {
            Debug.LogError($"[LevelStarsDisplay] Массив звезд должен содержать ровно 3 элемента! Уровень {levelIndex + 1}");
            return false;
        }

        // Проверяем, что все звезды назначены
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                Debug.LogError($"[LevelStarsDisplay] Звезда {i + 1} не назначена! Уровень {levelIndex + 1}");
                isValid = false;
            }
        }

        // Проверяем спрайты
        if (fullStarSprite == null)
        {
            Debug.LogWarning($"[LevelStarsDisplay] Спрайт полной звезды не назначен! Уровень {levelIndex + 1}");
            isValid = false;
        }

        if (emptyStarSprite == null)
        {
            Debug.LogWarning($"[LevelStarsDisplay] Спрайт пустой звезды не назначен! Уровень {levelIndex + 1}");
            isValid = false;
        }

        return isValid;
    }

    private void Update()
    {
        if (autoUpdate && isInitialized && Time.time - lastUpdateTime > updateInterval)
        {
            UpdateStarsDisplay();
            lastUpdateTime = Time.time;
        }
    }

    private void OnDestroy()
    {
        if (starManager != null)
        {
            starManager.OnLevelStarsUpdated -= OnLevelStarsUpdated;
            starManager.OnProgressLoaded -= OnProgressLoaded;
        }
    }

    private void OnProgressLoaded()
    {
        if (debugMode) Debug.Log($"[LevelStarsDisplay] Прогресс загружен, обновляем дисплей уровня {levelIndex + 1}");
        currentStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();
    }

    private void OnLevelStarsUpdated(int updatedLevelIndex, int newStars)
    {
        // Обновляем только если это наш уровень
        if (updatedLevelIndex == levelIndex)
        {
            if (debugMode) Debug.Log($"[LevelStarsDisplay] Получено событие обновления для уровня {levelIndex + 1}: {newStars} звезд");
            currentStars = -1; // Сбрасываем для принудительного обновления
            UpdateStarsDisplay();
        }
    }

    public void UpdateStarsDisplay()
    {
        if (!isInitialized || starManager == null || starImages == null)
        {
            if (debugMode && starManager == null)
                Debug.LogWarning($"[LevelStarsDisplay] StarManager отсутствует для уровня {levelIndex + 1}");
            return;
        }

        int stars = starManager.GetLevelStars(levelIndex);

        // Логируем каждое обновление для отладки
        if (debugMode && stars != currentStars)
        {
            Debug.Log($"[LevelStarsDisplay] Уровень {levelIndex + 1}: обновление {currentStars} -> {stars} звезд");
        }

        // Обновляем только если количество звезд изменилось
        if (stars == currentStars)
        {
            return;
        }

        int previousStars = currentStars;
        currentStars = stars;

        // Обновляем каждую звезду
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                bool shouldBeFilled = stars > i;
                bool wasFilledBefore = previousStars > i;

                UpdateSingleStar(i, shouldBeFilled);

                // Анимируем только новые звезды
                if (shouldBeFilled && !wasFilledBefore && previousStars >= 0)
                {
                    AnimateStar(starImages[i]);
                }
            }
        }

        if (debugMode) Debug.Log($"[LevelStarsDisplay] Уровень {levelIndex + 1}: дисплей обновлен на {stars} звезд");
    }

    private void UpdateSingleStar(int starIndex, bool isFilled)
    {
        Image starImage = starImages[starIndex];

        if (isFilled)
        {
            // Звезда заполнена
            starImage.sprite = fullStarSprite;
            starImage.color = Color.white;
        }
        else
        {
            // Звезда пустая
            starImage.sprite = emptyStarSprite;
            starImage.color = Color.gray;
        }
    }

    private void AnimateStar(Image starImage)
    {
        if (starImage == null) return;

        // Простая анимация появления звезды
        starImage.transform.localScale = Vector3.one * 1.2f;

        // Возвращаем к нормальному размеру через корутину
        StartCoroutine(ScaleBackCoroutine(starImage.transform));
    }

    private System.Collections.IEnumerator ScaleBackCoroutine(Transform starTransform)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = starTransform.localScale;
        Vector3 targetScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            starTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        starTransform.localScale = targetScale;
    }

    // Публичные методы для ручного управления
    public void SetLevelIndex(int newLevelIndex)
    {
        levelIndex = newLevelIndex;
        currentStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LevelStarsDisplay] Индекс уровня изменен на {newLevelIndex + 1}");
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public void ForceUpdate()
    {
        currentStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LevelStarsDisplay] Принудительное обновление уровня {levelIndex + 1}");
    }

    // Методы для отладки
    [ContextMenu("Обновить звезды")]
    public void ManualUpdate()
    {
        ForceUpdate();
    }

    [ContextMenu("Показать информацию об уровне")]
    public void ShowLevelInfo()
    {
        if (starManager == null)
        {
            Debug.LogWarning($"[LevelStarsDisplay] StarManager отсутствует");
            return;
        }

        int stars = starManager.GetLevelStars(levelIndex);
        bool unlocked = starManager.IsLevelUnlocked(levelIndex);

        Debug.Log($"[LevelStarsDisplay] Уровень {levelIndex + 1}: {stars} звезд, " +
                 (unlocked ? "разблокирован" : "заблокирован") +
                 $", текущий дисплей: {currentStars} звезд");
    }

    [ContextMenu("Тест - дать 1 звезду")]
    public void TestGiveOneStar()
    {
        if (starManager != null)
        {
            starManager.SetLevelStars(levelIndex, 1);
        }
    }

    [ContextMenu("Тест - дать 2 звезды")]
    public void TestGiveTwoStars()
    {
        if (starManager != null)
        {
            starManager.SetLevelStars(levelIndex, 2);
        }
    }

    [ContextMenu("Тест - дать 3 звезды")]
    public void TestGiveThreeStars()
    {
        if (starManager != null)
        {
            starManager.SetLevelStars(levelIndex, 3);
        }
    }

    [ContextMenu("Тест - сбросить звезды")]
    public void TestResetStars()
    {
        if (starManager != null)
        {
            starManager.SetLevelStars(levelIndex, 0);
        }
    }

    // Альтернативные методы установки спрайтов из кода
    public void SetStarSprites(Sprite fullStar, Sprite emptyStar)
    {
        fullStarSprite = fullStar;
        emptyStarSprite = emptyStar;
        ForceUpdate();
    }

    public void SetStarImages(Image[] stars)
    {
        if (stars != null && stars.Length == 3)
        {
            starImages = stars;
            ForceUpdate();
        }
        else
        {
            Debug.LogError("[LevelStarsDisplay] Массив звезд должен содержать ровно 3 элемента!");
        }
    }
}