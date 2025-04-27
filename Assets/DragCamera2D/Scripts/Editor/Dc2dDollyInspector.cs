using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Dc2dDolly))]
public class Dc2dDollyInspector : Editor
{
    Dc2dDolly dolly;

    private const float wpSize = 0.1f;
    private const float buttonSize = 0.04f;
    private const float coneSize = 0.35f;
    private const float scaleHandleSize = 1f;


    private Color buttonColour = new Color(1f, 1f, 1f, 1f);
    private Color textColour = Color.white;

    private Texture2D preAddTex;
    private Texture2D postAddTex;
    private Texture2D smoothTex;
    private Texture2D straightTex;

    private GUIStyle boxStyle = null;

    private void Awake() {
        preAddTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/DragCamera2D/images/wpplus_icon.png", typeof(Texture2D));
        postAddTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/DragCamera2D/images/wppluss_icon.png", typeof(Texture2D));
        smoothTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/DragCamera2D/images/wpcurve_icon.png", typeof(Texture2D));
        straightTex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/DragCamera2D/images/wpstraight_icon.png", typeof(Texture2D));
  
    }

    private void OnEnable() {
        dolly = (Dc2dDolly)target;

    }

    private Texture2D MakeTex(int width, int height, Color col) {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    protected virtual void OnSceneGUI() {

        if (dolly.inScreenControls) {
            drawMenu();
        }

        float zoom = HandleUtility.GetHandleSize(new Vector3(0,0,0)); // basically gets a scene view zoom level

        //create post waypoint button
        Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentGuideColor", "#FF0000FF"));

        Vector3 endwpPosition = dolly.allWaypoints[dolly.allWaypoints.Length - 1].position;
        Vector3 addwpPos = endwpPosition + new Vector3(1* zoom, 0, 0);
        
        Handles.DrawDottedLine(endwpPosition, addwpPos, 4.0f);
        if (Handles.Button(addwpPos, Quaternion.Euler(0, 90, 0), coneSize * zoom, coneSize + 0.1f * zoom, Handles.ConeHandleCap)) {
            dolly.addWaypointToEnd();
        }

        Vector3 prewpPosition = dolly.allWaypoints[0].position;
        Vector3 prewpPos = prewpPosition + new Vector3(-1 * zoom, 0, 0);

        Handles.DrawDottedLine(prewpPosition, prewpPos, 4.0f);
        if (Handles.Button(prewpPos, Quaternion.Euler(0, -90, 0), coneSize * zoom, coneSize + 0.1f * zoom, Handles.ConeHandleCap)) {
            dolly.addWaypointToStart();
        }

        for (int i = 0; i < dolly.allWaypoints.Length; i++) {
            Dc2dWaypoint wp = dolly.allWaypoints[i];

            // position handle
            Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("waypointColor", "#FF0000FF"));
            wp.position = Handles.Slider2D(wp.position, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.01f);


            if (i < dolly.allWaypoints.Length - 1) { // not last
                // calc legnth
                wp.arclength = dolly.FastArcLength(wp);

                // show handles to adjust time to next waypoint
                if (dolly.showTimeAdjustHandles && dolly.timeMode == Dc2dDolly.WaypointTimeMode.PerWaypointTime) {
                    Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("timeScaleHandleColor", "#FF0000FF"));
                    wp.timeToNextWaypoint = Handles.ScaleSlider(wp.timeToNextWaypoint, wp.position, Vector3.right, Quaternion.identity, scaleHandleSize * zoom, 0.01f);
                    if (wp.timeToNextWaypoint <= 0) {
                        wp.timeToNextWaypoint = 0.1f;
                    }
                }

                //show length to next 
                if (dolly.showLength) {
                Handles.Label(wp.position + (Vector3.up + Vector3.right * 2), "Length:" + dolly.FastArcLength(wp).ToString());
                }

                //show timeToNext
                if (dolly.showTimes) {
                    Handles.Label(wp.position + (Vector3.right * 2), "Next:" + wp.timeToNextWaypoint.ToString());
                }

                // tan1 handle
                Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentHandleColor", "#FF0000FF"));
                Vector3 tempt1 = Handles.Slider2D(wp.tanOne + wp.position, Vector3.forward, Vector3.right, Vector3.up, buttonSize * zoom, Handles.DotHandleCap, 0.01f) - wp.position;

                if (dolly.constantSpeed) {
                    tempt1 = tempt1.normalized;
                    tempt1 = ((wp.endPosition - wp.position).magnitude * 0.333f) * tempt1;
                    wp.tanOne = tempt1;
                } else {
                    wp.tanOne = tempt1;
                }

                if (wp.tanOne.x < 0) {
                    wp.tanOne.x = 0;
                }
                Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentGuideColor", "#FF0000FF"));
                Handles.DrawDottedLine(wp.position, wp.tanOne + wp.position, 4.0f);

                // tan2 handle
                Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentHandleColor", "#FF0000FF"));
                Vector3 tempt2 = Handles.Slider2D(wp.tanTwo + wp.endPosition, Vector3.forward, Vector3.right, Vector3.up, buttonSize * zoom, Handles.DotHandleCap, 0.01f) - wp.endPosition;

                if (dolly.constantSpeed) {
                    tempt2 = tempt2.normalized;
                    tempt2 = ((wp.position - wp.endPosition).magnitude * 0.333f) * tempt2;
                    wp.tanTwo = tempt2;
                } else {
                    wp.tanTwo = tempt2;
                }

                if (wp.tanTwo.x > 0) {
                    wp.tanTwo.x = 0;
                }
                Handles.color = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentGuideColor", "#FF0000FF"));
                Handles.DrawDottedLine(wp.endPosition, wp.tanTwo + wp.endPosition, 4.0f);

                // draw da curve
                Handles.DrawBezier(wp.position, wp.endPosition, wp.position + wp.tanOne, wp.endPosition + wp.tanTwo, Dc2dUtils.stringToColor(EditorPrefs.GetString("dollyTrackColor", "#FF0000FF")), null, 5f);

            }
            if (i > 0) { // not first
                // update position of previous
                dolly.allWaypoints[i - 1].endPosition = wp.position;

                if (wp.position.x < dolly.allWaypoints[i - 1].position.x) {
                    wp.position.x = dolly.allWaypoints[i - 1].position.x; // no going past previous point
                }
            }
        }
    }

    public override void OnInspectorGUI() {

        DrawDefaultInspector();
        if (GUILayout.Button("Add Waypoint")) {
            dolly.addWaypointToEnd();
        }

        GUI.enabled = false;

        if (GUILayout.Button(new GUIContent("Smooth All ", "PRO ONLY"))) {
            // PRO ONLY
        }
        if (GUILayout.Button(new GUIContent("Straighten All ", "PRO ONLY"))) {
            // PRO ONLY
        }
        if (GUILayout.Button(new GUIContent("Even Section Times", "PRO ONLY"))) {
            // PRO ONLY

        }
        GUI.enabled = true;

        if (GUILayout.Button("Dolly Settings")) {
            Dc2dEditorWindow window = (Dc2dEditorWindow)EditorWindow.GetWindow(typeof(Dc2dEditorWindow), false, "Settings for Dolly");
            window.Show();
        }
    }

    private void forceRepaint() {
        EditorWindow view = EditorWindow.GetWindow<SceneView>();
        view.Repaint();
    }

    private void drawMenu() {

        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(5, 5, 200, 150));

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, Dc2dUtils.stringToColor(EditorPrefs.GetString("sceneMenuBGCol", "#00000088"))); ;

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.Box(rect, GUIContent.none, boxStyle);

        // set text colour for label
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = textColour;
        guiStyle.fontStyle = FontStyle.Bold;
        GUI.color = textColour;


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Track Options - Pro Feature", guiStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.contentColor = textColour;
        GUI.backgroundColor = textColour;
        GUI.color = buttonColour;


        GUI.enabled = false;
        if (GUILayout.Button(preAddTex, GUILayout.Height(40))) {
            if (Event.current.shift) {
                dolly.removeWaypointFromStart();
            } else {
                dolly.addWaypointToStart();
            }
        }

        if (GUILayout.Button(smoothTex, GUILayout.Height(40))) {
            for (int i = 0; i < dolly.allWaypoints.Length - 1; i++) {
                dolly.smoothWaypoint(dolly.allWaypoints[i], dolly.allWaypoints[i + 1]);
            }
            forceRepaint();
        }

        if (GUILayout.Button(straightTex, GUILayout.Height(40))) {
            for (int i = 0; i < dolly.allWaypoints.Length - 1; i++) {
                dolly.straightenWaypoint(dolly.allWaypoints[i], dolly.allWaypoints[i + 1]);
            }
            forceRepaint();
        }

        if (GUILayout.Button(postAddTex, GUILayout.Height(40))) {
            if (Event.current.shift) {
                dolly.removeWaypointFromEnd();
            } else {
                dolly.addWaypointToEnd();
            }
        }

        GUI.enabled = true;


        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }
}
