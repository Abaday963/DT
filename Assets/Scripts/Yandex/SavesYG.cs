using System.Collections.Generic;
using YG;
using UnityEngine;

namespace YG
{
    public partial class SavesYG
    {
        // СТАРЫЕ ДАННЫЕ (можете оставить для совместимости или удалить)
        public List<int> levelStars = new List<int>();
        public int totalStars = 0;
        public int unlockedLevels = 1;
        public string lastSaveTime = "";

        // НОВАЯ СИСТЕМА - сохранение всего прогресса игры в JSON
        public string GameProgress = "";

        // Конструктор для инициализации базовых значений
        public SavesYG()
        {
            // Инициализируем список, если он пустой
            if (levelStars == null)
                levelStars = new List<int>();

            // Инициализируем новое поле
            if (string.IsNullOrEmpty(GameProgress))
                GameProgress = "";
        }
    }
}