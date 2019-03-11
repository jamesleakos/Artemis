using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    // Mostly Public Vars
    #region Movement States
    public enum MovementState {
        aerial,
        grounded,
        wallsliding,
        wall_jumping,
        up_jumping,
        falling,
        air_pause,
        ground_pause,
        attacking,
        wallJumpAway,
        powerDash,
        powerJump,
        powerReady
    };
    public MovementState movementState;
    #endregion

    #region Basic Movement: Speed, Acceleration, JumpHeight

    // character base speed
    public float moveSpeed = 25;
    public float moveSpeedWalk = 5;
    public float moveSpeedBow = 5;

    // accerlation times - only one I want significant is the wall jumping one
    float accelerationTimeAirborne = 0.06f;
    float accelerationTimeGrounded = 0.03f;

    // jumping behaviors
    public float jumpHeight = 7;
    public float timeToJumpApex = .3f;
    #endregion

    #region Wall-Jumping and Sliding
    // wall jumping
    public float climbX = 35;

    float endWallJumpSmoothTime;
    public float wallJumpSmoothTimeLength = 0.7f;
    public float accelerationTimeWallJumping = 0.2f;

    // wallsliding
    bool wallSliding;
    public float wallSlideSpeedMax = 3;
    #endregion

    #region Attacking

    // attacking
    bool attacking;
    public float attackVelocity = 40;
    float endAttackTime;
    public float attackLength = 0.1f;
    #endregion

    #region Air Pause and Shooting
    [HideInInspector]
    public Transform midriffBone;
    // shooting
    public float loadTime = 0.1f;
    public float fireCoolDownTime = 0.3f;
    bool firingArrow;
    bool loadingArrow;

    // Air Pause
    public float airPauseLength = 2;
    float airPauseSpeedMaxUp = 4;
    float airPauseSpeedMaxDown = 1.7f;
    float airPauseGravityReductionUp = 0.06f;
    float airPauseGravityReductionDown = 0.03f;
    float airPauseAcceleration = 0.2f;
    bool airPausing;
    bool loadAnArrow;
    #endregion

    #region Power Movement

    bool powerMoveReady;
    float endPowerReadyTime;
    public float powerMoveReadyLength;
    float velocityForPowerCharge;
    bool powerJumpActive;
    public float powerJumpMultiple = 4;
    float endPowerJumpTime;
    float powerJumpVelocity;
    public float powerJumpLength;
    bool powerDashActive;
    public float powerDashVelocity = 80;
    float endPowerDashTime;
    public float powerDashLength = 0.2f;

    #endregion

    // Private Vars

    #region Private Movement Vars
    [HideInInspector]
    public Vector2 input;
    //[HideInInspector]
    public bool inputOnFinal;
    //[HideInInspector]
    public bool inputOnUIScreen;
    //[HideInInspector]
    public bool inputOnButtonPress;

    // gravity and jumping - dependant on jump height and time to apex
    [HideInInspector]
    public float gravity;
    float jumpVelocity;
    [HideInInspector]
    public Vector3 velocity;

    // smoothing vars
    float targetVelocityX;
    float velocityXSmoothing;
    Vector2 airPauseVelocitySmoothing;
    #endregion

    #region Tolerance Vars: Jump (normal, wall, wall away) - all private

    // jump tolerance
    float endJumpTolerance;
    float jumpToleranceLength = 0.1f;

    // wall jump tolerance
    float endWallJumpTolerance;
    float wallJumpToleranceLength = 0.2f;
    #endregion

    #region Tracking Vars: Jumps, Attacks

    // tracking attacks
    [HideInInspector]
    public int maxAttacks = 1;
    [HideInInspector]
    public int attacksRemaining;

    // tracking jumps
    [HideInInspector]
    public int maxJumps = 2;
    [HideInInspector]
    public int jumpsRemaining;
    #endregion

    #region Misc - to clean at some point
    // shooting
    [HideInInspector]
    public bool bowOut = false;
    float endLoadTime;
    public Arrow arrow;
    Arrow arrowClone;
    Transform bow;

    // air pause and bow work
    [HideInInspector]
    public int maxAirPause = 1;
    [HideInInspector]
    public int airPauseRemaining;
    float endAirPauseTime;

    // tracking directions of last wall and face
    int lastWallDirX;
    int faceDirX;

    // random bools
    bool useJumpAway;
    bool falling;
    bool descendingAtAirPauseStart;
    bool arrowProof = false;
    #endregion

    #region state
    public enum ArtemisState {
        respawning,
        alive,
        dead
    }
    public ArtemisState artemisState;
    #endregion

    #region Animation
    Animator animator;
    float stateStartTime;
    float timeInState {
        get { return Time.time - stateStartTime; }
    }
    bool jumpAnimOn = false;
    const string Idle = "Idle";
    const string Run = "Run";
    const string WallslideIdle = "WallslideIdle";
    const string Jump = "Jump";
    const string JumpIdle = "JumpIdle";
    const string SecondJumpIdle = "SecondJumpIdle";
    const string Attack = "Attack";
    const string ArtemisLoadArrow = "ArtemisLoadArrow";
    const string Death = "Death";
    const string Respawn = "Respawn";
    const string Walk = "Walk";
    const string Crouch = "Crouch";
    const string PowerReady = "PowerReady";
    const string PowerDash = "PowerDash";
    const string PowerJump = "PowerJump";


    public enum AnimationState {
        Idle,
        Jump,
        Run,
        WallslideIdle,
        JumpIdle,
        SecondJumpIdle,
        Attack,
        BowOut,
        Death,
        Respawn,
        Walk,
        Crouch,
        PowerReady,
        PowerDash,
        PowerJump
    }
    public AnimationState animationState;

    #endregion

    #region Sound
    public bool muteSound;
    #endregion

    [HideInInspector]
    public Controller2D controller;
    GameMaster gm;
    AudioManager audioManager;

    // for enforcing singleton
    private static Player _instance;
    public static Player Instance { get { return _instance; } }

    #region Wake, Start, and Update
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        powerJumpVelocity = jumpVelocity * powerJumpMultiple;

        jumpsRemaining = maxJumps;
        attacksRemaining = maxAttacks;

        inputOnButtonPress = true;
        inputOnUIScreen = true;

        endLoadTime = Time.time;

        bow = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Bow");
        bow.localScale = new Vector3(1 / gameObject.transform.lossyScale.x, 1 / gameObject.transform.lossyScale.y, 1 / gameObject.transform.lossyScale.z);

        midriffBone = TransformDeepChildExtension.FindDeepChild(gameObject.transform, "Midriff Bone");

        faceDirX = 1;
        StartCoroutine(LateStart(0.2f));
    }

    IEnumerator LateStart(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        PlayRespawnSound();
    }

    void Update() {
        if (artemisState != ArtemisState.dead && artemisState != ArtemisState.respawning) {
            // moving and general controls
            velocity = CalculateVelocity();
            controller.Move(velocity * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.P)) {
                arrowProof = !arrowProof;
                Debug.Log("Switching Godmode");
            }
        }
        // Update animation state
        DetermineAnim();
        // Update Input State
        DetermineInput();
    }

    void LateUpdate() {
        BowWork();
    }
    #endregion

    #region Determining Input
    void DetermineInput() {
        inputOnFinal = inputOnUIScreen && inputOnButtonPress;
    }
    #endregion

    #region Animation Test
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
                animator.Play(Idle);
                break;
            case AnimationState.Run:
                animator.Play(Run);
                break;
            case AnimationState.JumpIdle:
                animator.Play(JumpIdle);
                break;
            case AnimationState.SecondJumpIdle:
                animator.Play(SecondJumpIdle);
                break;
            case AnimationState.Jump:
                animator.Play(Jump);
                break;
            case AnimationState.WallslideIdle:
                animator.Play(WallslideIdle);
                break;
            case AnimationState.Attack:
                animator.Play(Attack);
                break;
            case AnimationState.BowOut:
                animator.Play(ArtemisLoadArrow);
                break;
            case AnimationState.Death:
                animator.Play(Death);
                break;
            case AnimationState.Respawn:
                animator.Play(Respawn);
                break;
            case AnimationState.Walk:
                animator.Play(Walk);
                break;
            case AnimationState.Crouch:
                animator.Play(Crouch);
                break;
            case AnimationState.PowerDash:
                animator.Play(PowerDash);
                break;
            case AnimationState.PowerJump:
                animator.Play(PowerJump);
                break;
            case AnimationState.PowerReady:
                animator.Play(PowerReady);
                break;
        }

        this.animationState = state;
        stateStartTime = Time.time;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX * -1, v.y, v.z);

        if (artemisState == ArtemisState.dead) {
            SetOrKeepState(AnimationState.Death);
        } else if (artemisState == ArtemisState.respawning) {
            SetOrKeepState(AnimationState.Respawn);
        } else if (Time.time < endAttackTime) SetOrKeepState(AnimationState.Attack);
        else if (bowOut) SetOrKeepState(AnimationState.BowOut);
        else if (powerDashActive) SetOrKeepState(AnimationState.PowerDash);
        else if (powerJumpActive) SetOrKeepState(AnimationState.PowerJump);
        else if (powerMoveReady) SetOrKeepState(AnimationState.PowerReady);
        else if (controller.collisions.below && Mathf.Abs(velocity.x) > 0.5f && Input.GetKey(GameMaster.gm.useItem)) SetOrKeepState(AnimationState.Walk);
        else if (controller.collisions.below && Mathf.Abs(velocity.x) > 0.5f) SetOrKeepState(AnimationState.Run);
        else if (controller.collisions.below && Input.GetKey(GameMaster.gm.useItem)) SetOrKeepState(AnimationState.Crouch);
        else if (controller.collisions.below) SetOrKeepState(AnimationState.Idle);
        else if (wallSliding) SetOrKeepState(AnimationState.WallslideIdle);
        else if (jumpAnimOn) SetOrKeepState(AnimationState.Jump);
        else if (jumpsRemaining > 0) SetOrKeepState(AnimationState.JumpIdle);
        else SetOrKeepState(AnimationState.SecondJumpIdle);
    }
    #endregion

    #region Movement
    Vector3 CalculateVelocity() {

        // get input and directions - do not write code before this
        if (inputOnFinal) {
            //input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(GameMaster.gm.left) && !Input.GetKey(GameMaster.gm.right)) {
                input = Vector2.left;
            } else if (Input.GetKey(GameMaster.gm.right) && !Input.GetKey(GameMaster.gm.left)) {
                input = Vector2.right;
            } else {
                input = new Vector2(0, 0);
            }
        }
        int wallDirX = (controller.collisions.left) ? -1 : 1;
        // face direction 
        if ((int)input.x != 0) {
            faceDirX = (int)input.x;
        }

        // air pause and arrowLoad code-- can put before most other things to subvert unwanted calculation

        if (Input.GetKeyDown(GameMaster.gm.pause)) {
            // Load an arrow upon keyDown
            SetLoadArrow();
        }

        if (Input.GetKey(GameMaster.gm.pause) && airPauseRemaining > 0 && !controller.collisions.below && inputOnFinal) {
            AirPause();
        }
        if (Input.GetKeyUp(GameMaster.gm.pause) || controller.collisions.below || Time.time > endAirPauseTime) {
            airPausing = false;
        }
        if (Time.time > endPowerReadyTime) {
            powerMoveReady = false;
        }

        velocityForPowerCharge = velocity.x;

        if (airPausing) {
            movementState = MovementState.air_pause;
            
            if (descendingAtAirPauseStart) {
                velocity.y += gravity * airPauseGravityReductionDown * Time.deltaTime;
                Vector2 targetAirVelocity = new Vector2(velocity.x, velocity.y).normalized * airPauseSpeedMaxDown;
                velocity = Vector2.SmoothDamp(velocity, targetAirVelocity, ref airPauseVelocitySmoothing, airPauseAcceleration, Mathf.Infinity, Time.deltaTime);
            } else {
                velocity.y += gravity * airPauseGravityReductionUp * Time.deltaTime;
                Vector2 targetAirVelocity = new Vector2(velocity.x, velocity.y).normalized * airPauseSpeedMaxUp;
                velocity = Vector2.SmoothDamp(velocity, targetAirVelocity, ref airPauseVelocitySmoothing, airPauseAcceleration, Mathf.Infinity, Time.deltaTime);
            }
            return velocity;
        }

        // setting base speed - with bow is slower - note this doesn't trigger in air, that was taken care of above
        targetVelocityX = input.x * moveSpeed;
        if (bowOut) {
            targetVelocityX = input.x * moveSpeedBow;
        } else if (Input.GetKey(GameMaster.gm.useItem)) {
            targetVelocityX = input.x * moveSpeedWalk;
        }

        //// smoothing of X acceleration - main value is on the wall jumping
        if (Time.time < endWallJumpSmoothTime &&
            !(controller.collisions.left || controller.collisions.right || controller.collisions.below) &&
            (int)input.x == lastWallDirX
        ) {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeWallJumping);
        } else {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }

        // implement wall sliding via velocity.y and track if wallsliding and whether just left the wall
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && (int)input.x != 0) {
            wallSliding = true;
            movementState = MovementState.wallsliding;
            endWallJumpTolerance = Time.time + wallJumpToleranceLength;
            lastWallDirX = wallDirX;

            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }
        }

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        } else if (!wallSliding) {
            falling = ((velocity.y < 0) ? true : false);
        }

        // jumping and attacking controls

        // reset jumping and attacking if on the ground
        if (wallSliding) {
            attacking = false;
        }
        if (wallSliding || controller.collisions.below) {
            powerMoveReady = false;
            powerJumpActive = false;
            powerDashActive = false;

            jumpsRemaining = maxJumps;
            attacksRemaining = maxAttacks;
            airPauseRemaining = maxAirPause;
        }
        // if jump is pressed, perform the jump and remove a jumpsRemaining
        if ((Input.GetKeyDown(GameMaster.gm.jump) || (Time.time < endJumpTolerance)) && inputOnFinal) {

            if (powerMoveReady) {
                DoPowerJump();
            } else {
                if (jumpsRemaining > 0) {
                    if (wallSliding) {
                        movementState = MovementState.wall_jumping;
                        velocity.x = -lastWallDirX * climbX;
                        ArtemisJump();

                        // track time since jumped for smoothing
                        endWallJumpSmoothTime = Time.time + wallJumpSmoothTimeLength;
                    } else {
                        movementState = MovementState.up_jumping;
                        ArtemisJump();
                    }
                } else if (Input.GetKeyDown(GameMaster.gm.jump)) {
                    endJumpTolerance = Time.time + jumpToleranceLength;
                }
            }            
        }

        // if attack key is pressed, perform the attack and remove a attacksRemaining
        if (Input.GetKeyDown(GameMaster.gm.fireDash) && !bowOut && inputOnFinal) {
            if (powerMoveReady) {
                DoPowerDash();
            } else {
                DashAttack();
            }
        }
        if (Time.time > endAttackTime || powerMoveReady) {
            attacking = false;
        }
        if (attacking) {
            movementState = MovementState.attacking;
            velocity.y = 0;
            velocity.x = attackVelocity * faceDirX;
            return velocity;
        }
        if (Time.time > endPowerDashTime) {
            powerDashActive = false;
        }
        if (Time.time > endPowerJumpTime) {
            powerJumpActive = false;
        }
        if (powerDashActive) {
            movementState = MovementState.powerDash;
            velocity.y = 0;
            velocity.x = powerDashVelocity * faceDirX;
            return velocity;
        }
        if (powerJumpActive) {
            movementState = MovementState.powerJump;
            //velocity.x = 0;
        }

        if (powerMoveReady) {
            movementState = MovementState.powerReady;

            if (descendingAtAirPauseStart) {
                velocity.y += gravity * airPauseGravityReductionDown * Time.deltaTime;
                Vector2 targetAirVelocity = new Vector2(velocityForPowerCharge, velocity.y).normalized * airPauseSpeedMaxDown;
                Vector2 newV = new Vector2(velocityForPowerCharge, velocity.y);
                velocity = Vector2.SmoothDamp(newV, targetAirVelocity, ref airPauseVelocitySmoothing, airPauseAcceleration, Mathf.Infinity, Time.deltaTime);
            } else {
                velocity.y += gravity * airPauseGravityReductionUp * Time.deltaTime;
                Vector2 targetAirVelocity = new Vector2(velocityForPowerCharge, velocity.y).normalized * airPauseSpeedMaxUp;
                Vector2 newV = new Vector2(velocityForPowerCharge, velocity.y);
                velocity = Vector2.SmoothDamp(newV, targetAirVelocity, ref airPauseVelocitySmoothing, airPauseAcceleration, Mathf.Infinity, Time.deltaTime);
            }

            return velocity;
        }

        // add gravity
        velocity.y += gravity * Time.deltaTime;

        // return
        return velocity;
    }
    void ArtemisJump() {
        jumpsRemaining--;
        velocity.y = jumpVelocity;
        if (jumpsRemaining == 0) jumpAnimOn = true;
        PlayJumpSound();
    }
    public void TurnJumpAnimOff() {
        jumpAnimOn = false;
    }

    void DashAttack() {
        if (attacksRemaining > 0) {
            attacking = true;
            PlayDashSound();
            endAttackTime = Time.time + attackLength;
            attacksRemaining--;
        }
    }

    void DoPowerDash() {
        powerMoveReady = false;
        powerDashActive = true;
        PlayDashSound();
        endPowerDashTime = Time.time + powerDashLength;
    }

    void DoPowerJump() {
        powerMoveReady = false;
        powerJumpActive = true;
        velocity.y = powerJumpVelocity;
        PlayJumpSound();
        endPowerJumpTime = Time.time + powerJumpLength;
    }


    void AirPause() {
        powerMoveReady = false;
        airPausing = true;
        airPauseRemaining--;
        endAirPauseTime = Time.time + airPauseLength;
        descendingAtAirPauseStart = ((velocity.y < 0) ? true : false);
    }

    public void SetPowerMoveReady () {
        powerMoveReady = true;
        endPowerReadyTime = Time.time + powerMoveReadyLength;
    }
    #endregion

    #region Bow and Arrow Work
    void BowWork() {
        bowOut = false;
        if (loadAnArrow == true) {
            LoadArrow();
        }
        if (Input.GetKey(GameMaster.gm.pause) && inputOnFinal) {
            if (controller.collisions.below) {
                bowOut = true;
            }
            if (!controller.collisions.below && Time.time < endAirPauseTime) {
                bowOut = true;
            }
        }
        if (bowOut) {
            UpdateArrowRotation();
            if (arrowClone != null) {
                if (Input.GetKeyDown(GameMaster.gm.fireDash)) {
                    FireArrow();
                }
            }
        } else {
            foreach (Transform child in bow) {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    void SetLoadArrow() {
        loadAnArrow = true;
    }

    void LoadArrow() {
        loadAnArrow = false;
        foreach (Transform child in bow) {
            GameObject.Destroy(child.gameObject);
        }
        arrowClone = Instantiate(arrow, bow.transform.position, (faceDirX == 1) ? bow.transform.rotation * Quaternion.Euler(0, 180, 0) : bow.transform.rotation);

        arrowClone.arrowState = Arrow.ArrowState.notched;
        arrowClone.transform.parent = bow;
    }

    void FireArrow() {
        PlayArrowReleaseSound();

        faceDirX = ((gameObject.transform.position.x - Camera.main.ScreenToWorldPoint(Input.mousePosition).x) <= 0) ? 1 : -1;
        arrowClone.gameObject.transform.parent = null;
        arrowClone.FireArrow();
        arrowClone = null;
    }

    void UpdateArrowRotation() {
        faceDirX = ((gameObject.transform.position.x - Camera.main.ScreenToWorldPoint(Input.mousePosition).x) <= 0) ? 1 : -1;
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX * -1, v.y, v.z);

        Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 firePointPosition = new Vector2(bow.transform.position.x, bow.transform.position.y);
        Vector2 dir = mousePosition - firePointPosition;
        dir.Normalize();
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        rotation *= Quaternion.Euler(0, 0, -90); // this adds a 90 degrees Y rotation
        midriffBone.rotation = rotation;
    }
    #endregion

    #region Dying and Living
    public void HitByEnemy() {
        if (!arrowProof && artemisState != ArtemisState.dead) {
            artemisState = ArtemisState.dead;
            PlayDeathSound();
        }
    }
    public void KillPlayer() {
        gm.KillPlayer(this);
    }
    public void RevivePlayer() {
        artemisState = ArtemisState.alive;
    }
    #endregion

    #region Make Sounds
    void PlayFootstepSound() {

    }
    void PlayArrowReleaseSound() {
        if (!muteSound) {
            audioManager.PlaySound("ArrowRelease");
        }
    }
    void PlayJumpSound() {
        if (!muteSound) {
            audioManager.PlaySound("Jump");
        }
    }
    void PlayDashSound() {
        if (!muteSound) {
            audioManager.PlaySound("DashAttack");
        }
    }
    void PlayDeathSound() {
        if (!muteSound) {
            audioManager.PlaySound("ArtemisDeath");
        }
    }
    void PlayRespawnSound() {
        if (!muteSound) {
            audioManager.PlaySound("ArtemisRespawn");
        }
    }
    #endregion

}