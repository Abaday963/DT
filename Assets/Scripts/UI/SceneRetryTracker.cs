using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneRetryTracker : MonoBehaviour
{
    public int retryThreshold = 3; // Кол-во перезапусков до показа подсказки
    public GameObject hintUI; // UI элемент с подсказкой

    private string sceneKey;

    void Awake()
    {
        // Уникальный ключ для текущей сцены
        sceneKey = "RetryCount_" + SceneManager.GetActiveScene().name;

        // Увеличиваем счётчик каждый раз при загрузке сцены
        int currentRetry = PlayerPrefs.GetInt(sceneKey, 0);
        currentRetry++;
        PlayerPrefs.SetInt(sceneKey, currentRetry);
        PlayerPrefs.Save();

        // Показываем подсказку если превышен порог
        if (hintUI != null)
        {
            hintUI.SetActive(currentRetry >= retryThreshold);
        }
    }

    public void ResetRetryCount()
    {
        PlayerPrefs.SetInt(sceneKey, 0);
        PlayerPrefs.Save();
    }

    // Для теста: вызывай этот метод при прохождении уровня
    public void OnLevelComplete()
    {
        ResetRetryCount();
    }
}
