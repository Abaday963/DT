using System.Collections;
using mBuilding.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint
{
    private static GameEntryPoint _instance;
    private Coroutines _coroutines;
    private UIRootView _uiRoot;
    private string _currentLevel = Scenes.LEVEL1; // По умолчанию загружаем первый уровень
    private bool _isFirstLaunch = true; // Флаг для отслеживания первого запуска

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void AutostartGame()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        _instance = new GameEntryPoint();
        _instance.RunGame();
    }

    private GameEntryPoint()
    {
        _coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
        Object.DontDestroyOnLoad(_coroutines.gameObject);
        var prefabUIRoot = Resources.Load<UIRootView>("UIRoot");
        _uiRoot = Object.Instantiate(prefabUIRoot);
        Object.DontDestroyOnLoad(_uiRoot.gameObject);
    }

    private async void RunGame()
    {
#if UNITY_EDITOR
        var sceneName = SceneManager.GetActiveScene().name;
        // Проверяем, является ли текущая сцена одним из уровней
        if (IsLevelScene(sceneName))
        {
            _currentLevel = sceneName;
            _isFirstLaunch = false; // Это не первый запуск, если мы в уровне
            _coroutines.StartCoroutine(LoadAndStartLevel(_currentLevel));
            return;
        }
        if (sceneName == Scenes.MAIN_MENU)
        {
            _isFirstLaunch = false; // Это не первый запуск, если мы уже в меню
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
            return;
        }
        if (sceneName != Scenes.BOOT)
        {
            return;
        }
#endif
        _coroutines.StartCoroutine(LoadAndStartMainMenu());
    }

    // Определяет, является ли сцена одним из уровней
    private bool IsLevelScene(string sceneName)
    {
        return sceneName == Scenes.LEVEL1 ||
               sceneName == Scenes.LEVEL2 ||
               sceneName == Scenes.LEVEL3 ||
               sceneName == Scenes.LEVEL4 ||
               sceneName == Scenes.LEVEL5 ||
               sceneName == Scenes.LEVEL6 ||
               sceneName == Scenes.LEVEL7 ||
               sceneName == Scenes.LEVEL8 ||
               sceneName == Scenes.LEVEL9 ||
               sceneName == Scenes.LEVEL10;
    }

    // Загружает указанный уровень
    public void LoadLevel(string levelName)
    {
        _currentLevel = levelName;
        _isFirstLaunch = false; // После первого запуска всегда показываем загрузку
        _coroutines.StartCoroutine(LoadAndStartLevel(levelName));
    }

    // Загружает следующий уровень (если текущий уровень - последний, возвращаемся в главное меню)
    public void LoadNextLevel()
    {
        string nextLevel = GetNextLevel(_currentLevel);
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            // Если следующего уровня нет, возвращаемся в главное меню
            _isFirstLaunch = false; // Это уже не первый запуск
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        }
    }

    // Определяет следующий уровень
    private string GetNextLevel(string currentLevel)
    {
        switch (currentLevel)
        {
            case Scenes.LEVEL1:
                return Scenes.LEVEL2;
            case Scenes.LEVEL2:
                return Scenes.LEVEL3;
            case Scenes.LEVEL3:
                return Scenes.LEVEL4;
            case Scenes.LEVEL4:
                return Scenes.LEVEL5;
            case Scenes.LEVEL5:
                return Scenes.LEVEL6;
            case Scenes.LEVEL6:
                return Scenes.LEVEL7;
            case Scenes.LEVEL7:
                return Scenes.LEVEL8;
            case Scenes.LEVEL8:
                return Scenes.LEVEL9;
            case Scenes.LEVEL9:
                return Scenes.LEVEL10;
            default:
                return null; // После последнего уровня возвращаем null
        }
    }

    private IEnumerator LoadAndStartLevel(string levelName)
    {
        _uiRoot.ShowLoadingScreen();
        yield return LoadScene(Scenes.BOOT);
        yield return LoadScene(levelName);
        yield return new WaitForSeconds(0.5f);

        var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
        sceneEntryPoint.Run(_uiRoot);

        // Подписываемся на события
        sceneEntryPoint.GoToMainMainMenuRequested += () =>
        {
            _isFirstLaunch = false; // Это уже не первый запуск
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        };

        // Добавляем новое событие для перехода на следующий уровень
        sceneEntryPoint.GoToNextLevelRequested += () =>
        {
            LoadNextLevel();
        };

        // Добавляем событие для перезагрузки текущего уровня
        sceneEntryPoint.RestartLevelRequested += () =>
        {
            LoadLevel(_currentLevel);
        };

        _uiRoot.HideLoadingScreen();
    }

    private IEnumerator LoadAndStartMainMenu()
    {
        // Показываем экран загрузки только если это НЕ первый запуск
        if (!_isFirstLaunch)
        {
            _uiRoot.ShowLoadingScreen();
        }

        yield return LoadScene(Scenes.BOOT);
        yield return LoadScene(Scenes.MAIN_MENU);

        // Ждем только если это НЕ первый запуск
        if (!_isFirstLaunch)
        {
            yield return new WaitForSeconds(1.5f);
        }

        var sceneEntryPoint = Object.FindFirstObjectByType<MainMenuEntryPoint>();
        sceneEntryPoint.Run(_uiRoot);

        // Обновляем обработчик события для старта игры с передачей строкового параметра
        sceneEntryPoint.GoToGameplaySceneRequested += (string levelToLoad) =>
        {
            // Если уровень не указан или пустой, загружаем первый уровень по умолчанию
            string level = string.IsNullOrEmpty(levelToLoad) ? Scenes.LEVEL1 : levelToLoad;
            LoadLevel(level);
        };

        // Скрываем экран загрузки только если он был показан
        if (!_isFirstLaunch)
        {
            _uiRoot.HideLoadingScreen();
        }

        // После первого запуска меню, сбрасываем флаг
        _isFirstLaunch = false;
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}