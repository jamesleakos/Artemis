﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MaskController : MonoBehaviour {

	private MenuSystem menuSystem;
    Animator anim;

    const string Darken = "Darken";
    const string Lighten = "Lighten";
    const string onIdle = "onIdle";


    void Start() {
        anim = gameObject.GetComponent<Animator>();
		menuSystem = transform.root.GetComponent<MenuSystem>();
	}

    public void DarkenMask() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(Darken);
    }

    public void LightenMask() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(Lighten);
    }

    public void TurnOnIdle() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(onIdle);
    }

    public void loadLevel() {
		menuSystem.LoadOrRespawn();
	}
}
