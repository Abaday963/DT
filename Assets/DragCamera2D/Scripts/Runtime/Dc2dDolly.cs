using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dc2dDolly : MonoBehaviour
{
    public enum DollyMode {
        TrackObject,
        TrackTime
    }

    public enum WaypointTimeMode {
        PerWaypointTime,
        PerTrackTime
    }

    public DollyMode mode = DollyMode.TrackObject ;

    [Tooltip("Lock camera to track or follow tracked object when outside track area.")]
    public bool lockOnTrack = false;

    public Dc2dWaypoint[] allWaypoints;

    [Header("Track Time options")]
    [Tooltip("Delay Track start in seconds(can use negatives to jump ahead on track)")]
    public float delayStart = 0f;
    [Tooltip("Set time per waypoint or set time for track")]
    public WaypointTimeMode timeMode = WaypointTimeMode.PerWaypointTime;
    [Tooltip("Set the time for the camera to traverse from the start to the end")]
    public float totalTrackTime = 10f;

    [Header("Bezier & Smoothing")]
    [Tooltip("Limit curves so camera stays at constant speed.")]
    public bool constantSpeed = true;

    [Header("GUI Options")]
    [Tooltip("Show in scene controls")]
    public bool inScreenControls = true;
    [Tooltip("Show Length of curve to next waypoint")]
    public bool showLength = true;
    [Tooltip("Show Time to traverse curve to next waypoint")]
    public bool showTimes = true;
    [Tooltip("Show Handles to adjust time to next waypoint")]
    public bool showTimeAdjustHandles = true;
    [Tooltip("Show dolly track during gameplay.")]
    public bool renderDollyTrack = false;
    /*
    [Tooltip("Color of waypoints (Circles)")]
    public Color waypointColor = Color.white;
    [Tooltip("Color of handles that effect curvature (Small Squares)")]
    public Color tangentHandleColor = Color.white;
    [Tooltip("Color of hashed lines between parent")]
    public Color tangentGuideColor = Color.gray;
    [Tooltip("Color of waypoint connections")]
    public Color gizmoColor = Color.gray;
    [Tooltip("Color of Time Scale Handle")]
    public Color timeScaleHandleColor = Color.red;
    [Tooltip("Color of the Dolly track")]
    public Color dollyTrackColor = Color.red;
    */

    // private
    private LineRenderer lineRenderer;
    private int accuracy = 20;
    private float onTrackTime = 0f;
   

    private void Awake() {
    }

    private void Start() {
        if (!lineRenderer) {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerName = "Top";
        lineRenderer.material = new Material(Shader.Find("UI/Unlit/Text"));
        lineRenderer.material.color = Color.red;
#if UNITY_EDITOR
        Color dollyTrackColor = Dc2dUtils.stringToColor(UnityEditor.EditorPrefs.GetString("dollyTrackColor", "#FF0000FF"));
        lineRenderer.material.color = dollyTrackColor * 0.1f;
#endif
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material.color = Color.red;
        lineRenderer.material.SetColor("_TintColor", new Color(1, 0, 0, 1f));

    }

    private void Update() {
        onTrackTime += Time.deltaTime;




        if (renderDollyTrack) {
            renderTrack();
        } else {
            lineRenderer.enabled = false;

        }
    }

    private void renderTrack() {
        for (int j = 0; j < allWaypoints.Length - 1; j++) {
            for (int i = 1; i <= accuracy; i++) {
                float t = i / (float)accuracy;
                int nodeIndex = j * 3;
                Vector3 pixel = Dc2dUtils.CalculateCubicBezierPoint(t, allWaypoints[j].position, allWaypoints[j].position + allWaypoints[j].tanOne, allWaypoints[j].endPosition + allWaypoints[j].tanTwo, allWaypoints[j].endPosition);
                lineRenderer.positionCount = ((j * accuracy) + i);
                lineRenderer.SetPosition((j * accuracy) + (i - 1), pixel);
            }
        }
        lineRenderer.enabled = true;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < allWaypoints.Length-1; i++) {
            Gizmos.color = new Color(.3f,.3f,.3f,1f);
#if UNITY_EDITOR
            Color gizmoColor = Dc2dUtils.stringToColor(UnityEditor.EditorPrefs.GetString("gizmoColor", "#333333FF"));
            Gizmos.color = gizmoColor;  
#endif
            Gizmos.DrawLine(allWaypoints[i].position, allWaypoints[i+1].position);       
        }
    }

    private Vector3 getBezierTarget(Vector3 start, Vector3 end) {
        Vector3 bezPoint = (start - end) * 0.5f;
        bezPoint = start + bezPoint;
        return bezPoint;
    }

    public void removeWaypointFromStart() {
        Dc2dWaypoint[] tempPoints = new Dc2dWaypoint[allWaypoints.Length - 1];
        for (int i = 1; i < allWaypoints.Length; i++) {
            tempPoints[i - 1] = allWaypoints[i];
        }
        allWaypoints = tempPoints;
    }

    public void addWaypointToStart() {
        Dc2dWaypoint[] tempPoints = new Dc2dWaypoint[allWaypoints.Length + 1];

        for (int i = 0; i < allWaypoints.Length; i++) {
            tempPoints[i+1] = allWaypoints[i];
        }

        Dc2dWaypoint newpoint = new Dc2dWaypoint();
        tempPoints[0] = newpoint;
        newpoint.position = tempPoints[1].getPreSpawn();
        tempPoints[0].endPosition = tempPoints[1].position;
        allWaypoints = tempPoints;

    }

    public void removeWaypointFromEnd() {
        Dc2dWaypoint[] tempPoints = new Dc2dWaypoint[allWaypoints.Length - 1];
        for (int i = 0; i < allWaypoints.Length - 1; i++) {
            tempPoints[i] = allWaypoints[i];
        }
        allWaypoints = tempPoints;
    }

    public void addWaypointToEnd() {
        Dc2dWaypoint[] tempPoints = new Dc2dWaypoint[allWaypoints.Length + 1];

        for(int i = 0; i < allWaypoints.Length; i++) {
            tempPoints[i] = allWaypoints[i];
        }

        Dc2dWaypoint newpoint = new Dc2dWaypoint();
        newpoint.position = tempPoints[tempPoints.Length - 2].getPostSpawn();
        tempPoints[tempPoints.Length - 2].endPosition = newpoint.position;
        tempPoints[tempPoints.Length - 1] = newpoint;
        allWaypoints = tempPoints;

    }

    public Vector3 getTimeBasedPosition() {
        Vector3 reVector;
        float dollyDelta = (onTrackTime - delayStart);
        reVector = allWaypoints[allWaypoints.Length - 1].position; // defaults to end of track. if no matches then likely to have completed all so hang on track end
        if (dollyDelta <= 0f) {
            // not started yet so set to first waypoint pos
            return allWaypoints[0].position;
        }
        float waypointStart = 0f;
        for (int i = 0; i < allWaypoints.Length - 1; i++) {
            if (dollyDelta < waypointStart + allWaypoints[i].timeToNextWaypoint) {
                // time is less than end so on this waypoint
                float posOnTrack = (dollyDelta - waypointStart + 0.000000001f) / allWaypoints[i].timeToNextWaypoint;
                reVector = Dc2dUtils.CalculateCubicBezierPoint(posOnTrack, allWaypoints[i].position, allWaypoints[i].position + allWaypoints[i].tanOne, allWaypoints[i].endPosition + allWaypoints[i].tanTwo, allWaypoints[i].endPosition);
                return reVector;
            }
            waypointStart += allWaypoints[i].timeToNextWaypoint;
        }
        return reVector;
    }

    public Vector3 getTrackedObjectBasedPosition(Vector3 trackedObject) {
        Vector3 reVector = Vector3.zero;
        bool pointFound = false;
        for (int i = 0; i < allWaypoints.Length - 1; i++) {
            if (trackedObject.x > allWaypoints[i].position.x && trackedObject.x < allWaypoints[i + 1].position.x) {
                pointFound = true;
                // get start point
                Vector3 startPoint = allWaypoints[i].position;
                // distance of x into dolly line (0-1)
                float distance = 1.0f - (allWaypoints[i].endPosition.x - trackedObject.x) / (allWaypoints[i].endPosition.x - allWaypoints[i].position.x);
                // multiply vector by distance
                reVector = Dc2dUtils.CalculateCubicBezierPoint(distance, allWaypoints[i].position, allWaypoints[i].position + allWaypoints[i].tanOne, allWaypoints[i].endPosition + allWaypoints[i].tanTwo, allWaypoints[i].endPosition);
            }
        }
        // if lock to track
        if (lockOnTrack) {
            if (trackedObject.x < allWaypoints[0].position.x) {
                reVector = allWaypoints[0].position;
                pointFound = true;
            } else if (trackedObject.x > allWaypoints[allWaypoints.Length - 1].position.x) {
                reVector = allWaypoints[allWaypoints.Length - 1].position;
                pointFound = true;
            }
        }
        if (!pointFound) {
            reVector = trackedObject;
        }
        return reVector;
    }

    public Vector3 getPositionOnTrack(Vector3 trackedObject) {
        if (mode == DollyMode.TrackTime) {
            return getTimeBasedPosition();
        } else {
            return getTrackedObjectBasedPosition(trackedObject);
        }
    }

    public void straightenWaypoint(Dc2dWaypoint first, Dc2dWaypoint second) {
        // PRO ONLY
    }

    public void smoothWaypoint(Dc2dWaypoint first, Dc2dWaypoint second) {
        // PRO ONLY
    }

    public void evenWaypointTimes() {
        // PRO ONLY
    }

    public float FastArcLength(Dc2dWaypoint wp) {
        float arcLength = 0.0f;
        Dc2dUtils.ArcLengthUtil(wp.position, wp.position+wp.tanOne, wp.endPosition+wp.tanTwo, wp.endPosition, 5, ref arcLength);
        return arcLength;
    }

    public void setTrackTime(float time) {
        onTrackTime = time;
    }
}
