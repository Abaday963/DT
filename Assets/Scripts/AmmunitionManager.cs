    using UnityEngine;
    using System.Collections;

    public class AmmunitionManager : MonoBehaviour
    {
        public static AmmunitionManager Instance { get; private set; }

        [Header("Ammunition Settings")]
        [SerializeField] private int maxAmmunition = 3;          // Максимальное количество боеприпасов
        [SerializeField] private int currentAmmunition;          // Текущее количество боеприпасов
        [SerializeField] private SpriteRenderer[] ammunitionSprites; // UI спрайты для отображения боеприпасов

        [Header("Molotov Settings")]
        [SerializeField] private GameObject molotovPrefab;       // Префаб бутылки Молотова
        [SerializeField] private Transform spawnPoint;           // Точка появления в рогатке

        private GameObject currentMolotov;                       // Текущий боеприпас в рогатке
        private bool isReloading = false;                        // Флаг процесса перезарядки

        private void Awake()
        {
            // Singleton реализация
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Инициализация боеприпасов при старте
            ResetAmmunition();
        }

        // Сбросить количество боеприпасов до максимума
        public void ResetAmmunition()
        {
            currentAmmunition = maxAmmunition;
            UpdateAmmoUI();

            // Если нет текущего боеприпаса, создаем его
            if (currentMolotov == null)
            {
                SpawnMolotov();
            }
        }

        // Обновление UI спрайтов боеприпасов
        private void UpdateAmmoUI()
        {
            if (ammunitionSprites != null)
            {
                for (int i = 0; i < ammunitionSprites.Length; i++)
                {
                    if (ammunitionSprites[i] != null)
                    {
                        // Активны только спрайты доступных боеприпасов
                        ammunitionSprites[i].enabled = (i < currentAmmunition);
                    }
                }
            }
        }

        // Создаем новый молотов в рогатке
        public void SpawnMolotov()
        {
            if (molotovPrefab != null && spawnPoint != null && currentAmmunition > 0)
            {
                // Создаем новый боеприпас
                if (currentMolotov == null)
                {
                    currentMolotov = Instantiate(molotovPrefab, spawnPoint.position, spawnPoint.rotation);

                    // Настраиваем соединения для рогатки
                    SpringJoint2D springJoint = currentMolotov.GetComponent<SpringJoint2D>();
                    if (springJoint == null)
                    {
                        springJoint = currentMolotov.AddComponent<SpringJoint2D>();
                    }

                    // Ищем рогатку по тегу
                    GameObject slingshot = GameObject.FindGameObjectWithTag("FirstSlingshot");
                    if (slingshot != null)
                    {
                        springJoint.connectedBody = slingshot.GetComponent<Rigidbody2D>();
                        springJoint.distance = 0.5f;
                        springJoint.dampingRatio = 0.8f;
                        springJoint.frequency = 3.0f;
                    }

                    // Получаем скрипт Молотова и устанавливаем обратную связь с менеджером
                    MolotovAmmunition molotovScript = currentMolotov.GetComponent<MolotovAmmunition>();
                    if (molotovScript != null)
                    {
                        molotovScript.SetAmmunitionManager(this);
                    }

                    Debug.Log("Создан новый Молотов");
                }
            }
            else
            {
                Debug.LogWarning("Не удалось создать Молотов: префаб, точка спавна не назначены или закончились боеприпасы");
            }
        }

        // Вызывается, когда Молотов запущен
        public void OnMolotovLaunched()
        {
            if (currentAmmunition > 0)
            {
                currentAmmunition--;
                UpdateAmmoUI();

                // Отмечаем, что текущего боеприпаса нет
                currentMolotov = null;

                // Если остались боеприпасы, создаем следующий
                if (currentAmmunition > 0)
                {
                    // Небольшая задержка перед созданием нового боеприпаса
                    StartCoroutine(DelayedSpawn());
                }
            }
        }

        // Небольшая задержка перед созданием нового боеприпаса
        private IEnumerator DelayedSpawn()
        {
            yield return new WaitForSeconds(0.5f);
            SpawnMolotov();
        }

        // Геттеры для информации о боеприпасах
        public int GetCurrentAmmunition() => currentAmmunition;
        public int GetMaxAmmunition() => maxAmmunition;
        public bool HasAmmo() => currentAmmunition > 0;
    }