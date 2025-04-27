using UnityEngine;
using System.Collections;

public class Building1 : MonoBehaviour
{
    [Header("Fire Settings")]
    public GameObject fireEffectObject;    // Визуальный эффект огня
    public LayerMask fireLayer;            // Слой, на котором находятся объекты огня
    public float fireDamageRate = 5f;      // Скорость нанесения урона зданию (в секунду)
    public float maxHealth = 100f;         // Максимальное здоровье здания

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onFireStart;    // Событие при начале пожара
    public UnityEngine.Events.UnityEvent onDestroyed;    // Событие при разрушении здания

    private bool isOnFire = false;         // Флаг, указывающий, горит ли здание
    private float currentHealth;           // Текущее здоровье здания

    private void Start()
    {
        // Инициализация начального здоровья
        currentHealth = maxHealth;

        // Убедимся, что эффект огня изначально выключен
        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что объект ещё не горит и коллизия произошла с объектом из слоя огня
        if (!isOnFire && ((1 << collision.gameObject.layer) & fireLayer) != 0)
        {
            CatchFire();
        }
    }

    // Метод для проверки столкновения с триггерами
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Проверяем, что объект ещё не горит и коллизия произошла с объектом из слоя огня
        if (!isOnFire && ((1 << collider.gameObject.layer) & fireLayer) != 0)
        {
            CatchFire();
        }
    }

    // Отдельная функция для поджога здания
    public void CatchFire()
    {
        if (isOnFire) return; // Если здание уже горит, выходим из метода

        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(true);
            isOnFire = true;

            // Вызываем событие начала пожара
            onFireStart?.Invoke();

            // Запускаем корутину для нанесения урона от огня
            StartCoroutine(TakeDamageFromFire());
        }
        else
        {
            Debug.LogWarning("Fire effect object not assigned on " + gameObject.name);
        }
    }

    // Функция для тушения пожара
    public void ExtinguishFire()
    {
        if (!isOnFire) return; // Если здание не горит, выходим из метода

        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
            isOnFire = false;

            // Останавливаем все корутины (включая урон от огня)
            StopAllCoroutines();
        }
    }

    // Корутина для постепенного нанесения урона от огня
    private IEnumerator TakeDamageFromFire()
    {
        while (isOnFire && currentHealth > 0)
        {
            // Наносим урон
            currentHealth -= fireDamageRate * Time.deltaTime;

            // Проверяем, разрушено ли здание
            if (currentHealth <= 0)
            {
                DestroyBuilding();
                break;
            }

            yield return null; // Ждем до следующего кадра
        }
    }

    // Функция для разрушения здания
    private void DestroyBuilding()
    {
        // Вызываем событие разрушения здания
        onDestroyed?.Invoke();

        // Здесь можно добавить эффекты разрушения, звуки и т.д.
        Debug.Log(gameObject.name + " destroyed by fire!");

        // Можно уничтожить объект или заменить его на разрушенную версию
        // Например:
        // Instantiate(destroyedVersionPrefab, transform.position, transform.rotation);
        // Destroy(gameObject);
    }

    // Публичный метод для получения информации, горит ли здание
    public bool IsOnFire()
    {
        return isOnFire;
    }

    // Публичный метод для получения процента оставшегося здоровья
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}