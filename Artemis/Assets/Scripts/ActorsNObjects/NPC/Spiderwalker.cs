using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiderwalker : MonoBehaviour {

    #region Waypoints
    // Waypoints
    public bool patroller;
    enum PatrolMovementState { min, max }
    PatrolMovementState patrolMovementState;

    public Vector3 localMinPatrolRange;
    public Vector3 localMaxPatrolRange;
    Vector3 globalMinPatrolRange;
    Vector3 globalMaxPatrolRange;
    Vector3 destPoint;

    public Vector3 localMinInvestigateRange;
    public Vector3 localMaxInvestigateRange;
    Vector3 globalMinInvestigateRange;
    Vector3 globalMaxInvestigateRange;
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
    public float patrolSpeed = 8;
    public float flyingSpeed = 15;
    #endregion

    #region Movement State
    public enum MovementState { aerial, grounded };
    public MovementState movementState;

    enum TargetDirY { inline, up, down };
    TargetDirY targetDirY;

    #endregion

    #region Attributes - Health

    // health
    public int health = 1;
    #endregion

    #region Shooting
    public float loadTime = 0.1f;
    public float fireCoolDownTime = 0.4f;
    float endLoadTime;
    bool firingArrow;
    public Arrow arrow;
    Arrow arrowClone;
    Transform bow;
    #endregion

    #region State and determining state and FaceDirX

    public int faceDirX;

    // states
    public enum SpiderWalkerState { patrolling, alert, firing, shocking, flying, SwitchingToAlert, dying, dead };
    public SpiderWalkerState spiderWalkerState;

    public enum FiringState { prepping, ready, firing, turningOff };
    public FiringState firingState;

    float lastStateTime;

    #region Looking At Player
    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.05f;
    public LayerMask collisionMask;
    #endregion

    #region Patrolling

    float timeToChangeFaceDir;
    public float changeFaceDirLength = 5;
    public float patrolSwitchTime = 1;
    #endregion

    #region Alert
    public float alertSwitchTime = 1;
    public float autoAlertDistance = 30;
    public float HuntRangeYFactor = 4;
    public float alertTimeUntilFire = 5;
    float timeToFire;

    float lastTimePlayerSeen;
    Transform lastPlacePlayerSeen;
    public float alertAttentionSpan = 8;
    float endAlertTime;
    #endregion

    #region Shocking
    public Shockwave shockwave;
    public float shockwaveTriggerRange = 10;
    public float shockwaveCooldown = 5;
    float endShockwaveCooldownTime;
    #endregion

    #region Shooting
    Transform footbone;
    bool aimingOnline;
    float endFireTime;
    public float fireAttentionSpan = 5;
    #endregion

    #endregion

    #region Animation
    Animator animator;
    float stateStartTime;
    float timeInState {
        get { return Time.time - stateStartTime; }
    }
    const string MechSpiderIdle = "MechSpiderIdle";
    const string MechSpiderWalk = "MechSpiderWalk";
    const string MechSpiderFiring = "MechSpiderFiring";
    const string MechSpiderReadyToFire = "MechSpiderReadyToFire";
    const string MechSpiderSwitchingToFireMode = "MechSpiderSwitchingToFireMode";
    const string MechSpiderSwitchOffFireMode = "MechSpiderSwitchOffFireMode";
    const string MechSpiderShocking = "MechSpiderShocking";
    const string MechSpiderFlying = "MechSpiderFlying";
    const string MechSpiderSwitchingToAlert = "MechSpiderSwitchingToAlert";
    const string MechSpiderAlert = "MechSpiderAlert";
    const string MechSpiderDeath = "MechSpiderDeath";
    const string MechSpiderDead = "MechSpiderDead";

    enum AnimationState { Idle, Walking, Firing, FirePrep, FireReady, FireOff, Shocking, Flying, Alert, SwitchingToAlert, Death, Dead }
    AnimationState animationState;

    #endregion

    #region Private Movement Vars
    public float gravity;
    [HideInInspector]
    public Vector3 velocity;
    float baseVelocityX;
    #endregion

    #region Audio
    public AudioSource footstepSound;
    #endregion

    Controller2D controller;
    private Collider2D thisCollider;
	AudioManager audioManager;

    void Start() {

        footstepSound = GetComponent<AudioSource>();

        controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
		audioManager = GameObject.FindGameObjectWithTag ("AudioManager").GetComponent<AudioManager>();

        cam = Camera.main;

        footbone = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Front Foot");
        bow = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Bow");
        bow.localScale = new Vector3(1 / gameObject.transform.lossyScale.x, 1 / gameObject.transform.lossyScale.y, 1 / gameObject.transform.lossyScale.z);

        globalMaxInvestigateRange = localMaxInvestigateRange + transform.position;
        globalMinInvestigateRange = localMinInvestigateRange + transform.position;
        globalMaxPatrolRange = localMaxPatrolRange + transform.position;
        globalMinPatrolRange = localMinPatrolRange + transform.position;

        spiderWalkerState = SpiderWalkerState.patrolling;
        destPoint = globalMaxPatrolRange;
        patrolMovementState = PatrolMovementState.max;
        faceDirX = 1;
    }

    void Update() {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Debug.DrawRay(gameObject.transform.position, new Vector3(faceDirX, 0, 0), Color.red);

        if (playerTransform == null) {
            FindPlayer();
        }

        SpiderwalkerBehavior();
        DetermineAnim();
    }

    void LateUpdate() {
        if (spiderWalkerState == SpiderWalkerState.firing && aimingOnline && playerTransform != null) {
			if (player.artemisState != Player.ArtemisState.dead) {
				UpdateCannonRotation ();
			}
        }
    }

    #region Behavs
    void SpiderwalkerBehavior() {
        if (spiderWalkerState == SpiderWalkerState.patrolling) {
            PatrollingBehavior();
        }
        if (spiderWalkerState == SpiderWalkerState.alert) {
            AlertBehavior();
        }
        if (spiderWalkerState == SpiderWalkerState.SwitchingToAlert) {
            SwitchingToAlertBehavior();
        }
        if (spiderWalkerState == SpiderWalkerState.firing) {
            FiringBehavior();
        }
        if (spiderWalkerState == SpiderWalkerState.flying) {
            FlyingBehavior();
        }

        //// add gravity, cap velocities
        // movement
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
        velocity.y += gravity * Time.deltaTime;

        // move character
        controller.Move(velocity * Time.deltaTime);
        if (playerTransform != null && spiderWalkerState != SpiderWalkerState.dying && spiderWalkerState != SpiderWalkerState.dead) {
			if (player.artemisState != Player.ArtemisState.dead) {    
				if ((playerTransform.position - gameObject.transform.position).magnitude < shockwaveTriggerRange) {
					RaycastHit2D hit = LookAtPlayerSpiderWalker ();
					if (hit && hit.transform.tag == "Player") {
						ReleaseShockwave ();
					}
				}
			}
        }
        
    }

    void PatrollingBehavior() {
        #region Switching or Keeping State
        // switching or keeping state
        if (Time.time > NextTimeToRaycast && GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            RaycastHit2D hit = LookAtPlayerSpiderWalker();
            if (hit && hit.transform.tag == "Player") {
                if (faceDirX == Mathf.Sign(hit.transform.position.x - gameObject.transform.position.x) || hit.distance < autoAlertDistance) {
                    SwitchToSwitchingToAlert();
                }
            }
        }
        #endregion

        #region Movement
        if (!patroller) {
            velocity.x = 0;
            if (Time.time > timeToChangeFaceDir) {
                faceDirX = faceDirX * -1;
                timeToChangeFaceDir = Time.time + changeFaceDirLength;
            }
        } else {
            velocity.x = Mathf.Sign(destPoint.x - gameObject.transform.position.x) * patrolSpeed;

            if (Mathf.Abs(destPoint.x - gameObject.transform.position.x) < 1) {
                UpdatePatrolNode();
            }

            // face direction
            if (Mathf.Sign(velocity.x) < 0) {
                faceDirX = -1;
            } else if (Mathf.Sign(velocity.x) > 0) {
                faceDirX = 1;
            }
        }
        #endregion

    }
    void UpdatePatrolNode() {
        if (!patroller) {
            return;
        }
        if (patrolMovementState == PatrolMovementState.max) {
            destPoint = globalMinPatrolRange;
            patrolMovementState = PatrolMovementState.min;
        } else if (patrolMovementState == PatrolMovementState.min) {
            destPoint = globalMaxPatrolRange;
            patrolMovementState = PatrolMovementState.max;
        }
    }
    void AlertBehavior() {
        if (lastPlacePlayerSeen != null) {
            faceDirX = (int)Mathf.Sign(lastPlacePlayerSeen.position.x - gameObject.transform.position.x);
        }
        
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerSpiderWalker();
                if (hit && hit.transform.tag == "Player") {
                    if (Time.time > timeToFire) {
                        SwitchToFiring();
                    }
                    endAlertTime = Time.time + alertAttentionSpan;
                } else {
                    if (Time.time > endAlertTime) {
                        SwitchToPatrol();
                    }
                }
            }
        } else {
            if (Time.time > endAlertTime) {
                SwitchToPatrol();
            }
        }
        if (lastPlacePlayerSeen != null && playerTransform != null) {
			if (player.artemisState != Player.ArtemisState.dead) {
				if (WithinMovementRange (gameObject.transform.position) != Mathf.Sign (lastPlacePlayerSeen.position.x - gameObject.transform.position.x)) {
					velocity.x = Mathf.Sign (lastPlacePlayerSeen.position.x - gameObject.transform.position.x) * patrolSpeed;
					if (Mathf.Abs (playerTransform.position.y - gameObject.transform.position.y) < gameObject.transform.lossyScale.y * 4) {
						timeToFire = Time.time + alertTimeUntilFire;
					}
				} else {
					velocity.x = 0;
				}
			} else {
				velocity.x = 0;
			}
        } else {
            velocity.x = 0;
        }
    }
    void SwitchingToAlertBehavior() {
        velocity.x = 0;
    }
    int WithinMovementRange (Vector3 location) {
        if (location.x < globalMaxInvestigateRange.x && location.x > globalMinInvestigateRange.x) {
            return 0;
        } else if (location.x > globalMaxInvestigateRange.x) {
            return 1;
        } else {
            return -1;
        }
    }
    void FiringBehavior() {
        velocity.x = 0;
        if (lastPlacePlayerSeen != null) {
            faceDirX = (int)Mathf.Sign(lastPlacePlayerSeen.position.x - gameObject.transform.position.x);
        }
        
        #region Switching or Keeping State
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayerSpiderWalker();
                if (hit && hit.transform.tag == "Player") {
                    if (firingState == FiringState.ready && Time.time > endShockwaveCooldownTime) {
                        FireArrow();
                        firingState = FiringState.firing;
                    }
                    endFireTime = Time.time + fireAttentionSpan;
                } else {
                    if (Time.time > endFireTime) {
                        firingState = FiringState.turningOff;
                    }
                }
            }
        } else {
            if (Time.time > endFireTime) {
                firingState = FiringState.turningOff;
            }
        }
        #endregion
    }
    public void ReadyToFire() {
        firingState = FiringState.ready;
    }
    public void TurnOnAiming () {
        aimingOnline = true;
    }
    public void TurnOffAiming() {
        aimingOnline = false;
    }
    void FlyingBehavior() {

    }
    #endregion

    #region Behavior Switches
    void SwitchToPatrol() {
        spiderWalkerState = SpiderWalkerState.patrolling;
    }
    public void SwitchToAlert() {
        velocity.x = 0;
        spiderWalkerState = SpiderWalkerState.alert;
        endAlertTime = Time.time + alertAttentionSpan;
        timeToFire = Time.time + alertTimeUntilFire;
    }
    void SwitchToSwitchingToAlert() {
        velocity.x = 0;
        spiderWalkerState = SpiderWalkerState.SwitchingToAlert;
    }
    public void SwitchToAlertNoticePlayer() {
        if (playerTransform != null) {
			if (player.artemisState != Player.ArtemisState.dead) {
				faceDirX = (int)Mathf.Sign (playerTransform.position.x - gameObject.transform.position.x);
			}
        }
    }
    void SwitchToFiring() {
        spiderWalkerState = SpiderWalkerState.firing;
        firingState = FiringState.prepping;
        endFireTime = Time.time + fireAttentionSpan;

    }
    void SwitchToShocking() {
        velocity.x = 0;
        spiderWalkerState = SpiderWalkerState.shocking;
    }
    public void ReleaseShockwave() {
        if (Time.time > endShockwaveCooldownTime) {
			//SwitchToShocking ();
			if (spiderWalkerState == SpiderWalkerState.patrolling) {
				spiderWalkerState = SpiderWalkerState.SwitchingToAlert;
			}
            Instantiate(shockwave, gameObject.transform.position, gameObject.transform.rotation);
            endShockwaveCooldownTime = Time.time + shockwaveCooldown;
        }
    }
    void SwitchToFlying() {
        spiderWalkerState = SpiderWalkerState.flying;
    }
    void SwitchToDying() {
        spiderWalkerState = SpiderWalkerState.dying;
		PlayPowerDown ();
    }
    public void SwitchToDeath_AnimCall () {
        spiderWalkerState = SpiderWalkerState.dead;
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
    RaycastHit2D LookAtPlayerSpiderWalker() {
        RaycastHit2D hit;
        Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 1, 0);
        if (playerTransform != null) {
			if (player.artemisState != Player.ArtemisState.dead) {
				Vector2 target = playerTransform.position;
				Vector2 sightline = target - rayOrigin;
				hit = Physics2D.Raycast (rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

				if (hit && hit.transform.tag == "Player") {
					lastTimePlayerSeen = Time.time;
					lastPlacePlayerSeen = hit.transform;
				} else {
					rayOrigin = bow.transform.position;
					sightline = target - rayOrigin;
					hit = Physics2D.Raycast (rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

					if (hit && hit.transform.tag == "Player") {
						lastTimePlayerSeen = Time.time;
						lastPlacePlayerSeen = hit.transform;
					}
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
            case AnimationState.Idle:
                animator.Play(MechSpiderIdle);
                break;
            case AnimationState.Walking:
                animator.Play(MechSpiderWalk);
                break;
            case AnimationState.Flying:
                animator.Play(MechSpiderFlying);
                break;
            case AnimationState.Firing:
                animator.Play(MechSpiderFiring);
                break;
            case AnimationState.FirePrep:
                animator.Play(MechSpiderSwitchingToFireMode);
                break;
            case AnimationState.FireReady:
                animator.Play(MechSpiderReadyToFire);
                break;
            case AnimationState.FireOff:
                animator.Play(MechSpiderSwitchOffFireMode);
                break;
            case AnimationState.Alert:
                animator.Play(MechSpiderAlert);
                break;
            case AnimationState.SwitchingToAlert:
                animator.Play(MechSpiderSwitchingToAlert);
                break;
            case AnimationState.Shocking:
                animator.Play(MechSpiderShocking);
                break;
            case AnimationState.Death:
                animator.Play(MechSpiderDeath);
                break;
            case AnimationState.Dead:
                animator.Play(MechSpiderDead);
                break;
        }

        this.animationState = state;
        stateStartTime = Time.time;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX, v.y, v.z);

        if (spiderWalkerState == SpiderWalkerState.SwitchingToAlert) {
            SetOrKeepState(AnimationState.SwitchingToAlert);
        }
        else if (spiderWalkerState == SpiderWalkerState.alert) {
            if (Mathf.Abs(velocity.x) > 1) {
                SetOrKeepState(AnimationState.Walking);
            } else {
                SetOrKeepState(AnimationState.Alert);
            } 
        }
        else if (spiderWalkerState == SpiderWalkerState.patrolling) {
            if (Mathf.Abs(velocity.x) > 1) {
                SetOrKeepState(AnimationState.Walking);
            } else {
                SetOrKeepState(AnimationState.Idle);
            }
        } else if (spiderWalkerState == SpiderWalkerState.dying) {
            SetOrKeepState(AnimationState.Death);
        } else if (spiderWalkerState == SpiderWalkerState.dead) {
            SetOrKeepState(AnimationState.Dead);
        } else if (spiderWalkerState == SpiderWalkerState.shocking) {
            SetOrKeepState(AnimationState.Shocking);
        } else if (spiderWalkerState == SpiderWalkerState.firing) {
            if (firingState == FiringState.prepping) {
                SetOrKeepState(AnimationState.FirePrep);
            }
            if (firingState == FiringState.ready) {
                SetOrKeepState(AnimationState.FireReady);
            }
            if (firingState == FiringState.firing) {
                SetOrKeepState(AnimationState.Firing);
            }
            if (firingState == FiringState.turningOff) {
                SetOrKeepState(AnimationState.FireOff);
            }
        }
    }
    #endregion

    public void HitByArrowArmor() {
		PlayArrowHitSound ();

        if (spiderWalkerState != SpiderWalkerState.dying && spiderWalkerState != SpiderWalkerState.dead) {
            ReleaseShockwave();
        }
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
        velocity.x = 0;
        SwitchToDying();
    }

    public void FireArrow() {
        Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        Vector2 firePointPosition = new Vector2(bow.position.x, bow.position.y);
        Vector2 shotline = targetPosition - firePointPosition;
        shotline.Normalize();
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(shotline.y, shotline.x) * Mathf.Rad2Deg);

        arrowClone = Instantiate(arrow, bow.position, rotation);
        arrowClone.arrowState = Arrow.ArrowState.notched;
        arrowClone.FireArrow();
        arrowClone = null;

		PlayFireSound ();
    }

    void UpdateCannonRotation() {
        Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        Vector2 firePointPosition = new Vector2(bow.position.x, bow.position.y);
        Vector2 shotline = targetPosition - firePointPosition;
        shotline.Normalize();
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(shotline.y, shotline.x) * Mathf.Rad2Deg);

        if (faceDirX == -1) {
            rotation *= Quaternion.Euler(0, 0, 180.0f);
        }
        footbone.rotation = rotation;
    }

    void OnDrawGizmos() {
        if (true) {
            Gizmos.color = Color.blue;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMinPatrolRange : localMinPatrolRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.blue;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMaxPatrolRange : localMaxPatrolRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.red;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMinInvestigateRange : localMinInvestigateRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
        if (true) {
            Gizmos.color = Color.red;
            float size = .3f;

            Vector3 globalRangePos = (Application.isPlaying) ? globalMaxInvestigateRange : localMaxInvestigateRange + transform.position;
            Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
            Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);
        }
    }

	void PlayPowerDown() {
		audioManager.PlaySound("PowerDown");
	}
	void PlayArrowHitSound() {
		audioManager.PlaySound ("ArmourHit");
	}
	void PlayFireSound() {
		audioManager.PlaySound ("SpiderFire");
	}
    public void PlayFootstepSound() {
        if (playerTransform != null && ((playerTransform.position - transform.position).magnitude < footstepSound.maxDistance + 5f)) {
            footstepSound.Play();
        }
    }
}
