using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Rendering;

public class LevelManager : MonoBehaviour
{
    // ������������ ��� ������ ������ �������� �����
    public enum StarCountMode
    {
        ByConditionCount,    // �� ���������� ����������� �������
        ByConditionWeight,   // �� ���� ����������� �������
        ByTargetHit          // ����� �����: �� ����������� ��������� � ������
    }

    [Header("��������� �������� �����")]
    [SerializeField] private StarCountMode starCountMode = StarCountMode.ByTargetHit; // �� ��������� ���������� ����� �����
    [SerializeField] private WinCondition weightedStarCondition; // ������� � ����� ��� StarCountMode.ByConditionWeight
    [SerializeField] private int weightedStarValue = 3; // ������������ ���������� ����� ��� ������� � �����
    [SerializeField] private TargetHitCondition targetHitCondition; // ������ �� ������� ��������� � ������

    [Header("������� ������ � ���������")]
    [SerializeField] private WinCondition[] winConditions;
    [SerializeField] private LoseCondition[] loseConditions;

    [Header("������ �� ������� ������")]
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

    // �������� ��� ������ ���������� ����� (����� ���� ����������� �� ������ ��������)
    private int actualWeightedStarValue = 0;

    private void Start()
    {
        FindAndSetupReferences();
        ResetAllConditions();
    }

    private void Update()
    {
        if (isLevelCompleted) return;

        // ����������� ������ � ���������
        if (victoryPending)
        {
            victoryTimer += Time.deltaTime;
            if (victoryTimer >= victoryDelay)
            {
                CompleteWinLevel();
                return;
            }
        }

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

        // ������� ������� ��������� � ������, ���� ��� �� ������
        if (targetHitCondition == null)
        {
            targetHitCondition = FindAnyObjectByType<TargetHitCondition>();
        }
    }

    // ���������� ��� �������
    public void ResetAllConditions()
    {
        isLevelCompleted = false;
        completedWinConditions.Clear();
        actualWeightedStarValue = 0;

        // ��������: ����� ������ ������
        victoryPending = false;
        victoryTimer = 0f;

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

        Debug.Log("[LevelManager] ��� ������� � ����� ��������");
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

                // ���� ��� �������� ������� ��� ���������� �����, ��������� ������
                if (starCountMode == StarCountMode.ByConditionWeight && condition == weightedStarCondition)
                {
                    actualWeightedStarValue = ((TargetHitCondition)condition).GetStarsEarned();
                    //CalculateWeightedStarValue();
                }

                //// ���� ��� ��������� � ������ � � ��� ��������������� �����, ����� ������ �� �������
                if (starCountMode == StarCountMode.ByTargetHit && condition is TargetHitCondition)
                {
                    actualWeightedStarValue = ((TargetHitCondition)condition).GetStarsEarned();
                    Debug.Log($"��������� � ������! �������� {actualWeightedStarValue} �����.");
                }
            }
        }

        // ���� ��� ����� �� ���������� �������, ��������� ������� ��������
        if (starCountMode == StarCountMode.ByConditionCount && completedWinConditions.Count > 0)
        {
            WinLevel();
        }
        // ���� ��� ����� ����������� �������, ���������, ��������� �� �������� �������
        else if (starCountMode == StarCountMode.ByConditionWeight &&
                 weightedStarCondition != null &&
                 completedWinConditions.Contains(weightedStarCondition))
        {
            WinLevel();
        }
        // ���� ��� ����� ��������� � ������
        else if (starCountMode == StarCountMode.ByTargetHit &&
                 targetHitCondition != null &&
                 completedWinConditions.Contains(targetHitCondition))
        {
            WinLevel();
        }
    }

    // ������ ���������� ����� ��� ����������� ������
    private void CalculateWeightedStarValue()
    {
        // ����� ���������� ������ ���������� ���������� ����� (1, 2 ��� 3)
        // �� ��������� �������� ������������ ��������, �� ������ �������� �� ���� ������
        actualWeightedStarValue = weightedStarValue;

        // ������ ������ (��������):
        // �������� �������������� ������� ��� ����������� �������� �����������
        int additionalConditionsMet = 0;
        foreach (var condition in winConditions)
        {
            if (condition != weightedStarCondition &&
                condition != null &&
                condition.IsConditionMet())
            {
                additionalConditionsMet++;
            }
        }

        // ��������� ������: ��� ������ �������������� ������� ���������, ��� ������ �����
        if (additionalConditionsMet == 0)
        {
            actualWeightedStarValue = 1;
        }
        else if (additionalConditionsMet == 1)
        {
            actualWeightedStarValue = 2;
        }
        else
        {
            actualWeightedStarValue = 3;
        }

        Debug.Log($"����������� {actualWeightedStarValue} ����� �� {weightedStarValue} ���������");
    }

    // ������� ����� ��� ��������� ���������� ����� (����� ���������� �� ������ ��������)
    public void SetWeightedStarValue(int value)
    {
        actualWeightedStarValue = Mathf.Clamp(value, 0, weightedStarValue);
        Debug.Log($"����������� {actualWeightedStarValue} ����� ������� �������");
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

    // ��������� ��� �������� ����� ��������� ���������
    private float victoryDelay = 1f;
    private bool victoryPending = false;
    private float victoryTimer = 0f;

    // ����� ��� ������ �������� ������
    private void WinLevel()
    {
        if (isLevelCompleted) return;

        if (victoryPending)
        {
            // ���� ������ ��� ���������, ������ ���������, �� ��������� �� ����� ����������� �������
            return;
        }

        victoryPending = true;
        Debug.Log("�������� ������� ���������! �������� �������������� ������� ����� ��������...");
    }

    // ����� ��� �������������� ���������� ������ � �������
    private void CompleteWinLevel()
    {
        // �������������� �������� �� ��������� �����
        if (isLevelCompleted)
        {
            Debug.Log("[LevelManager] CompleteWinLevel ��� ��� ������, ����������");
            return;
        }

        isLevelCompleted = true;
        victoryPending = false; // ��������: ���������� ����

        // ���������� ���������� ������������ ����� � ����������� �� ������
        int starsEarned;
        if (starCountMode == StarCountMode.ByConditionCount)
        {
            starsEarned = Mathf.Min(completedWinConditions.Count, 3);
        }
        else if (starCountMode == StarCountMode.ByTargetHit)
        {
            starsEarned = targetHitCondition != null ? targetHitCondition.GetStarsEarned() : 1;
        }
        else // StarCountMode.ByConditionWeight
        {
            starsEarned = actualWeightedStarValue;
        }

        List<string> completedConditionNames = completedWinConditions
            .Select(c => c.Name)
            .ToList();

        Debug.Log($"[LevelManager] ������� �������! ���������� �����: {starsEarned}");

        OnLevelWon?.Invoke(starsEarned, completedConditionNames);
    }

    // ��������� ����������� ���������� ���������� �����
    public int GetMaxStarsEarned()
    {
        if (starCountMode == StarCountMode.ByConditionCount)
        {
            // � ������ �������� �� ���������� ������� ���������� ���������� ����������� �������
            return Mathf.Min(completedWinConditions.Count, 3); // �������� 3 ������
        }
        else if (starCountMode == StarCountMode.ByTargetHit)
        {
            // � ������ ��������� � ������ ���������� ���������� ����� �� �������
            return targetHitCondition != null ? targetHitCondition.GetStarsEarned() : 1;
        }
        else // StarCountMode.ByConditionWeight
        {
            // � ������ ����������� ������� ���������� ���������� �������� �����
            return actualWeightedStarValue;
        }
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
    public void StopAllLevelProcesses()
    {
        isLevelCompleted = true;
        victoryPending = false;
        victoryTimer = 0f;
        autoCheckTimer = 0f;
        completedWinConditions.Clear();
        actualWeightedStarValue = 0;

        Debug.Log("[LevelManager] ��� �������� ������ �����������");
    }

    // ������ ��� ������ � UI
    public List<string> GetCompletedConditionNames()
    {
        return completedWinConditions.Select(c => c.Name).ToList();
    }

    public bool IsLevelCompleted()
    {
        return isLevelCompleted;
    }

    // �������������� ������ ��� ��������� ���������� � ������ �����
    public StarCountMode GetStarCountMode()
    {
        return starCountMode;
    }

    public int GetWeightedMaxStars()
    {
        return weightedStarValue;
    }
}