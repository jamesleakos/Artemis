using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoom : MonoBehaviour {

    Camera cam;
    GameMaster gm;
    
    float targetCamSize;
    float oldCamSize;

    public float startingCamSize;
    public float zoomOutSize;

    public float easeAmount;
    public float zoomSpeed;

    float percentBetweenWaypoints;


    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        cam = gameObject.GetComponent<Camera>();
        startingCamSize = cam.orthographicSize;
        targetCamSize = startingCamSize;

        if (SceneManager.GetActiveScene().buildIndex == 0) {
            if (gm.loadSplashScreen) {
                cam.orthographicSize = zoomOutSize;
                targetCamSize = zoomOutSize;
                gm.loadSplashScreen = false;
            }
        }
    }

    void Update() {
        if (targetCamSize != cam.orthographicSize) {
            CalculateZoomMovement();
        } else {
            oldCamSize = cam.orthographicSize;
            percentBetweenWaypoints = 0;
        }
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void CalculateZoomMovement() {

        float distanceBetweenWaypoints = Mathf.Abs(targetCamSize - oldCamSize);
        percentBetweenWaypoints += Time.deltaTime * zoomSpeed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        float newPos = Mathf.Lerp(oldCamSize, targetCamSize, easedPercentBetweenWaypoints);

        cam.orthographicSize = newPos;
    }

    public void ZoomCamera (float newSize) {
        targetCamSize = newSize;
    }

    public void ZoomCameraOut () {
        ZoomCamera(zoomOutSize);
    }

    public void ResetZoom (string calledFrom) {
        ZoomCamera(startingCamSize);
    }
}
