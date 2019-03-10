﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartArch : MonoBehaviour {

    #region Animation
    Animator animator;
    const string WingsGoDown = "WingsGoDown";
    const string WingsUpIdle = "WingsUpIdle";
    const string WingsDownIdle = "WingsDownIdle";
    enum AnimationState { WingsUpIdle, WingsGoDown, WingsDownIdle }
    AnimationState animationState;
    #endregion

    GameMaster gm;
    MenuSystem menuSystem;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.tag == "Player" && animationState == AnimationState.WingsUpIdle) {
            animator.Play(WingsGoDown);
            animationState = AnimationState.WingsGoDown;
            menuSystem.SetTimer(true);
        }
    }

    public void PlayWingsDownIdle() {
        animator.Play(WingsDownIdle);
        animationState = AnimationState.WingsDownIdle;
    }
}
