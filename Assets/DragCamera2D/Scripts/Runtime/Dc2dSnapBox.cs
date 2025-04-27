using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Dc2dSnapBox : MonoBehaviour
{
    /*
     * This is a snap box and is a target window for the 2d camera. 
     * While the player/entity/camera is in this window the camera will try to stay in its bounds
     * if the character exits then the camera returns to free follow mode and can be picked up by another snap box or bounds
     * can overlap with othe snap boxes and highest priority box will have control
     * 
     */

    public int priority = 0;
    private DragCamera2D maincam; // reference to main cam for update

    public enum Mode { NONE, // no expansion
    EXPANDX, // zoom out until viewport >= width of snapbox or max zoom reached
    EXPANDY, // zoom out until viewport >= height of snapbox
    CONTRACTX, // zoom in until viewport =< width 
    CONTRACTY, // zoom in until viewport =< height
    BOTHX, // zoom in or out until width is = snapbox width
    BOTHY} // zoom in or out until viewport is = snapobox height
    public Vector3 endPoint;

    [Tooltip("Zoom otions")]
    public Mode expandMode = Mode.NONE;
    [Tooltip("Colour of snapbox in editor")]
    public Color guiColour = new Color(0.7f,0.7f,0.7f,0.2f);
    public string sbName = "snapbox1";
    private GameObject followTarget;
    [Range(0.001f,1f)]
    [Tooltip("Speed of zooming in or out")]
    public float zoomSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        maincam = Camera.main.GetComponent<DragCamera2D>();
        if(maincam.followTarget != null) {
            followTarget = maincam.followTarget;
}

    }
#if UNITY_EDITOR
    private void OnDrawGizmos() {
        drawme();
    }
#endif


    // Update is called once per frame
    void Update()
    {
        if(followTarget != null) {
            if (followTarget.transform.position.x > transform.position.x && followTarget.transform.position.x < endPoint.x) {
                if (followTarget.transform.position.y > transform.position.y && followTarget.transform.position.y < endPoint.y) {
                    //Debug.Log("Setting Snapbox to" + sbName);
                    maincam.setSnapTarget(this);
                }
            }
        } else {
            if (maincam.transform.position.x > transform.position.x && maincam.transform.position.x < endPoint.x) {
                if (maincam.transform.position.y > transform.position.y && maincam.transform.position.y < endPoint.y) {
                    //Debug.Log("Setting Snapbox to" + sbName);
                    maincam.setSnapTarget(this);
                }
            }
        }
    }

    public Vector3 getExpandYUpperBound() {
        Vector3 target = getCenter();
        target.y = endPoint.y;
        return target;
    }

    public Vector3 getExpandYLowerBound() {
        Vector3 target = getCenter();
        target.y = transform.position.y;
        return target;
    }

    public Vector3 getExpandXUpperBound() {
        Vector3 target = getCenter();
        target.x = endPoint.x;
        return target;
    }

    public Vector3 getExpandXLowerBound() {
        Vector3 target = getCenter();
        target.x = transform.position.x;
        return target;
    }

    public Vector3 getCenter() {
        float cx = (endPoint.x - transform.position.x) / 2 + transform.position.x;
        float cy = (endPoint.y - transform.position.y) / 2 + transform.position.y;
        return new Vector3(cx, cy, 0);
    }

    //editor only
    private void drawme() {
#if UNITY_EDITOR 
        Rect boundArea = new Rect();
        boundArea.x = transform.position.x;
        boundArea.y = transform.position.y;
        boundArea.width = endPoint.x - transform.position.x;
        boundArea.height = endPoint.y - transform.position.y;
        Vector3[] verts = new Vector3[]
        {
            transform.position,
            transform.position + new Vector3(0, boundArea.height, 0),
            transform.position + new Vector3(boundArea.width, boundArea.height, 0),
            transform.position + new Vector3(boundArea.width,0 , 0)
        };
        Handles.DrawSolidRectangleWithOutline(verts, guiColour, Color.black);
#endif
    }
}
