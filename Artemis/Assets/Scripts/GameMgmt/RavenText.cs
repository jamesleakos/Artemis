using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RavenText : MonoBehaviour
{
    #region Animation
    Animator animator;
    const string inactiveIdle = "inactiveIdle";
    const string activeIdle = "activeIdle";
    const string deactivating = "deactivating";
    const string activating = "activating";

    public enum AnimationState { inactiveIdle, activeIdle, deactivating, activating }
    public AnimationState animationState;
    #endregion

    GameMaster gm;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
    }



    public void PlayActivating() {
        animator.Play(activating);
        animationState = AnimationState.activating;
    }
    public void PlayActiveIdle() {
        animator.Play(activeIdle);
        animationState = AnimationState.activeIdle;
    }
    public void PlayDeactivating() {
        animator.Play(deactivating);
        animationState = AnimationState.deactivating;
    }
    public void PlayInactiveIdle() {
        animator.Play(inactiveIdle);
        animationState = AnimationState.inactiveIdle;
    }
}
