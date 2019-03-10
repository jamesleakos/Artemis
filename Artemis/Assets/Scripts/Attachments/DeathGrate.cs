using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathGrate : MonoBehaviour {

    #region Animation
    Animator animator;
    const string idle = "idle";
    const string pulse = "pulse";
    // enum AnimationState { idle, pulse }
    // AnimationState animationState;
    #endregion

    GameMaster gm;
    AudioManager audioManager;

    void Start() {
        animator = gameObject.GetComponent<Animator>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            Player player = collider.gameObject.GetComponent<Player>();
            if (player.artemisState == Player.ArtemisState.alive) {
                player.HitByEnemy();

                animator.Play(pulse);
                // animationState = AnimationState.pulse;
                audioManager.PlaySound("Slice");
            }

        } else {
            gm.DestroyCharacter(collider.gameObject);
        }
    }

    public void PlayIdle() {
        animator.Play(idle);
        // animationState = AnimationState.idle;
    }
}
