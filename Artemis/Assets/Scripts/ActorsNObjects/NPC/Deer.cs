using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]

public class Deer : MonoBehaviour {

    #region Habitat

    public Vector3 localMinInhabitRange;
    public Vector3 localMaxInhabitRange;
    Vector3 globalMinInhabitRange;
    Vector3 globalMaxInhabitRange;

    public Vector3 localMinSightRange;
    public Vector3 localMaxSightRange;
    Vector3 globalMinSightRange;
    Vector3 globalMaxSightRange;

    public Vector3 localMinFriendRange;
    public Vector3 localMaxFriendRange;
    Vector3 globalMinFriendRange;
    Vector3 globalMaxFriendRange;
    #endregion

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

    #region Basic Movement
    // base speed
    public float friendlySpeed;
    public float jumpSpeed;
    public float scaredSpeed;
    #endregion

    #region Attributes - Health
    // health
    public int health = 1;
    #endregion

    #region State and determining state and FaceDirX

    public int faceDirX;

    // states
    public enum DeerState { grazing, alert, friendly, scared, jumping };
    public DeerState deerState;

    #region Grazing
    public enum GrazingState { idle, movingForwards, movingBackwards, lookingUp, lookingDown };
    public GrazingState grazingState;

    public enum GrazingActionState { idle, tailTwitch, earTwitch, legTwitch };
    public GrazingActionState grazingActionState;

    public float autoAlertDistance = 25f;

    int grazingCycleCounter = 0;
    int nextGrazingAction;

    #endregion

    #region Alert
    public enum AlertActionState { idle, earTwitch, tailTwitch };
    public AlertActionState alertActionState;

    public float alertAttentionSpan = 8;
    float endAlertTime;

    int alertCycleCounter = 0;
    int nextAlertAction;
    #endregion

    #region Friendly
    public enum FriendlyActionState { walk, snuggle };
    public FriendlyActionState friendlyActionState;

    public float friendlyAttentionSpan = 8;
    float endFriendlyTime;

    bool isFriend = false;
    #endregion

    #region Looking At Player
    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.05f;
    public LayerMask collisionMask;
    #endregion

    #region Grazing

    #endregion

    #region Alert
    bool playerInSight = false;

    float lastTimePlayerSeen;
    Transform lastPlacePlayerSeen;
    
    #endregion


    #endregion

    #region Animation
    Animator animator;

    const string walk = "walk";
    const string run = "run";
    const string snuggle = "snuggle";
    const string jump = "jump";
    const string lookUp = "lookUp";
    const string lookDown = "lookDown";
    const string lookingTailTwitch = "lookingTailTwitch";
    const string lookingIdle = "lookingIdle";
    const string lookingEarTwitch = "lookingEarTwitch";
    const string grazeMoveForward = "grazeMoveForward";
    const string grazeMoveBackward = "grazeMoveBackward";
    const string grazeIdle = "grazeIdle";
    const string grazeTailTwitch = "grazeTailTwitch";
    const string grazeLegTwitch = "grazeLegTwitch";
    const string grazeEarTwitch = "grazeEarTwitch";

    enum AnimationState { walk, run, snuggle, jump, lookUp, lookDown, lookingTailTwitch, lookingIdle, lookingEarTwitch, grazeMoveForward, grazeMoveBackward,
        grazeTailTwitch, grazeLegTwitch, grazeEarTwitch, grazeIdle
    }
    AnimationState animationState;

    #endregion

    #region Private Movement Vars
    public float gravity;
    [HideInInspector]
    public Vector3 velocity;
    float baseVelocityX;
    #endregion

    #region Spawning

    GameMaster gm;
    bool spawnTriggered;

    public Vector3 localSpawnPoint;
    Vector3 globalSpawnPoint;

    #endregion

    Controller2D controller;
    private Collider2D thisCollider;
    AudioManager audioManager;

    Transform headBone;

    void Start() {
        controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();

        cam = Camera.main;

        headBone = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "HeadBone");

        gravity = -155f;

        globalMaxInhabitRange = localMaxInhabitRange + transform.position;
        globalMinInhabitRange = localMinInhabitRange + transform.position;

        globalMaxSightRange = localMaxSightRange + transform.position;
        globalMinSightRange = localMinSightRange + transform.position;

        globalMaxFriendRange = localMaxFriendRange + transform.position;
        globalMinFriendRange = localMinFriendRange + transform.position;

        globalSpawnPoint = localSpawnPoint + transform.position;

        deerState = DeerState.grazing;
        faceDirX = -1;
    }

    void Update() {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Debug.DrawRay(gameObject.transform.position, new Vector3(faceDirX, 0, 0), Color.red);

        if (playerTransform == null) {
            FindPlayer();
        }

        
        DeerBehavior();
        DetermineAnim();
    }

    void LateUpdate() {
        if (deerState == DeerState.alert
            && playerInSight && playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                //UpdateHeadRotation();  this just looks goofy
            }
        }
    }

    #region Behavs
    void DeerBehavior() {
        if (deerState == DeerState.grazing) {
            GrazingBehavior();
        }
        if (deerState == DeerState.alert) {
            AlertBehavior();
        }
        if (deerState == DeerState.friendly) {
            FriendlyBehavior();
        }
        if (deerState == DeerState.scared) {
            ScaredBehavior();
        }

        //// add gravity        
        velocity.y += gravity * Time.deltaTime;

        if (controller.collisions.above || controller.collisions.below || deerState == DeerState.jumping) {
            velocity.y = 0;
        }

        // move character
        controller.Move(velocity * Time.deltaTime);
    }

    void GrazingBehavior() {
        if (playerTransform != null) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerDeer();
                if (playerInSight) {
                    if (player.velocity.x > player.moveSpeedWalk && playerTransform.position.x > globalMinSightRange.x && playerTransform.position.x < globalMaxSightRange.x) { // playing running inside range
                        SwitchToScared();
                    } else if (player.velocity.x > 0 && playerTransform.position.x > globalMinFriendRange.x && playerTransform.position.x < globalMaxFriendRange.x) {  // player walking inside friend range
                        GrazingLookUp();
                    } else {
                        CycleGrazing();
                    }
                } else {
                    CycleGrazing();
                }
            }
        } else {
            CycleGrazing();
        }
    }

    void CycleGrazing() {
        if (grazingCycleCounter == nextGrazingAction) {
            grazingCycleCounter = 0;
            int actionSelection = Random.Range(1, 5);
            if (actionSelection == 1) {
                grazingActionState = GrazingActionState.earTwitch;
            } else if (actionSelection == 2) {
                grazingActionState = GrazingActionState.legTwitch;
            } else if (actionSelection == 3) {
                grazingActionState = GrazingActionState.tailTwitch;
            } else if (actionSelection == 4) {
                grazingActionState = GrazingActionState.idle;
            }

            actionSelection = Random.Range(1, 12);
            if (actionSelection == 1) {
                SwitchGrazingPosition();
            }

            actionSelection = Random.Range(1, 4);
            if (actionSelection == 1) {
                GrazingLookUp();
            }

            nextGrazingAction = 1;
        }
    }

    void SwitchGrazingPosition() {
        // might implement some movement forwards or backwards here - would need to make sure random walk didn't push off an edge.
    }

    public void IncrementGrazingCycleCounter() {
        grazingCycleCounter = grazingCycleCounter + 1;
    }
    public void IncrementAlertCycleCounter() {
        alertCycleCounter = alertCycleCounter + 1;
    }
    void GrazingLookUp() {
        grazingCycleCounter = 0;
        grazingState = GrazingState.lookingUp;
    }
    public void SwitchDirection() {
        faceDirX = faceDirX * -1;
    }

    void AlertBehavior() {

        // Test for Sightline
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerDeer();
                if (playerInSight && player.velocity.x > 1) {
                    SwitchToScared();
                } else {
                    if (Time.time > endAlertTime) {
                        if (playerTransform != null) {
                            if (playerTransform.position.x > globalMinFriendRange.x && playerTransform.position.x < globalMaxFriendRange.x) { // player inside friend zone
                                SwitchToFriendly();
                            } else {
                                SwitchToGrazing();
                            }
                        }
                    }
                }
            }
        } else {
            if (Time.time > endAlertTime) {
                SwitchToGrazing();
            } else {
                if (alertCycleCounter == nextAlertAction) {
                    alertCycleCounter = 0;
                    int actionSelection = Random.Range(1, 4);
                    if (actionSelection == 1) {
                        alertActionState = AlertActionState.earTwitch;
                    } else if (actionSelection == 2) {
                        alertActionState = AlertActionState.idle;
                    } else if (actionSelection == 3) {
                        alertActionState = AlertActionState.tailTwitch;
                    }

                    nextAlertAction = 1;
                }
            }
        }
    }

    void FriendlyBehavior() {

        if (!spawnTriggered) {
            spawnTriggered = true;
            gm.UpdateSpawn(globalSpawnPoint);
        }
        if (playerTransform != null) {
            if (playerTransform.position.x > globalMinInhabitRange.x && playerTransform.position.x < globalMaxInhabitRange.x) {
                if (lastPlacePlayerSeen != null) {
                    faceDirX = -1 * (int)Mathf.Sign(lastPlacePlayerSeen.position.x - gameObject.transform.position.x);
                }
                if (Mathf.Abs(playerTransform.position.x - transform.position.x) < 1.3f) {
                    friendlyActionState = FriendlyActionState.snuggle;
                    velocity.x = 0;
                } else {
                    friendlyActionState = FriendlyActionState.walk;
                    velocity.x = -1 * friendlySpeed * faceDirX;
                }
            } else {
                SwitchToGrazing();
            }
        }
        
    }

    void ScaredBehavior() {
        if (lastPlacePlayerSeen != null) {
            faceDirX = (int)Mathf.Sign(lastPlacePlayerSeen.position.x - gameObject.transform.position.x);
        }

        velocity.x = -1 * scaredSpeed * faceDirX;

        // if close to exit, swithc to jumping
        if (transform.position.x < globalMinInhabitRange.x || transform.position.x > globalMaxInhabitRange.x ||
            (playerTransform.position.x > globalMinFriendRange.x && playerTransform.position.x < globalMaxFriendRange.x)) {
            SwitchToJumping();
        }
    }
    #endregion

    #region Behavior Switches
    void SwitchToGrazing() {
        velocity.x = 0;
        deerState = DeerState.grazing;
        grazingState = GrazingState.lookingDown;
        grazingCycleCounter = 0;
    }
    public void SwitchToGrazingIdle() {
        // could implement random pick for forward or backward
        grazingState = GrazingState.idle;
        grazingCycleCounter = 0;
    }

    void SwitchToAlert() {
        velocity.x = 0;
        deerState = DeerState.alert;
        alertActionState = AlertActionState.idle;
        endAlertTime = Time.time + alertAttentionSpan;
    }
    void SwitchToFriendly() {
        velocity.x = 0;
        deerState = DeerState.friendly;
    }
    void SwitchToScared() {
        velocity.x = 0;
        deerState = DeerState.scared;
        gm.ResetSpawn();
    }
    public void SwitchToJumping() {
        velocity.x = -1 * jumpSpeed * faceDirX;
        deerState = DeerState.jumping;
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
    RaycastHit2D LookAtPlayerDeer() {
        RaycastHit2D hit;
        Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 1, 0);
        if (playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                Vector2 target = playerTransform.position;
                Vector2 sightline = target - rayOrigin;
                hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

                if (hit && hit.transform.tag == "Player") {
                    lastTimePlayerSeen = Time.time;
                    lastPlacePlayerSeen = hit.transform;
                    if (playerTransform.position.x > globalMinSightRange.x && playerTransform.position.x < globalMaxSightRange.x) {
                        playerInSight = true;
                    } else {
                        playerInSight = false;
                    }
                } else {
                    rayOrigin = headBone.transform.position;
                    sightline = target - rayOrigin;
                    hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);
                    playerInSight = false;
                }
                NextTimeToRaycast = Time.time + lookInterval;
            } else {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
            }
        } else {
            hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
        }
        return hit;
    }

    #region Animation
    void SetOrKeepState(AnimationState a_state) {
        if (this.animationState == a_state) return;
        EnterState(a_state);
    }

    void ExitState() {
    }

    void EnterState(AnimationState state) {
        //ExitState();
        switch (state) {
            case AnimationState.grazeEarTwitch:
                animator.Play(grazeEarTwitch);
                break;
            case AnimationState.grazeLegTwitch:
                animator.Play(grazeLegTwitch);
                break;
            case AnimationState.grazeTailTwitch:
                animator.Play(grazeTailTwitch);
                break;
            case AnimationState.grazeIdle:
                animator.Play(grazeIdle);
                break;
            case AnimationState.lookDown:
                animator.Play(lookDown);
                break;
            case AnimationState.grazeMoveBackward:
                animator.Play(grazeMoveBackward);
                break;
            case AnimationState.grazeMoveForward:
                animator.Play(grazeMoveForward);
                break;
            case AnimationState.lookUp:
                animator.Play(lookUp);
                break;
            case AnimationState.jump:
                animator.Play(jump);
                break;
            case AnimationState.lookingEarTwitch:
                animator.Play(lookingEarTwitch);
                break;
            case AnimationState.lookingIdle:
                animator.Play(lookingIdle);
                break;
            case AnimationState.lookingTailTwitch:
                animator.Play(lookingTailTwitch);
                break;
            case AnimationState.run:
                animator.Play(run);
                break;
            case AnimationState.snuggle:
                animator.Play(snuggle);
                break;
            case AnimationState.walk:
                animator.Play(walk);
                break;

        }

        this.animationState = state;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX, v.y, v.z);

        if (deerState == DeerState.alert) {
            if (alertActionState == AlertActionState.earTwitch) {
                SetOrKeepState(AnimationState.lookingEarTwitch);
            } else if (alertActionState == AlertActionState.idle) {
                SetOrKeepState(AnimationState.lookingIdle);
            } else if (alertActionState == AlertActionState.tailTwitch) {
                SetOrKeepState(AnimationState.lookingTailTwitch);
            }
        } else if (deerState == DeerState.friendly) {
            if (friendlyActionState == FriendlyActionState.walk) {
                SetOrKeepState(AnimationState.walk);
            } else if (friendlyActionState == FriendlyActionState.snuggle) {
                SetOrKeepState(AnimationState.snuggle);
            }
        } else if (deerState == DeerState.grazing) {

            if (grazingState == GrazingState.idle) {
                if (grazingActionState == GrazingActionState.earTwitch) {
                    SetOrKeepState(AnimationState.grazeEarTwitch);
                } else if (grazingActionState == GrazingActionState.legTwitch) {
                    SetOrKeepState(AnimationState.grazeLegTwitch);
                } else if (grazingActionState == GrazingActionState.tailTwitch) {
                    SetOrKeepState(AnimationState.grazeTailTwitch);
                } else if (grazingActionState == GrazingActionState.idle) {
                    SetOrKeepState(AnimationState.grazeIdle);
                }
            } else if (grazingState == GrazingState.lookingDown) {
                SetOrKeepState(AnimationState.lookDown);
            } else if (grazingState == GrazingState.lookingUp) {
                SetOrKeepState(AnimationState.lookUp);
            } else if (grazingState == GrazingState.movingBackwards) {

            } else if (grazingState == GrazingState.movingForwards) {
                
            }


        } else if (deerState == DeerState.jumping) {
            SetOrKeepState(AnimationState.jump);
        } else if (deerState == DeerState.scared) {
            SetOrKeepState(AnimationState.run);
        }
    }
    #endregion

    public void HitByArrow() {
        PlayArrowHitSound();
        SwitchToJumping();
    }

    public void KillSelf() {
        Destroy(gameObject);
    }

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

    void OnDrawGizmos() {
        if (true) {
            Gizmos.color = Color.red;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMinInhabitRange : localMinInhabitRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.red;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMaxInhabitRange : localMaxInhabitRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.green;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMinSightRange : localMinSightRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.green;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMaxSightRange : localMaxSightRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.blue;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMinFriendRange : localMinFriendRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.blue;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMaxFriendRange : localMaxFriendRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.white;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalSpawnPoint : localSpawnPoint + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
    }


    
    void PlayPowerDown() {
        audioManager.PlaySound("PowerDown");
    }
    void PlayArrowHitSound() {
        audioManager.PlaySound("ArmourHit");
    }
    void PlayFireSound() {
        audioManager.PlaySound("SpiderFire");
    }
}
