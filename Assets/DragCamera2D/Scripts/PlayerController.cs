using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    // simple player script to test player follow
    public float speed = 10;
    private bool jump = false;

    private Rigidbody2D rb;

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update() {
        // moved event to update for missed triggers on high refresh rates
        if (Input.GetKeyDown(KeyCode.Space)) {
            jump = true;
        } 
    }


    private void FixedUpdate() {

        float x = Input.GetAxisRaw("Horizontal") * speed;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            x *= 2f;
        }

        if (jump) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10);
            jump = false; // event handled
        }

        rb.AddForce(new Vector2(x, 0));

        transform.GetChild(0).transform.rotation = Quaternion.Euler(0.0f, 0.0f, gameObject.transform.rotation.z * -1.0f);
    }
}