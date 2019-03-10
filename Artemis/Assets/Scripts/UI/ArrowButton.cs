using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour {

    #region Animation
    Animator animator;
    const string mouseIn = "mouseIn";
    const string mouseOut = "mouseOut";
    enum AnimationState { mouseIn, mouseOut }
    AnimationState animationState;
    #endregion

    #region Visuals
    public int minFont;
    public int maxFont;
    #endregion

    float nextTimeToSearch;
    Text myText;
    Player player;

    MenuSystem menuSystem;
    AudioManager audioManager;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        myText = gameObject.GetComponentInChildren<Text>();
        if (player == null) {
            FindPlayer();
        }
        //menuDisable();
    }

    //void OnEnable() {
        //print("got here");
        //menuDisable();
    //}

    //void OnDisable() {
        //menuDisable();
    //}

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
        if (animator.enabled) {
            animator.Play(mouseOut);
        }
        animationState = AnimationState.mouseOut;
    }

    void PlayMouseIn() {
        if (animator.enabled) {
            animator.Play(mouseIn);
        }
        animationState = AnimationState.mouseIn;
    }

    public void menuDisable() {
        if (gameObject.activeSelf == true) {
            PlayMouseOut();
            myText.fontSize = minFont;
            if (player != null) {
                player.inputOnButtonPress = true;
            }
        }
    }// END

    // Method by setting the active menuItem (mouseEnter)
    public void menuEnable() {
        if (gameObject.activeSelf == true) {
            PlayMouseIn();
            //audioManager.PlaySound("MenuClick");
            myText.fontSize = maxFont;
            if (player != null) {
                player.inputOnButtonPress = false;
            }
        }
    }

}
