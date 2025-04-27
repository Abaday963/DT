using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneExampleController : MonoBehaviour
{
    public DragCamera2D cam;
    public Dc2dDolly dolly;
    public Text dragSpeed;
    public Text edgeSpeed;
    public Text edgeSize;
    public Text trackMode;
    public GameObject cameraIcon;


    public GameObject panel;
    private bool isDisplayed = true;

    public void setBoundaryMode(bool onOff) {
        cam.clampCamera = onOff;
    }

    public void setDragMode(bool onOff) {
        cam.dragEnabled = onOff;
    }

    public void setDragSpeed(float speed) {
        dragSpeed.text = speed.ToString("F2");
        cam.dragSpeed = speed;
    }

    public void setDCZoom(bool onOff) {
        cam.DCZoom = onOff;
    }

    public void setDCMove(bool onOff) {
        cam.DCTranslate = onOff;
    }

    public void setEdgeScrollSize(float size) {
        cam.edgeBoundary = (int) size;
        edgeSize.text = cam.edgeBoundary.ToString();
        
    }

    public void setEdgeScrollSpeed(float speed) {
        cam.edgeSpeed = speed;
        edgeSpeed.text = speed.ToString("F2");

    }

    public void toggleControls() {
        isDisplayed = !isDisplayed;
        panel.SetActive(isDisplayed);
    }

    public void trackToggle() {
        if(dolly.mode == Dc2dDolly.DollyMode.TrackObject) {
            dolly.mode = Dc2dDolly.DollyMode.TrackTime;
            dolly.setTrackTime(0f);
            trackMode.text = "Track Time";
        } else {
            dolly.mode = Dc2dDolly.DollyMode.TrackObject;
            trackMode.text = "Track Object";
        }
    }

    public void toggleTrackLock() {
        dolly.lockOnTrack = !dolly.lockOnTrack;
    }

    public void renderCam() {
        cameraIcon.SetActive(!cameraIcon.activeSelf);
    }

    public void renderTrack() {
        dolly.renderDollyTrack = !dolly.renderDollyTrack;
    }
}
