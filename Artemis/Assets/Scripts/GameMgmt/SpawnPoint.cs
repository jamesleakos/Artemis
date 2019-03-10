using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
	GameMaster gm;
	bool spawnTriggered;

	void Start () {
		gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
	}

	void OnTriggerEnter2D () {
		if (spawnTriggered == false) {
			bool spawnTriggered = true;
			gm.UpdateSpawn (gameObject.transform.position);
		}
	}
}
