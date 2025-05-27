using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Animator animator;
    private bool isPopped = false;
    private AudioSource audioSource;
    public GameObject parentObject;

    [SerializeField] private AudioClip popSound;
    [SerializeField] private float soundVolume = 1.0f;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPopped)
        {
            isPopped = true;

            if (popSound != null)
            {
                audioSource.PlayOneShot(popSound, soundVolume);
            }

            animator.SetTrigger("Pop");

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
