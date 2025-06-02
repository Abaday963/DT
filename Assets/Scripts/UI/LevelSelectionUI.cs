using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text totalStarsText;
    [SerializeField] private Transform levelButtonsParent;
    [SerializeField] private GameObject levelButtonPrefab;

    [Header("Settings")]
    [SerializeField] private int totalLevels = 20;
    [SerializeField] private int firstLevelSceneIndex = 2; // Индекс первого уровня в Build Settings

    private List<LevelButton> levelButtons = new List<LevelButton>();

    private void Start()
    {
        // Подписываемся на события StarManager
        if (StarManager.Instance != null)
        {
            StarManager.Instance.OnStarsChanged += UpdateTotalStarsUI;
            StarManager.Instance.OnLevelUnlocked += OnLevelUnlocked;
            StarManager.Instance.OnProgressLoaded += RefreshUI;
        }

        CreateLevelButtons();
        RefreshUI();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (StarManager.Instance != null)
        {
            StarManager.Instance.OnStarsChanged -= UpdateTotalStarsUI;
            StarManager.Instance.OnLevelUnlocked -= OnLevelUnlocked;
            StarManager.Instance.OnProgressLoaded -= RefreshUI;
        }
    }

    private void CreateLevelButtons()
    {
        if (levelButtonPrefab == null || levelButtonsParent == null) return;

        for (int i = 0; i < totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsParent);
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();

            if (levelButton != null)
            {
                levelButton.Initialize(i, this);
                levelButtons.Add(levelButton);
            }
        }
    }

    private void RefreshUI()
    {
        UpdateTotalStarsUI(StarManager.Instance?.GetTotalStars() ?? 0);

        // Обновляем все кнопки уровней
        for (int i = 0; i < levelButtons.Count; i++)
        {
            UpdateLevelButton(i);
        }
    }

    private void UpdateLevelButton(int levelIndex)
    {
        if (levelIndex >= levelButtons.Count) return;

        LevelButton button = levelButtons[levelIndex];

        bool isUnlocked = StarManager.Instance?.IsLevelUnlocked(levelIndex) ?? (levelIndex == 0);
        int stars = StarManager.Instance?.GetLevelStars(levelIndex) ?? 0;

        button.UpdateButton(isUnlocked, stars);
    }

    private void UpdateTotalStarsUI(int totalStars)
    {
        if (totalStarsText != null)
        {
            totalStarsText.text = totalStars.ToString();
        }
    }

    private void OnLevelUnlocked(int levelIndex)
    {
        UpdateLevelButton(levelIndex);

        // Можно добавить анимацию разблокировки
        Debug.Log($"Уровень {levelIndex + 1} разблокирован!");
    }

    public void LoadLevel(int levelIndex)
    {
        int sceneIndex = firstLevelSceneIndex + levelIndex;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Сцена с индексом {sceneIndex} не существует!");
        }
    }
}

[System.Serializable]
public class LevelButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Text levelNumberText;
    [SerializeField] private GameObject[] starIcons; // 3 иконки звёзд
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color starActiveColor = Color.yellow;
    [SerializeField] private Color starInactiveColor = Color.gray;

    private int levelIndex;
    private LevelSelectionUI levelSelectionUI;

    public void Initialize(int index, LevelSelectionUI selectionUI)
    {
        levelIndex = index;
        levelSelectionUI = selectionUI;

        if (levelNumberText != null)
        {
            levelNumberText.text = (index + 1).ToString();
        }

        if (button != null)
        {
            button.onClick.AddListener(() => OnButtonClick());
        }
    }

    public void UpdateButton(bool isUnlocked, int starsCount)
    {
        // Обновляем интерактивность кнопки
        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        // Обновляем фон кнопки
        if (backgroundImage != null)
        {
            backgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // Показываем/скрываем замок
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // Обновляем звёзды
        UpdateStars(starsCount, isUnlocked);
    }

    private void UpdateStars(int starsCount, bool isUnlocked)
    {
        if (starIcons == null) return;

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] != null)
            {
                bool shouldShowStar = isUnlocked && i < starsCount;
                starIcons[i].SetActive(shouldShowStar);

                // Можно также изменить цвет звёзд
                Image starImage = starIcons[i].GetComponent<Image>();
                if (starImage != null)
                {
                    starImage.color = shouldShowStar ? starActiveColor : starInactiveColor;
                }
            }
        }
    }

    private void OnButtonClick()
    {
        if (levelSelectionUI != null)
        {
            levelSelectionUI.LoadLevel(levelIndex);
        }
    }
}