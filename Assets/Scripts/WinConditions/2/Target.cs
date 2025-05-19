using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private bool isDestroyedOnHit = true;
    [SerializeField] private GameObject hitEffect; // Опциональный эффект при попадании

    private TimerTargetHitCondition hitCondition;
    private Collider2D targetCollider;

    private void Start()
    {
        // Находим условие для регистрации попаданий
        hitCondition = FindObjectOfType<TimerTargetHitCondition>();

        // Получаем коллайдер, если есть
        targetCollider = GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            Debug.LogError("На цели отсутствует коллайдер!");
        }
    }

    // Вызывается при столкновении с другим коллайдером (2D)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    // Вызывается при входе в триггер (2D)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    // Обработчик попадания
    private void HandleHit(GameObject hitter)
    {
        // Проверяем, является ли объект, столкнувшийся с мишенью, стрелой или снарядом
        // Здесь можно добавить проверку тега или компонента
        if (hitter.CompareTag("Arrow") || hitter.CompareTag("Projectile"))
        {
            Debug.Log("Попадание в цель: " + gameObject.name);
            RegisterHit();
        }
    }

    // Метод для регистрации попадания (может вызываться напрямую)
    public void RegisterHit()
    {
        Debug.Log("RegisterHit вызван для цели: " + gameObject.name);

        if (hitCondition != null)
        {
            hitCondition.RegisterHit(transform);

            // Создаем эффект попадания, если он назначен
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