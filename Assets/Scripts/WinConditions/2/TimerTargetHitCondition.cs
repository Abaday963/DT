using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TimerTargetHitCondition : WinCondition
{
    [Header("Настройки целей")]
    [SerializeField] private Transform[] targets; // Массив мишеней
    [SerializeField] private int requiredTargetHits = 4; // Требуемое количество попаданий

    [Header("Настройки таймера")]
    [SerializeField] private TimerSlider levelTimer; // Ссылка на таймер уровня

    [Header("UI")]
    [SerializeField] private TargetHitCounter hitCounter; // Ссылка на счетчик попаданий

    // Внутренние переменные
    private int currentHits = 0;
    private List<Transform> hitTargets = new List<Transform>();
    private int starsEarned = 0;

    // Публичное свойство для доступа к requiredTargetHits
    public int RequiredTargetHits => requiredTargetHits;

    private void Awake()
    {
        conditionName = "Попадание в цели";
        conditionDescription = "Попадите во все цели";

        Debug.Log("TimerTargetHitCondition инициализирован");
    }

    private void Start()
    {
        // Найти все мишени, если не заданы вручную
        if (targets == null || targets.Length == 0)
        {
            targets = GameObject.FindGameObjectsWithTag("Target")
                             .Select(go => go.transform)
                             .ToArray();

            Debug.Log($"Автоматически найдено {targets.Length} целей с тегом 'Target'");
        }

        // Найти таймер, если не задан вручную
        if (levelTimer == null)
        {
            levelTimer = FindObjectOfType<TimerSlider>();
            if (levelTimer != null)
                Debug.Log("Таймер уровня найден автоматически");
            else
                Debug.LogWarning("Таймер уровня не найден!");
        }

        // Найти счетчик попаданий, если не задан вручную
        if (hitCounter == null)
        {
            hitCounter = FindObjectOfType<TargetHitCounter>();
        }

        // Обновить счетчик при старте
        if (hitCounter != null)
        {
            hitCounter.UpdateCounter(currentHits, requiredTargetHits);
        }
    }

    // Вызывается, когда игрок попадает в цель
    public void RegisterHit(Transform target)
    {
        Debug.Log($"RegisterHit вызван для {target.name}");

        // Проверяем, что цель еще не была поражена
        if (!hitTargets.Contains(target))
        {
            hitTargets.Add(target);
            currentHits++;

            Debug.Log($"Попадание в цель! {currentHits}/{requiredTargetHits}");

            // Обновить счетчик попаданий
            if (hitCounter != null)
            {
                hitCounter.UpdateCounter(currentHits, requiredTargetHits);
            }

            // Определяем количество звезд в зависимости от текущего состояния таймера
            if (levelTimer != null)
            {
                TimerState currentState = levelTimer.GetCurrentState();
                switch (currentState)
                {
                    case TimerState.Green:
                        starsEarned = 3;
                        Debug.Log("Зеленый таймер - 3 звезды");
                        break;
                    case TimerState.Yellow:
                        starsEarned = 2;
                        Debug.Log("Желтый таймер - 2 звезды");
                        break;
                    case TimerState.Red:
                        starsEarned = 1;
                        Debug.Log("Красный таймер - 1 звезда");
                        break;
                    default:
                        starsEarned = 0;
                        Debug.Log("Время истекло - 0 звезд");
                        break;
                }

                // Устанавливаем количество звезд, если есть доступ к LevelManager
                if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
                {
                    starsAwarded = starsEarned;
                    GameManager.Instance.LevelManager.SetWeightedStarValue(starsEarned);
                }
            }

            // Проверяем, выполнено ли условие
            if (IsConditionMet())
            {
                Debug.Log("Условие выполнено! Требуемое количество целей поражено.");
                if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
                {
                    GameManager.Instance.LevelManager.CheckWinConditions();
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance или LevelManager не найдены!");
                }
            }
        }
        else
        {
            Debug.Log($"Цель {target.name} уже была поражена ранее");
        }
    }

    // Получить количество заработанных звезд
    public int GetStarsEarned()
    {
        return starsEarned;
    }

    public override bool IsConditionMet()
    {
        bool result = currentHits >= requiredTargetHits;
        //Debug.Log($"IsConditionMet: {result} ({currentHits}/{requiredTargetHits})");
        return result;
    }

    public override void ResetCondition()
    {
        Debug.Log("Сброс условия TimerTargetHitCondition");
        currentHits = 0;
        hitTargets.Clear();
        starsEarned = 0;

        // Обновить счетчик при сбросе
        if (hitCounter != null)
        {
            hitCounter.UpdateCounter(currentHits, requiredTargetHits);
        }
    }
}

// 2. Таймер уровня для отслеживания состояний (зеленый, желтый, красный)
public enum TimerState
{
    Green,  // Первые 5 секунд
    Yellow, // Следующие 4 секунды
    Red,    // Оставшееся время
    Expired // Время вышло
}

// ========================================
// ОТДЕЛЬНЫЙ ФАЙЛ: TargetHitCounter.cs
// ========================================

/*
using UnityEngine;
using TMPro;

public class TargetHitCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TimerTargetHitCondition targetCondition;

    private void Start()
    {
        // Автоматически найти компоненты, если не заданы
        if (counterText == null)
        {
            counterText = GetComponent<TextMeshProUGUI>();
        }

        if (targetCondition == null)
        {
            targetCondition = FindObjectOfType<TimerTargetHitCondition>();
        }

        UpdateCounter(0, targetCondition != null ? targetCondition.RequiredTargetHits : 0);
    }

    public void UpdateCounter(int current, int required)
    {
        if (counterText != null)
        {
            counterText.text = $"{current}/{required}";
        }
    }
}
*/