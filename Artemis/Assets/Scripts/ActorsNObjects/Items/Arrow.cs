
using UnityEngine;
using System.Collections;

public class Arrow : RaycastController {

    #region Variables 
    // arrow base speed
    public float moveSpeed;
    public float stickDistance = 0.5f;

    public Vector3 scale = new Vector3 (2,0.2f,1);

    public Vector3 velocity;
    public enum ArrowState { notched, flying, stuck }
    public ArrowState arrowState;

    [TagSelector]
    public string[] TagFilterArray = new string[] { };

    public CollisionInfo collisions;
    Transform tip;
    Transform back;
    #region Animation
    Animator animator;
    const string Fade = "Fade";
    #endregion

    #endregion
    //GameMaster gm;
    

    public override void Start() {
        base.Start();
        //gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        animator = gameObject.GetComponent<Animator>();
        tip = gameObject.transform.Find("Tip").GetComponent<Transform>();
        back = gameObject.transform.Find("Back").GetComponent<Transform>();

    }

    void Update() {
        if (arrowState == ArrowState.flying) {
            RaycastCollisionDetection();
            Move(velocity * Time.deltaTime);
        }
    }

    public void FireArrow() {
        arrowState = ArrowState.flying;
        // need to add stuff about rotation direction
        if (gameObject.transform.lossyScale.x < 0) {
            velocity.x = -1 * moveSpeed;
        } else {
            velocity.x = moveSpeed;
        }
        
    }

    public void UpdateRotation(Quaternion rotation) {
        if (arrowState == ArrowState.notched) {
            transform.rotation = rotation;
        }
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;

        if (velocity.x != 0) {
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        transform.Translate(velocity);
    }

    void RaycastCollisionDetection () {
        float rayLength = Mathf.Abs(velocity.x * Time.deltaTime);
        Vector2 rayOrigin = tip.position;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, tip.position - back.position, rayLength, collisionMask);

        Debug.DrawRay(rayOrigin, (tip.position - back.position).normalized * rayLength, Color.red);

        if (hit) {
            if (gameObject.transform.lossyScale.x < 0) {
                velocity.x = -1 * hit.distance / Time.deltaTime;
            } else {
                velocity.x = hit.distance / Time.deltaTime;
            }
            velocity.y = 0;
        } 
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (arrowState == ArrowState.flying) {
            if (System.Array.Exists(TagFilterArray, element => element == collider.tag))
            {
                arrowState = ArrowState.stuck;
                velocity.x = 0;
                velocity.y = 0;
                if (collider.tag == "Deer") {
                    Deer deer = collider.gameObject.GetComponent<Deer>();
                    deer.HitByArrow();
                    animator.Play(Fade);
                }
                if (collider.tag == "SmartRangedEnemy") {
                    SmartRangedEnemy enemy = collider.gameObject.GetComponent<SmartRangedEnemy>();
                    enemy.HitByArrow();
                    animator.Play(Fade);
                }
                if (collider.tag == "Player") {
                    Player player = collider.gameObject.GetComponent<Player>();
                    player.HitByEnemy();
                    animator.Play(Fade);
                }
                if (collider.tag == "Spiderwalker") {
                    Spiderwalker enemy = collider.gameObject.GetComponent<Spiderwalker>();
                    enemy.HitByArrowArmor();
                    animator.Play(Fade);
                }
                if (collider.tag == "SWFoot") {
                    Spiderwalker enemy = collider.transform.root.GetComponent<Spiderwalker>();
                    enemy.HitByArrowArmor();
                    animator.Play(Fade);
                }
                if (collider.tag == "SWViewport") {
                    Spiderwalker enemy = collider.transform.root.GetComponent<Spiderwalker>();
                    enemy.HitByArrowFace();
                    animator.Play(Fade);
                }
                if (collider.tag == "SpiderShooter") {
                    SpiderShooter enemy = collider.transform.root.GetComponent<SpiderShooter>();
                    print(enemy);
                    enemy.HitByArrowArmor();
                    animator.Play(Fade);
                }
                if (collider.tag == "SpiderBall") {
                    SpiderShooter enemy = collider.transform.root.GetComponent<SpiderShooter>();
                    enemy.HitByArrowFace();
                    animator.Play(Fade);
                }
            }
            
        }
    }

    public void DestroyThis() {
        Destroy(gameObject);
    }

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public Vector2 direction;
        public Vector3 velocityOld;
        public int faceDir;

        public void Reset() {
            above = below = false;
            left = right = false;
        }
    }
}