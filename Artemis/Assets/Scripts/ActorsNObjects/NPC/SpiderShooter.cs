using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderShooter: MonoBehaviour {

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

    #region Attributes - Health

    // health
    public int health = 1;
    #endregion

    #region Shooting and Ball Exploding
    public Arrow arrow;
    Arrow arrowClone;
    Transform bow;
    Transform balls;
    #endregion

    #region State and determining state and FaceDirX

    public int faceDirX;

    // states
    public enum SpiderShooterState { inactive, alertActivating, alertLoaded, alertFiring, shocking, deactivating, dying, dead };
    public SpiderShooterState spiderShooterState;

    float lastStateTime;

    #region Looking At Player
    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.05f;
    public LayerMask collisionMask;
    #endregion

    bool playerInSight;
    float lastTimePlayerSeen;
    Vector3 lastPlacePlayerSeen;
    public float alertAttentionSpan = 8;
    float endAlertTime;

    #region Shocking
    public Shockwave shockwave;
    public float shockwaveTriggerRange = 10;
    public float shockwaveCooldown = 5;
    float endShockwaveCooldownTime;
    #endregion

    #region Shooting
    Transform headBone;
    public float fireAttentionSpan = 5;
    #endregion

    #endregion

    #region Animation
    Animator animator;

    const string drawnIdle = "drawnIdle";
    const string fire = "fire";
    const string fold = "fold";
    const string idle = "idle";
    const string unfold = "unfold";
    const string shocking = "shocking";
    const string death = "death";
    const string dead = "dead";

    enum AnimationState { drawnIdle, fire, fold, idle, unfold, shocking, death, dead }
    AnimationState animationState;

    #endregion

    Controller2D controller;
    private Collider2D thisCollider;
    AudioManager audioManager;

    void Start() {
        controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        cam = Camera.main;

        headBone = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "HeadBone");
        bow = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Bow");
        balls = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Balls");
        bow.localScale = new Vector3(1 / gameObject.transform.lossyScale.x, 1 / gameObject.transform.lossyScale.y, 1 / gameObject.transform.lossyScale.z);

        spiderShooterState = SpiderShooterState.inactive;
        faceDirX = 1;
    }

    void Update() {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Debug.DrawRay(gameObject.transform.position, new Vector3(faceDirX, 0, 0), Color.red);

        if (playerTransform == null) {
            FindPlayer();
        }

        if (playerTransform != null && (spiderShooterState == SpiderShooterState.alertFiring || spiderShooterState == SpiderShooterState.alertLoaded)) {
            if (player.artemisState != Player.ArtemisState.dead) {
                if ((playerTransform.position - gameObject.transform.position).magnitude < shockwaveTriggerRange) {
                    RaycastHit2D hit = LookAtPlayerSpiderWalker();
                    if (hit && hit.transform.tag == "Player") {
                        ReleaseShockwave();
                    }
                }
            }
        }

        SpidershooterBehavior();
        DetermineAnim();
    }

    void LateUpdate() {
        if ((spiderShooterState == SpiderShooterState.alertLoaded ||
            spiderShooterState == SpiderShooterState.alertActivating ||
            spiderShooterState == SpiderShooterState.alertFiring)
            && playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                UpdateBowRotation();
            }
        }
    }

    #region Behavs
    void SpidershooterBehavior() {
        if (spiderShooterState == SpiderShooterState.inactive) {
            InactiveBehavior();
        }
        if (spiderShooterState == SpiderShooterState.alertActivating) {
            AlertActivatingBehavior();
        }
        if (spiderShooterState == SpiderShooterState.alertFiring) {
            AlertFiringBehavior();
        }
        if (spiderShooterState == SpiderShooterState.alertLoaded) {
            AlertLoadedBehavior();
        }
    }

    void InactiveBehavior() {
        #region Switching or Keeping State
        // switching or keeping state
        if (Time.time > NextTimeToRaycast && GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            RaycastHit2D hit = LookAtPlayerSpiderWalker();
            if (hit && hit.transform.tag == "Player") {
                faceDirX = (int)Mathf.Sign(hit.transform.position.x - gameObject.transform.position.x);
                SwitchToAlertActivating();
            }
        }
        #endregion
    }
    void AlertActivatingBehavior() {
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerSpiderWalker();
                if (hit && hit.transform.tag == "Player") {
                    faceDirX = (int)Mathf.Sign(playerTransform.position.x - gameObject.transform.position.x);
                    lastTimePlayerSeen = Time.time;
                    lastPlacePlayerSeen = hit.transform.position;
                    endAlertTime = Time.time + alertAttentionSpan;
                    playerInSight = true;
                }
            }
        }
    }
    void AlertFiringBehavior() {
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerSpiderWalker();
                if (hit && hit.transform.tag == "Player") {
                    faceDirX = (int)Mathf.Sign(playerTransform.position.x - gameObject.transform.position.x);
                    lastTimePlayerSeen = Time.time;
                    lastPlacePlayerSeen = hit.transform.position;
                    endAlertTime = Time.time + alertAttentionSpan;
                    playerInSight = true;
                }
            }
        }
    }
    void AlertLoadedBehavior() {
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerSpiderWalker();
                if (hit && hit.transform.tag == "Player") {
                    if ((int)Mathf.Sign(playerTransform.position.x - gameObject.transform.position.x)==faceDirX) {
                        SwitchToAlertFiring();
                        lastTimePlayerSeen = Time.time;
                        lastPlacePlayerSeen = hit.transform.position;
                        endAlertTime = Time.time + alertAttentionSpan;
                        playerInSight = true;
                    } else {
                        if (Time.time > endAlertTime) {
                            SwitchToDeactivating();
                        }
                    }
                } else {
                    if (Time.time > endAlertTime) {
                        SwitchToDeactivating();
                    }
                }
            }
        } else {
            if (Time.time > endAlertTime) {
                SwitchToDeactivating();
            }
        }
    }

    RaycastHit2D LookAtPlayerSpiderWalker() {
        RaycastHit2D hit;
        Vector2 rayOrigin = bow.position;
        if (playerTransform != null) {
            if (player.artemisState != Player.ArtemisState.dead) {
                Vector2 target = playerTransform.position;
                Vector2 sightline = target - rayOrigin;
                hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);
                if (hit && hit.transform.tag == "Player") {
                    // don't need to do anuthing but am leaving this here
                } else {
                    rayOrigin = bow.transform.position;
                    sightline = target - rayOrigin;
                    hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);
                    playerInSight = false;
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
    public void SwitchToInactive() {
        spiderShooterState = SpiderShooterState.inactive;
    }
    public void SwitchToAlertActivating() {
        spiderShooterState = SpiderShooterState.alertActivating;
        endAlertTime = Time.time + alertAttentionSpan;
    }
    public void SwitchToAlertLoaded() {
        spiderShooterState = SpiderShooterState.alertLoaded;
    }
    public void SwitchToAlertFiring() {
        spiderShooterState = SpiderShooterState.alertFiring;
    }
    public void SwitchToDeactivating() {
        spiderShooterState = SpiderShooterState.deactivating;
    }
    public void SwitchToShocking() {
        spiderShooterState = SpiderShooterState.shocking;
    }
    public void ReleaseShockwave() {
        if (Time.time > endShockwaveCooldownTime) {
            Instantiate(shockwave, gameObject.transform.position, gameObject.transform.rotation);
            endShockwaveCooldownTime = Time.time + shockwaveCooldown;
        }
    }
    public void SwitchToDying() {
        spiderShooterState = SpiderShooterState.dying;
        PlayPowerDown();
    }
    public void SwitchToDead() {
        spiderShooterState = SpiderShooterState.dead;
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

    void ExitState() {
    }

    void EnterState(AnimationState state) {
        //ExitState();
        switch (state) {
            case AnimationState.idle:
                animator.Play(idle);
                break;
            case AnimationState.dead:
                animator.Play(dead);
                break;
            case AnimationState.death:
                animator.Play(death);
                break;
            case AnimationState.drawnIdle:
                animator.Play(drawnIdle);
                break;
            case AnimationState.fire:
                animator.Play(fire);
                break;
            case AnimationState.fold:
                animator.Play(fold);
                break;
            case AnimationState.shocking:
                animator.Play(shocking);
                break;
            case AnimationState.unfold:
                animator.Play(unfold);
                break;
        }

        this.animationState = state;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX, v.y, v.z);

        if (spiderShooterState == SpiderShooterState.alertActivating) {
            SetOrKeepState(AnimationState.unfold);
        } else if (spiderShooterState == SpiderShooterState.alertFiring) {
            SetOrKeepState(AnimationState.fire);
        } else if (spiderShooterState == SpiderShooterState.alertLoaded) {
            SetOrKeepState(AnimationState.drawnIdle);
        } else if (spiderShooterState == SpiderShooterState.deactivating) {
            SetOrKeepState(AnimationState.fold);
        } else if (spiderShooterState == SpiderShooterState.dead) {
            SetOrKeepState(AnimationState.dead);
        } else if (spiderShooterState == SpiderShooterState.dying) {
            SetOrKeepState(AnimationState.death);
        } else if (spiderShooterState == SpiderShooterState.inactive) {
            SetOrKeepState(AnimationState.idle);
        } else if (spiderShooterState == SpiderShooterState.shocking) {
            SetOrKeepState(AnimationState.shocking);
        }
    }
    #endregion

    public void HitByArrowArmor() {
        PlayArrowHitSound();
    }
    public void HitByArrowFace() {
        health = health - 1;
        if (health == 0) {
            KillSelf();
        } else {
            HitByArrowArmor();
        }
    }
    public void KillSelf() {
        SwitchToDying();
    }

    public void FireArrow() {
        UpdateBowRotation();

        arrowClone = Instantiate(arrow, bow.transform.position, (faceDirX == -1) ? bow.transform.rotation * Quaternion.Euler(0, 180, 0) : bow.transform.rotation);
        //arrowClone = Instantiate(arrow, bow.transform.position, bow.transform.rotation);
        arrowClone.arrowState = Arrow.ArrowState.notched;
        arrowClone.FireArrow();
        arrowClone = null;

        PlayFireSound();
    }
    public void TriggerBalls() {
        for (int i = 0; i < balls.childCount; i++) {
            balls.GetChild(i).GetComponent<SpiderBall>().ReleaseShockwave();
        }
    }

    void UpdateBowRotation() {
        Vector2 targetPosition;
        if (playerInSight) {
            targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        } else {
            targetPosition = new Vector2(lastPlacePlayerSeen.x, lastPlacePlayerSeen.y);
        }
        Vector2 firePointPosition = new Vector2(bow.position.x, bow.position.y);
        Vector2 shotline = targetPosition - firePointPosition;

        bool smartshooter = false;
        if (smartshooter) {
            float timeToTarget = shotline.magnitude / arrow.moveSpeed;
            Vector2 futurePosition = new Vector2(
                lastPlacePlayerSeen.x + player.velocity.x * timeToTarget,
                lastPlacePlayerSeen.y + ((player.controller.collisions.above || player.controller.collisions.below) ? 0 : player.velocity.y * timeToTarget)
            );
            targetPosition = futurePosition;

            Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 0, 0);
            Vector2 sightline = targetPosition - rayOrigin;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

            if (!hit) {
                shotline = targetPosition - firePointPosition;
            }
        }

        shotline.Normalize();
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(shotline.y, shotline.x) * Mathf.Rad2Deg);

        rotation *= Quaternion.Euler(0, 0, 90.0f); // this adds 90 degrees

        headBone.rotation = rotation;
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
