using System.Collections;
using mBuilding.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint
{
    private static GameEntryPoint _instance;
    private Coroutines _coroutines;
    private UIRootView _uiRoot;
    private string _currentLevel = Scenes.LEVEL1; // ѕо умолчанию загружаем первый уровень

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
        // ѕровер€ем, €вл€етс€ ли текуща€ сцена одним из уровней
        if (IsLevelScene(sceneName))
        {
            _currentLevel = sceneName;
            _coroutines.StartCoroutine(LoadAndStartLevel(_currentLevel));
            return;
        }
        if (sceneName == Scenes.MAIN_MENU)
        {
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

    // ќпредел€ет, €вл€етс€ ли сцена одним из уровней
    private bool IsLevelScene(string sceneName)
    {
        return sceneName == Scenes.LEVEL1 ||
               sceneName == Scenes.LEVEL2 ||
               sceneName == Scenes.LEVEL3 ||
               sceneName == Scenes.LEVEL4 ||
               sceneName == Scenes.LEVEL5;
    }

    // «агружает указанный уровень
    public void LoadLevel(string levelName)
    {
        _currentLevel = levelName;
        _coroutines.StartCoroutine(LoadAndStartLevel(levelName));
    }

    // «агружает следующий уровень (если текущий уровень - последний, возвращаемс€ в главное меню)
    public void LoadNextLevel()
    {
        string nextLevel = GetNextLevel(_currentLevel);
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            // ≈сли следующего уровн€ нет, возвращаемс€ в главное меню
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        }
    }

    // ќпредел€ет следующий уровень
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
            default:
                return null; // ѕосле последнего уровн€ возвращаем null
        }
    }

    private IEnumerator LoadAndStartLevel(string levelName)
    {
        _uiRoot.ShowLoadingScreen();
        yield return LoadScene(Scenes.BOOT);
        yield return LoadScene(levelName);
        yield return new WaitForSeconds(1);

        var sceneEntryPoint = Object.FindFirstObjectByType<GameplayEntryPoint>();
        sceneEntryPoint.Run(_uiRoot);

        // ѕодписываемс€ на событи€
        sceneEntryPoint.GoToMainMainMenuRequested += () =>
        {
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        };

        // ƒобавл€ем новое событие дл€ перехода на следующий уровень
        sceneEntryPoint.GoToNextLevelRequested += () =>
        {
            LoadNextLevel();
        };

        // ƒобавл€ем событие дл€ перезагрузки текущего уровн€
        sceneEntryPoint.RestartLevelRequested += () =>
        {
            LoadLevel(_currentLevel);
        };

        _uiRoot.HideLoadingScreen();
    }

    private IEnumerator LoadAndStartMainMenu()
    {
        _uiRoot.ShowLoadingScreen();
        yield return LoadScene(Scenes.BOOT);
        yield return LoadScene(Scenes.MAIN_MENU);
        yield return new WaitForSeconds(2);

        var sceneEntryPoint = Object.FindFirstObjectByType<MainMenuEntryPoint>();
        sceneEntryPoint.Run(_uiRoot);

        // ќбновл€ем обработчик событи€ дл€ старта игры с передачей строкового параметра
        sceneEntryPoint.GoToGameplaySceneRequested += (string levelToLoad) =>
        {
            // ≈сли уровень не указан или пустой, загружаем первый уровень по умолчанию
            string level = string.IsNullOrEmpty(levelToLoad) ? Scenes.LEVEL1 : levelToLoad;
            LoadLevel(level);
        };

        _uiRoot.HideLoadingScreen();
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}