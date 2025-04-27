//using UnityEngine;

//// Базовый класс для условий победы
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

//// Базовый класс для условий проигрыша
//public abstract class LoseCondition : MonoBehaviour
//{
//    [SerializeField] protected string conditionName;
//    [SerializeField] protected string conditionDescription;

//    public string Name => conditionName;
//    public string Description => conditionDescription;

//    public abstract bool IsConditionMet();
//    public abstract void ResetCondition();
//}

//// Условие победы: Поджигание здания любым количеством боеприпаса (1 звезда)
//public class BuildingCaughtFireCondition : WinCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    private bool buildingCaughtFire = false;

//    private void Awake()
//    {
//        starsAwarded = 1; // Гарантируем 1 звезду для этого условия
//        conditionName = "Поджигание здания";
//        conditionDescription = "Поджечь здание любым количеством боеприпаса";
//    }

//    public void OnBuildingCaughtFire()
//    {
//        buildingCaughtFire = true;

//        // После выполнения условия проверяем, выиграл ли игрок
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

//// Условие победы: Поджигание здания первым боеприпасом (2 звезды)
//public class BuildingCaughtFirstShotCondition : WinCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    private bool buildingCaughtFirstShot = false;
//    private int shotsUsed = 0;

//    private void Awake()
//    {
//        starsAwarded = 2; // 2 звезды для этого условия
//        conditionName = "Меткий стрелок";
//        conditionDescription = "Поджечь здание первым боеприпасом";
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

//            // После выполнения условия проверяем, выиграл ли игрок
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

//// Условие победы: Попадание стрелы в молотов (3 звезды)
//public class ArrowHitMolotovCondition : WinCondition
//{
//    private bool arrowHitMolotov = false;

//    private void Awake()
//    {
//        starsAwarded = 3; // 3 звезды для этого условия
//        conditionName = "Тройной удар";
//        conditionDescription = "Попасть стрелой в молотов";
//    }

//    public void OnArrowHitMolotov()
//    {
//        arrowHitMolotov = true;

//        // После выполнения условия проверяем, выиграл ли игрок
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

//// Условие проигрыша: Здание не было поджжено (0 звезд)
//public class BuildingNotCaughtFireCondition : LoseCondition
//{
//    [SerializeField] private GameObject targetBuilding;
//    [SerializeField] private float checkDelay = 5.0f; // Задержка проверки

//    private bool resourcesExhausted = false;
//    private bool buildingCaughtFire = false;
//    private float timer = 0f;

//    private void Awake()
//    {
//        conditionName = "Провал";
//        conditionDescription = "Здание не было поджжено";
//    }

//    private void Update()
//    {
//        if (resourcesExhausted && !buildingCaughtFire)
//        {
//            timer += Time.deltaTime;

//            if (timer >= checkDelay)
//            {
//                // Проверяем, проиграл ли игрок
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

//// Вспомогательные классы для связи с игровыми объектами

//// Класс для здания
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

//// Класс для боеприпаса (молотов)
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

//// Класс для стрелы
//public class ArrowController : MonoBehaviour
//{
//    [SerializeField] private ArrowHitMolotovCondition arrowHitCondition;

//    private void OnCollisionEnter(Collision collision)
//    {
//        // Проверяем, стрела попала в молотов
//        if (collision.gameObject.CompareTag("Molotov"))
//        {
//            if (arrowHitCondition != null)
//            {
//                arrowHitCondition.OnArrowHitMolotov();
//            }
//        }
//    }
//}

//// Класс для менеджера ресурсов, который отслеживает, закончились ли ресурсы
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
