using System.Collections.Generic;
using YG;
using UnityEngine;


namespace YG
{
    public partial class SavesYG
    {
        // Данные о звёздах для каждого уровня (ключ - индекс уровня, значение - количество звёзд)
        public List<int> levelStars = new List<int>();

        // Общее количество звёзд
        public int totalStars = 0;

        // Количество разблокированных уровней
        public int unlockedLevels = 1;

        // Дата последнего сохранения (для отладки)
        public string lastSaveTime = "";

        // Конструктор для инициализации базовых значений
        public SavesYG()
        {
            // Инициализируем список, если он пустой
            if (levelStars == null)
                levelStars = new List<int>();
        }
    }
}