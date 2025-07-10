using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public Collider2D shieldCollider;

    [Header("Настройки анимации")]
    public Animator shieldAnimator;

    [Header("Настройки времени")]
    public float timeToVineGrowth = 10f; // Время до начала роста лозы на щите
    public float delayBeforeIgnite = 2f; // Задержка перед возгоранием щита

    [Header("Переключатели")]
    public bool enableVines = true; // Новый переключатель

    [Header("Параметры триггеров для Animator")]
    public string vineGrowthTrigger = "VineGrowth";
    public string igniteExplosionTrigger = "IgniteExplosion"; // один триггер для анимации возгорание+взрыв

    [Header("Названия анимаций (для справки)")]
    public string idleAnimation = "Shield_Idle";
    public string vineGrowthAnimation = "Shield_VineGrowth";
    public string vineStandingAnimation = "Shield_VineStanding";
    public string igniteExplosionAnimation = "Shield_IgniteExplosion"; // новая объединённая анимация

    private bool vineGrown = false;
    private bool isIgnited = false;

    void Start()
    {
        VineController.OnVineIgnited += OnVineIgnited;

        if (enableVines)
        {
            StartCoroutine(StartVineGrowthTimer());
        }
    }

    void OnDestroy()
    {
        VineController.OnVineIgnited -= OnVineIgnited;
    }

    IEnumerator StartVineGrowthTimer()
    {
        yield return new WaitForSeconds(timeToVineGrowth);
        StartVineGrowth();
    }

    void StartVineGrowth()
    {
        if (!vineGrown)
        {
            vineGrown = true;
            shieldAnimator.SetTrigger(vineGrowthTrigger);
            StartCoroutine(WaitForVineGrowthEnd());
        }
    }

    IEnumerator WaitForVineGrowthEnd()
    {
        yield return new WaitForSeconds(GetAnimationLength(vineGrowthAnimation));
        // Переход к vineStanding должен быть настроен в Animator автоматически
    }

    void OnVineIgnited()
    {
        if (vineGrown && !isIgnited)
        {
            StartCoroutine(DelayedIgniteAndExplode());
        }
    }

    IEnumerator DelayedIgniteAndExplode()
    {
        yield return new WaitForSeconds(delayBeforeIgnite);

        if (!isIgnited)
        {
            isIgnited = true;
            shieldAnimator.SetTrigger(igniteExplosionTrigger);

            yield return new WaitForSeconds(GetAnimationLength(igniteExplosionAnimation));

            shieldCollider.enabled = false;

            // Destroy(gameObject);
        }
    }

    float GetAnimationLength(string animationName)
    {
        AnimationClip[] clips = shieldAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 1f; // значение по умолчанию
    }

    public bool IsVineGrown()
    {
        return vineGrown;
    }

    public bool IsIgnited()
    {
        return isIgnited;
    }
}
