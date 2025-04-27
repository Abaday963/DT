using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Dc2dSnapBox))]
public class Dc2dSnapBoxInspector : Editor
{
    Dc2dSnapBox sb;
    private Rect boundArea;

    private const float wpSize = 0.1f;

    private void OnEnable() {
        sb = (Dc2dSnapBox)target;
    }

    string text = "Help and tips \n" +
        "Set transform position to start of snap box zone (bottom left)\n" +
        "End point is the end of the snapbox zone (top right)\n\n" +
        "Priority: Higher value means higher priority for overlapping zones\n\n" +
        "Expand mode:\n" +
        "   EXPANDX: zoom out until viewport >= width of snapbox or max zoom reached\n" +
        "   EXPANDY: zoom out until viewport >= height of snapbox\n" +
        "   CONTRACTX: zoom in until viewport =< width \n" +
        "   CONTRACTY: zoom in until viewport =< height\n" +
        "   BOTHX: zoom in or out until width is almost snapbox width\n" +
        "   BOTHY: zoom in or out until width is almost snapbox height\n\n" +
        "Sb Name: name your snapbox \n(can query cameras current snapTarget to trigger events)";

    TextAsset txtAsset;
    Vector2 scroll;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        text = EditorGUILayout.TextArea(text, GUILayout.Height(300));
        EditorGUILayout.EndScrollView();

    }

    private void OnSceneGUI() {
        float zoom = HandleUtility.GetHandleSize(new Vector3(0, 0, 0)); // basically gets a scene view zoom level

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.black;

        boundArea.x = sb.transform.position.x;
        boundArea.y = sb.transform.position.y;
        boundArea.width = sb.endPoint.x - sb.transform.position.x;
        boundArea.height = sb.endPoint.y - sb.transform.position.y;

        Handles.Label(sb.transform.position + new Vector3(0, -0.2f, 0), sb.sbName+":Bottom Left", guiStyle);
        Handles.Label(sb.endPoint + new Vector3(0, -0.2f, 0), sb.sbName + ":Top Right", guiStyle);

        sb.endPoint = Handles.Slider2D(sb.endPoint, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.1f);
        sb.transform.position = Handles.Slider2D(sb.transform.position, Vector3.forward, Vector3.right, Vector3.up, wpSize * zoom, Handles.CircleHandleCap, 0.1f);

        if (sb.endPoint.x < sb.transform.position.x) {
            sb.endPoint.x = sb.transform.position.x;
        }

        if (sb.endPoint.y < sb.transform.position.y) {
            sb.endPoint.y = sb.transform.position.y;
        }

        Vector3 pos = sb.transform.position;

        Vector3[] verts = new Vector3[]
        {
            sb.transform.position,
            sb.transform.position + new Vector3(0, boundArea.height, 0),
            sb.transform.position + new Vector3(boundArea.width, boundArea.height, 0),
            sb.transform.position + new Vector3(boundArea.width,0 , 0)
        };

        Handles.DrawSolidRectangleWithOutline(verts, sb.guiColour, Color.white);
    }
}
