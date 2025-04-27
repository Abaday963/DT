//using UnityEngine;
//public class LevelManager : MonoBehaviour
//{
//    public static LevelManager Instance { get; private set; }
//    [Header("Условия уровня")]
//    [SerializeField] private LevelObjective[] objectives;
//    [SerializeField] private int requiredObjectivesForWin = 1;
//    [Header("Таймеры и ожидания")]
//    [SerializeField] private float failWaitTime = 3.0f;
//    private int completedObjectives = 0;
//    private int currentStars = 0;
//    private bool isLevelCompleted = false;
//    private bool isLevelFailed = false;
//    private float failTimer = 0f;
//    private bool waitingForFailure = false;
//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }
//        ResetLevelState();
//    }
//    private void Update()
//    {
//        if (waitingForFailure && !isLevelCompleted)
//        {
//            failTimer += Time.deltaTime;
//            if (failTimer >= failWaitTime)
//            {
//                waitingForFailure = false;
//                CheckLevelOutcome();
//            }
//        }
//    }
//    public void ResetLevelState()
//    {
//        completedObjectives = 0;
//        currentStars = 0;
//        isLevelCompleted = false;
//        isLevelFailed = false;
//        waitingForFailure = false;
//        failTimer = 0f;
//        UIManager.Instance?.HideAll();
//        foreach (var objective in objectives)
//            objective?.ResetObjective();
//        WinConditionManager.Instance?.ResetConditions();
//    }
//    public void OnObjectiveCompleted(LevelObjective objective)
//    {
//        completedObjectives++;
//        if (completedObjectives >= requiredObjectivesForWin && !isLevelCompleted)
//        {
//            isLevelCompleted = true;
//            currentStars = WinConditionManager.Instance?.CalculateStars() ?? 1;
//            GameManager.Instance?.SetLevelStars(currentStars);
//            HandleLevelWin();
//        }
//    }
//    public void OnResourcesExhausted()
//    {
//        if (!isLevelCompleted && !waitingForFailure)
//        {
//            waitingForFailure = true;
//            failTimer = 0f;
//        }
//    }
//    private void CheckLevelOutcome()
//    {
//        if (!isLevelCompleted)
//        {
//            isLevelFailed = true;
//            HandleLevelLose();
//        }
//    }
//    private void HandleLevelWin()
//    {
//        UIManager.Instance?.ShowWinScreen(currentStars);
//    }
//    private void HandleLevelLose()
//    {
//        UIManager.Instance?.ShowLoseScreen();
//    }
//    public void LoadNextLevel()
//    {
//        GameManager.Instance?.LoadNextLevel();
//    }
//    public void RestartLevel()
//    {
//        GameManager.Instance?.RestartCurrentLevel();
//    }
//    public void ReturnToMainMenu()
//    {
//        GameManager.Instance?.LoadMainMenu();
//    }
//}

//public class AmmunitionManager : MonoBehaviour
//{
//    public static AmmunitionManager Instance { get; private set; }
//    [SerializeField] private int totalAmmunition = 3;
//    [SerializeField] private int remainingAmmunition;
//    [SerializeField] private Image[] ammunitionIcons;
//    private void Awake()
//    {
//        // Singleton implementation
//        if (Instance == null)
//        {
//            Instance = this;
//            // Удалите DontDestroyOnLoad отсюда
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }
//        // Переместите это в метод инициализации
//        ResetAmmunition();
//    }
//    // Новый метод для сброса патронов
//    public void ResetAmmunition()
//    {
//        remainingAmmunition = totalAmmunition;
//        // Активируем все иконки патронов
//        if (ammunitionIcons != null)
//        {
//            for (int i = 0; i < ammunitionIcons.Length; i++)
//            {
//                if (ammunitionIcons[i] != null)
//                {
//                    ammunitionIcons[i].gameObject.SetActive(true);
//                }
//            }
//        }
//    }
//    public void OnAmmunitionImpact()
//    {
//        // Decrease remaining ammunition
//        if (remainingAmmunition > 0)
//        {
//            // Decrease index before removing icon
//            remainingAmmunition--;
//            // Remove corresponding UI element
//            if (ammunitionIcons != null && remainingAmmunition >= 0 && remainingAmmunition < ammunitionIcons.Length)
//            {
//                ammunitionIcons[remainingAmmunition].gameObject.SetActive(false);
//            }
//            if (remainingAmmunition <= 0)
//            {
//                //
//            }
//        }
//    }
//    public int GetRemainingAmmunition()
//    {
//        return remainingAmmunition;
//    }
//    public int GetTotalAmmunition()
//    {
//        return totalAmmunition;
//    }
//}