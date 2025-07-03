using UnityEngine;

public class RotateInShield : MonoBehaviour
{
    [Header("Настройки вращения")]
    public float angleMin = -415f;  // Минимальный угол
    public float angleMax = -350f;  // Максимальный угол
    public float speed = 50f;       // Скорость вращения (чем больше, тем быстрее качание)

    private float t = 0f;

    void Update()
    {
        // Увеличиваем таймер с учетом скорости
        t += Time.deltaTime * speed;

        // Получаем угол в пределах от 0 до 1
        float lerp = Mathf.PingPong(t, 1f);

        // Интерполируем между минимальным и максимальным углом
        float currentZ = Mathf.Lerp(angleMin, angleMax, lerp);

        // Устанавливаем новый угол
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            currentZ
        );
    }
}
