using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceFog : MonoBehaviour {

    private CameraZoom camZoom;
    private Camera cam;
    Image fogImage;
    public float maxFogAlpha = 255;

    // Use this for initialization
    void Start() {
        cam = Camera.main;
        camZoom = cam.GetComponent<CameraZoom>();
        fogImage = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () {
        float percentCamZoom = (cam.orthographicSize - camZoom.startingCamSize) / (camZoom.zoomOutSize - camZoom.startingCamSize);
        var tempColor = fogImage.color;
        tempColor.a = percentCamZoom * maxFogAlpha/255;
        fogImage.color = tempColor;
	}
}
