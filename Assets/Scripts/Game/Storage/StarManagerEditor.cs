#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StarManager))]
public class StarManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StarManager starManager = (StarManager)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Данные прогресса доступны только во время игры", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== ТЕКУЩИЙ ПРОГРЕСС ===", EditorStyles.boldLabel);

        if (starManager != null)
        {
            GameProgress progress = starManager.GetGameProgress();

            EditorGUILayout.LabelField($"Всего звезд: {starManager.GetTotalStars()}");
            EditorGUILayout.LabelField($"Максимум: {progress.levels.Count * 3}");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Прогресс по уровням:", EditorStyles.boldLabel);

            for (int i = 0; i < progress.levels.Count; i++)
            {
                LevelProgress level = progress.levels[i];
                string lockStatus = level.isUnlocked ? "🔓" : "🔒";
                string stars = GetStarsString(level.stars);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Уровень {i + 1}:", GUILayout.Width(80));
                EditorGUILayout.LabelField($"{stars} {lockStatus}", GUILayout.Width(100));

                // Кнопки для быстрого изменения
                if (GUILayout.Button("0★", GUILayout.Width(30)))
                {
                    starManager.SetLevelStars(i, 0);
                }
                if (GUILayout.Button("1★", GUILayout.Width(30)))
                {
                    starManager.SetLevelStars(i, 1);
                }
                if (GUILayout.Button("2★", GUILayout.Width(30)))
                {
                    starManager.SetLevelStars(i, 2);
                }
                if (GUILayout.Button("3★", GUILayout.Width(30)))
                {
                    starManager.SetLevelStars(i, 3);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Сбросить прогресс"))
            {
                starManager.ResetProgress();
            }
            if (GUILayout.Button("Разблокировать все"))
            {
                starManager.UnlockAllLevels();
            }
            if (GUILayout.Button("Дать 3★ всем"))
            {
                starManager.GiveMaxStarsToAll();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(starManager);
        }
    }

    private string GetStarsString(int stars)
    {
        switch (stars)
        {
            case 0: return "☆☆☆";
            case 1: return "⭐☆☆";
            case 2: return "⭐⭐☆";
            case 3: return "⭐⭐⭐";
            default: return "???";
        }
    }
}

[System.Serializable]
public class ProgressEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;

    // ДОБАВЛЯЕМ НАСТРОЙКУ СМЕЩЕНИЯ СЦЕН
    private const int FIRST_LEVEL_SCENE_INDEX = 2; // Индекс первой игровой сцены

    [MenuItem("Tools/Progress Inspector")]
    public static void ShowWindow()
    {
        GetWindow<ProgressEditorWindow>("Progress Inspector");
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Инструмент доступен только во время игры", MessageType.Warning);
            return;
        }

        StarManager starManager = StarManager.Instance;
        if (starManager == null)
        {
            EditorGUILayout.HelpBox("StarManager не найден в сцене", MessageType.Error);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GameProgress progress = starManager.GetGameProgress();

        EditorGUILayout.LabelField("=== ПРОГРЕСС ИГРЫ ===", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Всего звезд: {progress.totalStars}/{progress.levels.Count * 3}");

        float progressPercent = (float)progress.totalStars / (progress.levels.Count * 3) * 100f;
        EditorGUILayout.LabelField($"Прогресс: {progressPercent:F1}%");

        EditorGUILayout.Space();

        // Статистика
        int completedLevels = 0;
        int perfectLevels = 0;
        int unlockedLevels = 0;

        foreach (var level in progress.levels)
        {
            if (level.isUnlocked) unlockedLevels++;
            if (level.stars > 0) completedLevels++;
            if (level.stars == 3) perfectLevels++;
        }

        EditorGUILayout.LabelField("=== СТАТИСТИКА ===", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Разблокировано: {unlockedLevels}/{progress.levels.Count}");
        EditorGUILayout.LabelField($"Пройдено: {completedLevels}/{progress.levels.Count}");
        EditorGUILayout.LabelField($"Идеально: {perfectLevels}/{progress.levels.Count}");

        EditorGUILayout.Space();

        // Уровни
        EditorGUILayout.LabelField("=== УРОВНИ ===", EditorStyles.boldLabel);

        // ДОБАВЛЯЕМ ИНФОРМАЦИЮ О МАППИНГЕ
        EditorGUILayout.HelpBox($"Уровни маппятся на сцены начиная с индекса {FIRST_LEVEL_SCENE_INDEX}", MessageType.Info);

        for (int i = 0; i < progress.levels.Count; i++)
        {
            LevelProgress level = progress.levels[i];

            EditorGUILayout.BeginHorizontal();

            string lockIcon = level.isUnlocked ? "🔓" : "🔒";
            int sceneIndex = FIRST_LEVEL_SCENE_INDEX + i; // ПРАВИЛЬНЫЙ МАППИНГ
            EditorGUILayout.LabelField($"Уровень {i + 1} (Сцена {sceneIndex}) {lockIcon}", GUILayout.Width(150));

            // Звезды
            for (int star = 1; star <= 3; star++)
            {
                string starIcon = level.stars >= star ? "⭐" : "☆";
                EditorGUILayout.LabelField(starIcon, GUILayout.Width(20));
            }

            GUILayout.FlexibleSpace();

            // ИСПРАВЛЕННАЯ кнопка "Перейти к уровню"
            if (GUILayout.Button("Перейти", GUILayout.Width(60)))
            {
                if (EditorApplication.isPlaying)
                {
                    // ИСПОЛЬЗУЕМ ПРАВИЛЬНЫЙ ИНДЕКС СЦЕНЫ!
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
                    Debug.Log($"[ProgressEditor] Переход к уровню {i + 1} (сцена {sceneIndex})");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Кнопки управления
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Обновить"))
        {
            Repaint();
        }
        if (GUILayout.Button("Сбросить прогресс"))
        {
            if (EditorUtility.DisplayDialog("Подтверждение",
                "Вы уверены, что хотите сбросить весь прогресс?", "Да", "Отмена"))
            {
                starManager.ResetProgress();
            }
        }
        EditorGUILayout.EndHorizontal();

        // ДОБАВЛЯЕМ РАЗДЕЛ ОТЛАДКИ МАППИНГА
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== ОТЛАДКА МАППИНГА ===", EditorStyles.boldLabel);

        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelIndex = currentSceneIndex - FIRST_LEVEL_SCENE_INDEX;

        EditorGUILayout.LabelField($"Текущая сцена: {currentSceneIndex}");
        if (currentLevelIndex >= 0 && currentLevelIndex < progress.levels.Count)
        {
            EditorGUILayout.LabelField($"Это уровень: {currentLevelIndex + 1}");
            LevelProgress currentLevel = progress.levels[currentLevelIndex];
            EditorGUILayout.LabelField($"Звезды текущего уровня: {currentLevel.stars}");
        }
        else
        {
            EditorGUILayout.LabelField("Не игровая сцена");
        }

        EditorGUILayout.EndScrollView();
    }

    private void Update()
    {
        // Автообновление каждые 2 секунды
        if (EditorApplication.timeSinceStartup % 2 < 0.1f)
        {
            Repaint();
        }
    }
}
#endif