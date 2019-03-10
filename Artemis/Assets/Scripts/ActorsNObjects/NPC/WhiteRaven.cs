﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteRaven : MonoBehaviour {

    #region Camera
    // camera
    private Camera cam;
    private Plane[] planes;

    #endregion

    #region Player Target 
    // get player target
    Transform playerTransform;
    Player player;
    float nextTimeToSearch;
    float searchInterval = 0.5f;
    #endregion

    #region Dodging Arrows
    public float frightenAngle = 45;

    Quaternion boneRotation;
    Vector2 firePointPosition;
    Vector2 firePointDirection;
    Quaternion firePointRotation;
    #endregion

    #region Attributes - Health

    // health
    public int health = 1;
    #endregion

    #region State and determining state and FaceDirX

    public int faceDirX;

    // states
    public enum RavenState { idle, squacking, flyingAway };
    public RavenState ravenState;

    public enum IdleState { idle, turning, tailTwitching, flapping, squacking };
    public IdleState idleState;

    int idleCycleCounter = 0;
    int nextIdleAction;

    #region Looking At Player
    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.05f;
    public LayerMask collisionMask;

    bool playerInSight;
    bool playerInTalkingDistance;

    public float talkingDistance = 50;
    #endregion

    #endregion

    #region Animation
    Animator animator;

    const string idle = "idle";
    const string squack = "squack";
    const string hop = "hop";
    const string tailTwitch = "tailTwitch";
    const string flap = "flap";
    const string squackFlap = "squackFlap";
    const string fly = "fly";

    enum AnimationState { idle, squack, hop, tailTwitch, flap, squackFlap, fly }
    AnimationState animationState;

    Transform headBone;

    #endregion

    Controller2D controller;
    private Collider2D thisCollider;
    AudioManager audioManager;
    public RavenText ravenText;

    #region Spawning

    GameMaster gm;
    bool spawnTriggered;

    public Vector3 localSpawnPoint;
    Vector3 globalSpawnPoint;

    #endregion


    void Start() {
        FindPlayer();

        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();

        globalSpawnPoint = localSpawnPoint + transform.position;


        controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        cam = Camera.main;

        headBone = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "HeadBone");

        ravenState = RavenState.idle;
        faceDirX = -1;
    }

    void Update() {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);

        if (playerTransform == null) {
            FindPlayer();
        }

        RavenBehavior();
        DetermineAnim();
    }

    void LateUpdate() {
        if (ravenState == RavenState.idle && (idleState == IdleState.idle ||
            idleState == IdleState.tailTwitching ||
            idleState == IdleState.flapping)
            && playerInSight && playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                UpdateHeadRotation();
            }
        }
    }

    #region Behavs
    void RavenBehavior() {
        if (ravenState == RavenState.idle) {
            IdleBehavior();
        }
        if (ravenState == RavenState.squacking) {
            SquackingBehavior();
        }
        if (ravenState == RavenState.flyingAway) {
            FlyingBehavior();
        }
    }

    void IdleBehavior() {
        #region Switching or Keeping State
        // switching or keeping state
        if (Time.time > NextTimeToRaycast && GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            RaycastHit2D hit = LookAtPlayerRaven();
            if (hit && hit.transform.tag == "Player") {
                if ((int)Mathf.Sign(hit.transform.position.x - gameObject.transform.position.x) != faceDirX) {
                    SwitchToIdleTurning();
                    idleCycleCounter = 0;
                }
            }
        }

        if (idleCycleCounter == nextIdleAction) {
            if (idleState == IdleState.idle) {
                idleCycleCounter = 0;
                int actionSelection = Random.Range(1, 10);
                if (actionSelection >= 8) {
                    SwitchToIdleFlapping();
                } else {
                    SwitchToIdleTwitching();
                }

                nextIdleAction = Random.Range(2, 4);
            }
        }

        if (playerInTalkingDistance) {

            if (Input.GetKeyDown(KeyCode.W)) {

                SwitchToIdleSquacking();
                idleCycleCounter = 0;

                if (ravenText != null) {
                    if (ravenText.animationState == RavenText.AnimationState.inactiveIdle) {
                        ravenText.PlayActivating();
                    } else if (ravenText.animationState == RavenText.AnimationState.activeIdle) {
                        ravenText.PlayDeactivating();
                    }
                }
            }

            if (!spawnTriggered) {
                spawnTriggered = true;
                gm.UpdateSpawn(globalSpawnPoint);
                SwitchToSquacking();
            }

        } else {
            if (ravenText != null) {
                if (ravenText.animationState == RavenText.AnimationState.activeIdle) {
                    ravenText.PlayDeactivating();
                }
            }
        }

        if (playerInSight) {
            if (player.bowOut) {
                boneRotation = player.midriffBone.rotation * Quaternion.Euler(0, 0, 90);

                firePointPosition = new Vector2(player.midriffBone.position.x, player.midriffBone.transform.position.y);
                firePointDirection = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y) - firePointPosition;
                firePointDirection.Normalize();
                firePointRotation = Quaternion.Euler(0, 0, Mathf.Atan2(firePointDirection.y, firePointDirection.x) * Mathf.Rad2Deg);

                float absAngle = Mathf.Abs(Quaternion.Angle(firePointRotation, boneRotation));

                if (absAngle < frightenAngle) {
                    SwitchToFlying();
                }
            }
        }

        #endregion
    }
    public void IncrementIdleCycleCounter() {
        idleCycleCounter = idleCycleCounter + 1;
    }
    public void SwitchDirection() {
        faceDirX = faceDirX * -1;
    }

    void SquackingBehavior() {
        if (ravenText != null) {
            if (ravenText.animationState == RavenText.AnimationState.activeIdle) {
                ravenText.PlayDeactivating();
            }
        }
    }
    void FlyingBehavior() {
        gm.ResetSpawn();
        if (ravenText != null) {
            if (ravenText.animationState == RavenText.AnimationState.activeIdle) {
                ravenText.PlayDeactivating();
            }
        }
    }
    public void DestroyThis() {
        Destroy(gameObject);
    }



    RaycastHit2D LookAtPlayerRaven() {
        RaycastHit2D hit;
        Vector2 rayOrigin = headBone.position;
        if (playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                Vector2 target = playerTransform.position;
                Vector2 sightline = target - rayOrigin;
                hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);
                if (hit && hit.transform.tag == "Player") {
                    playerInSight = true;
                    if (hit.distance < talkingDistance) {
                        playerInTalkingDistance = true;
                    } else {
                        playerInTalkingDistance = false;
                    }
                } else {
                    rayOrigin = headBone.transform.position;
                    sightline = target - rayOrigin;
                    hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);
                    playerInSight = false;
                    playerInTalkingDistance = false;
                }
                NextTimeToRaycast = Time.time + lookInterval;
            } else {
                playerInSight = false;
                hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
            }
        } else {
            playerInSight = false;
            hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
        }
        return hit;
    }

    #endregion

    #region Behavior Switches
    public void SwitchToIdle() {
        ravenState = RavenState.idle;
        SwitchToIdleIdle();
        idleCycleCounter = 0;
    }
    public void SwitchToSquacking() {
        ravenState = RavenState.squacking;
    }

    // idle behavs
    public void SwitchToIdleIdle() {
        idleState = IdleState.idle;
    }
    public void SwitchToIdleFlapping() {
        idleState = IdleState.flapping;
    }
    public void SwitchToIdleTwitching() {
        idleState = IdleState.tailTwitching;
    }
    public void SwitchToIdleTurning() {
        idleState = IdleState.turning;
    }
    public void SwitchToIdleSquacking() {
        idleState = IdleState.squacking;
    }

    public void SwitchToFlying() {
        ravenState = RavenState.flyingAway;
    }

    #endregion


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

    #region Animation
    void SetOrKeepState(AnimationState a_state) {
        if (this.animationState == a_state) return;
        EnterState(a_state);
    }


    void EnterState(AnimationState state) {
        switch (state) {
            case AnimationState.idle:
                animator.Play(idle);
                break;
            case AnimationState.fly:
                animator.Play(fly);
                break;
            case AnimationState.flap:
                animator.Play(flap);
                break;
            case AnimationState.hop:
                animator.Play(hop);
                break;
            case AnimationState.squack:
                animator.Play(squack);
                break;
            case AnimationState.squackFlap:
                animator.Play(squackFlap);
                break;
            case AnimationState.tailTwitch:
                animator.Play(tailTwitch);
                break;
        }

        this.animationState = state;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX * -1, v.y, v.z);

        if (ravenState == RavenState.idle) {
            if (idleState == IdleState.idle) {
                SetOrKeepState(AnimationState.idle);
            } else if (idleState == IdleState.flapping) {
                SetOrKeepState(AnimationState.flap);
            } else if (idleState == IdleState.tailTwitching) {
                SetOrKeepState(AnimationState.tailTwitch);
            } else if (idleState == IdleState.turning) {
                SetOrKeepState(AnimationState.hop);
            } else if (idleState == IdleState.squacking) {
                SetOrKeepState(AnimationState.squack);
            }
        } else if (ravenState == RavenState.squacking) {
            SetOrKeepState(AnimationState.squackFlap);
        } else if (ravenState == RavenState.flyingAway) {
            SetOrKeepState(AnimationState.fly);
        }
    }
    #endregion

    void UpdateHeadRotation() {
        Vector2 targetPosition;
        targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);

        Vector2 firePointPosition = new Vector2(headBone.position.x, headBone.position.y);
        Vector2 shotline = targetPosition - firePointPosition;

        shotline.Normalize();
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(shotline.y, shotline.x) * Mathf.Rad2Deg);

        if (faceDirX == 1)
            rotation *= Quaternion.Euler(0, 0, 180.0f);

        headBone.rotation = rotation;
    }

    public void PlaySquackSound() {
        audioManager.PlaySound("ravencall");
    }
    void PlayFlapSound() {
        audioManager.PlaySound("ArtemisDeath");
    }


    void OnDrawGizmos() {
        if (true) {
            Gizmos.color = Color.red;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalSpawnPoint : localSpawnPoint + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
    }
}
