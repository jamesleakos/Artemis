using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroTextController : MonoBehaviour {

    private MenuSystem menuSystem;
    Animator anim;

    const string fadeIn = "fadeIn";
    const string fadeOut = "fadeOut";
    const string onIdle = "onIdle";


    void Start() {
        anim = gameObject.GetComponent<Animator>();
        menuSystem = transform.root.GetComponent<MenuSystem>();
    }

    public void FadeIn() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(fadeIn);
    }

    public void FadeOut() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(fadeOut);
    }

    public void PlayOnIdle() {
        anim = gameObject.GetComponent<Animator>();
        anim.Play(onIdle);
    }

    public void LoadLevel() {
        menuSystem.LoadLevelforIntroText();
    }
}
