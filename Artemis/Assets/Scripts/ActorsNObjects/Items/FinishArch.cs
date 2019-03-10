using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishArch : MonoBehaviour {

    #region Animation
    Animator animator;
    const string WingsUp = "WingsUp";
    const string WingsUpIdle = "WingsUpIdle";
    enum AnimationState { Idle, WingsUp, WingsUpIdle }
    AnimationState animationState;
    #endregion

    GameMaster gm;
	MenuSystem menuSystem;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
		menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player" && animationState == AnimationState.Idle) {
            animator.Play(WingsUp);
            animationState = AnimationState.WingsUp;
        }
    }

    public void PlayWingsUpIdle () {
        animator.Play(WingsUpIdle);
        animationState = AnimationState.WingsUpIdle;
    }
}
