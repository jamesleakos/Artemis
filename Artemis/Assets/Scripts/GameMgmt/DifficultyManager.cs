using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour {

    GameMaster gm;
    public bool onLittleGirl = true;
    public bool onGoddess = true;

    // Start and Update
    #region Start and Update, On Enable/Disable

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        SetDifficulty();        
    }

    private void OnEnable() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        MenuSystem.onDifficultyChange += SetDifficulty;
    }

    private void OnDisable() {
        MenuSystem.onDifficultyChange -= SetDifficulty;
    }
    #endregion


    public void SetDifficulty() {
        StartArch startArch = GameObject.FindObjectOfType<StartArch>();
        if (!(startArch != null)) {
            SetDifficultySorter();
        } else {
            if (startArch.animationState == StartArch.AnimationState.WingsUpIdle) {
                SetDifficultySorter();
            }
        }
    }

    public void SetDifficultySorter() {

        if (gm.gameDifficulty == GameMaster.GameDifficulty.littlegirl) {
            if (onLittleGirl) {
                EnableChildrenAndComponents(true);
            } else {
                EnableChildrenAndComponents(false);
            }
        } else if (gm.gameDifficulty == GameMaster.GameDifficulty.goddess) {
            if (onGoddess) {
                EnableChildrenAndComponents(true);
            } else {
                EnableChildrenAndComponents(false);
            }
        }
    }

    public void EnableChildrenAndComponents(bool setTo) {
        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in comps) {
            if (!(component is DifficultyManager)) {
                component.enabled = setTo;
            }
        }
        foreach (Collider2D c in GetComponents<Collider2D>()) {
            c.enabled = setTo;
        }
        foreach (AudioSource a in GetComponents<AudioSource>()) {
            a.enabled = setTo;
        }
        foreach (Transform child in transform) {
            child.gameObject.SetActive(setTo);
        }
    }
}