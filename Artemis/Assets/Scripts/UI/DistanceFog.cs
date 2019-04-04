using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DistanceFog : MonoBehaviour {

    private CameraZoom camZoom;
    private Camera cam;
    Image fogImage;
    public float maxFogAlpha = 255;
    public float maxFogAlphaMainMenu = 150;

    // Use this for initialization
    void Start() {
        cam = Camera.main;
        camZoom = cam.GetComponent<CameraZoom>();
        fogImage = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () {
        float percentCamZoom = (cam.orthographicSize - camZoom.currentSetCamSize) / (camZoom.zoomOutSize - camZoom.currentSetCamSize);
        var tempColor = fogImage.color;
        tempColor.a = percentCamZoom * maxFogAlpha/255;
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            tempColor.a = tempColor.a * maxFogAlphaMainMenu / 255;
        }
        fogImage.color = tempColor;
	}
}
