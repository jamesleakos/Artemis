using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveText : MonoBehaviour {

	#region Animation
	Animator animator;
	const string inactiveIdle = "inactiveIdle";
	const string alertIdle = "alertIdle";
	const string activeIdle = "activeIdle";
	const string deactivating = "deactivating";
	const string activating = "activating";
	const string alerting = "alerting";
	const string dealerting = "dealerting";
	public enum AnimationState { inactiveIdle, alertIdle, activeIdle, deactivating, activating, alerting, dealerting }
	public AnimationState animationState;
	#endregion

	GameMaster gm;

	void Start() {
		animator = gameObject.GetComponent<Animator>();
		gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
	}

	void Update () {
		if (animationState == AnimationState.alertIdle ||
			animationState == AnimationState.alerting) 
		{
			if (Input.GetKeyDown (KeyCode.W)) {
				animator.Play(activating);
				animationState = AnimationState.activating;
			}
		}
		if (animationState == AnimationState.activeIdle) {
			if (Input.GetKeyDown (KeyCode.W)) {
				animator.Play(deactivating);
				animationState = AnimationState.deactivating;
			}
		}
	}

	void OnTriggerStay2D(Collider2D collider) {
		if (collider.tag == "Player" && 
			animationState == AnimationState.inactiveIdle)
		{
			animator.Play(alerting);
			animationState = AnimationState.alerting;
		}
	}

	void OnTriggerExit2D(Collider2D collider) {
		if (collider.tag == "Player" && 
			(animationState == AnimationState.activeIdle ||
			animationState == AnimationState.activating)) {
			animator.Play(deactivating);
			animationState = AnimationState.deactivating;
		} else if (collider.tag == "Player" && 
			(animationState == AnimationState.alertIdle ||
			animationState == AnimationState.alerting)) {
			animator.Play(dealerting);
			animationState = AnimationState.dealerting;
		}
	}

	public void PlayActivating () {
		animator.Play(activating);
		animationState = AnimationState.activating;
	}
	public void PlayActiveIdle () {
		animator.Play(activeIdle);
		animationState = AnimationState.activeIdle;
	}
	public void PlayDeactivating () {
		animator.Play(deactivating);
		animationState = AnimationState.deactivating;
	}
	public void PlayAlertIdle () {
		animator.Play(alertIdle);
		animationState = AnimationState.alertIdle;
	}
	public void PlayInactiveIdle () {
		animator.Play(inactiveIdle);
		animationState = AnimationState.inactiveIdle;
	}
	public void PlayAlerting () {
		animator.Play(alerting);
		animationState = AnimationState.alerting;
	}
	public void PlayDealerting () {
		animator.Play(dealerting);
		animationState = AnimationState.dealerting;
	}
}
