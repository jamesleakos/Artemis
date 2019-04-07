using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartArch : MonoBehaviour {

    #region Animation
    Animator animator;
    const string WingsGoDown = "WingsGoDown";
    const string WingsUpIdle = "WingsUpIdle";
    const string WingsDownIdle = "WingsDownIdle";
    public enum AnimationState { WingsUpIdle, WingsGoDown, WingsDownIdle }
    public AnimationState animationState;
    #endregion

    GameMaster gm;
    MenuSystem menuSystem;
    Text StartText;
    

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        StartText = gameObject.GetComponentInChildren<Text>();
        StartText.text = gm.ReturnSceneNames(SceneManager.GetActiveScene().buildIndex);
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
