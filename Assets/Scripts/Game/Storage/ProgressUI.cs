using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text currentLevelText;
    [SerializeField] private Text currentLevelStarsText;
    [SerializeField] private Text totalStarsText;
    [SerializeField] private Image[] currentLevelStarIcons;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressPercentText;

    [Header("Settings")]
    [SerializeField] private bool showInGameUI = true;
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private Color completedStarColor = Color.yellow;
    [SerializeField] private Color emptyStarColor = Color.gray;

    private StarManager starManager;
    private GameManager gameManager;

    private void Start()
    {
        starManager = StarManager.Instance;
        gameManager = GameManager.Instance;

        if (starManager != null)
        {
            starManager.OnLevelStarsUpdated += OnLevelStarsUpdated;
            starManager.OnTotalStarsUpdated += OnTotalStarsUpdated;
        }

        UpdateUI();
        DontDestroyOnLoad(gameObject);

    }

    private void OnDestroy()
    {
        if (starManager != null)
        {
            starManager.OnLevelStarsUpdated -= OnLevelStarsUpdated;
            starManager.OnTotalStarsUpdated -= OnTotalStarsUpdated;
        }
    }

    private void OnLevelStarsUpdated(int levelIndex, int newStars)
    {
        if (autoUpdate)
        {
            UpdateUI();
        }
    }

    private void OnTotalStarsUpdated(int totalStars)
    {
        if (autoUpdate)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (starManager == null) return;

        UpdateCurrentLevelInfo();
        UpdateTotalProgress();
        UpdateProgressSlider();
    }

    private void UpdateCurrentLevelInfo()
    {
        if (gameManager == null) return;

        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1;
        int currentStars = starManager.GetLevelStars(currentLevel);

        if (currentLevelText != null)
        {
            currentLevelText.text = $"Уровень {currentLevel + 1}";
        }

        if (currentLevelStarsText != null)
        {
            currentLevelStarsText.text = $"{currentStars}/3";
        }

        UpdateCurrentLevelStars(currentStars);
    }

    private void UpdateCurrentLevelStars(int stars)
    {
        if (currentLevelStarIcons == null) return;

        for (int i = 0; i < currentLevelStarIcons.Length; i++)
        {
            if (currentLevelStarIcons[i] != null)
            {
                Image starImage = currentLevelStarIcons[i];
                starImage.color = i < stars ? completedStarColor : emptyStarColor;

                // Можно также изменить sprite для заполненных/пустых звезд
                if (starImage.GetComponent<Outline>() != null)
                {
                    starImage.GetComponent<Outline>().enabled = i < stars;
                }
            }
        }
    }

    private void UpdateTotalProgress()
    {
        if (totalStarsText != null)
        {
            int totalStars = starManager.GetTotalStars();
            GameProgress progress = starManager.GetGameProgress();
            int maxStars = progress.levels.Count * 3;

            totalStarsText.text = $"Всего звезд: {totalStars}/{maxStars}";
        }
    }

    private void UpdateProgressSlider()
    {
        if (progressSlider == null) return;

        GameProgress progress = starManager.GetGameProgress();
        int totalStars = starManager.GetTotalStars();
        int maxStars = progress.levels.Count * 3;

        float progressValue = maxStars > 0 ? (float)totalStars / maxStars : 0f;
        progressSlider.value = progressValue;

        if (progressPercentText != null)
        {
            progressPercentText.text = $"{progressValue * 100:F1}%";
        }
    }

    // Методы для кнопок
    public void ShowDetailedProgress()
    {
        ProgressInspector inspector = FindObjectOfType<ProgressInspector>();
        if (inspector != null)
        {
            //inspector.ManualRefresh();
        }
        else
        {
            Debug.Log("ProgressInspector не найден в сцене");
        }
    }

    public void ResetProgress()
    {
        if (starManager != null)
        {
            starManager.ResetProgress();
            UpdateUI();
        }
    }

    public void UnlockAllLevels()
    {
        if (starManager != null)
        {
            starManager.UnlockAllLevels();
            UpdateUI();
        }
    }
}