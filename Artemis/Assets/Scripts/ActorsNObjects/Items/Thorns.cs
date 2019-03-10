using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorns : MonoBehaviour {

    GameMaster gm;
    AudioManager audioManager;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            audioManager.PlaySound("Slice");

            Player player = collider.gameObject.GetComponent<Player>();
            player.HitByEnemy();
        }
    }
}
