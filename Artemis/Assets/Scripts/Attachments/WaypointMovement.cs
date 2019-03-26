using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMovement : MonoBehaviour {

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


    // curves
    public bool curveOn = false;
    float curveOffset;
    float curveAngle;


    Player player;

    void Start() {

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++) {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void Update() {

        velocity = CalculatePlatformMovement();
        transform.Translate(velocity);
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
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
                Vector3 newVect = globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex];
                float newTestAngle = Vector3.SignedAngle(newVect, velocity, Vector3(0, 0, 1));
                if (Mathf.ABS(newTestAngle > 90)) {
                    //circle and then set to something 
                } else {
                    //float h = (newVect.length/2*Mathf.sin(newTestAngle in degress/rads))) * (1 - sin(pi/2 - a)))
                    Vector3 perpendicular = Vector3.Cross(Vector3.forward, newVect);
                    float newTestAngle2 = Vector3.SignedAngle(newVect, perpendicular, Vector3(0, 0, 1));
                    //check if signs match, if not multply perp by -1
                    new Vector3 shortperp = perpendicular.normalized;
                    Vector3 newh = shortperp * h;
                }
            }
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            rotation *= Quaternion.Euler(0, 0, -90); // this adds a 90 degrees Y rotation
        }

        Vector3 newPosFinal = newPos;

        if (curveOn) {
            newPosFinal = newPos + (1 - 2 * Mathf.Abs(0.5f - percentBetweenWaypoints) * newh);
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
