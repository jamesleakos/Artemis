using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]

public class OldDeer : MonoBehaviour {
	
	// get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;

    // exit location
    public Transform exitLocation;

    // states
    private enum DeerState { calm, alert, scared, friendly };
    private DeerState deerState;

    #region Basic Movement
    // deer base speed
    public float scaredSpeed = 15;
    public float friendlySpeed = 5;

    // accerlation times - only one I want significant is the wall jumping one
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    #endregion

    #region Jumping
    // jumping behaviors
    public float jumpHeight = 6;
    public float timeToJumpApex = .3f;
    #endregion

    #region Determining State

    float lastStateTime;

    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.1f;

    // general sight stuff
    public float sightLength;
    public float closeLength;
    bool playerInSight;
    public LayerMask collisionMask;

    public float speedBuffer;

    // calm
    float nextTimeForCalmLook;
    public float calmLookIntervalAverage = 3;
    public float lookUpTime = 1.5f;

    // alert 
    public float waitTimeToBeFriends = 3;
    public float waitTimeToBeCalm = 1;

    // scared 
    public float scaredTime = 3;
    #endregion

    #region Private Movement Vars
    float gravity;
	float jumpVelocity;
	[HideInInspector]
	public Vector3 velocity;

	// smoothing vars
	float targetVelocityX;
	float velocityXSmoothing;
    #endregion

    Controller2D controller;
    Collider2D thisCollider;

	void Start() {
		controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();

		gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);

        deerState = DeerState.calm;
    }

    void Update() {
        if (playerTransform == null) {
            FindPlayer();
            // To do: play calm animation or some other placeholder

            return;
        }
        if (deerState == DeerState.calm) {
            CalmAction();
        }
        if (deerState == DeerState.alert) {
            AlertAction();
        }
        if (deerState == DeerState.scared) {
            ScaredAction();
        }
        if (deerState == DeerState.friendly) {
            FriendlyAction();
        }

        MoveDeer();
    }

    void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                player = searchResult.GetComponent<Player>();
                playerTransform = searchResult.GetComponent<Transform>();
            }
            nextTimeToSearch = Time.time + searchInterval;
        }
    }

    RaycastHit2D LookAtPlayer() {
        Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 0, 0);
        Vector2 target = playerTransform.position;
        Vector2 sightline = target - rayOrigin;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightLength, collisionMask);

        Debug.DrawRay(rayOrigin, sightline.normalized * sightLength, Color.red);

        NextTimeToRaycast = Time.time + lookInterval;
        return hit;
    }

    void CalmDeer () {
        // Debug.Log("Becoming Calm");
        deerState = DeerState.calm;
        nextTimeForCalmLook = Time.time + calmLookIntervalAverage + Random.Range(-1, 1);
        lastStateTime = Time.time;
    }
    void AlertDeer() {
        // Debug.Log("Becoming Alert");
        deerState = DeerState.alert;
        lastStateTime = Time.time;
    }
    void ScareDeer() {
        // Debug.Log("Becoming Scared");
        deerState = DeerState.scared;
        lastStateTime = Time.time;
    }
    void FriendDeer() {
        // Debug.Log("Becoming Friends");
        deerState = DeerState.friendly;
        lastStateTime = Time.time;
    }

    void CalmAction () {
        if (Time.time > NextTimeToRaycast) {
            RaycastHit2D hit = LookAtPlayer();
            if (hit && hit.transform.tag == "Player") {
                if (player.velocity.magnitude > player.moveSpeedBow + speedBuffer) {
                    // Debug.Log("Player velocity: " + player.velocity.magnitude + "   moveSpeedBow: " + player.moveSpeedBow);
                    ScareDeer();
                    return;
                }
                if (hit.distance < closeLength) {
                    StartCoroutine(CloseToDeer());
                    return;
                }
            }
        }
        if (Time.time > nextTimeForCalmLook) {
            StartCoroutine(CalmLook());
        }
    }
    IEnumerator CloseToDeer() {
        // Debug.Log("Looking Up");

        NextTimeToRaycast = Time.time + lookUpTime + 0.1f;
        yield return new WaitForSeconds(lookUpTime);

        AlertDeer();
    }
    IEnumerator CalmLook() {
        // start animation here
        // Debug.Log("Looking Up");

        nextTimeForCalmLook = Time.time + calmLookIntervalAverage + Random.Range(-1, 1);
        yield return new WaitForSeconds(lookUpTime);

        RaycastHit2D hit = LookAtPlayer();
        if (hit && hit.transform.tag == "Player") {
            if (player.velocity.magnitude > speedBuffer) { // had zero originally but wanted to account for small errors
                // Debug.Log("Player velocity: " + player.velocity.magnitude + "   speedBuffer: " + speedBuffer);
                ScareDeer();
            } else {
                AlertDeer();
            }
        }

        // Debug.Log("Looking Down");
        // back to calm animation 
    }

    void AlertAction () {
        if (Time.time > NextTimeToRaycast) {
            RaycastHit2D hit = LookAtPlayer();
            if (hit && hit.transform.tag == "Player") {
                if (player.velocity.magnitude > speedBuffer) { // had zero originally but wanted to account for small errors
                    // Debug.Log("Player velocity: " + player.velocity.magnitude + "   speedBuffer: " + speedBuffer);
                    ScareDeer();
                } else if (hit.distance <= closeLength && Time.time > lastStateTime + waitTimeToBeFriends) {
                    FriendDeer();
                    return;
                } else if (hit.distance > closeLength && Time.time > lastStateTime + waitTimeToBeCalm) {
                    CalmDeer();
                }
            } else {
                CalmDeer();
            }
        }
    }

    void ScaredAction () {
        if (Time.time > lastStateTime + scaredTime) {
            AlertDeer();
        }
    }

    void FriendlyAction () {
        if (Time.time > NextTimeToRaycast) {
            RaycastHit2D hit = LookAtPlayer();
            if (!(hit && hit.transform.tag == "Player")) {
                CalmDeer();
            }
        }
    }

    void MoveDeer() {
        if (deerState == DeerState.calm) {
            targetVelocityX = 0;
        }
        if (deerState == DeerState.alert) {
            targetVelocityX = 0;
        }
        if (deerState == DeerState.scared) {
            targetVelocityX = Mathf.Sign(playerTransform.position.x - thisCollider.transform.position.x) * scaredSpeed * -1;
        } 
        if (deerState == DeerState.friendly) {
            targetVelocityX = Mathf.Sign(playerTransform.position.x - thisCollider.transform.position.x) * friendlySpeed;
        }

        // smoothing of X acceleration
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        // add gravity
        velocity.y += gravity * Time.deltaTime;
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

        // move deer
        controller.Move(velocity * Time.deltaTime);
    }

    public void HitByArrow () {
		Destroy(gameObject);
    }
}