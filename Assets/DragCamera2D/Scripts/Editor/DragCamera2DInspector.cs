using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragCamera2D))]
public class DragCamera2DInspector : Editor {

    DragCamera2D dc2d;
    protected static bool edgeScrollingEnabled = true;
    protected static bool cameraBoundsEnabled = true;
    protected static bool followObjectEnabled = true;
    protected static bool zoomGUIEnabled = true;
    protected static bool touchKeyboardEnabled = true;
    protected static bool doubleClickEnabled = true;
    private Texture dollyAddTex;

    private void OnEnable() {
        dc2d = (DragCamera2D)target;
        dollyAddTex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/DragCamera2D/Images/plus-sm.png", typeof(Texture));
    }

    public override void OnInspectorGUI() {

        DragCamera2D aTarget = dc2d as DragCamera2D; // define target for vars to be set to

        dc2d.cam = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera to control withj DragCamera2D script"), dc2d.cam, typeof(Camera), true);
        dc2d.dragEnabled = EditorGUILayout.Toggle(new GUIContent("Drag Enabled", "Enable/Disable camera movement"), dc2d.dragEnabled);
        dc2d.mouseButton = (DragCamera2D.MouseButton)EditorGUILayout.EnumPopup(new GUIContent("Mouse Button", "Mouse button responsible for drag."), dc2d.mouseButton);
        dc2d.dragSpeed = EditorGUILayout.Slider(new GUIContent("Drag Speed", "Speed the camera moves when dragged."), dc2d.dragSpeed, -5f, 5f);

        GUILayout.Space(20f);

        // Double Click control
        doubleClickEnabled = EditorGUILayout.Foldout(doubleClickEnabled, new GUIContent("Double Click (DC)", "Double Click Actions and settings"), EditorStyles.foldoutHeader);
        if (doubleClickEnabled) {
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            dc2d.ztTarget = (Dc2dZoomTarget) EditorGUILayout.ObjectField(new GUIContent("Zoom Target", "Object used as target for DC actions"), dc2d.ztTarget, typeof(Object), true);
            if (GUILayout.Button(dollyAddTex, GUILayout.Width(25))) {
                dc2d.addZoomTarget();
            }
            GUILayout.EndHorizontal();
            dc2d.DCTranslate = EditorGUILayout.Toggle(new GUIContent("DC Translate", "Move to DC location"), dc2d.DCTranslate);
            dc2d.DCZoom = EditorGUILayout.Toggle(new GUIContent("DC Zoom", "Zoom DC location"), dc2d.DCZoom);
            dc2d.DCZoomTargetIn = EditorGUILayout.Slider(new GUIContent("DC Zoom Target In", "Target Zoom Level for DC"), dc2d.DCZoomTargetIn, 0.1f, 10f);
            dc2d.DCZoomTranslateSpeed = EditorGUILayout.Slider(new GUIContent("DC Zoom Translate Speed", "DC Translation Speed"), dc2d.DCZoomTranslateSpeed, 0.01f, 1f);
            dc2d.DCZoomTargetOut = EditorGUILayout.Slider(new GUIContent("DC Zoom Target Out", "DCZoomSpeed"), dc2d.DCZoomTargetOut, 0.1f, 10f);
            dc2d.DCZoomSpeed = EditorGUILayout.Slider(new GUIContent("DC Zoom Speed", "DC Zoom Speed"), dc2d.DCZoomSpeed, 0.01f, 1f);
            EditorGUI.indentLevel--;
        }
        GUILayout.Space(20f);

        // Touch & Keyboard control
        touchKeyboardEnabled = EditorGUILayout.Foldout(touchKeyboardEnabled, new GUIContent("Touch & Keyboard", "Touch & Keyboard settings"), EditorStyles.foldoutHeader);
        if (touchKeyboardEnabled) {
            EditorGUI.indentLevel++;
            dc2d.keyboardInput = EditorGUILayout.Toggle(new GUIContent("Keyboard Input", "Enable or disable Keyboard input"), dc2d.keyboardInput);
            dc2d.inverseKeyboard = EditorGUILayout.Toggle(new GUIContent("Invert Keyboard Direction", "Invert keyboard direction"), dc2d.inverseKeyboard);

            GUI.enabled = false;
            dc2d.touchEnabled = EditorGUILayout.Toggle(new GUIContent("Touch Enabled", "Enable or disable touch input (PRO)"), dc2d.touchEnabled);
            dc2d.touchDragSpeed = EditorGUILayout.Slider(new GUIContent("Touch Drag Speed", "Drag Speed for touch controls (PRO)"), dc2d.touchDragSpeed, -5, 5);
            GUI.enabled = true;

            EditorGUI.indentLevel--;
        }
        GUILayout.Space(20f);


        // Zoom control
        zoomGUIEnabled = EditorGUILayout.Foldout(zoomGUIEnabled, new GUIContent("Zoom", "Zoom Settings"), EditorStyles.foldoutHeader);
        if (zoomGUIEnabled) {
            EditorGUI.indentLevel++;
            dc2d.zoomEnabled = EditorGUILayout.Toggle(new GUIContent("Zoom Enabled", "Enable or disable zooming"), dc2d.zoomEnabled);
            dc2d.linkedZoomDrag = EditorGUILayout.Toggle(new GUIContent("Linked Zoom Drag", "Scale drag movement with zoom level"), dc2d.linkedZoomDrag);
            dc2d.maxZoom = EditorGUILayout.FloatField(new GUIContent("Max Zoom", "Maximum Zoom Level"), dc2d.maxZoom);
            dc2d.minZoom = EditorGUILayout.Slider(new GUIContent("Min Zoom", "Minimum Zoom Level"), dc2d.minZoom, 0, 10);
            dc2d.zoomStepSize = EditorGUILayout.Slider(new GUIContent("Zoom Step Size", "The Speed the zoom changes"), dc2d.zoomStepSize, 0, 10);
            dc2d.zoomToMouse = EditorGUILayout.Toggle(new GUIContent("Zoom To Mouse", "Enable Zooming to mouse pointer"), dc2d.zoomToMouse);
            EditorGUI.indentLevel--;
        }
        GUILayout.Space(20f);

        // Edge Scrolling
        edgeScrollingEnabled = EditorGUILayout.Foldout(edgeScrollingEnabled, "Edge Scrolling", EditorStyles.foldoutHeader);
        if (edgeScrollingEnabled) {
            EditorGUI.indentLevel++;
            dc2d.edgeBoundary = EditorGUILayout.IntField(new GUIContent("Edge Boundary", "Size of outer zone to trigger edge scrolling"), dc2d.edgeBoundary);
            dc2d.edgeSpeed = EditorGUILayout.Slider(new GUIContent("Edge Scroll Speed", "Speed the camera moves when mouse enters screen edge."), dc2d.edgeSpeed, 0, 10);
            EditorGUI.indentLevel--;
        }
        GUILayout.Space(20f);
        
        // Follow Object
        followObjectEnabled = EditorGUILayout.Foldout(followObjectEnabled, "Follow Object", EditorStyles.foldoutHeader);
        if (followObjectEnabled) {
            EditorGUI.indentLevel++;
            dc2d.followTarget = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Follow Target", "Target Object to follow (transform.position)"), dc2d.followTarget, typeof(GameObject), true);
            dc2d.lerpSpeed = EditorGUILayout.Slider(new GUIContent("Lerp Speed", "How slow/fast the camera moves to target"), dc2d.lerpSpeed, 0.01f, 1f);
            EditorGUI.indentLevel--;
        }
        GUILayout.Space(20f);
        
        // Camera Bounds
        cameraBoundsEnabled = EditorGUILayout.Foldout(cameraBoundsEnabled, "Camera Bounds | Dolly Rail", EditorStyles.foldoutHeader);
        if (cameraBoundsEnabled) {
            EditorGUI.indentLevel++;
            dc2d.clampCamera = EditorGUILayout.Toggle(new GUIContent("Clamp Camera", "Limit camera motion to area defined in Camera Bounds"), dc2d.clampCamera);
            GUILayout.BeginHorizontal(); 
                dc2d.bounds = (CameraBounds)EditorGUILayout.ObjectField(new GUIContent("Bounds", "Camera Bounds Object that defines viewable area"), dc2d.bounds, typeof(Object), true);
                if (GUILayout.Button(dollyAddTex, GUILayout.Width(25))) {
                    dc2d.addCameraBounds();
                }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                dc2d.dollyRail = (Dc2dDolly)EditorGUILayout.ObjectField(new GUIContent("Dolly Rail", "Camera Dolly Rail object:: set path for camera to travel"), dc2d.dollyRail, typeof(Object), true);

                if (GUILayout.Button(dollyAddTex, GUILayout.Width(25))) {
                    dc2d.addCameraDolly();
                }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;


        }

        if(GUILayout.Button(new GUIContent("Like this asset?", "Please leave a review, comment or suggestion to help make this asset the best on the store. (opens in a browser window)"))){
            Application.OpenURL("https://assetstore.unity.com/packages/tools/camera/drag-camera-2d-126070");
        }


        if (GUI.changed)
            EditorUtility.SetDirty(dc2d);

        //temp show all options
        //DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();

    }
}
