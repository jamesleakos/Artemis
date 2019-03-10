using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spidershell : MonoBehaviour {
    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;
    #endregion

    #region Looking At Player
    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.1f;
    public LayerMask collisionMask;
    #endregion

    #region Shocking
    public Shockwave shockwave;
    public float shockwaveTriggerRange = 10;
    #endregion
    Arrow arrow;

    void Start() {
        arrow = gameObject.GetComponent<Arrow>();
    }

    void Update() {
        if (playerTransform == null) {
            FindPlayer();
        }
        if (arrow.arrowState == Arrow.ArrowState.stuck) {
            ReleaseShockwave();
        }
        if (playerTransform != null) {
            if ((playerTransform.position - gameObject.transform.position).magnitude < shockwaveTriggerRange) {
                RaycastHit2D hit = LookAtPlayer();
                if (hit && hit.transform.tag == "Player") {
                    ReleaseShockwave();
                }
            }
        }
    }

    public void ReleaseShockwave() {
        Instantiate(shockwave, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
    }

    RaycastHit2D LookAtPlayer() {
        Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 0, 0);
        RaycastHit2D hit;

        if (playerTransform != null) {
            Vector2 target = playerTransform.position;
            Vector2 sightline = target - rayOrigin;
            hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

            Debug.DrawRay(rayOrigin, sightline, Color.red);

            NextTimeToRaycast = Time.time + lookInterval;
        } else {
            hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
        }
        return hit;
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
}
