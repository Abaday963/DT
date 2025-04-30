using UnityEngine;

public class ExplosionAnimator : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        PlayExplosion();
    }

    public void PlayExplosion()
    {
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
    }
}
