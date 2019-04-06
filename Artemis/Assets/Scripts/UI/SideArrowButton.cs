using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SideArrowButton : MonoBehaviour {

    #region Animation
    //Animator animator;
    //const string mouseIn = "mouseIn";
    //const string mouseOut = "mouseOut";
    //enum AnimationState { mouseIn, mouseOut }
    //AnimationState animationState;
    #endregion

    Vector3 startingScale;

    float nextTimeToSearch;
    Player player;

    MenuSystem menuSystem;
    AudioManager audioManager;

    void Awake() {
        startingScale = gameObject.transform.localScale;
    }

    void Start() {
        // currently am not using any animation, but there is a side arrow button animator;
        //animator = gameObject.GetComponent<Animator>();
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        if (player == null) {
            FindPlayer();
        }
    }

    void OnEnable() {
        menuDisable();
    }

    void OnDisable() {
        menuDisable();
    }

    void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                player = searchResult.GetComponent<Player>();
            }
            nextTimeToSearch = Time.time + 0.5f;
        }
    }

    void PlayMouseOut() {
        //animator.Play(mouseOut);
        //animationState = AnimationState.mouseOut;
    }

    void PlayMouseIn() {
        //animator.Play(mouseIn);
        //animationState = AnimationState.mouseIn;
    }

    public void menuDisable() {
        if (gameObject.activeSelf == true) {

            gameObject.transform.localScale = startingScale;

            PlayMouseOut();

            if (player != null) {
                player.inputOnButtonPress = true;
            }
        }
    }

    public void menuEnable() {
        if (gameObject.activeSelf == true) {

            gameObject.transform.localScale = startingScale * 1.2f;

            PlayMouseIn();

            //audioManager.PlaySound("MenuClick");

            if (player != null) {
                player.inputOnButtonPress = false;
            }
        }
    }
}
