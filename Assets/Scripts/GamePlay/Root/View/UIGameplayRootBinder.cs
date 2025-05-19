using UnityEngine;
using System;

public class UIGameplayRootBinder : MonoBehaviour
{
    public event Action GoToMainMenuButtonClicked;
    public event Action NextLevelButtonClicked;
    public event Action RestartLevelButtonClicked;

    // Ваши существующие компоненты UI
    [SerializeField] public GameObject _mainMenuButton;
    [SerializeField] public GameObject _nextLevelButton;
    [SerializeField] public GameObject _restartLevelButton;

    private void Start()
    {
        // Настраиваем кнопки, если они назначены в инспекторе
        if (_mainMenuButton != null)
        {
            _mainMenuButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                GoToMainMenuButtonClicked?.Invoke();
            });
        }

        if (_nextLevelButton != null)
        {
            _nextLevelButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                NextLevelButtonClicked?.Invoke();
            });
        }

        if (_restartLevelButton != null)
        {
            _restartLevelButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                RestartLevelButtonClicked?.Invoke();
            });
        }
    }

    // Методы для программного вызова кнопок (например, при завершении уровня)
    public void OnMainMenuButtonClicked()
    {
        GoToMainMenuButtonClicked?.Invoke();
    }

    public void OnNextLevelButtonClicked()
    {
        NextLevelButtonClicked?.Invoke();
    }

    public void OnRestartLevelButtonClicked()
    {
        RestartLevelButtonClicked?.Invoke();
    }
}