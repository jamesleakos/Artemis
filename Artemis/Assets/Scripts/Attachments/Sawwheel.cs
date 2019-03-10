using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sawwheel : MonoBehaviour {

    GameMaster gm;
    AudioManager audioManager;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            Player player = collider.gameObject.GetComponent<Player>();
            if (player.artemisState == Player.ArtemisState.alive) {
                player.HitByEnemy();

                audioManager.PlaySound("Slice");
            }
        } else {
            gm.DestroyCharacter(collider.gameObject);
        }
    }
}
