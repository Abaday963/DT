using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Условия победы и проигрыша")]
    [SerializeField] private WinCondition[] winConditions;
    [SerializeField] private LoseCondition[] loseConditions;

    [Header("Ссылки на объекты уровня")]
    [SerializeField] private Building1 targetBuilding;
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
                WinLevel(); // Повторный вызов завершит уровень
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
        if (targetBuilding == null)
        {
            targetBuilding = FindAnyObjectByType<Building1>();
        }

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

    }

    // Сбрасываем все условия
    public void ResetAllConditions()
    {
        isLevelCompleted = false;
        completedWinConditions.Clear();

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
            }
        }

        // Если есть хотя бы одно выполненное условие и здание горит - уровень пройден
        if (completedWinConditions.Count > 0 && targetBuilding.IsOnFire())
        {
            WinLevel();
        }
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

    // Вызывается при победе
    private float victoryDelay = 1f; // Задержка в секундах перед финальной проверкой
    private bool victoryPending = false;
    private float victoryTimer = 0f;

    // Изменения в методе WinLevel()
    private void WinLevel()
    {
        if (isLevelCompleted || victoryPending) return;

        victoryPending = true;
        Debug.Log("Основное условие выполнено! Проверка дополнительных условий через задержку...");
        StartCoroutine(DelayedVictory());
    }

    private IEnumerator DelayedVictory()
    {
        yield return new WaitForSeconds(victoryDelay);

        isLevelCompleted = true;

        int starsEarned = completedWinConditions.Count;
        List<string> completedConditionNames = completedWinConditions
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"Уровень пройден! Заработано звезд: {starsEarned}");

        OnLevelWon?.Invoke(starsEarned, completedConditionNames);
    }

    // Изменения в методе GetMaxStarsEarned()
    public int GetMaxStarsEarned()
    {
        // Изменяем: возвращаем количество выполненных условий вместо максимального значения звезд
        return completedWinConditions.Count;
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

    // Методы для работы с UI
    public List<string> GetCompletedConditionNames()
    {
        return completedWinConditions.Select(c => c.Name).ToList();
    }

    public bool IsLevelCompleted()
    {
        return isLevelCompleted;
    }
}