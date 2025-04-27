using UnityEngine;
using System.Collections;

public class Building1 : MonoBehaviour
{
    [Header("Fire Settings")]
    public GameObject fireEffectObject;    // ���������� ������ ����
    public LayerMask fireLayer;            // ����, �� ������� ��������� ������� ����
    public float fireDamageRate = 5f;      // �������� ��������� ����� ������ (� �������)
    public float maxHealth = 100f;         // ������������ �������� ������

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onFireStart;    // ������� ��� ������ ������
    public UnityEngine.Events.UnityEvent onDestroyed;    // ������� ��� ���������� ������

    private bool isOnFire = false;         // ����, �����������, ����� �� ������
    private float currentHealth;           // ������� �������� ������

    private void Start()
    {
        // ������������� ���������� ��������
        currentHealth = maxHealth;

        // ��������, ��� ������ ���� ���������� ��������
        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ���������, ��� ������ ��� �� ����� � �������� ��������� � �������� �� ���� ����
        if (!isOnFire && ((1 << collision.gameObject.layer) & fireLayer) != 0)
        {
            CatchFire();
        }
    }

    // ����� ��� �������� ������������ � ����������
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // ���������, ��� ������ ��� �� ����� � �������� ��������� � �������� �� ���� ����
        if (!isOnFire && ((1 << collider.gameObject.layer) & fireLayer) != 0)
        {
            CatchFire();
        }
    }

    // ��������� ������� ��� ������� ������
    public void CatchFire()
    {
        if (isOnFire) return; // ���� ������ ��� �����, ������� �� ������

        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(true);
            isOnFire = true;

            // �������� ������� ������ ������
            onFireStart?.Invoke();

            // ��������� �������� ��� ��������� ����� �� ����
            StartCoroutine(TakeDamageFromFire());
        }
        else
        {
            Debug.LogWarning("Fire effect object not assigned on " + gameObject.name);
        }
    }

    // ������� ��� ������� ������
    public void ExtinguishFire()
    {
        if (!isOnFire) return; // ���� ������ �� �����, ������� �� ������

        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
            isOnFire = false;

            // ������������� ��� �������� (������� ���� �� ����)
            StopAllCoroutines();
        }
    }

    // �������� ��� ������������ ��������� ����� �� ����
    private IEnumerator TakeDamageFromFire()
    {
        while (isOnFire && currentHealth > 0)
        {
            // ������� ����
            currentHealth -= fireDamageRate * Time.deltaTime;

            // ���������, ��������� �� ������
            if (currentHealth <= 0)
            {
                DestroyBuilding();
                break;
            }

            yield return null; // ���� �� ���������� �����
        }
    }

    // ������� ��� ���������� ������
    private void DestroyBuilding()
    {
        // �������� ������� ���������� ������
        onDestroyed?.Invoke();

        // ����� ����� �������� ������� ����������, ����� � �.�.
        Debug.Log(gameObject.name + " destroyed by fire!");

        // ����� ���������� ������ ��� �������� ��� �� ����������� ������
        // ��������:
        // Instantiate(destroyedVersionPrefab, transform.position, transform.rotation);
        // Destroy(gameObject);
    }

    // ��������� ����� ��� ��������� ����������, ����� �� ������
    public bool IsOnFire()
    {
        return isOnFire;
    }

    // ��������� ����� ��� ��������� �������� ����������� ��������
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}