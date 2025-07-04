using System.Collections;
using UnityEngine;

public class VineController : MonoBehaviour
{
    [Header("Настройки анимации")]
    public Animator vineAnimator;

    [Header("Настройки коллайдера")]
    public Collider2D vineCollider;
    public string targetTag = "Player"; // Тег объекта, который может поджечь лозу

    [Header("Настройки времени")]
    public float timeToAppear = 10f; // Время до появления лозы

    [Header("Параметры триггеров для Animator")]
    public string appearTrigger = "Appear";
    public string igniteTrigger = "Ignite";

    [Header("Названия анимаций (для справки)")]
    public string idleAnimation = "Vine_Idle";
    public string appearAnimation = "Vine_Appear";
    public string standingAnimation = "Vine_Standing";
    public string igniteAnimation = "Vine_Ignite";
    public string burningAnimation = "Vine_Burning";

    private bool isAppeared = false;
    private bool isIgnited = false;

    // Событие для уведомления щита о возгорании
    public static System.Action OnVineIgnited;

    void Start()
    {
        // Animator должен начать с Idle состояния по умолчанию
        // Отключаем коллайдер до появления лозы
        vineCollider.enabled = false;

        // Запускаем таймер появления
        StartCoroutine(StartAppearanceTimer());
    }

    IEnumerator StartAppearanceTimer()
    {
        yield return new WaitForSeconds(timeToAppear);
        AppearVine();
    }

    void AppearVine()
    {
        if (!isAppeared)
        {
            isAppeared = true;
            // Используем триггер вместо прямого вызова Play()
            vineAnimator.SetTrigger(appearTrigger);

            // Ждем окончания анимации появления и включаем коллайдер
            StartCoroutine(WaitForAppearanceEnd());
        }
    }

    IEnumerator WaitForAppearanceEnd()
    {
        // Ждем окончания анимации появления
        yield return new WaitForSeconds(GetAnimationLength(appearAnimation));

        // Включаем коллайдер для взаимодействия
        vineCollider.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("popl");
        // Проверяем тег объекта и что лоза еще не горит
        if (other.CompareTag(targetTag) && !isIgnited && isAppeared)
        {
            Debug.Log("popl - 2 ");

            IgniteVine();
        }
    }

    void IgniteVine()
    {
        if (!isIgnited)
        {
            isIgnited = true;
            // Используем триггер вместо прямого вызова Play()
            vineAnimator.SetTrigger(igniteTrigger);

            // Уведомляем щит о возгорании
            OnVineIgnited?.Invoke();
        }
    }

    IEnumerator WaitForIgniteEnd()
    {
        yield return new WaitForSeconds(GetAnimationLength(igniteAnimation));
        // Переход к горению должен происходить автоматически через Animator
        // Если нужно, можно добавить еще один триггер
    }

    // Вспомогательный метод для получения длины анимации
    float GetAnimationLength(string animationName)
    {
        AnimationClip[] clips = vineAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 1f; // Значение по умолчанию
    }

    // Публичный метод для проверки состояния лозы
    public bool IsIgnited()
    {
        return isIgnited;
    }
}