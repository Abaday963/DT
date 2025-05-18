using UnityEngine;

public class TargetCollider2D : MonoBehaviour
{
    public enum TargetZone
    {
        Inner,  // Центральная зона (3 звезды)
        Middle, // Средняя зона (2 звезды)
        Outer   // Внешняя зона (1 звезда)
    }

    [SerializeField] private TargetZone zoneType;
    [SerializeField] private TargetHitCondition targetHitCondition;

    private void Awake()
    {
        // Если не назначен targetHitCondition, ищем его в родительском объекте
        if (targetHitCondition == null)
        {
            targetHitCondition = GetComponentInParent<TargetHitCondition>();
            if (targetHitCondition == null)
            {
                Debug.LogError($"TargetCollider2D: Не найден компонент TargetHitCondition для {gameObject.name}!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, является ли объект стрелой или другим подходящим снарядом
        ArrowAmmunition arrow = other.GetComponent<ArrowAmmunition>();
        if (arrow != null)
        {
            HandleHit(arrow, other.transform.position);
            return;
        }

        //// Можно добавить проверки для других типов снарядов
        //Projectile projectile = other.GetComponent<Projectile>();
        //if (projectile != null)
        //{
        //    HandleHit(projectile, other.transform.position);
        //    return;
        //}
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Аналогичная проверка для столкновений (не триггеров)
        ArrowAmmunition arrow = collision.gameObject.GetComponent<ArrowAmmunition>();
        if (arrow != null)
        {
            HandleHit(arrow, collision.GetContact(0).point);
            return;
        }

        //Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        //if (projectile != null)
        //{
        //    HandleHit(projectile, collision.GetContact(0).point);
        //    return;
        //}
    }

    // Обработка попадания стрелы
    private void HandleHit(ArrowAmmunition arrow, Vector2 hitPosition)
    {
        if (targetHitCondition == null) return;

        switch (zoneType)
        {
            case TargetZone.Inner:
                targetHitCondition.OnInnerHit(hitPosition);
                break;
            case TargetZone.Middle:
                targetHitCondition.OnMiddleHit(hitPosition);
                break;
            case TargetZone.Outer:
                targetHitCondition.OnOuterHit(hitPosition);
                break;
        }

        // Останавливаем стрелу при попадании в мишень
        //arrow.OnHitTarget();
    }

    // Обработка попадания другого снаряда
    //private void HandleHit(Projectile projectile, Vector2 hitPosition)
    //{
    //    if (targetHitCondition == null) return;
    //
    //    switch (zoneType)
    //    {
    //        case TargetZone.Inner:
    //            targetHitCondition.OnInnerHit(hitPosition);
    //            break;
    //        case TargetZone.Middle:
    //            targetHitCondition.OnMiddleHit(hitPosition);
    //            break;
    //        case TargetZone.Outer:
    //            targetHitCondition.OnOuterHit(hitPosition);
    //            break;
    //    }
    //
    //    // Можно добавить вызов метода для обработки попадания снаряда
    //    projectile.OnHitTarget();
    //}
}