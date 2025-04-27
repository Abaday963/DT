using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dc2dZoomTarget : MonoBehaviour
{
    [HideInInspector]
    public enum Trigger{ MouseLeft = 0,
        MouseRight =1,
        MouseMiddle =3
    }

    float lastClickTime = 0f;
    [Range(0.01f,0.5f)]
    public float doubleClickMaxDelay = 0.3f;
    bool doubleClickValid = false;
    public Trigger doubleClickTrigger = Trigger.MouseLeft;
    bool lastMouseState = false; // true = mousedown
    [HideInInspector]
    public bool zoomComplete = true;

    [HideInInspector]
    public bool zoomToMe = false;

    void Update()
    {
        if (lastMouseState == false && Input.GetMouseButton((int)doubleClickTrigger)) {
            onTriggereDown();
            lastMouseState = true;
        }else if(lastMouseState == true && Input.GetMouseButton((int)doubleClickTrigger) == false) {
            onTriggerUp();
            lastMouseState = false;
        }

        lastClickTime += Time.deltaTime;
        if(doubleClickValid && lastClickTime > doubleClickMaxDelay) {
            doubleClickValid = false;
        }
    }

    void onTriggereDown() {
        if(doubleClickValid && lastClickTime <= doubleClickMaxDelay) {
            zoomToMe = true;
            zoomComplete = false;
            Invoke("stopAll", 0.5f); // temp fix to stop zoom to target lock when clamped tp area
            moveToMouseLoc();
        }
        lastClickTime = 0;
    }

    void moveToMouseLoc() {
        Vector3 clickpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = clickpos +  new Vector3(0,0,9);
    }

    void onTriggerUp() {
        doubleClickValid = true;
    }

    public void stopAll() {
        zoomToMe = false;
        zoomComplete = true;
    }

}
