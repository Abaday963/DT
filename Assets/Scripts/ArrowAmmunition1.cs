using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowAmmunition1 : MonoBehaviour, IAmmunition
{
    [SerializeField] private bool isPressed = false;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float launchForce = 10f; // ��������� ����
    [SerializeField] private Rigidbody2D shootRigid; // ������ �� �������
    [SerializeField] public GameObject ammoPrefab; // ������ ����� �� ����������
    [SerializeField] public Transform spawnPoint; // ����� ��������� ������ ����������
    [SerializeField] private float arrowDamage = 5f; // ���� �� ������

    private Vector2 startPosition;
    private Vector2 releaseDirection;
    private Rigidbody2D arrowRigidbody;

    private void Start()
    {
        arrowRigidbody = GetComponent<Rigidbody2D>();

        // ��������� ��������� �������
        startPosition = transform.position;

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        // ���� �� ��������� �������, ������ � �������������
        if (shootRigid == null)
        {
            GameObject slingshot = GameObject.FindGameObjectWithTag("Slingshot");
            if (slingshot != null)
            {
                shootRigid = slingshot.GetComponent<Rigidbody2D>();
            }
        }

        // ���� �� ��������� ����� ������, ��������� ����� �
        if (spawnPoint == null)
        {
            GameObject spawner = GameObject.FindGameObjectWithTag("AmmoSpawner");
            if (spawner != null)
            {
                spawnPoint = spawner.transform;
            }
        }

        // ��������� SpringJoint2D ���� ��� ���
        if (shootRigid != null && !GetComponent<SpringJoint2D>())
        {
            SpringJoint2D springJoint = gameObject.AddComponent<SpringJoint2D>();
            springJoint.connectedBody = shootRigid;
            springJoint.distance = 0.5f;
            springJoint.dampingRatio = 0.1f;
            springJoint.frequency = 2.0f;
        }
    }

    private void Update()
    {
        // ��������� PC �����
        if (Input.GetMouseButtonDown(0) && !isPressed)
        {
            CheckTouch(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            ReleaseArrow();
        }

        // ��������� ���������� �����
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && !isPressed)
            {
                CheckTouch(Camera.main.ScreenToWorldPoint(touch.position));
            }

            if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isPressed)
            {
                ReleaseArrow();
            }
        }

        // ����������� ������ ��� ���������
        if (isPressed && shootRigid != null)
        {
            Vector2 inputPos;

            if (Input.touchCount > 0)
            {
                inputPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            else
            {
                inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            // ����������� ��������� �����������
            if (Vector2.Distance(inputPos, shootRigid.position) > maxDistance)
            {
                arrowRigidbody.position = shootRigid.position +
                    (inputPos - shootRigid.position).normalized * maxDistance;
            }
            else
            {
                arrowRigidbody.position = inputPos;
            }

            // ��������� ����������� ������� ��� ������������� �����
            // �����: ����������� �� ������ � ������� ��� ����������� �������
            releaseDirection = (shootRigid.position - arrowRigidbody.position).normalized;

            // ������������ ������ � ����������� ��������
            if ((arrowRigidbody.position - shootRigid.position).sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(releaseDirection.y, releaseDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle + 90); // +90 ����� ������ ������ �������� ������
            }
        }

        // ����� ������ �����, ����������� � �� ����������� ��������
        if (!isPressed && arrowRigidbody.bodyType == RigidbodyType2D.Dynamic && arrowRigidbody.linearVelocity.sqrMagnitude > 0.1f && GetComponent<SpringJoint2D>() == null)
        {
            float angle = Mathf.Atan2(arrowRigidbody.linearVelocity.y, arrowRigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);  // -90 ����� ������ ������ �������� ������
        }
    }

    private void CheckTouch(Vector2 touchPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(touchPos);

        if (hit != null && hit.gameObject == gameObject)
        {
            isPressed = true;
            arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void ReleaseArrow()
    {
        if (!isPressed) return;

        isPressed = false;
        arrowRigidbody.bodyType = RigidbodyType2D.Dynamic;

        // ������������ ���� �� ������ ���������� �� �������
        float distance = Vector2.Distance(arrowRigidbody.position, shootRigid.position);
        float forceMagnitude = distance * launchForce;

        // �������� ����� ���������� IAmmunition
        Launch(releaseDirection * forceMagnitude);
    }

    private IEnumerator LaunchWithForce(float force)
    {
        // ������� ��������� ����������
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
            Destroy(springJoint);
        }

        // ��������� ���� - ��� ����� �����!
        arrowRigidbody.AddForce(releaseDirection * force, ForceMode2D.Impulse);

        this.enabled = false;

        yield return new WaitForSeconds(2);

        // ������� ����� ������
        SpawnNewAmmo();

        Destroy(gameObject, 5);
    }

    private void OnMouseDown()
    {
        isPressed = true;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnMouseUp()
    {
        if (isPressed)
        {
            ReleaseArrow();
        }
    }

    private void SpawnNewAmmo()
    {
        if (ammoPrefab != null && spawnPoint != null)
        {
            GameObject newAmmo = Instantiate(ammoPrefab, spawnPoint.position, Quaternion.identity);

            // ��������, ��� ���������� �������
            ArrowAmmunition newArrow = newAmmo.GetComponent<ArrowAmmunition>();
            if (newArrow != null)
            {
                newArrow.enabled = true;
            }

            SpringJoint2D newSpringJoint = newAmmo.GetComponent<SpringJoint2D>();
            if (newSpringJoint != null)
            {
                newSpringJoint.enabled = true;
            }
        }
        else
        {
            StartCoroutine(ReloadScene());
        }
    }

    private IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ���������, �������� �� ������������� ������ �����������
        IAmmunition ammunitionComponent = collision.gameObject.GetComponent<IAmmunition>();

        // ���� ������ - ���������, �� ���������� ������������
        if (ammunitionComponent != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            return; // ��������� return, ����� ����� �� ������ ��� ������ OnImpact()
        }

        // �������� ����� ���������� IAmmunition ������ ���� ��� �� ��������
        OnImpact();

        // ������� ����, ���� ������ ������������ ��������� IDamageable
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(arrowDamage);
        }
    }

    // ���������� ���������� IAmmunition
    public void OnImpact()
    {
        // ���������, �� ����������� �� �� � ������ �����������
        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
        bool hitAmmo = false;

        Debug.Log("OnImpact: ������� " + colliders.Length + " ����������� � ����� ������������");

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                Debug.Log("��������� ������: " + col.gameObject.name);

                if (col.gameObject.GetComponent<IAmmunition>() != null)
                {
                    Debug.Log("���������� ��������: " + col.gameObject.name + ", ���������� ��������� ������������");
                    hitAmmo = true;
                    break;
                }
            }
        }

        // ���� ����������� � ���������, ������ ���������� ��� ������������
        if (hitAmmo)
        {
            Debug.Log("������������ � ���������, ������� ��� ���������");
            return;
        }

        Debug.Log("������������� �������� ������ � ������ ��� �� Kinematic");
        // ��������� �������� ������ ������ ���� ��� �� ��������
        arrowRigidbody.linearVelocity = Vector2.zero;
        arrowRigidbody.angularVelocity = 0f;
        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

        // ����������� ������ � �������, � ������� ��� ������
        if (transform.parent == null) // ������ ���� ��� �� �����������
        {
            Debug.Log("�������� ���������� ������ � �������");
            foreach (Collider2D col in colliders)
            {
                if (col.gameObject != gameObject && col.gameObject.GetComponent<IAmmunition>() == null)
                {
                    Debug.Log("����������� ������ � �������: " + col.transform.name);
                    transform.parent = col.transform;
                    break;
                }
            }
        }
    }

    // ���������� ���������� IAmmunition
    public void Launch(Vector2 force)
    {
        // ������� ��������� ����������
        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
        if (springJoint != null)
        {
            springJoint.enabled = false;
            Destroy(springJoint);
        }

        // ��������� ����
        arrowRigidbody.AddForce(force, ForceMode2D.Impulse);
        this.enabled = false;

        // ��������� �������� ��� �������� ������ �������
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(2);
        SpawnNewAmmo();
        Destroy(gameObject, 5);
    }
}
