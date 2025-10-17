using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Camera cam;
    private Vector3 shakeStartPosition; // Позиция на момент начала тряски
    private bool isShaking = false;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    /// <summary>
    /// Запускает тряску камеры
    /// </summary>
    /// <param name="duration">Длительность тряски в секундах</param>
    /// <param name="magnitude">Сила тряски</param>
    public void StartShake(float duration = 0.5f, float magnitude = 0.1f)
    {
        // Запоминаем текущую позицию камеры как точку отсчета для тряски
        shakeStartPosition = cam.transform.localPosition;

        // Останавливаем предыдущую тряску, если она была
        if (isShaking)
            StopAllCoroutines();

        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Генерируем случайное смещение
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Применяем смещение относительно позиции начала тряски
            cam.transform.localPosition = new Vector3(
                shakeStartPosition.x + x,
                shakeStartPosition.y + y,
                shakeStartPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null; // Ждем один кадр
        }

        // Возвращаем камеру в позицию начала тряски
        cam.transform.localPosition = shakeStartPosition;
        isShaking = false;
    }

    /// <summary>
    /// Останавливает тряску и возвращает камеру в позицию начала тряски
    /// </summary>
    public void StopShake()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            cam.transform.localPosition = shakeStartPosition;
            isShaking = false;
        }
    }
}
