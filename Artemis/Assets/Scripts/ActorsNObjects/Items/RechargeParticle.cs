using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RechargeParticle : MonoBehaviour {

	float activationTime;
    public float refreshTime;
    bool refreshing;

	#region Animation
	Animator animator;
	const string inactiveIdle = "inactiveIdle";
	const string activeIdle = "activeIdle";
	const string activating = "activating";
	const string deactivating = "deactivating";

	public enum AnimationState { inactiveIdle, activeIdle, activating, deactivating }
	public AnimationState animationState;
	#endregion

	void Start() {
		animator = gameObject.GetComponent<Animator>();
		animationState = AnimationState.activeIdle;
	}

	void Update () {
		if (animationState == AnimationState.inactiveIdle && Time.time > activationTime) {
			PlayActivating ();
		}
	}

    void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag == "Player" && (animationState == AnimationState.activeIdle || animationState == AnimationState.activating)) {
            Player player = collider.gameObject.GetComponent<Player>();
            player.jumpsRemaining = player.maxJumps;
            player.attacksRemaining = player.maxAttacks;
            player.airPauseRemaining = player.maxAirPause;

			PlayDeactivating ();
        }
    }

	public void PlayActiveIdle () {
		animator.Play(activeIdle);
		animationState = AnimationState.activeIdle;
	}
	public void PlayInactiveIdle () {
		animator.Play(inactiveIdle);
		animationState = AnimationState.inactiveIdle;
		activationTime = Time.time + refreshTime;
	}
	public void PlayActivating () {
		animator.Play(activating);
		animationState = AnimationState.activating;
	}
	public void PlayDeactivating () {
		animator.Play(deactivating);
		animationState = AnimationState.deactivating;
	}

}
