using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public string playerName = "Player";
    public List<int> completedLevels = new List<int>();

    // Вспомогательные методы
    public void AddCompletedLevel(int level)
    {
        if (!completedLevels.Contains(level))
        {
            completedLevels.Add(level);
        }
    }

    public bool IsLevelCompleted(int level)
    {
        return completedLevels.Contains(level);
    }
}