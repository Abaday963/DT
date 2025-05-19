using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFloating = true;
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 1f;
    private Vector3 startPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // чтобы не падал
        startPos = transform.position;
    }

    private void Update()
    {
        if (isFloating)
        {
            float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
        }
    }

    public void OnBalloonPopped()
    {
        isFloating = false;
        rb.isKinematic = false;
        rb.gravityScale = 1f; // включаем падение
    }
}
