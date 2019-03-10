using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogMovement : MonoBehaviour {

    public float speed;

    void LateUpdate() {
        transform.Translate(Vector3.right * Time.deltaTime * speed);
    }
}
