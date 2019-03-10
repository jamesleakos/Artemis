using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MaskController : MonoBehaviour {

	private MenuSystem menuSystem;

	void Start() {
		menuSystem = transform.root.GetComponent<MenuSystem>();
	}

	public void loadLevel() {
		menuSystem.LoadOrRespawn ();
	}
}
