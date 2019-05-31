using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeerWinCanvas: MonoBehaviour {

    #region Animation
    Animator animator;
    const string WingsUp = "WingsUp";
    const string WingsUpIdle = "WingsUpIdle";
    enum AnimationState { Idle, WingsUp, WingsUpIdle }
    AnimationState animationState;
    #endregion

    GameMaster gm;
    MenuSystem menuSystem;

    Text RunTimeText;
    Text BestTimeText;

    Vector3 startingTransform;
    Vector3 switchTransform;
    Deer deer;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();
        deer = GetComponentInParent<Deer>();
        startingTransform = transform.localScale;
        switchTransform = new Vector3(startingTransform.x * -1, startingTransform.y, startingTransform.z);

        if (SceneManager.GetActiveScene().buildIndex != gm.levelPaddingBesidesMain) {
            RunTimeText = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "RunTimeText").GetComponent<Text>();
            BestTimeText = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "BestTimeText").GetComponent<Text>();

            RunTimeText.text = "";
            BestTimeText.text = "";
        }
    }

    void Update() {
        if (deer.faceDirX == 1) {
            transform.localScale = startingTransform;
        } else {
            transform.localScale = switchTransform;
        }
    }

    public void SwitchToEnded() {
        if (animationState == AnimationState.Idle) {
            animator.Play(WingsUp);
            animationState = AnimationState.WingsUp;
            menuSystem.SetTimer(false);

            PlayerPrefs.SetFloat("LastLevelCompleted", SceneManager.GetActiveScene().buildIndex);

            if (SceneManager.GetActiveScene().buildIndex != gm.levelPaddingBesidesMain) {
                float RunTime = menuSystem.timer.GetComponent<Timer>().runningTime;
                RunTimeText.text = "Run Time: " + RunTime.ToString("0.#");
                float LevelBest = PlayerPrefs.GetFloat("BestTimeLevel" + PlayerPrefs.GetString("GameDifficulty") + SceneManager.GetActiveScene().buildIndex, 0f);

                // Not best time
                if (RunTime > LevelBest && LevelBest != 0f) {
                    BestTimeText.text = "Best Time: " + LevelBest.ToString("0.#");
                } else {
                    // Best time
                    PlayerPrefs.SetFloat("BestTimeLevel" + PlayerPrefs.GetString("GameDifficulty") + SceneManager.GetActiveScene().buildIndex, RunTime);
                    BestTimeText.text = "New Best Time!";
                }
            }
        }
    }

    public void PlayWingsUpIdle() {
        animator.Play(WingsUpIdle);
        animationState = AnimationState.WingsUpIdle;
    }
}
