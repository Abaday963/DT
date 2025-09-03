using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Rendering;

public class LevelManager : MonoBehaviour
{
    // Перечисление для выбора режима подсчета звезд
    public enum StarCountMode
    {
        ByConditionCount,    // По количеству выполненных условий
        ByConditionWeight,   // По весу конкретного условия
        ByTargetHit          // Новый режим: по результатам попадания в мишень
    }

    [Header("Настройки подсчета звезд")]
    [SerializeField] private StarCountMode starCountMode = StarCountMode.ByTargetHit; // По умолчанию используем новый режим
    [SerializeField] private WinCondition weightedStarCondition; // Условие с весом для StarCountMode.ByConditionWeight
    [SerializeField] private int weightedStarValue = 3; // Максимальное количество звезд для условия с весом
    [SerializeField] private TargetHitCondition targetHitCondition; // Ссылка на условие попадания в мишень

    [Header("Условия победы и проигрыша")]
    [SerializeField] private WinCondition[] winConditions;
    [SerializeField] private LoseCondition[] loseConditions;

    [Header("Ссылки на объекты уровня")]
    [SerializeField] private AmmunitionManager ammunitionManager;

    [Header("Настройки")]
    [SerializeField] private bool autoCheckForWin = true;
    [SerializeField] private float autoCheckInterval = 0.5f;

    private bool isLevelCompleted = false;
    private float autoCheckTimer = 0f;
    private List<WinCondition> completedWinConditions = new List<WinCondition>();

    // События для GameManager и UI
    public delegate void LevelEvent(int starsEarned, List<string> completedConditions);
    public event LevelEvent OnLevelWon;
    public event LevelEvent OnLevelLost;

    // Значение для режима взвешенных звезд (может быть установлено из других скриптов)
    private int actualWeightedStarValue = 0;

    private void Start()
    {
        FindAndSetupReferences();
        ResetAllConditions();
    }

    private void Update()
    {
        if (isLevelCompleted) return;

        // Отслеживаем победу с задержкой
        if (victoryPending)
        {
            victoryTimer += Time.deltaTime;
            if (victoryTimer >= victoryDelay)
            {
                CompleteWinLevel();
                return;
            }
        }

        // Автоматическая проверка условий
        if (autoCheckForWin)
        {
            autoCheckTimer += Time.deltaTime;
            if (autoCheckTimer >= autoCheckInterval)
            {
                autoCheckTimer = 0f;
                CheckWinConditions();
                CheckLoseConditions();
            }
        }
    }

    // Находим и настраиваем ссылки на объекты, если они не заданы в инспекторе
    private void FindAndSetupReferences()
    {
        if (ammunitionManager == null)
        {
            ammunitionManager = FindAnyObjectByType<AmmunitionManager>();
        }

        // Находим все условия победы и проигрыша, если массивы пусты
        if (winConditions == null || winConditions.Length == 0)
        {
            winConditions = FindObjectsOfType<WinCondition>();
        }

        if (loseConditions == null || loseConditions.Length == 0)
        {
            loseConditions = FindObjectsOfType<LoseCondition>();
        }

        // Находим условие попадания в мишень, если оно не задано
        if (targetHitCondition == null)
        {
            targetHitCondition = FindAnyObjectByType<TargetHitCondition>();
        }
    }

    // Сбрасываем все условия
    public void ResetAllConditions()
    {
        isLevelCompleted = false;
        completedWinConditions.Clear();
        actualWeightedStarValue = 0;

        // ДОБАВИТЬ: Сброс флагов победы
        victoryPending = false;
        victoryTimer = 0f;

        if (winConditions != null)
        {
            foreach (var condition in winConditions)
            {
                if (condition != null) condition.ResetCondition();
            }
        }

        if (loseConditions != null)
        {
            foreach (var condition in loseConditions)
            {
                if (condition != null) condition.ResetCondition();
            }
        }

        // Сбрасываем таймер автопроверки
        autoCheckTimer = 0f;

        Debug.Log("[LevelManager] Все условия и флаги сброшены");
    }

    // Проверка условий победы
    public void CheckWinConditions()
    {
        if (isLevelCompleted) return;

        // Проверяем каждое условие победы
        foreach (var condition in winConditions)
        {
            if (condition != null && condition.IsConditionMet() && !completedWinConditions.Contains(condition))
            {
                completedWinConditions.Add(condition);
                Debug.Log($"Выполнено условие победы: {condition.Name}");

                // Если это основное условие для взвешенных звезд, назначаем звезды
                if (starCountMode == StarCountMode.ByConditionWeight && condition == weightedStarCondition)
                {
                    actualWeightedStarValue = ((TargetHitCondition)condition).GetStarsEarned();
                    //CalculateWeightedStarValue();
                }

                //// Если это попадание в мишень и у нас соответствующий режим, берем звезды из условия
                if (starCountMode == StarCountMode.ByTargetHit && condition is TargetHitCondition)
                {
                    actualWeightedStarValue = ((TargetHitCondition)condition).GetStarsEarned();
                    Debug.Log($"Попадание в мишень! Получено {actualWeightedStarValue} звезд.");
                }
            }
        }

        // Если это режим по количеству условий, проверяем обычным способом
        if (starCountMode == StarCountMode.ByConditionCount && completedWinConditions.Count > 0)
        {
            WinLevel();
        }
        // Если это режим взвешенного условия, проверяем, выполнено ли основное условие
        else if (starCountMode == StarCountMode.ByConditionWeight &&
                 weightedStarCondition != null &&
                 completedWinConditions.Contains(weightedStarCondition))
        {
            WinLevel();
        }
        // Если это режим попадания в мишень
        else if (starCountMode == StarCountMode.ByTargetHit &&
                 targetHitCondition != null &&
                 completedWinConditions.Contains(targetHitCondition))
        {
            WinLevel();
        }
    }

    // Расчет количества звезд для взвешенного режима
    private void CalculateWeightedStarValue()
    {
        // Здесь реализуйте логику вычисления количества звезд (1, 2 или 3)
        // По умолчанию поставим максимальное значение, вы можете заменить на свою логику
        actualWeightedStarValue = weightedStarValue;

        // Пример логики (заглушка):
        // Проверка дополнительных условий для определения качества прохождения
        int additionalConditionsMet = 0;
        foreach (var condition in winConditions)
        {
            if (condition != weightedStarCondition &&
                condition != null &&
                condition.IsConditionMet())
            {
                additionalConditionsMet++;
            }
        }

        // Примерная логика: чем больше дополнительных условий выполнено, тем больше звезд
        if (additionalConditionsMet == 0)
        {
            actualWeightedStarValue = 1;
        }
        else if (additionalConditionsMet == 1)
        {
            actualWeightedStarValue = 2;
        }
        else
        {
            actualWeightedStarValue = 3;
        }

        Debug.Log($"Установлено {actualWeightedStarValue} звезд из {weightedStarValue} возможных");
    }

    // Внешний метод для установки количества звезд (может вызываться из других скриптов)
    public void SetWeightedStarValue(int value)
    {
        actualWeightedStarValue = Mathf.Clamp(value, 0, weightedStarValue);
        Debug.Log($"Установлено {actualWeightedStarValue} звезд внешним методом");
    }

    // Проверка условий проигрыша
    public void CheckLoseConditions()
    {
        if (isLevelCompleted) return;

        foreach (var condition in loseConditions)
        {
            if (condition != null && condition.IsConditionMet())
            {
                LoseLevel();
                Debug.Log("v level managere uroven proigran");
                return;
            }
        }
    }

    // Параметры для задержки перед финальной проверкой
    private float victoryDelay = 1f;
    private bool victoryPending = false;
    private float victoryTimer = 0f;

    // Метод для начала процесса победы
    private void WinLevel()
    {
        if (isLevelCompleted) return;

        if (victoryPending)
        {
            // Если победа уже ожидается, просто проверяем, не появились ли новые выполненные условия
            return;
        }

        victoryPending = true;
        Debug.Log("Основное условие выполнено! Проверка дополнительных условий через задержку...");
    }

    // Метод для окончательного завершения уровня с победой
    private void CompleteWinLevel()
    {
        // Дополнительная проверка на повторный вызов
        if (isLevelCompleted)
        {
            Debug.Log("[LevelManager] CompleteWinLevel уже был вызван, пропускаем");
            return;
        }

        isLevelCompleted = true;
        victoryPending = false; // ДОБАВИТЬ: сбрасываем флаг

        // Определяем количество заработанных звезд в зависимости от режима
        int starsEarned;
        if (starCountMode == StarCountMode.ByConditionCount)
        {
            starsEarned = Mathf.Min(completedWinConditions.Count, 3);
        }
        else if (starCountMode == StarCountMode.ByTargetHit)
        {
            starsEarned = targetHitCondition != null ? targetHitCondition.GetStarsEarned() : 1;
        }
        else // StarCountMode.ByConditionWeight
        {
            starsEarned = actualWeightedStarValue;
        }

        List<string> completedConditionNames = completedWinConditions
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"[LevelManager] Уровень пройден! Заработано звезд: {starsEarned}");

        OnLevelWon?.Invoke(starsEarned, completedConditionNames);
    }

    // Получение максимально возможного количества звезд
    public int GetMaxStarsEarned()
    {
        if (starCountMode == StarCountMode.ByConditionCount)
        {
            // В режиме подсчета по количеству условий возвращаем количество выполненных условий
            return Mathf.Min(completedWinConditions.Count, 3); // Максимум 3 звезды
        }
        else if (starCountMode == StarCountMode.ByTargetHit)
        {
            // В режиме попадания в мишень возвращаем количество звезд из условия
            return targetHitCondition != null ? targetHitCondition.GetStarsEarned() : 1;
        }
        else // StarCountMode.ByConditionWeight
        {
            // В режиме взвешенного условия возвращаем актуальное значение звезд
            return actualWeightedStarValue;
        }
    }

    // Вызывается при проигрыше
    private void LoseLevel()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;

        // Получаем причины проигрыша
        List<string> loseReasons = loseConditions
            .Where(c => c != null && c.IsConditionMet())
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"Уровень проигран! Причина: {string.Join(", ", loseReasons)}");

        // Вызываем событие проигрыша
        OnLevelLost?.Invoke(0, loseReasons);
    }
    public void StopAllLevelProcesses()
    {
        isLevelCompleted = true;
        victoryPending = false;
        victoryTimer = 0f;
        autoCheckTimer = 0f;
        completedWinConditions.Clear();
        actualWeightedStarValue = 0;

        Debug.Log("[LevelManager] Все процессы уровня остановлены");
    }

    // Методы для работы с UI
    public List<string> GetCompletedConditionNames()
    {
        return completedWinConditions.Select(c => c.Name).ToList();
    }

    public bool IsLevelCompleted()
    {
        return isLevelCompleted;
    }

    // Дополнительные методы для получения информации о режиме звезд
    public StarCountMode GetStarCountMode()
    {
        return starCountMode;
    }

    public int GetWeightedMaxStars()
    {
        return weightedStarValue;
    }
}