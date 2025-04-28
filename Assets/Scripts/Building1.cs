using UnityEngine;
using System.Collections;

public class Building1 : MonoBehaviour
{
    public GameObject fireEffectObject;
    public LayerMask fireLayer;
    private bool isOnFire = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"2D Collision detected with: {collision.gameObject.name}");

        // Check if object is not already on fire 
        // And collision is with object on 'Shard fire' layer
        if (!isOnFire && ((1 << collision.gameObject.layer) & fireLayer) != 0)
        {
            // Enable fire effect child object
            if (fireEffectObject != null)
            {
                fireEffectObject.SetActive(true);
                isOnFire = true;

                // Notify GameManager that building has caught fire
                GameManager.Instance.OnBuilding1Caught();

                // Start burning process
                StartCoroutine(HandleBurning());
            }
            else
            {
                Debug.LogWarning("Fire effect object not assigned!");
            }
        }
    }

    // Coroutine to manage burning process
    private IEnumerator HandleBurning()
    {
        Debug.Log("Object caught fire from specific object!");

        // Burning duration
        yield return new WaitForSeconds(5f);

        // Stop burning
        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
            isOnFire = false;
        }
    }
}