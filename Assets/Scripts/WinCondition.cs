//using UnityEngine;

//// ������� ����� ��� ������� ������
//public abstract class WinCondition : MonoBehaviour
//{
//    [SerializeField] protected string conditionName;
//    [SerializeField] protected string conditionDescription;
//    [SerializeField] protected int starsAwarded = 1;

//    public string Name => conditionName;
//    public string Description => conditionDescription;
//    public int GetStarsAwarded() => starsAwarded;

//    public abstract bool IsConditionMet();
//    public abstract void ResetCondition();
//}

//// ������� ����� ��� ������� ���������
//public abstract class LoseCondition : MonoBehaviour
//{
//    [SerializeField] protected string conditionName;
//    [SerializeField] protected string conditionDescription;

//    public string Name => conditionName;
//    public string Description => conditionDescription;

//    public abstract bool IsConditionMet();
//    public abstract void ResetCondition();
//}

//// ������� ������: ���������� ������ ����� ����������� ���������� (1 ������)
//public class BuildingCaughtFireCondition : WinCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    private bool buildingCaughtFire = false;

//    private void Awake()
//    {
//        starsAwarded = 1; // ����������� 1 ������ ��� ����� �������
//        conditionName = "���������� ������";
//        conditionDescription = "������� ������ ����� ����������� ����������";
//    }

//    public void OnBuildingCaughtFire()
//    {
//        buildingCaughtFire = true;

//        // ����� ���������� ������� ���������, ������� �� �����
//        if (LevelManager.Instance != null)
//        {
//            LevelManager.Instance.CheckWinConditions();
//        }
//    }

//    public override bool IsConditionMet()
//    {
//        return buildingCaughtFire;
//    }

//    public override void ResetCondition()
//    {
//        buildingCaughtFire = false;
//    }
//}

//// ������� ������: ���������� ������ ������ ����������� (2 ������)
//public class BuildingCaughtFirstShotCondition : WinCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    private bool buildingCaughtFirstShot = false;
//    private int shotsUsed = 0;

//    private void Awake()
//    {
//        starsAwarded = 2; // 2 ������ ��� ����� �������
//        conditionName = "������ �������";
//        conditionDescription = "������� ������ ������ �����������";
//    }

//    public void OnShotFired()
//    {
//        shotsUsed++;
//    }

//    public void OnBuildingCaughtFire()
//    {
//        if (shotsUsed == 1)
//        {
//            buildingCaughtFirstShot = true;

//            // ����� ���������� ������� ���������, ������� �� �����
//            if (LevelManager.Instance != null)
//            {
//                LevelManager.Instance.CheckWinConditions();
//            }
//        }
//    }

//    public override bool IsConditionMet()
//    {
//        return buildingCaughtFirstShot;
//    }

//    public override void ResetCondition()
//    {
//        buildingCaughtFirstShot = false;
//        shotsUsed = 0;
//    }
//}

//// ������� ������: ��������� ������ � ������� (3 ������)
//public class ArrowHitMolotovCondition : WinCondition
//{
//    private bool arrowHitMolotov = false;

//    private void Awake()
//    {
//        starsAwarded = 3; // 3 ������ ��� ����� �������
//        conditionName = "������� ����";
//        conditionDescription = "������� ������� � �������";
//    }

//    public void OnArrowHitMolotov()
//    {
//        arrowHitMolotov = true;

//        // ����� ���������� ������� ���������, ������� �� �����
//        if (LevelManager.Instance != null)
//        {
//            LevelManager.Instance.CheckWinConditions();
//        }
//    }

//    public override bool IsConditionMet()
//    {
//        return arrowHitMolotov;
//    }

//    public override void ResetCondition()
//    {
//        arrowHitMolotov = false;
//    }
//}

//// ������� ���������: ������ �� ���� �������� (0 �����)
//public class BuildingNotCaughtFireCondition : LoseCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    [SerializeField] private float checkDelay = 5.0f; // �������� ��������

//    private bool resourcesExhausted = false;
//    private bool buildingCaughtFire = false;
//    private float timer = 0f;

//    private void Awake()
//    {
//        conditionName = "������";
//        conditionDescription = "������ �� ���� ��������";
//    }

//    private void Update()
//    {
//        if (resourcesExhausted && !buildingCaughtFire)
//        {
//            timer += Time.deltaTime;

//            if (timer >= checkDelay)
//            {
//                // ���������, �������� �� �����
//                if (LevelManager.Instance != null)
//                {
//                    LevelManager.Instance.CheckLoseConditions();
//                }
//            }
//        }
//    }

//    public void OnResourcesExhausted()
//    {
//        resourcesExhausted = true;
//        timer = 0f;
//    }

//    public void OnBuildingCaughtFire()
//    {
//        buildingCaughtFire = true;
//    }

//    public override bool IsConditionMet()
//    {
//        return resourcesExhausted && !buildingCaughtFire && timer >= checkDelay;
//    }

//    public override void ResetCondition()
//    {
//        resourcesExhausted = false;
//        buildingCaughtFire = false;
//        timer = 0f;
//    }
//}

//// ��������������� ������ ��� ����� � �������� ���������

//// ����� ��� ������
//public class BuildingController : MonoBehaviour
//{
//    [SerializeField] private BuildingCaughtFireCondition basicWinCondition;
//    [SerializeField] private BuildingCaughtFirstShotCondition firstShotCondition;
//    [SerializeField] private BuildingNotCaughtFireCondition loseCondition;

//    public void OnCaughtFire()
//    {
//        if (basicWinCondition != null)
//        {
//            basicWinCondition.OnBuildingCaughtFire();
//        }

//        if (firstShotCondition != null)
//        {
//            firstShotCondition.OnBuildingCaughtFire();
//        }

//        if (loseCondition != null)
//        {
//            loseCondition.OnBuildingCaughtFire();
//        }
//    }
//}

//// ����� ��� ���������� (�������)
//public class MolotovController : MonoBehaviour
//{
//    [SerializeField] private BuildingCaughtFirstShotCondition firstShotCondition;

//    private void Start()
//    {
//        if (firstShotCondition != null)
//        {
//            firstShotCondition.OnShotFired();
//        }
//    }
//}

//// ����� ��� ������
//public class ArrowController : MonoBehaviour
//{
//    [SerializeField] private ArrowHitMolotovCondition arrowHitCondition;

//    private void OnCollisionEnter(Collision collision)
//    {
//        // ���������, ������ ������ � �������
//        if (collision.gameObject.CompareTag("Molotov"))
//        {
//            if (arrowHitCondition != null)
//            {
//                arrowHitCondition.OnArrowHitMolotov();
//            }
//        }
//    }
//}

//// ����� ��� ��������� ��������, ������� �����������, ����������� �� �������
//public class ResourceManager : MonoBehaviour
//{
//    [SerializeField] private BuildingNotCaughtFireCondition loseCondition;
//    [SerializeField] private int totalResources = 3;
//    private int usedResources = 0;

//    public void UseResource()
//    {
//        usedResources++;

//        if (usedResources >= totalResources)
//        {
//            if (loseCondition != null)
//            {
//                loseCondition.OnResourcesExhausted();
//            }
//        }
//    }

//    public void ResetResources()
//    {
//        usedResources = 0;
//    }
//}
