using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    public float delay = 20f; // Период между включениями
    public float lifetime = 5f; // Сколько секунд объект будет включен
    public Vector3 offset = new Vector3(-1.33f, 2f, 0f); // Смещение относительно пули

    private Transform targetBullet;

    void Start()
    {
        StartCoroutine(ToggleLoop());
    }

    System.Collections.IEnumerator ToggleLoop()
    {
        while (true)
        {
            gameObject.SetActive(false); // выключаем
            yield return new WaitForSeconds(delay);

            // Ищем пулю перед включением
            GameObject bulletObject = GameObject.FindWithTag("EnemyBullet");
            if (bulletObject != null)
            {
                targetBullet = bulletObject.transform;
                Debug.Log("gde mlya!");

            }
            else
            {
                targetBullet = null;
                Debug.LogWarning("EnemyBullet не найден в сцене!");
            }

            gameObject.SetActive(true); // включаем
            yield return new WaitForSeconds(lifetime);

            gameObject.SetActive(false); // снова выключаем
        }
    }

    void Update()
    {
        if (gameObject.activeSelf && targetBullet != null)
        {
            // Следуем за пулей с оффсетом
            transform.position = targetBullet.position + offset;
        }
    }
}
