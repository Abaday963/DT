using UnityEngine;

public class ArrowHoming : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D rb;

    [SerializeField] private float homingStrength = 1f; // Сила подстройки траектории
    [SerializeField] private float maxHomingDistance = 10f; // Максимальное расстояние для самонаведения
    [SerializeField] private ArrowHitMolotovCondition arrowHitCondition;
    [SerializeField] private LayerMask molotovLayer; // Слой для молотова

    private bool isTargetInRange = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(ShowSequence());
    }
    private System.Collections.IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(2);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(this);
            return;
        }

        // Текущая скорость
        float currentSpeed = rb.linearVelocity.magnitude;

        // Расстояние до цели
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Проверяем, вошла ли цель в зону действия
        bool targetInRange = distanceToTarget < maxHomingDistance;

        isTargetInRange = targetInRange;

        // Самонаведение работает только в пределах определенного расстояния
        if (distanceToTarget < maxHomingDistance)
        {
            // Направление к цели
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            // Корректируем скорость с учетом самонаведения
            Vector2 newVelocity = Vector2.Lerp(rb.linearVelocity.normalized, directionToTarget, homingStrength * Time.fixedDeltaTime).normalized * currentSpeed;
            rb.linearVelocity = newVelocity;

            // Обновляем поворот стрелы
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, имеет ли объект компонент IAmmunition
        IAmmunition targetComponent = collision.gameObject.GetComponent<IAmmunition>();

        // Проверка на попадание в молотов
        if (((1 << collision.gameObject.layer) & molotovLayer) != 0)
        {
            Debug.Log("Стрела попала в молотов!");
            if (arrowHitCondition != null)
            {
                arrowHitCondition.OnArrowHitMolotov();
            }
        }
    }
}
