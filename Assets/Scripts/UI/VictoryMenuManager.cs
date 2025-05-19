using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryMenuManager : MonoBehaviour
{
    [Header("Элементы меню")]
    [SerializeField] private GameObject victoryMenuPanel;
    [SerializeField] private Text starsCountText;

    [Header("Кнопки")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Настройки уровней")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool isLastLevel = false; // Установите true, если это последний уровень

    private int collectedStars = 0;
    private string currentSceneName;

    private void Awake()
    {
        // Изначально скрываем меню победы
        if (victoryMenuPanel != null)
            victoryMenuPanel.SetActive(false);

        // Получаем имя текущей сцены
        currentSceneName = SceneManager.GetActiveScene().name;

        // Назначаем обработчики нажатий на кнопки
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            // Скрываем кнопку следующего уровня, если это последний уровень
            if (isLastLevel)
                nextLevelButton.gameObject.SetActive(false);
        }

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    // Метод для вызова из других скриптов при победе
    public void ShowVictoryMenu(int stars)
    {
        collectedStars = stars;

        // Отображаем количество собранных звезд
        if (starsCountText != null)
            starsCountText.text = "Звезды: " + collectedStars.ToString();

        // Показываем меню победы
        if (victoryMenuPanel != null)
            victoryMenuPanel.SetActive(true);

        // Можно добавить сохранение прогресса
        SaveProgress();
    }

    private void RestartLevel()
    {
        // Перезапускаем текущий уровень
        SceneManager.LoadScene(currentSceneName);
    }

    private void LoadNextLevel()
    {
        // Получаем индекс следующей сцены
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Проверяем, существует ли следующая сцена
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Следующий уровень не найден. Возврат в главное меню.");
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        // Загружаем сцену главного меню
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SaveProgress()
    {
        // Здесь можно реализовать сохранение прогресса игрока
        // Например, сохранение количества звезд для текущего уровня
        PlayerPrefs.SetInt(currentSceneName + "_Stars", collectedStars);
        PlayerPrefs.Save();

        // Если это не последний уровень, разблокируем следующий
        if (!isLastLevel)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            PlayerPrefs.SetInt("Level_" + nextSceneIndex + "_Unlocked", 1);
            PlayerPrefs.Save();
        }
    }
}