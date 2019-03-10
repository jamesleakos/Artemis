using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour {

    GameMaster gm;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player") {
			Player player = collider.gameObject.GetComponent<Player>();
			player.HitByEnemy();
		} else {
			gm.DestroyCharacter(collider.gameObject);
		}
    }
}
