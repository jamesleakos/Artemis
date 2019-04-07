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
    GameObject ArrowGraphic;
    Player player;

    MenuSystem menuSystem;
    AudioManager audioManager;
    public GameMaster gm;

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
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
        if (gameObject.activeSelf == true) {
            animator.Play(mouseOut);
        }
        animationState = AnimationState.mouseOut;
    }

    void PlayMouseIn() {
        if (gameObject.activeSelf == true) {
            animator.Play(mouseIn);
        }
        animationState = AnimationState.mouseIn;
    }

    public void MenuDisable() {
        if (gameObject.activeSelf == true) {
            PlayMouseOut();
            myText.fontSize = minFont;
            if (player != null) {
                player.inputOnButtonPress = true;
            }
        }
    }

    public void MenuEnable() {
        if (gameObject.activeSelf == true) {
            PlayMouseIn();
            audioManager.PlaySound("MenuHover");
            myText.fontSize = maxFont;
            if (player != null) {
                player.inputOnButtonPress = false;
            }
        }
    }

    public void MenuClick() {
        if (gameObject.activeSelf == true) {
            MenuDisable();
            audioManager.PlaySound("MenuClick");
        }
    }

    public void FinishArchNextLevelClick() {
        gm.NextLevel();
    }


    public void FinishArchReplayLevelClick() {
        gm.ReplayLevel();
    }

    public void TurnOffArrow() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).name == "ArrowMask") {
                transform.GetChild(i).GetComponentInChildren<Image>().color = new Color32(255,255,255,0);
            }
        }
    }
    public void TurnOnArrow() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).name == "ArrowMask") {
                transform.GetChild(i).GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 255);
            }
        }
    }

}
