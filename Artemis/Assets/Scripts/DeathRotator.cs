using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRotator : MonoBehaviour {
    GameMaster gm;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        gameObject.transform.localPosition = new Vector3(gameObject.transform.lossyScale.x/2, 0, 0);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        gm.DestroyCharacter(collision.gameObject);
    }
}
