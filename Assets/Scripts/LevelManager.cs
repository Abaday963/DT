using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [Header("������� ������ � ���������")]
    [SerializeField] private WinCondition[] winConditions;
    [SerializeField] private LoseCondition[] loseConditions;

    [Header("������ �� ������� ������")]
    [SerializeField] private Building1 targetBuilding;
    [SerializeField] private AmmunitionManager ammunitionManager;

    [Header("���������")]
    [SerializeField] private bool autoCheckForWin = true;
    [SerializeField] private float autoCheckInterval = 0.5f;

    private bool isLevelCompleted = false;
    private float autoCheckTimer = 0f;
    private List<WinCondition> completedWinConditions = new List<WinCondition>();

    // ������� ��� GameManager � UI
    public delegate void LevelEvent(int starsEarned, List<string> completedConditions);
    public event LevelEvent OnLevelWon;
    public event LevelEvent OnLevelLost;

    private void Start()
    {
        FindAndSetupReferences();
        ResetAllConditions();
    }

    private void Update()
    {
        if (isLevelCompleted) return;

        // �������������� �������� �������
        if (autoCheckForWin)
        {
            autoCheckTimer += Time.deltaTime;
            if (autoCheckTimer >= autoCheckInterval)
            {
                autoCheckTimer = 0f;
                CheckWinConditions();
                CheckLoseConditions();
            }
        }
    }

    // ������� � ����������� ������ �� �������, ���� ��� �� ������ � ����������
    private void FindAndSetupReferences()
    {
        if (targetBuilding == null)
        {
            targetBuilding = FindAnyObjectByType<Building1>();
        }

        if (ammunitionManager == null)
        {
            ammunitionManager = FindAnyObjectByType<AmmunitionManager>();
        }

        // ������� ��� ������� ������ � ���������, ���� ������� �����
        if (winConditions == null || winConditions.Length == 0)
        {
            winConditions = FindObjectsOfType<WinCondition>();
        }

        if (loseConditions == null || loseConditions.Length == 0)
        {
            loseConditions = FindObjectsOfType<LoseCondition>();
        }

    }

    // ���������� ��� �������
    public void ResetAllConditions()
    {
        isLevelCompleted = false;
        completedWinConditions.Clear();

        if (winConditions != null)
        {
            foreach (var condition in winConditions)
            {
                if (condition != null) condition.ResetCondition();
            }
        }

        if (loseConditions != null)
        {
            foreach (var condition in loseConditions)
            {
                if (condition != null) condition.ResetCondition();
            }
        }

        // ���������� ������ ������������
        autoCheckTimer = 0f;
    }

    // �������� ������� ������
    public void CheckWinConditions()
    {
        if (isLevelCompleted) return;

        // ��������� ������ ������� ������
        foreach (var condition in winConditions)
        {
            if (condition != null && condition.IsConditionMet() && !completedWinConditions.Contains(condition))
            {
                completedWinConditions.Add(condition);
                Debug.Log($"��������� ������� ������: {condition.Name}");
            }
        }

        // ���� ���� ���� �� ���� ����������� ������� � ������ ����� - ������� �������
        if (completedWinConditions.Count > 0 && targetBuilding.IsOnFire())
        {
            WinLevel();
        }
    }

    // �������� ������� ���������
    public void CheckLoseConditions()
    {
        if (isLevelCompleted) return;

        foreach (var condition in loseConditions)
        {
            if (condition != null && condition.IsConditionMet())
            {
                LoseLevel();
                Debug.Log("v level managere uroven proigran");
                return;
            }
        }
    }

    // ���������� ��� ������
    private void WinLevel()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;

        // ������������ ������������ ���������� �����
        int starsEarned = completedWinConditions.Count > 0
            ? completedWinConditions.Max(c => c.Stars)
            : 0;

        // �������� ����� ����������� �������
        List<string> completedConditionNames = completedWinConditions
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"������� �������! ���������� �����: {starsEarned}");

        // �������� ������� ������
        OnLevelWon?.Invoke(starsEarned, completedConditionNames);
    }

    // ���������� ��� ���������
    private void LoseLevel()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;

        // �������� ������� ���������
        List<string> loseReasons = loseConditions
            .Where(c => c != null && c.IsConditionMet())
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"������� ��������! �������: {string.Join(", ", loseReasons)}");

        // �������� ������� ���������
        OnLevelLost?.Invoke(0, loseReasons);
    }

    // ������ ��� ������ � UI
    public List<string> GetCompletedConditionNames()
    {
        return completedWinConditions.Select(c => c.Name).ToList();
    }

    public int GetMaxStarsEarned()
    {
        return completedWinConditions.Count > 0
            ? completedWinConditions.Max(c => c.Stars)
            : 0;
    }

    public bool IsLevelCompleted()
    {
        return isLevelCompleted;
    }
}