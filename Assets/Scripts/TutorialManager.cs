using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Settings")]
    public float intervalTime = 5f; // Интервал между показами
    public string tutorialAnimationName = "TutorialAnimation"; // Имя анимации обучения
    public string idleAnimationName = "Idle"; // Имя анимации ожидания

    private Animator animator;
    private Coroutine tutorialCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Запускаем цикл показа подсказок
        tutorialCoroutine = StartCoroutine(TutorialLoop());
    }

    private IEnumerator TutorialLoop()
    {
        while (true)
        {
            // Ждём интервал
            yield return new WaitForSeconds(intervalTime);

            // Запускаем анимацию обучения
            animator.Play(tutorialAnimationName);

            // Ждём пока анимация закончится
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                !animator.IsInTransition(0));

            // Возвращаемся к idle
            animator.Play(idleAnimationName);
        }
    }

    // Метод для остановки показа подсказок
    public void StopTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            animator.Play(idleAnimationName);
        }
    }

    // Метод для запуска показа подсказок
    public void StartTutorial()
    {
        if (tutorialCoroutine != null)
            StopCoroutine(tutorialCoroutine);

        tutorialCoroutine = StartCoroutine(TutorialLoop());
    }
}