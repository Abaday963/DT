using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Animator animator;
    private bool isPopped = false;
    public GameObject parentObject;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPopped)
        {
            isPopped = true;
            animator.SetTrigger("Pop"); // триггер анимации
            if (parentObject != null)
            {
                parentObject.GetComponent<FloatingObject>().OnBalloonPopped();
            }
        }
    }

    // вызывается анимацией через Animation Event
    public void DestroyBalloon()
    {
        Destroy(gameObject);
    }
}
