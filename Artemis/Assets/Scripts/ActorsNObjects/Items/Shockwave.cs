using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour {

	AudioManager audioManager;


	void Start() {
		audioManager = GameObject.FindGameObjectWithTag ("AudioManager").GetComponent<AudioManager>();
		audioManager.PlaySound ("Shockwave");
	}

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            Player player = collider.gameObject.GetComponent<Player>();
            player.HitByEnemy();
        }
    }

    public void DestroyThis() {
        Destroy(gameObject);
    }


}
