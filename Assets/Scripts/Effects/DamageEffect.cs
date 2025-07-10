using UnityEngine;
using System.Collections;

public class DamageEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // перетащи сюда SpriteRenderer из инспектора
    public float flashDuration = 0.1f; // длительность эффекта

    private Color originalColor;

    void Start()
    {
        originalColor = spriteRenderer.color;
    }

    public void TakeDamageEffect()
    {
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
