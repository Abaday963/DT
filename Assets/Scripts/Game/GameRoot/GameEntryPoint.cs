using System.Collections;
using mBuilding.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint
{
    private static GameEntryPoint _instance;
    private Coroutines _coroutines;
    private UIRootView _uiRoot;
    private string _currentLevel = Scenes.LEVEL1; // �� ��������� ��������� ������ �������
    private bool _isFirstLaunch = true; // ���� ��� ������������ ������� �������

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
        // ���������, �������� �� ������� ����� ����� �� �������
        if (IsLevelScene(sceneName))
        {
            _currentLevel = sceneName;
            _isFirstLaunch = false; // ��� �� ������ ������, ���� �� � ������
            _coroutines.StartCoroutine(LoadAndStartLevel(_currentLevel));
            return;
        }
        if (sceneName == Scenes.MAIN_MENU)
        {
            _isFirstLaunch = false; // ��� �� ������ ������, ���� �� ��� � ����
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

    // ����������, �������� �� ����� ����� �� �������
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

    // ��������� ��������� �������
    public void LoadLevel(string levelName)
    {
        _currentLevel = levelName;
        _isFirstLaunch = false; // ����� ������� ������� ������ ���������� ��������
        _coroutines.StartCoroutine(LoadAndStartLevel(levelName));
    }

    // ��������� ��������� ������� (���� ������� ������� - ���������, ������������ � ������� ����)
    public void LoadNextLevel()
    {
        string nextLevel = GetNextLevel(_currentLevel);
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            // ���� ���������� ������ ���, ������������ � ������� ����
            _isFirstLaunch = false; // ��� ��� �� ������ ������
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        }
    }

    // ���������� ��������� �������
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
                return null; // ����� ���������� ������ ���������� null
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

        // ������������� �� �������
        sceneEntryPoint.GoToMainMainMenuRequested += () =>
        {
            _isFirstLaunch = false; // ��� ��� �� ������ ������
            _coroutines.StartCoroutine(LoadAndStartMainMenu());
        };

        // ��������� ����� ������� ��� �������� �� ��������� �������
        sceneEntryPoint.GoToNextLevelRequested += () =>
        {
            LoadNextLevel();
        };

        // ��������� ������� ��� ������������ �������� ������
        sceneEntryPoint.RestartLevelRequested += () =>
        {
            LoadLevel(_currentLevel);
        };

        _uiRoot.HideLoadingScreen();
    }

    private IEnumerator LoadAndStartMainMenu()
    {
        // ���������� ����� �������� ������ ���� ��� �� ������ ������
        if (!_isFirstLaunch)
        {
            _uiRoot.ShowLoadingScreen();
        }

        yield return LoadScene(Scenes.BOOT);
        yield return LoadScene(Scenes.MAIN_MENU);

        // ���� ������ ���� ��� �� ������ ������
        if (!_isFirstLaunch)
        {
            yield return new WaitForSeconds(1.5f);
        }

        var sceneEntryPoint = Object.FindFirstObjectByType<MainMenuEntryPoint>();
        sceneEntryPoint.Run(_uiRoot);

        // ��������� ���������� ������� ��� ������ ���� � ��������� ���������� ���������
        sceneEntryPoint.GoToGameplaySceneRequested += (string levelToLoad) =>
        {
            // ���� ������� �� ������ ��� ������, ��������� ������ ������� �� ���������
            string level = string.IsNullOrEmpty(levelToLoad) ? Scenes.LEVEL1 : levelToLoad;
            LoadLevel(level);
        };

        // �������� ����� �������� ������ ���� �� ��� �������
        if (!_isFirstLaunch)
        {
            _uiRoot.HideLoadingScreen();
        }

        // ����� ������� ������� ����, ���������� ����
        _isFirstLaunch = false;
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}