using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoBirds : MonoBehaviour {

    #region Waypoint and Movement Stuff
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    Vector3 velocity;
    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0, 2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;
    #endregion

    #region Curving Stuff
    // curves
    public bool curveOn = false;
    float curveOffset;
    float curveAngle;
    Vector3 newh;
    #endregion

    #region Movement States
    //Movement States
    public enum MovementState { followingPlayer, flying, circling }
    public MovementState movementState;

    #endregion

    #region Animation
    Animator animator;

    const string circling = "circling";
    const string flying = "flying";

    enum AnimationState { circling, flying }
    AnimationState animationState;

    #endregion

    #region Following Player
    public float followPlayerLength;
    float endFollowPlayer;

    public float followPlayerCooldownLength;
    float endFollowPlayerCooldown;

    public float followPlayerDistance;
    #endregion

    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;
    #endregion

    void Start() {

        animator = GetComponent<Animator>();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++) {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

        movementState = MovementState.flying;
    }

    void Update() {

        if (playerTransform == null) {
            FindPlayer();
        }

        if (movementState == MovementState.circling) {

        } else if (movementState == MovementState.flying) {
            if (player != null) {
                if (Time.time > endFollowPlayerCooldown && (Mathf.Abs((player.transform.position - transform.position).magnitude) < followPlayerDistance)) {
                    movementState = MovementState.followingPlayer;
                    endFollowPlayer = Time.time + followPlayerLength;
                }
            }

            velocity = CalculatePlatformMovement();
            transform.Translate(velocity);

        } else if (movementState == MovementState.followingPlayer) {
            if (player != null) {
                transform.position = player.transform.position;
            } else {
                movementState = MovementState.flying;
            }

            if (Time.time > endFollowPlayer) {
                movementState = MovementState.flying;
                endFollowPlayerCooldown = Time.time + followPlayerCooldownLength;
            }
        }

        DetermineAnim();
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
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

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * Mathf.Sign(velocity.x), v.y, v.z);

        if (movementState == MovementState.flying) {
            animator.Play(flying);
        } else if (movementState == MovementState.circling) {
            animator.Play(circling);
        }
    }

    Vector3 CalculatePlatformMovement() {

        if (Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        if (globalWaypoints.Length == 0) {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1) {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic) {
                if (fromWaypointIndex >= globalWaypoints.Length - 1) {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;

            if (curveOn) {
                fromWaypointIndex %= globalWaypoints.Length;
                toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
                newh = Vector3.Cross(globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex],Vector3.forward);
                newh.Normalize();
                newh = newh * (globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex]).magnitude/2;
            }
        }

        Vector3 newPosFinal = newPos;

        if (curveOn) {
            float scalehfloat = (1 - 2 * Mathf.Abs(0.5f - percentBetweenWaypoints));
            Vector3 scaledH = scalehfloat * newh;
            newPosFinal = newPos + scaledH;
        }

        return newPosFinal - transform.position;
    }

    void OnDrawGizmos() {
        if (localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++) {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
