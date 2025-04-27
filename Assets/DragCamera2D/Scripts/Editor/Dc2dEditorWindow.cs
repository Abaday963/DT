using UnityEditor;
using UnityEngine;

public class Dc2dEditorWindow : EditorWindow
{
    [Tooltip("Waypoint Color (Circles)")]
    public Color waypointColor = Color.white;
    [Tooltip("Curve Handle Color (Small Squares)")]
    public Color tangentHandleColor = Color.white;
    [Tooltip("Curve Connection Color")]
    public Color tangentGuideColor = Color.gray;
    [Tooltip("Waypoint Link Color")]
    public Color gizmoColor = Color.gray;
    [Tooltip("Time Handle Color")]
    public Color timeScaleHandleColor = Color.red;
    [Tooltip("Camera Track Color")]
    public Color dollyTrackColor = Color.red;
    [Tooltip("Scene Menu Background Color")]
    public Color sceneMenuBGCol = new Color(0F,0F,0F,0.8f);

    //string myString = "Hello World";
    //bool groupEnabled;
    //bool myBool = true;
    //float myFloat = 1.23f;

    private void Awake() {
        waypointColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("waypointColor", "#FFFFFFFF"));
        tangentHandleColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentHandleColor", "#FFFFFFFF"));
        tangentGuideColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("tangentGuideColor", "#333333FF"));
        gizmoColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("gizmoColor", "#333333FF"));
        timeScaleHandleColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("timeScaleHandleColor", "#FF0000FF"));
        dollyTrackColor = Dc2dUtils.stringToColor(EditorPrefs.GetString("dollyTrackColor", "#FF0000FF"));
        sceneMenuBGCol = Dc2dUtils.stringToColor(EditorPrefs.GetString("sceneMenuBGCol", "#000000FF"));


    }

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Drag Camera Settings")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        Dc2dEditorWindow window = (Dc2dEditorWindow)EditorWindow.GetWindow(typeof(Dc2dEditorWindow));
        window.Show();
    }

    void OnGUI() {
        EditorGUIUtility.labelWidth = 225;
        GUILayout.Label("Drag Camer Settings", EditorStyles.boldLabel);

        bool dirty = false;
        Color tempColor;

        tempColor = EditorGUILayout.ColorField("Waypoint Color (Circles)", waypointColor);
        if (tempColor != waypointColor) {
            waypointColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Curve Handle Color (Small Squares)", tangentHandleColor);
        if (tempColor != tangentHandleColor) {
            tangentHandleColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Curve Connection Color", tangentGuideColor);
        if (tempColor != tangentGuideColor) {
            tangentGuideColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Waypoint Link Color", gizmoColor);
        if (tempColor != gizmoColor) {
            gizmoColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Time Handle Color", timeScaleHandleColor);
        if (tempColor != timeScaleHandleColor) {
            timeScaleHandleColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Camera Track Color", dollyTrackColor);
        if (tempColor != dollyTrackColor) {
            dollyTrackColor = tempColor;
            dirty = true;
        }

        tempColor = EditorGUILayout.ColorField("Scene Menu Background Color", sceneMenuBGCol);
        if (tempColor != sceneMenuBGCol) {
            sceneMenuBGCol = tempColor;
            dirty = true;
        }

        if (dirty) {
            EditorPrefs.SetString("waypointColor", Dc2dUtils.colorToString(waypointColor));
            EditorPrefs.SetString("tangentHandleColor", Dc2dUtils.colorToString(tangentHandleColor));
            EditorPrefs.SetString("tangentGuideColor", Dc2dUtils.colorToString(tangentGuideColor));
            EditorPrefs.SetString("gizmoColor", Dc2dUtils.colorToString(gizmoColor));
            EditorPrefs.SetString("timeScaleHandleColor", Dc2dUtils.colorToString(timeScaleHandleColor));
            EditorPrefs.SetString("dollyTrackColor", Dc2dUtils.colorToString(dollyTrackColor));
            EditorPrefs.SetString("sceneMenuBGCol", Dc2dUtils.colorToString(sceneMenuBGCol));
        }

        
        //myString = EditorGUILayout.TextField("Text Field", myString);

        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }

    

    private void showButtins() {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(20, 20, 150, 60));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        GUI.Box(rect, GUIContent.none);

        GUI.contentColor = Color.white; // does not effect text 
        GUI.backgroundColor = Color.white; // is background colour
        GUI.color = Color.white; // overall colour (not text in label)

        // set text colour for label
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Rotate", guiStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Left")) {
            Debug.Log("RL");
        }

        if (GUILayout.Button("Right")) {
            Debug.Log("RR");
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }
}
