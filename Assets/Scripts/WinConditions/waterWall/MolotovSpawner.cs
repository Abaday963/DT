using UnityEngine;
using System.Collections;

public class MolotovSpawner : MonoBehaviour
{
    [Header("Spawn Mode")]
    [SerializeField] private bool infiniteMode = true;       // Режим бесконечного спавна

    [Header("Molotov Settings")]
    [SerializeField] private GameObject molotovPrefab;       // Префаб бутылки Молотова
    [SerializeField] private Transform spawnPoint;           // Точка появления в рогатке

    private GameObject currentMolotov;                       // Текущий боеприпас в рогатке

    private void Start()
    {
        // Создаем первый молотов при старте только в бесконечном режиме
        if (infiniteMode)
        {
            SpawnMolotov();
        }
    }

    // Создаем новый молотов в рогатке
    public void SpawnMolotov()
    {
        if (molotovPrefab != null && spawnPoint != null)
        {
            // Создаем новый боеприпас только если текущего нет
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

                // Получаем скрипт Молотова и устанавливаем обратную связь
                MolotovAmmunition molotovScript = currentMolotov.GetComponent<MolotovAmmunition>();
                if (molotovScript != null)
                {
                    // Устанавливаем ссылку на спавнер только в бесконечном режиме
                    if (infiniteMode)
                    {
                        molotovScript.SetSpawner(this);
                    }
                }
            }
        }
    }

    // Вызывается, когда Молотов запущен (только в бесконечном режиме)
    public void OnMolotovLaunched()
    {
        if (!infiniteMode) return;

        // Отмечаем, что текущего боеприпаса нет
        currentMolotov = null;

        // Создаем новый боеприпас с небольшой задержкой
        StartCoroutine(DelayedSpawn());
    }

    // Небольшая задержка перед созданием нового боеприпаса
    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(2f);
        SpawnMolotov();
    }

    // Метод для переключения режима из кода
    public void SetInfiniteMode(bool enabled)
    {
        infiniteMode = enabled;
    }
}