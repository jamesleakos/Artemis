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
    float radius;
    float deviationHeight;
    Vector3 deviationVector;
    Vector3 newPathVector;
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
    //public float followPlayerLength;
    //float endFollowPlayer;

    //public float followPlayerCooldownLength;
    //float endFollowPlayerCooldown;

    public bool playerFollower;
    public float followPlayerDistance;

    bool inSwitchPosition = false;
    Transform bird1;

    Vector3 targetPosition;
    public float smoothTime = 0.01F;
    private Vector3 smoothVelocity = Vector3.zero;
    #endregion

    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;
    #endregion

    Transform RotationObject;

    void Start() {
        bird1 = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "bird1");
        RotationObject = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "RotationObject");
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
                // if (Time.time > endFollowPlayerCooldown) {
                if (playerFollower && (Mathf.Abs((player.transform.position - bird1.position).magnitude) < followPlayerDistance)) {
                    movementState = MovementState.followingPlayer;
                    //endFollowPlayer = Time.time + followPlayerLength;
                }
            }

            velocity = CalculatePlatformMovement();
            transform.Translate(velocity);

            Quaternion tempRotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            //tempRotation *= Quaternion.Euler(0, 0, -90); // this adds a 90 degrees Y rotation
            RotationObject.rotation = tempRotation;

        } else if (movementState == MovementState.followingPlayer) {
            if (player != null) {
                transform.position = Vector3.SmoothDamp(transform.position, playerTransform.position, ref smoothVelocity, smoothTime, speed);

            } else {
                //movementState = MovementState.flying;
            }

            //if (Time.time > endFollowPlayer) {
            //    movementState = MovementState.flying;
            //    endFollowPlayerCooldown = Time.time + followPlayerCooldownLength;
            //}
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

        if (movementState == MovementState.flying) {
            animator.Play(flying);
        } else if (movementState == MovementState.circling && inSwitchPosition) {
            animator.Play(circling);
        } else if (movementState == MovementState.followingPlayer && inSwitchPosition) {
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

            #region Broken Curve Code
            //if (curveOn) {
            //    fromWaypointIndex %= globalWaypoints.Length;
            //    toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;

            //    newPathVector = globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex];
            //    float newTestAngle = Vector3.SignedAngle(newPathVector, velocity, Vector3.forward);
            //    if (Mathf.Abs(newTestAngle) > 90) {
            //        deviationVector = new Vector3(0f, 0f, 0f);
            //    } else {
            //        // this may still be fine - hard to say without calculating the correct scaling first
            //        radius = (newPathVector.magnitude / 2 * Mathf.Sin(newTestAngle * Mathf.Deg2Rad));
            //        deviationHeight = radius * (1 - Mathf.Sin(Mathf.PI / 2 - (newTestAngle * Mathf.Deg2Rad)));
            //        Vector3 perpendicularVector = Vector3.Cross(Vector3.forward, newPathVector);
            //        float newTestAngle2 = Vector3.SignedAngle(newPathVector, perpendicularVector, Vector3.forward);
            //        if (Mathf.Sign(newTestAngle2) == Mathf.Sign(newTestAngle)) {
            //            perpendicularVector = perpendicularVector * -1;
            //        }
            //        Vector3 perpendicularVectorNormalized = perpendicularVector.normalized;
            //        deviationVector = perpendicularVectorNormalized * deviationHeight;
            //    }
            //}
            #endregion
        }
        Vector3 newPosFinal = newPos;

        #region Broken Curve Code
        //if (curveOn) {
        //    float finalDeviationMagnitude;
        //    if (percentBetweenWaypoints < 0.5f) {
        //        finalDeviationMagnitude = Mathf.Sqrt(Mathf.Pow(radius, 2f) - Mathf.Pow(newPathVector.magnitude / 2f - (percentBetweenWaypoints * newPathVector.magnitude), 2f)) -
        //        (radius - deviationHeight);
        //    } else if (percentBetweenWaypoints > 0.5f && percentBetweenWaypoints < 1f) {
        //        finalDeviationMagnitude = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(newPathVector.magnitude / 2f - ((1f - percentBetweenWaypoints) * newPathVector.magnitude), 2f)) -
        //        (radius - deviationHeight);
        //    } else if (percentBetweenWaypoints == 0.5f) {
        //        finalDeviationMagnitude = deviationHeight;
        //    } else {
        //        finalDeviationMagnitude = 0;
        //    }
        //    if (float.IsNaN(finalDeviationMagnitude)) {
        //        finalDeviationMagnitude = 0;
        //    }


        //    //  ISNT WORKING, KILLING THIS FOR NOW BUT LEAVING THE CODE\
        //    finalDeviationMagnitude = 0;
        //    // THIS KILLS IT



        //    Vector3 scaledDeviationVector = finalDeviationMagnitude * deviationVector.normalized;

        //    newPosFinal = newPos + scaledDeviationVector;
        //}
        #endregion

        return newPosFinal - transform.position;
    }

    void OnDrawGizmos() {
        if (localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = 2f;

            for (int i = 0; i < localWaypoints.Length; i++) {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

    public void TurnOnInSwitchPosition () {
        inSwitchPosition = true;
    }
}
