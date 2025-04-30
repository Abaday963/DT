using UnityEngine;

public class Building1 : MonoBehaviour
{
    public LayerMask fireLayer;
    private bool isOnFire = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"2D Collision detected with: {collision.gameObject.name}");

        if (!isOnFire && ((1 << collision.gameObject.layer) & fireLayer) != 0)
        {
            // Включить первый дочерний объект как эффект огня
            if (transform.childCount > 0)
            {
                Transform fireEffect = transform.GetChild(0);
                fireEffect.gameObject.SetActive(true);

                isOnFire = true;

                Debug.Log("Object caught fire from specific object!");
            }
            else
            {
                Debug.LogWarning("No child objects to activate fire effect!");
            }
        }
    }

    // Геттер: возвращает, загорелось ли здание
    public bool IsOnFire() => isOnFire;
    
}
