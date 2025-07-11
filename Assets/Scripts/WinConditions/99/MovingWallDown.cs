using UnityEngine;
using System.Collections;

public class MovingWallDown : MonoBehaviour
{
    [Header("Wall Movement Settings")]
    [SerializeField] private float moveDistance = 15f; // Расстояние опускания
    [SerializeField] private float moveDownTime = 1f; // Время опускания
    [SerializeField] private float holdTime = 2f; // Время удержания внизу
    [SerializeField] private float riseTime = 0.8f; // Время подъема обратно
    [SerializeField] private float bounceHeight = 0.5f; // Высота отскока
    [SerializeField] private float bounceTime = 0.3f; // Время отскока

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve moveDownCurve = new AnimationCurve(new Keyframe(0, 0, 0, 2), new Keyframe(1, 1, 2, 0));
    [SerializeField] private AnimationCurve riseCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 3, 3));
    [SerializeField] private AnimationCurve bounceCurve = new AnimationCurve(new Keyframe(0, 0, 2, 2), new Keyframe(1, 1, 0, 0));

    private Vector3 originalPosition;
    private bool isMoving = false;

    private void Start()
    {
        originalPosition = transform.position;
    }

    public void ActivateWall()
    {
        if (!isMoving)
        {
            StartCoroutine(WallMovementSequence());
        }
    }

    private IEnumerator WallMovementSequence()
    {
        isMoving = true;

        // Опускание стены
        yield return StartCoroutine(MoveWallDown());

        // Удержание внизу
        yield return new WaitForSeconds(holdTime);

        // Подъем стены обратно
        yield return StartCoroutine(MoveWallUp());

        // Анимация отскока
        yield return StartCoroutine(BounceAnimation());

        isMoving = false;
    }

    private IEnumerator MoveWallDown()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = originalPosition + Vector3.down * moveDistance;
        float elapsedTime = 0f;

        while (elapsedTime < moveDownTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDownTime;
            float curveValue = moveDownCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            yield return null;
        }

        transform.position = targetPos;
    }

    private IEnumerator MoveWallUp()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = originalPosition;
        float elapsedTime = 0f;

        while (elapsedTime < riseTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / riseTime;
            float curveValue = riseCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            yield return null;
        }

        transform.position = targetPos;
    }

    private IEnumerator BounceAnimation()
    {
        Vector3 startPos = originalPosition;
        Vector3 bouncePos = originalPosition + Vector3.down * bounceHeight;
        float elapsedTime = 0f;

        // Подскок вниз
        while (elapsedTime < bounceTime / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (bounceTime / 2);
            float curveValue = bounceCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(startPos, bouncePos, curveValue);
            yield return null;
        }

        elapsedTime = 0f;

        // Возврат вверх
        while (elapsedTime < bounceTime / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (bounceTime / 2);
            float curveValue = bounceCurve.Evaluate(1f - progress);

            transform.position = Vector3.Lerp(startPos, bouncePos, curveValue);
            yield return null;
        }

        transform.position = originalPosition;
    }

    // Публичный метод для проверки состояния (если нужно)
    public bool IsWallMoving()
    {
        return isMoving;
    }
}