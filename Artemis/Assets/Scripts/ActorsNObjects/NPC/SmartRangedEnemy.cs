using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]

public class SmartRangedEnemy : MonoBehaviour {
    #region Waypoints
    // Waypoints
    public Node[] patrolNodes;
    public int destPoint;

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
    public float investigateSpeed = 15;
    public float attackSpeed = 15;

    // accerlation times
    public float wallJumpAcceration = 5f;

    public int maxJumps = 2;
    public int jumpsRemaining;


    #endregion

    #region Movement State
    public enum MovementState { aerial, grounded, wallsliding_right, wallsliding_left, wall_jumping, up_jumping, side_jumping, falling_to_target };
    public MovementState movementState;

    enum TargetDirX { inline, right, left };
    enum TargetDirY { inline, up, down };
    TargetDirX targetDirX;
    TargetDirY targetDirY;

    Node lastNode;
    #endregion

    #region Attributes - Health

    // health
    public int health = 2;
    #endregion

    #region Jumping and Falling
    // jumping behaviors
    public float maxJumpHeight = 10;
    public float timeToJumpApex = .4f;
    public float maxFallVelocity = -35;

    public float wallJumpGravity = 130;
    int wallJumpGravityDirection;
    float wallJumpTime;
    public float jumpRange;
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

    #region State and determining state
    // states
    public enum RangedEnemyState { patrolling, alert, investigating, attacking };
    public RangedEnemyState rangedEnemyState;

    bool switchingState;

    //float lastStateTime;
    public float patrolSwitchTime = 1;
    public float alertSwitchTime = 1;
    public float investigateSwitchTime = 0.2f;
    public float attackSwitchTime = 0.2f;

    // tracking when to cast ray
    float NextTimeToRaycast;
    float lookInterval = 0.1f;

    // general sight stuff
    public LayerMask collisionMask;
    public int faceDirX;
    float lastTimePlayerSeen;
    Transform lastPlacePlayerSeen;
    public float attackAttentionSpan = 4;
    public float investigateAttentionSpan = 10;
    float endInvestigationTime;
    public float autoAlertDistance = 10;
    #endregion

    #region Animation
    Animator animator;
    float stateStartTime;
    float timeInState {
        get { return Time.time - stateStartTime; }
    }
    const string MechArcherRun = "MechArcherRun";
    const string MechArcherJump = "MechArcherJump";
    const string MechArcherIdle = "MechArcherIdle";
    const string MechArcherWalk = "MechArcherWalk";
    const string MechArcherFall = "MechArcherFall";
    const string MechArcherWallJump = "MechArcherWallJump";
    const string MechArcherSideJump = "MechArcherSideJump";
    const string MechArcherWallSlide = "MechArcherWallSlide";

    enum AnimationState {
        Idle,
        Run,
        JumpIdle,
        Walk,
        Fall,
        WallJump,
        SideJump,
        WallSlide
    }
    AnimationState animationState;

    #endregion

    #region Private Movement Vars
    [HideInInspector]
    float gravity;
    [HideInInspector]
    float jumpVelocity;

    public float maxVelocityMultiple;
    float maxVelocityX;
    float maxVelocityY;
    [HideInInspector]
    public Vector3 velocity;
    float baseVelocityX;
    

    // smoothing vars
    float targetVelocityX;
    float velocityXSmoothing;
    #endregion

    #region Pathing
    PathManager pather;
    float nextTimeToPath;
    public float pathingInterval;
    public float updateNodeBuffer;
    public Node targetNode;
    #endregion

    Controller2D controller;
    private Collider2D thisCollider;
    Node thisNode;

    void Start() {
        controller = GetComponent<Controller2D>();
        thisCollider = GetComponent<Collider2D>();
        pather = GetComponent<PathManager>();
        animator = GetComponent<Animator>();
        thisNode = GetComponentInChildren<Node>();

        cam = Camera.main;

        bow = gameObject.transform.Find("Bow");
        bow.localScale = new Vector3(1 / gameObject.transform.lossyScale.x, 1 / gameObject.transform.lossyScale.y, 1 / gameObject.transform.lossyScale.z);

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);

        maxVelocityX = investigateSpeed * maxVelocityMultiple;
        maxVelocityY = jumpVelocity * maxVelocityMultiple;

        globalMaxInvestigateRange = localMaxInvestigateRange + transform.position;
        globalMinInvestigateRange = localMinInvestigateRange + transform.position;

        rangedEnemyState = RangedEnemyState.patrolling;
        faceDirX = 1;
    }
    
    void Update() {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Debug.DrawRay(gameObject.transform.position, new Vector3(faceDirX, 0, 0), Color.red);

        if (playerTransform == null) {
            FindPlayer();
            StartCoroutine(SwitchToPatrol());
        }
        if (rangedEnemyState == RangedEnemyState.patrolling) {
            PatrolCasting();
        }
        if (rangedEnemyState == RangedEnemyState.alert) {
            AlertCasting();
        }
        if (rangedEnemyState == RangedEnemyState.investigating) {
            InvestigateCasting();
        }
        if (rangedEnemyState == RangedEnemyState.attacking) {
            AttackCasting();
        }
        PathUpdator();
        Move();

        // Update animation state
        DetermineAnim();
    }

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
                animator.Play(MechArcherIdle);
                break;
            case AnimationState.Run:
                animator.Play(MechArcherRun);
                break;
            case AnimationState.JumpIdle:
                animator.Play(MechArcherJump);
                break;
            case AnimationState.Walk:
                animator.Play(MechArcherWalk);
                break;
            case AnimationState.Fall:
                animator.Play(MechArcherFall);
                break;
            case AnimationState.WallJump:
                animator.Play(MechArcherWallJump);
                break;
            case AnimationState.WallSlide:
                animator.Play(MechArcherWallSlide);
                break;
            case AnimationState.SideJump:
                animator.Play(MechArcherSideJump);
                break;
        }

        this.animationState = state;
        stateStartTime = Time.time;
    }

    void DetermineAnim() {
        Vector3 v = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(Mathf.Abs(v.x) * faceDirX * -1, v.y, v.z);

        if (controller.collisions.below && Mathf.Abs(velocity.x) < 1) SetOrKeepState(AnimationState.Idle);
        else if (movementState == MovementState.falling_to_target) SetOrKeepState(AnimationState.Fall);
        else if (movementState == MovementState.wall_jumping) SetOrKeepState(AnimationState.WallJump);
        else if (movementState == MovementState.wallsliding_left || movementState == MovementState.wallsliding_right) SetOrKeepState(AnimationState.WallSlide);
        else if (controller.collisions.below && rangedEnemyState == RangedEnemyState.patrolling) SetOrKeepState(AnimationState.Walk);
        else if (controller.collisions.below && (rangedEnemyState == RangedEnemyState.investigating || rangedEnemyState == RangedEnemyState.attacking)) SetOrKeepState(AnimationState.Run);
        else if (controller.collisions.below) SetOrKeepState(AnimationState.Run);
        else if (controller.collisions.left || controller.collisions.right) SetOrKeepState(AnimationState.WallSlide);
        else SetOrKeepState(AnimationState.JumpIdle);
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
    RaycastHit2D LookAtPlayer() {
        Vector2 rayOrigin = gameObject.transform.TransformPoint(.5f, 0, 0);
        RaycastHit2D hit;

        if (playerTransform != null) {
            Vector2 target = playerTransform.position;
            Vector2 sightline = target - rayOrigin;
            hit = Physics2D.Raycast(rayOrigin, sightline.normalized, sightline.magnitude, collisionMask);

            Debug.DrawRay(rayOrigin, sightline, Color.red);

            if (hit && hit.transform.tag == "Player") {
                lastTimePlayerSeen = Time.time;
                lastPlacePlayerSeen = hit.transform;
            }

            NextTimeToRaycast = Time.time + lookInterval;
        } else {
            hit = Physics2D.Raycast(rayOrigin, Vector2.up, 0.0001f, collisionMask);
        }
        return hit;
    }

    IEnumerator SwitchToPatrol() {
        if (!switchingState) {
            switchingState = true;
            foreach (Transform child in bow) {
                GameObject.Destroy(child.gameObject);
            }

            yield return new WaitForSeconds(patrolSwitchTime);

            rangedEnemyState = RangedEnemyState.patrolling;
            //lastStateTime = Time.time;
            switchingState = false;
        }
    }
    IEnumerator SwitchToAlert() {
        if (!switchingState) {
            switchingState = true;
            foreach (Transform child in bow) {
                GameObject.Destroy(child.gameObject);
            }

            yield return new WaitForSeconds(alertSwitchTime);

            rangedEnemyState = RangedEnemyState.alert;
            //lastStateTime = Time.time;
            switchingState = false;
        }
    }
    IEnumerator SwitchToInvestigating() {
        if (!switchingState) {
            switchingState = true;

            foreach (Transform child in bow) {
                GameObject.Destroy(child.gameObject);
            }

            yield return new WaitForSeconds(investigateSwitchTime);

            rangedEnemyState = RangedEnemyState.investigating;
            endInvestigationTime = Time.time + investigateAttentionSpan;
            //lastStateTime = Time.time;
            switchingState = false;
        }
    }
    IEnumerator SwitchToAttacking() {
        if (!switchingState) {
            switchingState = true;

            foreach (Transform child in bow) {
                GameObject.Destroy(child.gameObject);
            }
            arrowClone = LoadArrow();

            yield return new WaitForSeconds(attackSwitchTime);

            rangedEnemyState = RangedEnemyState.attacking;

            //lastStateTime = Time.time;
            switchingState = false;
        }
    }

    void PatrolCasting() {
        if (Time.time > NextTimeToRaycast && GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            RaycastHit2D hit = LookAtPlayer();
            if (hit && hit.transform.tag == "Player") {
                if (faceDirX == Mathf.Sign(hit.transform.position.x - gameObject.transform.position.x) || hit.distance < autoAlertDistance) {
                    StartCoroutine(SwitchToAlert());
                }
            }
        }
    }
    void AlertCasting() {
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayer();
                if (hit && hit.transform.tag == "Player") {
                    StartCoroutine(SwitchToAttacking());
                } else {
                    StartCoroutine(SwitchToInvestigating());
                }
            }
        } else {
            StartCoroutine(SwitchToInvestigating());
        }
    }
    void InvestigateCasting() {
        if (GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            if (Time.time > NextTimeToRaycast) {
                RaycastHit2D hit = LookAtPlayer();
                if (hit && hit.transform.tag == "Player") {
                    StartCoroutine(SwitchToAttacking());
                    return;
                }
            }
        }
        //if (Time.time > endInvestigationTime) {
        //    StartCoroutine(SwitchToPatrol());
        //}
    }
    void AttackCasting() {
        if (arrowClone != null) {
            //UpdateArrowRotation(false);
        }
        if (Time.time > endLoadTime && !firingArrow) {
            RaycastHit2D hit = LookAtPlayer();
            if (hit && hit.transform.tag == "Player") {
                faceDirX = (playerTransform.position.x - thisCollider.transform.position.x < 0) ? -1 : 1;
                UpdateArrowRotation(true);
                StartCoroutine(FireArrow());
            } else {
                StartCoroutine(SwitchToInvestigating());
            }
        }
        if (!GeometryUtility.TestPlanesAABB(planes, thisCollider.bounds)) {
            StartCoroutine(SwitchToInvestigating());
        }
    }

    void UpdatePatrolNode() {
        if (patrolNodes.Length == 0) {
            return;
        }
        destPoint = (destPoint + 1) % patrolNodes.Length;
    }
    void PathUpdator() {
        if (Time.time > nextTimeToPath) {
            if (rangedEnemyState == RangedEnemyState.patrolling) {
                pather.NavigateTo(patrolNodes[destPoint].transform.position);
                nextTimeToPath = Time.time + pathingInterval;
            } else {
                if (playerTransform) {
                    if (!(movementState == MovementState.aerial ||
                            movementState == MovementState.side_jumping ||
                            movementState == MovementState.up_jumping ||
                            movementState == MovementState.wall_jumping ||
                            movementState == MovementState.falling_to_target
                        )) {
                        pather.NavigateTo(playerTransform.position);
                        nextTimeToPath = Time.time + pathingInterval;
                    }
                }
            }
        }
    }
    void UpdatePath() {
        if (pather.pathList.Count == 0) {
            return;
        }
        if ((pather.pathList[0].transform.position - thisNode.transform.position).magnitude < updateNodeBuffer) {
            Debug.Log("Close enough");
            if (pather.pathList[0].nodeType == Node.NodeType.air
                    && (movementState == MovementState.aerial || movementState == MovementState.up_jumping || movementState == MovementState.side_jumping || movementState == MovementState.falling_to_target)) {
                Debug.Log("Node type acceptable");
                targetNode = pather.pathList[1];
                AerialMovement(targetNode);
            }
            if (rangedEnemyState == RangedEnemyState.patrolling) {
                pather.pathList.RemoveAt(0);
                if (Mathf.Abs((patrolNodes[destPoint].transform.position - thisCollider.transform.position).magnitude) < updateNodeBuffer) {
                    UpdatePatrolNode();
                    pather.NavigateTo(patrolNodes[destPoint].transform.position);
                    nextTimeToPath = Time.time + pathingInterval;
                }
            } else if (pather.pathList.Count > 1) {
                pather.pathList.RemoveAt(0);
            }
        }
    }

    void Move() {
        CalculateMovementState();

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
        // set base velocity
        baseVelocityX = 0;
        if (rangedEnemyState == RangedEnemyState.patrolling) {
            baseVelocityX = patrolSpeed;
        } else if (rangedEnemyState == RangedEnemyState.investigating || rangedEnemyState == RangedEnemyState.attacking) {
            baseVelocityX = investigateSpeed;
        }
        
        if (pather.pathList.Count > 0) {
            targetNode = pather.pathList[0];
            Vector3 target = targetNode.transform.position;

            #region Setup TargetDirX and TargetDirY
            if ((target.y - thisNode.transform.position.y) > gameObject.transform.lossyScale.y / 2) {
                targetDirY = TargetDirY.up;
            } else if ((target.y - thisNode.transform.position.y) < -1 * gameObject.transform.lossyScale.y / 2) {
                targetDirY = TargetDirY.down;
            } else {
                targetDirY = TargetDirY.inline;
            }
            if ((target.x - gameObject.transform.position.x) > gameObject.transform.lossyScale.x / 2) {
                targetDirX = TargetDirX.right;
            } else if ((target.x - gameObject.transform.position.x) < -1 * gameObject.transform.lossyScale.x / 2) {
                targetDirX = TargetDirX.left;
            } else {
                targetDirX = TargetDirX.inline;
            }
            #endregion

            CalculateMovementType(targetNode);
        }
        #region Smooth, Face Dir, Gravity, and Move
        // face direction
        if (movementState != MovementState.wall_jumping) {
            if (Mathf.Sign(velocity.x) < 0) {
                faceDirX = -1;
            } else if (Mathf.Sign(velocity.x) > 0) {
                faceDirX = 1;
            }
        }
        
        //// add gravity, cap velocities
        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < maxFallVelocity) {
            velocity.y = maxFallVelocity;
        }
        if (velocity.y > maxVelocityY) {
            velocity.y = maxVelocityY;
        }
        if (velocity.x > maxVelocityX) {
            velocity.x = maxVelocityX;
        }

        // move character
        controller.Move(velocity * Time.deltaTime);
        #endregion

        CalculateMovementState();
        UpdatePath();

    }
    #region Move() Helper Functions

    void CalculateMovementState() {
        if (controller.collisions.below) {
            movementState = MovementState.grounded;
            jumpsRemaining = maxJumps;
        } else if (controller.collisions.right) {
            movementState = MovementState.wallsliding_right;
            jumpsRemaining = maxJumps;
        } else if (controller.collisions.left) {
            movementState = MovementState.wallsliding_left;
            jumpsRemaining = maxJumps;
        }
    }

    #region Movement Types - Calculate and Execute

    void CalculateMovementType(Node targetNode) {
        if (movementState == MovementState.grounded) {
            GroundedMovement(targetNode);
            return;
        }
        if (movementState == MovementState.wallsliding_right) {
            WallSlidingRightMovement(targetNode);
            return;
        }
        if (movementState == MovementState.wallsliding_left) {
            WallSlidingLeftMovement(targetNode);
            return;
        }
        //if (movementState == MovementState.aerial || movementState == MovementState.up_jumping || movementState == MovementState.side_jumping || movementState == MovementState.falling_to_target) {
        //    AerialMovement(target, targetNode);
        //    return;
        //}
        if (movementState == MovementState.wall_jumping) {
            WallJumpingMovement(targetNode);
            return;
        }
    }

    void GroundedMovement(Node targetNode) {
        Vector3 target = targetNode.transform.position;
        if (targetNode.nodeType == Node.NodeType.air ||
                pather.pathList[0].nodeType == Node.NodeType.trv_down) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                Vector2 rayOrigin = new Vector2 ((faceDirX == 1) ? thisCollider.bounds.max.x : thisCollider.bounds.min.x, thisCollider.bounds.min.y);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * faceDirX + Vector2.down, 0.1f, collisionMask);

                if (hit) {
                    RunToTarget(target);
                } else {
                    JumpSidewaysToTarget(target);
                }
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.edge_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                Vector2 rayOrigin = new Vector2((faceDirX == 1) ? thisCollider.bounds.max.x : thisCollider.bounds.min.x, thisCollider.bounds.min.y);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * faceDirX + Vector2.down, 0.1f, collisionMask);

                if (hit) {
                    RunToTarget(target);
                } else {
                    JumpSidewaysToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
                }
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.edge_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                Vector2 rayOrigin = new Vector2((faceDirX == 1) ? thisCollider.bounds.max.x : thisCollider.bounds.min.x, thisCollider.bounds.min.y);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * faceDirX + Vector2.down, 0.1f, collisionMask);

                if (hit) {
                    RunToTarget(target);
                } else {
                    JumpSidewaysToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
                }
            }
        }
        if (targetNode.nodeType == Node.NodeType.trv_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                if (targetDirX.Equals(TargetDirX.left)) {
                    JumpUpToTarget(target);
                } else {
                    velocity.x = baseVelocityX;
                }
            } else if (targetDirY.Equals(TargetDirY.down)) {
                if (targetDirX.Equals(TargetDirX.left)) {
                    FallDownToTarget(target);
                } else {
                    velocity.x = baseVelocityX;
                }
            } else {
                Vector2 rayOrigin = new Vector2((faceDirX == 1) ? thisCollider.bounds.max.x : thisCollider.bounds.min.x, thisCollider.bounds.min.y);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * faceDirX + Vector2.down, 0.1f, collisionMask);

                if (hit) {
                    RunToTarget(target);
                } else {
                    JumpSidewaysToTarget(target);
                }
            }
        }
        if (targetNode.nodeType == Node.NodeType.trv_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                if (targetDirX.Equals(TargetDirX.right)) {
                    JumpUpToTarget(target);
                } else {
                    velocity.x = baseVelocityX * -1;
                }
            } else if (targetDirY.Equals(TargetDirY.down)) {
                if (targetDirX.Equals(TargetDirX.right)) {
                    FallDownToTarget(target);
                } else {
                    velocity.x = baseVelocityX * -1;
                }
            } else {
                Vector2 rayOrigin = new Vector2((faceDirX == 1) ? thisCollider.bounds.max.x : thisCollider.bounds.min.x, thisCollider.bounds.min.y);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * faceDirX + Vector2.down, 0.1f, collisionMask);

                if (hit) {
                    RunToTarget(target);
                } else {
                    JumpSidewaysToTarget(target);
                }
            }
        }
    }

    void WallSlidingRightMovement(Node targetNode) {
        Vector3 target = targetNode.transform.position;
        if (targetNode.nodeType == Node.NodeType.air ||
                pather.pathList[0].nodeType == Node.NodeType.trv_down) {
            if (targetDirY.Equals(TargetDirY.up)) {
                //if (!targetDirX.Equals(TargetDirX.left)) {
                //    WallJumpUpToTarget(target, -1);
                //} else {
                JumpUpToTarget(target);
                //}
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
        if (targetNode.nodeType == Node.NodeType.edge_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                WallJumpUpToTarget(target + Vector3.up * targetNode.gameObject.transform.lossyScale.y, -1);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpSidewaysToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            }
        }
        if (targetNode.nodeType == Node.NodeType.edge_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target + Vector3.up * targetNode.gameObject.transform.lossyScale.y);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpSidewaysToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            }
        }
        if (targetNode.nodeType == Node.NodeType.trv_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
        if (targetNode.nodeType == Node.NodeType.trv_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                WallJumpUpToTarget(target, -1);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
    }

    void WallSlidingLeftMovement(Node targetNode) {
        Vector3 target = targetNode.transform.position;
        if (targetNode.nodeType == Node.NodeType.air ||
                pather.pathList[0].nodeType == Node.NodeType.trv_down) {
            if (targetDirY.Equals(TargetDirY.up)) {
                //if (!targetDirX.Equals(TargetDirX.right)) {
                //WallJumpUpToTarget(target, 1);
                //} else {
                JumpUpToTarget(target);
                //}
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.edge_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target + Vector3.up * targetNode.gameObject.transform.lossyScale.y);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpSidewaysToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.edge_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                WallJumpUpToTarget(target + Vector3.up * targetNode.gameObject.transform.lossyScale.y, 1);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpSidewaysToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.trv_left) {
            if (targetDirY.Equals(TargetDirY.up)) {
                WallJumpUpToTarget(target, 1);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
        if (pather.pathList[0].nodeType == Node.NodeType.trv_right) {
            if (targetDirY.Equals(TargetDirY.up)) {
                JumpUpToTarget(target);
            } else if (targetDirY.Equals(TargetDirY.down)) {
                FallDownToTarget(target);
            } else {
                JumpSidewaysToTarget(target);
            }
        }
    }

    void WallJumpingMovement(Node targetNode) {
        Vector3 target = targetNode.transform.position;
        velocity.x += wallJumpGravity * wallJumpGravityDirection * Time.deltaTime;
        if (Time.time > wallJumpTime) {
            movementState = MovementState.aerial;
            velocity.x = velocity.x / 2;
        }
    }

    void AerialMovement(Node targetNode) {
        Vector3 target = targetNode.transform.position;
        if ((target.y - thisNode.transform.position.y) > gameObject.transform.lossyScale.y && jumpsRemaining > 0) {
            Debug.Log("Aerial Movement Function - Jump Up");

            if (targetNode.nodeType == Node.NodeType.edge_right) {
                JumpUpToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            } else if (targetNode.nodeType == Node.NodeType.edge_left) {
                JumpUpToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpUpToTarget(target);
            }
        } else if ((target.y - thisNode.transform.position.y) > -2 * gameObject.transform.lossyScale.y && jumpsRemaining > 0) {
            Debug.Log("Aerial Movement Function - Jump Sideways");

            if (targetNode.nodeType == Node.NodeType.edge_right) {
                JumpSidewaysToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            } else if (targetNode.nodeType == Node.NodeType.edge_left) {
                JumpSidewaysToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                JumpSidewaysToTarget(target);
            }
        } else {
            Debug.Log("Aerial Movement Function - Fall to Target");
            if (targetNode.nodeType == Node.NodeType.edge_right) {
                FallDownToTarget(target + Vector3.left * targetNode.gameObject.transform.lossyScale.x * 2);
            } else if (targetNode.nodeType == Node.NodeType.edge_left) {
                FallDownToTarget(target + Vector3.right * targetNode.gameObject.transform.lossyScale.x * 2);
            } else {
                FallDownToTarget(target);
            }
        }
    }

    #endregion


    void JumpUpToTarget(Vector3 target) {
        if ((target - thisNode.transform.position).magnitude > jumpRange) {
            return;
        }
        velocity.y = Mathf.Sqrt(-2.0f * (target.y - thisNode.transform.position.y + gameObject.transform.lossyScale.y) * gravity);
        float timeToApex = -1.0f * velocity.y / gravity;
        velocity.x = (target.x - thisNode.transform.position.x) / timeToApex;

        movementState = MovementState.up_jumping;
        jumpsRemaining = jumpsRemaining - 1;
    }
    void FallDownToTarget(Vector3 target) {
        // update to include jumping if out of fall range
        float timeToBottom = Mathf.Sqrt(2.0f * (target.y - thisNode.transform.position.y) / gravity);
        velocity.x = (target.x - thisNode.transform.position.x) / timeToBottom;

        movementState = MovementState.falling_to_target;
    }
    void JumpSidewaysToTarget(Vector3 target) {

        velocity.x = baseVelocityX * Mathf.Sign(target.x - thisNode.transform.position.x);
        float timeToTarget = (target.x - thisNode.transform.position.x) / velocity.x;
        velocity.y = ((target.y - thisNode.transform.position.y) - (0.5f) * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        movementState = MovementState.side_jumping;
        jumpsRemaining = jumpsRemaining - 1;
    }
    void WallJumpUpToTarget(Vector3 target, int DirX) {
        if ((target - thisNode.transform.position).magnitude > jumpRange) {
            return;
        }

        velocity.y = Mathf.Sqrt(-2.0f * (target.y - thisNode.transform.position.y + gameObject.transform.lossyScale.y) * gravity);
        float timeToApex = -1.0f * velocity.y / gravity;

        wallJumpGravityDirection = -1 * DirX;

        velocity.x = ((target.x - thisNode.transform.position.x) - (0.5f) * wallJumpGravityDirection * wallJumpGravity * Mathf.Pow(timeToApex, 2)) / timeToApex;
        wallJumpTime = Time.time + timeToApex;

        movementState = MovementState.wall_jumping;
        jumpsRemaining = jumpsRemaining - 1;
    }
    void RunToTarget(Vector3 target) {
        velocity.x = baseVelocityX * Mathf.Sign(target.x - thisNode.transform.position.x);

        movementState = MovementState.aerial;
    }

    #endregion


    void OnDrawGizmos() {
        float size;
        if (patrolNodes.Length > 0) {
            Gizmos.color = Color.red;
            size = 1;

            for (int i = 0; i < patrolNodes.Length; i++) {
                Vector3 globalWaypointPos = patrolNodes[i].transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }

        Gizmos.color = Color.red;
        size = .3f;

        Vector3 globalRangePos = (Application.isPlaying) ? globalMinInvestigateRange : localMinInvestigateRange + transform.position;
        Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);

        globalRangePos = (Application.isPlaying) ? globalMaxInvestigateRange : localMaxInvestigateRange + transform.position;
        Gizmos.DrawLine(globalRangePos - Vector3.up * size, globalRangePos + Vector3.up * size);
        Gizmos.DrawLine(globalRangePos - Vector3.left * size, globalRangePos + Vector3.left * size);

    }

    public void HitByArrow() {
        health = health - 1;
        if (health == 0) {
            GameMaster.KillEnemy(gameObject);
        } else {
            if (rangedEnemyState != RangedEnemyState.attacking) {
                StartCoroutine(SwitchToAlert());
            }
        }
    }
    public Arrow LoadArrow() {
        endLoadTime = Time.time + loadTime;
        Arrow arrowClone = Instantiate(arrow, bow.transform.position, bow.transform.rotation);
        arrowClone.arrowState = Arrow.ArrowState.notched;
        arrowClone.transform.parent = bow;

        return arrowClone;
    }
    IEnumerator FireArrow() {
        if (!firingArrow) {
            firingArrow = true;
            if (arrowClone) {
                arrowClone.FireArrow();
            }
            yield return new WaitForSeconds(fireCoolDownTime);

            if (rangedEnemyState == RangedEnemyState.attacking) {
                arrowClone = LoadArrow();
            }
            firingArrow = false;
        }
    }
    void UpdateArrowRotation(bool smartshooter) {

        Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        Vector2 firePointPosition = new Vector2(arrowClone.transform.position.x, arrowClone.transform.position.y);
        Vector2 shotline = targetPosition - firePointPosition;
        if (smartshooter) {
            float timeToTarget = shotline.magnitude / arrowClone.moveSpeed;
            Vector2 futurePosition = new Vector2(
                playerTransform.position.x + player.velocity.x * timeToTarget,
                playerTransform.position.y + ((player.controller.collisions.above || player.controller.collisions.below) ? 0 : player.velocity.y * timeToTarget + (player.gravity * Mathf.Pow(timeToTarget, 2)) / 2)
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

        arrowClone.UpdateRotation(rotation);
    }
}