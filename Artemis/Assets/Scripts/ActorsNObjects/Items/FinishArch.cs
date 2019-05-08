using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    Text RunTimeText;
    Text BestTimeText;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
		menuSystem = GameObject.FindGameObjectWithTag("MenuSystem").GetComponent<MenuSystem>();

        if (SceneManager.GetActiveScene().buildIndex != gm.levelPaddingBesidesMain) {
            RunTimeText = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "RunTimeText").GetComponent<Text>();
            BestTimeText = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "BestTimeText").GetComponent<Text>();

            RunTimeText.text = "";
            BestTimeText.text = "";
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player" && animationState == AnimationState.Idle) {
            animator.Play(WingsUp);
            animationState = AnimationState.WingsUp;
            menuSystem.SetTimer(false);

            if (SceneManager.GetActiveScene().buildIndex != gm.levelPaddingBesidesMain) {
                float RunTime = menuSystem.timer.GetComponent<Timer>().runningTime;
                RunTimeText.text = "Run Time: " + RunTime.ToString("0.#");
                float LevelBest = PlayerPrefs.GetFloat("BestTimeLevel" + SceneManager.GetActiveScene().buildIndex, 0f);

                // Not best time
                if (RunTime > LevelBest && LevelBest != 0f) {
                    BestTimeText.text = "Best Time: " + LevelBest.ToString("0.#");
                } else {
                    // Best time
                    PlayerPrefs.SetFloat("BestTimeLevel" + SceneManager.GetActiveScene().buildIndex, RunTime);
                    BestTimeText.text = "New Best Time!";

                    if (PlayerPrefs.GetFloat("HighestLevelCompleted", 0f) < SceneManager.GetActiveScene().buildIndex) {
                        PlayerPrefs.SetFloat("HighestLevelCompleted", SceneManager.GetActiveScene().buildIndex);
                    }
                }
            }
        }
    }

    public void PlayWingsUpIdle () {
        animator.Play(WingsUpIdle);
        animationState = AnimationState.WingsUpIdle;
    }
}
