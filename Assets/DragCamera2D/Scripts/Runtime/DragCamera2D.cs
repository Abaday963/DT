using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragCamera2D : MonoBehaviour
{
    /*
     *TODO: 
     *  DONE: replace dolly with bezier dolly system
     *  DONE: add dolly track smoothing
     *  DONE: add dolly track straightening
     *  DONE: Dolly track + gizmo colours
     *  DONE: add non tracked constant speed dolly system(continuous movement based on time)
     *  WONTDO: [REPLACED BY FEATURE BELOW] add button to split dolly track evenly (between start and end) for time based dolly movement
     *  DONE: button to adjust times on all waypoints so camera moves at a constant speed
     *  DONE: add per waypoint time (seconds on this segment)
     *  DONE: add scaler for time to next waypoint in scene viewe gui
     *  DONE: improve GUI elements (full custom editor inspector)
     *  DONE:    add waypoint gui  scene view button
     *  DONE: better designed example scenes
     *  DONE: option to lock camera to track even if object escapes area
     *  add multiple dolly tracks to allow creating loops etc
     *  add track change triggers
     *  add bounds ids for multiple bounds
     *  add bounds triggers(e.g. small bounds until x event(obtain key etc) then larger bounds
     *  add configurable keymap to allow developers/usres to map keys to actions
     *  DONE: add in scene dolly track controls
     *  possibly add event system for lerping camera to position
     *  possibly make dolly track event system to allow camera to track dolly after an event then return to user control(for cutscenes/tutorial etc)
     *  
     *  Requests:
     *  ADDED: Zoom/Translate to double click position
     *  ADDED:  Translate to double click
     *  ADDED: Zoom to double click
     *  TODO: Scroll Snapping
     *  
     *  Bugfixes:
     *  The name does not exists in current content during build : fix supplied by  @chimerian
     *  zoom to mouse would translate camera position even when fully zoomed in our out. FIXED
     *  
     *  BUGS TO FIX:
     *  Double clicking area restricted by area clamp locks camera in lerp to double click target
    */

    public Camera cam;

    [Header("Camera Movement")]

    [Tooltip("Allow the Camera to be dragged.")]
    public bool dragEnabled = true;
    [Tooltip("Mouse button responsible for drag.")]
    public MouseButton mouseButton;
    [Range(-5, 5)]
    [Tooltip("Speed the camera moves when dragged.")]
    public float dragSpeed = -0.06f;

    [Header("Double Click Action (DC)")]
    public Dc2dZoomTarget ztTarget;
    [Tooltip("Move to DC location")]
    public bool DCTranslate = true;
    [Tooltip("Zoom DC location")]
    public bool DCZoom = true;
    [Tooltip("Target Zoom Level for DC")]
    [Range(0.1f, 10f)]
    public float DCZoomTargetIn = 4f;
    [Tooltip("DC Translation Speed")]
    [Range(0.01f, 1f)]
    public float DCZoomTranslateSpeed = 0.5f;
    [Tooltip("Target Zoom Level for DC")]
    [Range(0.1f, 10f)]
    public float DCZoomTargetOut = 10f;
    [Tooltip("DC Zoom Speed")]
    [Range(0.01f, 1f)]
    public float DCZoomSpeed = 0.5f;
    private bool DCZoomedOut = true;

    [Header("Edge Scrolling")]
    [Tooltip("Pixel Border to trigger edge scrolling")]
    public int edgeBoundary = 20;
    [Range(0, 10)]
    [Tooltip("Speed the camera moves Mouse enters screen edge.")]
    public float edgeSpeed = 1f;

    [Header("Touch(PRO) & Keyboard Input")]
    [Tooltip("Enable or disable Keyboard input")]
    public bool keyboardInput = false;
    [Tooltip("Invert keyboard direction")]
    public bool inverseKeyboard = false;
    [Tooltip("Enable or disable touch input")]
    public bool touchEnabled = false;
    [Tooltip("Drag Speed for touch controls")]
    [Range(-5,5)]
    public float touchDragSpeed = -0.03f;

    [Header("Zoom")]
    [Tooltip("Enable or disable zooming")]
    public bool zoomEnabled = true;
    [Tooltip("Scale drag movement with zoom level")]
    public bool linkedZoomDrag = true;
    [Tooltip("Maximum Zoom Level")]
    public float maxZoom = 10;
    [Tooltip("Minimum Zoom Level")]
    [Range(0.01f, 10)]
    public float minZoom = 0.5f;
    [Tooltip("The Speed the zoom changes")]
    [Range(0.1f, 10f)]
    public float zoomStepSize = 0.5f;
    [Tooltip("Enable Zooming to mouse pointer")]
    public bool zoomToMouse = false;

   


    [Header("Follow Object")]
    public GameObject followTarget;
    [Range(0.01f,1f)]
    public float lerpSpeed = 0.5f;
    public Vector3 offset = new Vector3(0,0,-10);


    [Header("Camera Bounds")]
    public bool clampCamera = true;
    public CameraBounds bounds; 
    public Dc2dDolly dollyRail;

    
    //hidden
    [HideInInspector]
    public enum MouseButton {
        Left = 0,
        Middle = 2,
        Right = 1
    }

    // private vars
    Vector3 bl;
    Vector3 tr;
    private Vector2 touchOrigin = -Vector2.one;

    public Dc2dSnapBox snapTarget;

    int frameid = 0;

    void Start() {
        if (cam == null) {
            cam = Camera.main;
        }
    }

    void LateUpdate() {
        frameid++;
        if (dragEnabled) {
            panControl();
        }

        if (edgeBoundary > 0) {
            edgeScroll();
        }


        if (zoomEnabled) {
            zoomControl();
        }

        if (snapTarget != null) {
            //using snap targets do da snap
            conformToSnapTarget();
        } else {
            if (followTarget != null) {
                transform.position = Vector3.Lerp(transform.position, followTarget.transform.position + offset, lerpSpeed);
            }
        }

        if (DCTranslate || DCZoom) {
            zoomTarget();
        }

        if (clampCamera) {
            cameraClamp();
        }

        if (touchEnabled) {
            doTouchControls();
        }

        if(dollyRail != null) {
            stickToDollyRail();
        }
    }

    private void zoomTarget() {
        if (ztTarget == null) {
            throw new UnassignedReferenceException("No Dc2dZoomTarget object. Please add one to your scene from the prefab folder, create an object with the Dc2dZoomTarget script or turn off Double Click zoom actions");
        }

        if (ztTarget.zoomToMe && DCTranslate) {
            Vector3 targetLoc = ztTarget.transform.position;
            targetLoc.z = offset.z; // lock ofset to cams offset
            transform.position = Vector3.Lerp(transform.position , targetLoc, 0.3f);
            if(ztTarget.zoomToMe && Vector3.Distance(transform.position, targetLoc) < 0.2f) {
                ztTarget.zoomToMe = false;
            }
        }
        if (DCZoom && !ztTarget.zoomComplete) {
            if (DCZoomedOut) {
                Camera.main.orthographicSize = Mathf.Lerp(DCZoomTargetIn, Camera.main.orthographicSize, 0.1f);
                if (Camera.main.orthographicSize == DCZoomTargetIn) {
                    ztTarget.zoomComplete = true;
                    DCZoomedOut = !DCZoomedOut;
                }
            } else {
                Camera.main.orthographicSize = Mathf.Lerp(DCZoomTargetOut, Camera.main.orthographicSize, 0.1f);
                if (Camera.main.orthographicSize == DCZoomTargetOut) {
                    ztTarget.zoomComplete = true;
                    DCZoomedOut = !DCZoomedOut;
                }
            }
        }
    }

    private void edgeScroll() {
        float x = 0;
        float y = 0;
        if (Input.mousePosition.x >= Screen.width - edgeBoundary) {
            // Move the camera
            x = Time.deltaTime * edgeSpeed;
        }
        if (Input.mousePosition.x <= 0 + edgeBoundary) {
            // Move the camera
            x = Time.deltaTime * -edgeSpeed;
        }
        if (Input.mousePosition.y >= Screen.height - edgeBoundary) {
            // Move the camera
            y = Time.deltaTime * edgeSpeed
;
        }
        if (Input.mousePosition.y <= 0 + edgeBoundary) {
            // Move the camera
            y =  Time.deltaTime * -edgeSpeed
;
        }
        transform.Translate(x, y, 0);
    }

    public void addCameraDolly() {
        if (dollyRail == null) {
            GameObject go = new GameObject("Dolly");
            Dc2dDolly dolly = go.AddComponent<Dc2dDolly>();

            Dc2dWaypoint wp1 = new Dc2dWaypoint();
            wp1.position = new Vector3(0, 0, 0);

            Dc2dWaypoint wp2 = new Dc2dWaypoint();
            wp2.position = new Vector3(1, 0, 0);

            Dc2dWaypoint[] dc2dwaypoints = new Dc2dWaypoint[2];
            dc2dwaypoints[0] = wp1;
            dc2dwaypoints[1] = wp2;
            wp1.endPosition = wp2.position;


            dolly.allWaypoints = dc2dwaypoints;

            //Vector3[] waypoints = new Vector3[2];
            //waypoints[0] = new Vector3(0, 0, 0);
            //waypoints[1] = new Vector3(1, 1, 0);
            //dolly.dollyWaypoints = waypoints;

            //Vector3[] bezpoints = new Vector3[1];
            //bezpoints[0] = new Vector3(0.5f, 0.5f, 0);
            //dolly.bezierWaypoints = bezpoints;

            this.dollyRail = dolly;

#if UNITY_EDITOR
            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
#endif
        }
    }

    public void addCameraBounds() {
        if (bounds == null) {
            GameObject go = new GameObject("CameraBounds");
            CameraBounds cb = go.AddComponent<CameraBounds>();
            cb.guiColour = new Color(0,0,1f,0.1f);
            cb.pointa = new Vector3(20,20,0);
            this.bounds = cb;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    // adds a zoom target object to the scene to enable double click zooming
    public void addZoomTarget() {
        if (ztTarget == null) {
            GameObject go = new GameObject("Dc2dZoomTarget");
            Dc2dZoomTarget zt = go.AddComponent<Dc2dZoomTarget>();
            this.ztTarget = zt;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    public void doTouchControls() {
       // PRO Only
    }

    //click and drag
    public void panControl() {
        // if keyboard input is allowed
        if (keyboardInput) {
            float x = -Input.GetAxis("Horizontal") * dragSpeed;
            float y = -Input.GetAxis("Vertical") * dragSpeed;

            if (linkedZoomDrag) {
                x *= Camera.main.orthographicSize;
                y *= Camera.main.orthographicSize;
            }

            if (inverseKeyboard) {
                x = -x;
                y = -y;
            }
            transform.Translate(x, y, 0);
        }

       

       // if mouse is down
        if (Input.GetMouseButton((int)mouseButton)) {
            float x = Input.GetAxis("Mouse X") * dragSpeed;
            float y = Input.GetAxis("Mouse Y") * dragSpeed;

            if (linkedZoomDrag) {
                x *= Camera.main.orthographicSize;
                y *= Camera.main.orthographicSize;
            }

            transform.Translate(x, y, 0);
        }

        
    }

    private void clampZoom() {
        Camera.main.orthographicSize =  Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        Mathf.Max(cam.orthographicSize, 0.1f);


    }

    void ZoomOrthoCamera(Vector3 zoomTowards, float amount) {
        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / Camera.main.orthographicSize * amount);
        // Move camera
        transform.position += (zoomTowards - transform.position) * multiplier;
        // Zoom camera
        Camera.main.orthographicSize -= amount;
        // Limit zoom
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }

    // managae zooming
    public void zoomControl() {
        if (zoomToMouse) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && minZoom < Camera.main.orthographicSize) // forward
            {
                ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), zoomStepSize);
            }
            if(Input.GetAxis("Mouse ScrollWheel") < 0 && maxZoom > Camera.main.orthographicSize) // back            
            {
                ZoomOrthoCamera(Camera.main.ScreenToWorldPoint(Input.mousePosition), -zoomStepSize);
            }

        } else {

            if (Input.GetAxis("Mouse ScrollWheel") > 0 && minZoom < Camera.main.orthographicSize) // forward
            {
                Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && maxZoom > Camera.main.orthographicSize) // back            
            {
                Camera.main.orthographicSize = Camera.main.orthographicSize + zoomStepSize;
            }
        }
        clampZoom();
    }


    private bool lfxmax = false;
    private bool lfxmin = false;
    private bool lfymax = false;
    private bool lfymin = false;

    // Clamp Camera to bounds
    private void cameraClamp() {
        tr = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bl = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if(bounds == null) {
            Debug.Log("Clamp Camera Enabled but no Bounds has been set.");
            return;
        }

        float boundsMaxX = bounds.pointa.x;
        float boundsMinX = bounds.transform.position.x;
        float boundsMaxY = bounds.pointa.y;
        float boundsMinY = bounds.transform.position.y;

        if (tr.x > boundsMaxX && bl.x < boundsMinX) {
            Debug.Log("User tried to zoom out past x axis bounds - locked to bounds");
            Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
            clampZoom();
        }

        if (tr.y > boundsMaxY && bl.y < boundsMinY) {
            Debug.Log("User tried to zoom out past y axis bounds - locked to bounds");
            Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
            clampZoom();
        }

        bool tfxmax = false;
        bool tfxmin = false;
        bool tfymax = false;
        bool tfymin = false;

        if (tr.x > boundsMaxX) {
            if (lfxmin) {
                Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
                clampZoom();
            } else {
                transform.position = new Vector3(transform.position.x - (tr.x - boundsMaxX), transform.position.y, transform.position.z);
                tfxmax = true;
            }
        }
        if (tr.y > boundsMaxY) {
            if (lfymin) {
                Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
                clampZoom();
            } else {
                transform.position = new Vector3(transform.position.x, transform.position.y - (tr.y - boundsMaxY), transform.position.z);
                tfymax = true;
            }
        } 
        if (bl.x < boundsMinX) {
            if (lfxmax) {
                Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
                clampZoom();
            } else {
                transform.position = new Vector3(transform.position.x + (boundsMinX - bl.x), transform.position.y, transform.position.z);
                tfxmin = true;
            }
        }
        if (bl.y < boundsMinY) {
            if (lfymax) {
                Camera.main.orthographicSize = Camera.main.orthographicSize - zoomStepSize; // zoomControl in to compensate
                clampZoom();
            } else {
                transform.position = new Vector3(transform.position.x, transform.position.y + (boundsMinY - bl.y), transform.position.z);
                tfymin = true;
            }
        }

        lfxmax = tfxmax;
        lfxmin = tfxmin;
        lfymax = tfymax;
        lfymin = tfymin;
    }

    public void stickToDollyRail() {
        if(dollyRail != null && followTarget != null) {
            Vector3 campos = dollyRail.getPositionOnTrack(followTarget.transform.position);
            transform.position = new Vector3(campos.x, campos.y, transform.position.z);
        }
    }

    public void conformToSnapTarget() {
        float cx = (snapTarget.endPoint.x - snapTarget.transform.position.x)/2 + snapTarget.transform.position.x;
        float cy = (snapTarget.endPoint.y - snapTarget.transform.position.y)/ 2 + snapTarget.transform.position.y;
        transform.position = Vector3.Lerp(transform.position, new Vector3(cx, cy, transform.position.z), 0.5f ); // has to be fast to counter zoom jitter
        expandToZoomTarget();
        snapTarget = null; // target set rest for next frame
    }

    private void expandToZoomTarget() {
        if(snapTarget != null) {
            
            //contractzoom
            if (snapTarget.expandMode == Dc2dSnapBox.Mode.CONTRACTY || snapTarget.expandMode == Dc2dSnapBox.Mode.BOTHY) {
                if (Camera.main.WorldToViewportPoint(snapTarget.getExpandYUpperBound()).y < 1.049f) {
                    Camera.main.orthographicSize = Camera.main.orthographicSize - snapTarget.zoomSpeed;
                    //Debug.Log(snapTarget.sbName + ":CONTRACTY for:" + frameid);
                }
            } else if (snapTarget.expandMode == Dc2dSnapBox.Mode.CONTRACTX || snapTarget.expandMode == Dc2dSnapBox.Mode.BOTHX) {
                if (Camera.main.WorldToViewportPoint(snapTarget.getExpandXUpperBound()).x < 1.049f) {
                    Camera.main.orthographicSize = Camera.main.orthographicSize - snapTarget.zoomSpeed;
                    //Debug.Log(snapTarget.sbName + ":CONTRACTX for:" + frameid);
                }
            }
            //expandzoom
            if (snapTarget.expandMode == Dc2dSnapBox.Mode.EXPANDY || snapTarget.expandMode == Dc2dSnapBox.Mode.BOTHY) {
                if (Camera.main.WorldToViewportPoint(snapTarget.getExpandYUpperBound()).y > 0.95f) {
                    Camera.main.orthographicSize = Camera.main.orthographicSize + snapTarget.zoomSpeed;
                    //Debug.Log(snapTarget.sbName + ":EXPANDY for:" + frameid);
                }
            } else if (snapTarget.expandMode == Dc2dSnapBox.Mode.EXPANDX || snapTarget.expandMode == Dc2dSnapBox.Mode.BOTHX) {
                if (Camera.main.WorldToViewportPoint(snapTarget.getExpandXUpperBound()).x > 0.95f) {
                    Camera.main.orthographicSize = Camera.main.orthographicSize + snapTarget.zoomSpeed;
                    //Debug.Log(snapTarget.sbName + ":EXPANDX for:" + frameid);
                }
            }
        }
    }

    public void setSnapTarget(Dc2dSnapBox sb) {
        if(snapTarget == null) {
            //Debug.Log("New snapboxes:"+ frameid);
            snapTarget = sb;
            return;
        }
        if(sb.priority > snapTarget.priority) {
            //Debug.Log("Swapping snapboxes:"+ frameid);
            snapTarget = sb;
        }
    }
}
