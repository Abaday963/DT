using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private bool isDestroyedOnHit = true;
    [SerializeField] private GameObject hitEffect; // Опциональный эффект при попадании

    [Tooltip("Время в секундах между возможными регистрациями попаданий")]
    public float hitCooldown = 10f;

    private TimerTargetHitCondition hitCondition;
    private Collider2D targetCollider;

    // Переменная для хранения времени последнего попадания
    private float lastHitTime = -Mathf.Infinity;

    private void Start()
    {
        hitCondition = FindObjectOfType<TimerTargetHitCondition>();
        targetCollider = GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            Debug.LogError("На цели отсутствует коллайдер!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitter)
    {
        if (hitter.CompareTag("Arrow") || hitter.CompareTag("Projectile"))
        {
            Debug.Log("Попадание в цель: " + gameObject.name);
            RegisterHit();
        }
    }

    public void RegisterHit()
    {
        // Проверяем, прошло ли достаточно времени с последнего попадания
        if (Time.time - lastHitTime < hitCooldown)
        {
            Debug.Log("Попадание не зарегистрировано, нужно подождать: " + (hitCooldown - (Time.time - lastHitTime)).ToString("F1") + " сек.");
            return;
        }

        Debug.Log("RegisterHit вызван для цели: " + gameObject.name);

        lastHitTime = Time.time; // Обновляем время последнего попадания

        if (hitCondition != null)
        {
            hitCondition.RegisterHit(transform);

            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Уничтожаем объект, если это предусмотрено
            //if (isDestroyedOnHit)
            //{
            //    gameObject.SetActive(false); // или Destroy(gameObject);
            //}
        }
        else
        {
            Debug.LogError("hitCondition не найден!");
        }
    }
}
