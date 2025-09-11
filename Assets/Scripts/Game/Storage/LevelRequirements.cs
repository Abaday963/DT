using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelRequirements", menuName = "Game/Level Requirements")]
public class LevelRequirements : ScriptableObject
{
    [System.Serializable]
    public class LevelRequirement
    {
        [Header("Настройки уровня")]
        public string levelName;
        [Tooltip("Логический индекс уровня (начиная с 0)")]
        public int levelIndex;

        [Header("Требования для разблокировки")]
        [Tooltip("Минимальное количество звезд для разблокировки")]
        public int requiredStars;

        [Tooltip("Обязательные уровни, которые должны быть пройдены")]
        public int[] requiredLevels;

        [Header("Дополнительные настройки")]
        [Tooltip("Всегда разблокирован (например, первый уровень)")]
        public bool alwaysUnlocked;
    }

    [Header("Требования для всех уровней")]
    public LevelRequirement[] levelRequirements = new LevelRequirement[10];

    private void OnValidate()
    {
        // Автоматически заполняем базовые данные при создании
        if (levelRequirements != null)
        {
            for (int i = 0; i < levelRequirements.Length; i++)
            {
                if (levelRequirements[i] == null)
                {
                    levelRequirements[i] = new LevelRequirement();
                }

                // Автоматически заполняем имя уровня и индекс
                levelRequirements[i].levelIndex = i;
                levelRequirements[i].levelName = $"LEVEL{i + 1}";

                // Первый уровень всегда разблокирован
                if (i == 0)
                {
                    levelRequirements[i].alwaysUnlocked = true;
                    levelRequirements[i].requiredStars = 0;
                }
            }
        }
    }

    /// <summary>
    /// Получить требования для конкретного уровня
    /// </summary>
    public LevelRequirement GetLevelRequirement(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelRequirements.Length)
        {
            return levelRequirements[levelIndex];
        }
        return null;
    }

    /// <summary>
    /// Проверить, разблокирован ли уровень
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex, StarManager starManager)
    {
        var requirement = GetLevelRequirement(levelIndex);
        if (requirement == null) return false;

        // Если уровень всегда разблокирован
        if (requirement.alwaysUnlocked) return true;

        // Проверяем общее количество звезд
        int totalStars = starManager.GetTotalStars();
        if (totalStars < requirement.requiredStars)
        {
            return false;
        }

        // Проверяем обязательные уровни
        if (requirement.requiredLevels != null)
        {
            foreach (int requiredLevel in requirement.requiredLevels)
            {
                if (starManager.GetLevelStars(requiredLevel) == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Получить текст с объяснением требований для разблокировки
    /// </summary>
    public string GetRequirementText(int levelIndex, StarManager starManager)
    {
        var requirement = GetLevelRequirement(levelIndex);
        if (requirement == null || requirement.alwaysUnlocked)
            return "";

        string text = "";

        // Требования по звездам
        if (requirement.requiredStars > 0)
        {
            int currentStars = starManager.GetTotalStars();
            if (currentStars < requirement.requiredStars)
            {
                text += $"Нужно {requirement.requiredStars} звезд (у вас {currentStars})";
            }
        }

        // Требования по обязательным уровням
        if (requirement.requiredLevels != null && requirement.requiredLevels.Length > 0)
        {
            foreach (int requiredLevel in requirement.requiredLevels)
            {
                if (starManager.GetLevelStars(requiredLevel) == 0)
                {
                    if (!string.IsNullOrEmpty(text)) text += "\n";
                    text += $"Пройдите уровень {requiredLevel + 1}";
                }
            }
        }

        return text;
    }
}