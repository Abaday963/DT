//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class BulletAmmunition : MonoBehaviour
//{
//    [SerializeField] private Rigidbody2D arrowRigidbody;
//    [SerializeField] private bool isPressed = false;
//    [SerializeField] private float maxDistance = 3f;
//    [SerializeField] private float launchForce = 10f; // Added force multiplier
//    [SerializeField] private Rigidbody2D shootRigid; // Reference to slingshot
//    [SerializeField] public GameObject ammoPrefab; // Prefab of this ammo
//    [SerializeField] public Transform spawnPoint; // Spawn point for new ammo

//    private Vector2 startPosition;
//    private Vector2 releaseDirection;

//    private void Start()
//    {
//        arrowRigidbody = GetComponent<Rigidbody2D>();

//        // Store initial position
//        startPosition = transform.position;

//        if (GetComponent<Collider2D>() == null)
//        {
//            gameObject.AddComponent<BoxCollider2D>();
//        }

//        // If no slingshot is assigned, find it automatically
//        if (shootRigid == null)
//        {
//            GameObject slingshot = GameObject.FindGameObjectWithTag("Slingshot");
//            if (slingshot != null)
//            {
//                shootRigid = slingshot.GetComponent<Rigidbody2D>();
//            }
//        }

//        // If no spawn point is assigned, try to find it
//        if (spawnPoint == null)
//        {
//            GameObject spawner = GameObject.FindGameObjectWithTag("AmmoSpawner");
//            if (spawner != null)
//            {
//                spawnPoint = spawner.transform;
//            }
//        }

//        // Add SpringJoint2D if it doesn't exist
//        if (shootRigid != null && !GetComponent<SpringJoint2D>())
//        {
//            SpringJoint2D springJoint = gameObject.AddComponent<SpringJoint2D>();
//            springJoint.connectedBody = shootRigid;
//            springJoint.distance = 0.5f;
//            springJoint.dampingRatio = 0.1f;
//            springJoint.frequency = 2.0f;
//        }
//    }

//    private void Update()
//    {
//        // PC input handling
//        if (Input.GetMouseButtonDown(0) && !isPressed)
//        {
//            CheckTouch(Camera.main.ScreenToWorldPoint(Input.mousePosition));
//        }

//        if (Input.GetMouseButtonUp(0) && isPressed)
//        {
//            ReleaseArrow();
//        }

//        // Mobile input handling
//        if (Input.touchCount > 0)
//        {
//            Touch touch = Input.GetTouch(0);

//            if (touch.phase == TouchPhase.Began && !isPressed)
//            {
//                CheckTouch(Camera.main.ScreenToWorldPoint(touch.position));
//            }

//            if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isPressed)
//            {
//                ReleaseArrow();
//            }
//        }

//        // Move arrow when held
//        if (isPressed && shootRigid != null)
//        {
//            Vector2 inputPos;

//            if (Input.touchCount > 0)
//            {
//                inputPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
//            }
//            else
//            {
//                inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//            }

//            // Limit pull distance
//            if (Vector2.Distance(inputPos, shootRigid.position) > maxDistance)
//            {
//                arrowRigidbody.position = shootRigid.position +
//                    (inputPos - shootRigid.position).normalized * maxDistance;
//            }
//            else
//            {
//                arrowRigidbody.position = inputPos;
//            }

//            // Calculate release direction for later use
//            releaseDirection = (shootRigid.position - arrowRigidbody.position).normalized;

//            // Rotate arrow in the direction of movement
//            if ((arrowRigidbody.position - shootRigid.position).sqrMagnitude > 0.1f)
//            {
//                float angle = Mathf.Atan2(releaseDirection.y, releaseDirection.x) * Mathf.Rad2Deg;
//                transform.rotation = Quaternion.Euler(0, 0, angle + 90); // +90 to make the arrow point forward
//            }
//        }

//        // When the arrow is flying, align it with its velocity direction
//        if (!isPressed && arrowRigidbody.bodyType == RigidbodyType2D.Dynamic && arrowRigidbody.linearVelocity.sqrMagnitude > 0.1f && GetComponent<SpringJoint2D>() == null)
//        {
//            float angle = Mathf.Atan2(arrowRigidbody.linearVelocity.y, arrowRigidbody.linearVelocity.x) * Mathf.Rad2Deg;
//            transform.rotation = Quaternion.Euler(0, 0, angle - 90);  // -90 to make arrow point forward
//        }
//    }

//    private void CheckTouch(Vector2 touchPos)
//    {
//        Collider2D hit = Physics2D.OverlapPoint(touchPos);

//        if (hit != null && hit.gameObject == gameObject)
//        {
//            isPressed = true;
//            arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
//        }
//    }

//    private void ReleaseArrow()
//    {
//        if (!isPressed) return;

//        isPressed = false;
//        arrowRigidbody.bodyType = RigidbodyType2D.Dynamic;

//        // Calculate force based on distance from slingshot
//        float distance = Vector2.Distance(arrowRigidbody.position, shootRigid.position);
//        float forceMagnitude = distance * launchForce;

//        // Apply force in the release direction 
//        StartCoroutine(LaunchWithForce(forceMagnitude));
//    }

//    private IEnumerator LaunchWithForce(float force)
//    {
//        // Remove the spring joint
//        SpringJoint2D springJoint = GetComponent<SpringJoint2D>();
//        if (springJoint != null)
//        {
//            springJoint.enabled = false;
//            Destroy(springJoint);
//        }

//        // Apply force
//        arrowRigidbody.AddForce(releaseDirection * -force, ForceMode2D.Impulse);

//        this.enabled = false;

//        yield return new WaitForSeconds(2);

//        // Create new ammo
//        SpawnNewAmmo();

//        Destroy(gameObject, 5);
//    }

//    private void OnMouseDown()
//    {
//        isPressed = true;
//        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;
//    }

//    private void OnMouseUp()
//    {
//        if (isPressed)
//        {
//            ReleaseArrow();
//        }
//    }

//    private void SpawnNewAmmo()
//    {
//        if (ammoPrefab != null && spawnPoint != null)
//        {
//            GameObject newAmmo = Instantiate(ammoPrefab, spawnPoint.position, Quaternion.identity);

//            // Make sure components are active
//            ArrowAmmunition newArrow = newAmmo.GetComponent<ArrowAmmunition>();
//            if (newArrow != null)
//            {
//                newArrow.enabled = true;
//            }

//            SpringJoint2D newSpringJoint = newAmmo.GetComponent<SpringJoint2D>();
//            if (newSpringJoint != null)
//            {
//                newSpringJoint.enabled = true;
//            }
//        }
//        else
//        {
//            StartCoroutine(ReloadScene());
//        }
//    }

//    private IEnumerator ReloadScene()
//    {
//        yield return new WaitForSeconds(1);
//        SceneManager.LoadScene(0);
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        // Stop arrow movement on collision
//        arrowRigidbody.linearVelocity = Vector2.zero;
//        arrowRigidbody.angularVelocity = 0f;
//        arrowRigidbody.bodyType = RigidbodyType2D.Kinematic;

//        // Attach arrow to the object it hit
//        transform.parent = collision.transform;
//    }
//}