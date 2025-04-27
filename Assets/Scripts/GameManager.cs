using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ������������ ��������� ����
    private bool hasBuilding1Caught = false;
    private bool allShardsThrown = false;
    private int currentStars = 0;
    private bool lastMolotovThrown = false;
    private float lastMolotovWaitTime = 3.0f; // ����� �������� � �������� ��� ���������� ��������
    private float lastMolotovTimer = 0f;

    // ������ �� UI � ������� ��������
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Text starsText;

    private void Awake()
    {
        // ���������� Singleton
        if (Instance == null)
        {
            Instance = this;
            // ������� DontDestroyOnLoad ������
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ������������� ��������� ����
        ResetGameState();
    }
    private void Update()
    {
        // ���� ��������� ������� ��� ������, �� ������ ��� �� ����������
        if (lastMolotovThrown && !hasBuilding1Caught)
        {
            lastMolotovTimer += Time.deltaTime;

            // ���� ������ ���������� �������, � ������ ��� � �� ����������
            if (lastMolotovTimer >= lastMolotovWaitTime)
            {
                // ������ ������� �� ����� ��� �� ��������
                lastMolotovThrown = false;
                OnAllShardsThrown(); // ������ ����� �������, ��� ��� �������� ���� ������� � �� ������
            }
        }
    }
    // ����� ����� ��� ������ ��������� ����
    public void ResetGameState()
    {
        hasBuilding1Caught = false;
        allShardsThrown = false;
        currentStars = 0;
        lastMolotovThrown = false; // ���������� ���� ���������� ��������

        // �������� ��� ������
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);

        // �������� ��������� ��������
        //if (AmmunitionManager.Instance != null)
        //{
        //    AmmunitionManager.Instance.ResetAmmunition();
        //}
    }

    public void OnBuilding1Caught()
    {
        hasBuilding1Caught = true;
        currentStars = 3;
        HandleGameWin();
    }

    public void OnAllShardsThrown()
    {
        allShardsThrown = true;
        CheckGameOutcome();
    }

    private void CheckGameOutcome()
    {
        if (allShardsThrown)
        {
            if (!hasBuilding1Caught)
            {
                if (restartButton != null)
                {
                    restartButton.SetActive(true);
                }

                if (losePanel != null)
                {
                    losePanel.SetActive(true);
                }
            }
        }
    }

    private void HandleGameWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (starsText != null)
        {
            starsText.text = $"������: {currentStars}";
        }
    }

    public void RestartGame()
    {
        // ��������� ����� ������
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ������� �����, ������� ����� ���������� ����� �������� �����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameState();
    }

    public void OnLastMolotovThrown()
    {
        lastMolotovThrown = true;
        lastMolotovTimer = 0f; // ���������� ������
    }

    private void OnEnable()
    {
        // ������������� �� ������� �������� �����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ������������ �� ������� �������� �����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}