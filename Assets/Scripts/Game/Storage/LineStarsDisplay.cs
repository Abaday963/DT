using UnityEngine;
using UnityEngine.UI;

public class LineStarsDisplay : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private int lineIndex = 0; // 0 - первая линия, 1 - вторая линия

    [Header("UI Elements")]
    [SerializeField] private Text starsText; // Текст для отображения звезд

    [Header("Display Settings")]
    [SerializeField] private string textFormat = "{0}/{1}"; // Формат текста: заработано/максимум
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool debugMode = true;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.yellow; // Цвет когда все звезды собраны

    private StarManager starManager;
    private int currentEarnedStars = -1; // Для отслеживания изменений
    private int maxStarsPerLine = 18; // 6 уровней × 3 звезды
    private int levelsPerLine = 6;
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
            if (debugMode) Debug.LogError($"[LineStarsDisplay] StarManager не найден! Линия {lineIndex + 1}");
            // Попробуем еще раз через короткое время
            Invoke(nameof(InitializeDisplay), 0.1f);
            return;
        }

        // Проверяем, что у нас есть все необходимые элементы
        if (!ValidateSetup()) return;

        // Подписываемся на события
        starManager.OnLevelStarsUpdated += OnLevelStarsUpdated;
        starManager.OnTotalStarsUpdated += OnTotalStarsUpdated;
        starManager.OnProgressLoaded += OnProgressLoaded;

        isInitialized = true;

        // Первоначальное обновление
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LineStarsDisplay] Инициализирован для линии {lineIndex + 1}");
    }

    private bool ValidateSetup()
    {
        bool isValid = true;

        // Проверяем текстовый компонент
        if (starsText == null)
        {
            Debug.LogError($"[LineStarsDisplay] Text компонент не назначен! Линия {lineIndex + 1}");
            isValid = false;
        }

        // Проверяем корректность индекса линии
        if (lineIndex < 0 || lineIndex > 1)
        {
            Debug.LogError($"[LineStarsDisplay] Некорректный индекс линии: {lineIndex}. Должен быть 0 или 1");
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
            starManager.OnTotalStarsUpdated -= OnTotalStarsUpdated;
            starManager.OnProgressLoaded -= OnProgressLoaded;
        }
    }

    private void OnProgressLoaded()
    {
        if (debugMode) Debug.Log($"[LineStarsDisplay] Прогресс загружен, обновляем дисплей линии {lineIndex + 1}");
        currentEarnedStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();
    }

    private void OnLevelStarsUpdated(int updatedLevelIndex, int newStars)
    {
        // Проверяем, принадлежит ли обновленный уровень нашей линии
        int levelLine = updatedLevelIndex / levelsPerLine;
        if (levelLine == lineIndex)
        {
            if (debugMode) Debug.Log($"[LineStarsDisplay] Получено событие обновления для уровня {updatedLevelIndex + 1} в линии {lineIndex + 1}");
            currentEarnedStars = -1; // Сбрасываем для принудительного обновления
            UpdateStarsDisplay();
        }
    }

    private void OnTotalStarsUpdated(int totalStars)
    {
        // Обновляем при любом изменении общего количества звезд
        currentEarnedStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();
    }

    public void UpdateStarsDisplay()
    {
        if (!isInitialized || starManager == null || starsText == null)
        {
            if (debugMode && starManager == null)
                Debug.LogWarning($"[LineStarsDisplay] StarManager отсутствует для линии {lineIndex + 1}");
            return;
        }

        int earnedStars = CalculateLineStars();

        // Обновляем только если количество звезд изменилось
        if (earnedStars == currentEarnedStars)
        {
            return;
        }

        if (debugMode && earnedStars != currentEarnedStars)
        {
            Debug.Log($"[LineStarsDisplay] Линия {lineIndex + 1}: обновление {currentEarnedStars} -> {earnedStars} звезд");
        }

        currentEarnedStars = earnedStars;

        // Обновляем текст
        string displayText = string.Format(textFormat, earnedStars, maxStarsPerLine);
        starsText.text = displayText;

        // Обновляем цвет в зависимости от прогресса
        if (earnedStars >= maxStarsPerLine)
        {
            starsText.color = completedColor;
            if (debugMode) Debug.Log($"[LineStarsDisplay] Линия {lineIndex + 1} завершена! Все {maxStarsPerLine} звезд собраны");
        }
        else
        {
            starsText.color = normalColor;
        }

        if (debugMode) Debug.Log($"[LineStarsDisplay] Линия {lineIndex + 1}: дисплей обновлен - {displayText}");
    }

    private int CalculateLineStars()
    {
        int totalStars = 0;

        // Вычисляем начальный и конечный индекс уровней для этой линии
        int startLevelIndex = lineIndex * levelsPerLine; // 0 или 6
        int endLevelIndex = startLevelIndex + levelsPerLine; // 6 или 12

        for (int i = startLevelIndex; i < endLevelIndex; i++)
        {
            totalStars += starManager.GetLevelStars(i);
        }

        return totalStars;
    }

    // Публичные методы для ручного управления
    public void SetLineIndex(int newLineIndex)
    {
        if (newLineIndex < 0 || newLineIndex > 1)
        {
            Debug.LogError($"[LineStarsDisplay] Некорректный индекс линии: {newLineIndex}. Должен быть 0 или 1");
            return;
        }

        lineIndex = newLineIndex;
        currentEarnedStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LineStarsDisplay] Индекс линии изменен на {newLineIndex + 1}");
    }

    public int GetLineIndex()
    {
        return lineIndex;
    }

    public void ForceUpdate()
    {
        currentEarnedStars = -1; // Сбрасываем для принудительного обновления
        UpdateStarsDisplay();

        if (debugMode) Debug.Log($"[LineStarsDisplay] Принудительное обновление линии {lineIndex + 1}");
    }

    public void SetTextFormat(string newFormat)
    {
        textFormat = newFormat;
        ForceUpdate();
    }

    public void SetColors(Color normal, Color completed)
    {
        normalColor = normal;
        completedColor = completed;
        ForceUpdate();
    }

    // Методы для отладки
    [ContextMenu("Обновить дисплей")]
    public void ManualUpdate()
    {
        ForceUpdate();
    }

    [ContextMenu("Показать информацию о линии")]
    public void ShowLineInfo()
    {
        if (starManager == null)
        {
            Debug.LogWarning($"[LineStarsDisplay] StarManager отсутствует");
            return;
        }

        int earnedStars = CalculateLineStars();
        float progress = (float)earnedStars / maxStarsPerLine * 100f;

        Debug.Log($"[LineStarsDisplay] Линия {lineIndex + 1}: {earnedStars}/{maxStarsPerLine} звезд ({progress:F1}%)");

        // Показываем детальную информацию по уровням в линии
        int startLevelIndex = lineIndex * levelsPerLine;
        int endLevelIndex = startLevelIndex + levelsPerLine;

        Debug.Log($"=== ПОДРОБНОСТИ ЛИНИИ {lineIndex + 1} ===");
        for (int i = startLevelIndex; i < endLevelIndex; i++)
        {
            int stars = starManager.GetLevelStars(i);
            bool unlocked = starManager.IsLevelUnlocked(i);
            bool isSpecial = starManager.IsSpecialLevel(i);

            string levelType = isSpecial ? " (СПЕЦИАЛЬНЫЙ)" : "";
            string unlockStatus = unlocked ? "разблокирован" : "заблокирован";

            Debug.Log($"  Уровень {i + 1}{levelType}: {stars}/3 звезд, {unlockStatus}");
        }
    }

    [ContextMenu("Тест - дать максимум звезд линии")]
    public void TestGiveMaxStarsToLine()
    {
        if (starManager == null) return;

        int startLevelIndex = lineIndex * levelsPerLine;
        int endLevelIndex = startLevelIndex + levelsPerLine;

        for (int i = startLevelIndex; i < endLevelIndex; i++)
        {
            starManager.SetLevelStars(i, 3);
        }

        Debug.Log($"[LineStarsDisplay] Дано максимум звезд линии {lineIndex + 1}");
    }

    [ContextMenu("Тест - сбросить звезды линии")]
    public void TestResetLineStars()
    {
        if (starManager == null) return;

        int startLevelIndex = lineIndex * levelsPerLine;
        int endLevelIndex = startLevelIndex + levelsPerLine;

        for (int i = startLevelIndex; i < endLevelIndex; i++)
        {
            starManager.SetLevelStars(i, 0);
        }

        Debug.Log($"[LineStarsDisplay] Сброшены звезды линии {lineIndex + 1}");
    }

    // Альтернативный метод установки текстового компонента из кода
    public void SetTextComponent(Text textComponent)
    {
        starsText = textComponent;
        ForceUpdate();
    }
}