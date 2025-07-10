using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    [Header("Настройки здоровья")]
    public int maxHearts = 3; // Максимальное количество сердец
    private int currentHearts;

    [Header("UI элементы")]
    public Image[] heartImages; // Массив изображений сердец
    public Sprite fullHeart; // Спрайт полного сердца
    public Sprite emptyHeart; // Спрайт пустого сердца

    [Header("Звуки")]
    public AudioClip heartLostSound; // Звук потери сердца
    public AudioClip gameOverSound; // Звук окончания игры

    [Header("События")]
    public UnityEvent OnHeartLost; // Событие потери сердца
    public UnityEvent OnGameOver; // Событие окончания игры

    private List<Kobok> registeredKoboks = new List<Kobok>();
    private AudioSource audioSource;

    void Start()
    {
        currentHearts = maxHearts;

        // Получаем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Инициализируем UI
        UpdateHeartUI();

        Debug.Log($"Менеджер здоровья инициализирован. Сердец: {currentHearts}/{maxHearts}");
    }

    public void RegisterKobok(Kobok kobok)
    {
        if (!registeredKoboks.Contains(kobok))
        {
            registeredKoboks.Add(kobok);
            Debug.Log($"Кобок {kobok.name} зарегистрирован в менеджере здоровья");
        }
    }

    public void UnregisterKobok(Kobok kobok)
    {
        if (registeredKoboks.Contains(kobok))
        {
            registeredKoboks.Remove(kobok);
            Debug.Log($"Кобок {kobok.name} удален из менеджера здоровья");
        }
    }

    public void OnKobokHit(Kobok kobok)
    {
        // Проверяем, что кобок действительно зарегистрирован
        if (registeredKoboks.Contains(kobok))
        {
            LoseHeart();
            Debug.Log($"Кобок {kobok.name} получил урон. Потеряно сердце!");
        }
    }

    public void OnKobokDeath(Kobok kobok)
    {
        Debug.Log($"Кобок {kobok.name} погиб");
        // Здесь можно добавить дополнительную логику при смерти кобока
    }

    public void LoseHeart()
    {
        if (currentHearts > 0)
        {
            currentHearts--;
            UpdateHeartUI();

            // Воспроизводим звук потери сердца
            if (audioSource != null && heartLostSound != null)
            {
                audioSource.PlayOneShot(heartLostSound);
            }

            // Вызываем событие потери сердца
            OnHeartLost?.Invoke();

            Debug.Log($"Потеряно сердце! Осталось: {currentHearts}/{maxHearts}");

            // Проверяем окончание игры
            if (currentHearts <= 0)
            {
                GameOver();
            }
        }
    }

    public void GainHeart()
    {
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartUI();
            Debug.Log($"Получено сердце! Текущее количество: {currentHearts}/{maxHearts}");
        }
    }

    void UpdateHeartUI()
    {
        if (heartImages != null && heartImages.Length > 0)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (i < maxHearts)
                {
                    heartImages[i].gameObject.SetActive(true);

                    // Устанавливаем спрайт в зависимости от количества сердец
                    if (i < currentHearts)
                    {
                        heartImages[i].sprite = fullHeart;
                    }
                    else
                    {
                        heartImages[i].sprite = emptyHeart;
                    }
                }
                else
                {
                    heartImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void GameOver()
    {

        // Вызываем событие окончания игры
        OnGameOver?.Invoke();

        // Можно добавить дополнительную логику окончания игры
        // Например, показать экран Game Over, остановить игру и т.д.

        // Пример: останавливаем время (паузим игру)
    }

    // Метод для сброса здоровья (для новой игры)
    public void ResetHealth()
    {
        currentHearts = maxHearts;
        UpdateHeartUI();
        Time.timeScale = 1f; // Возобновляем игру
        Debug.Log("Здоровье сброшено!");
    }

    // Геттеры для внешнего доступа
    public int GetCurrentHearts()
    {
        return currentHearts;
    }

    public int GetMaxHearts()
    {
        return maxHearts;
    }

    public bool IsGameOver()
    {
        return currentHearts <= 0;
    }
}