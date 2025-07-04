using UnityEngine;

public class ArrowSpawnAfterDelay : MonoBehaviour
{
    public float delay = 20f;

    void Start()
    {
        gameObject.SetActive(false); // Сразу выключаем объект
        Invoke(nameof(EnableSelf), delay);
    }

    void EnableSelf()
    {
        gameObject.SetActive(true); // Включаем объект
    }
}
