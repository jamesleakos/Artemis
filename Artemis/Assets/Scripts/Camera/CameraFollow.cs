using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    Controller2D target;
    public Player targetPlayer;
    public float verticalOffset;
    public float lookAheadDstX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;
    public Vector2 focusAreaSize;

    FocusArea focusArea;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;

    float smoothLookVelocityX;
    float smoothVelocityY;

    // test start
    public bool simpleFocusY;
    float currentLookAheadY;
    float targetLookAheadY;
    float lookAheadDirY;
    bool lookAheadStoppedY;
    public float lookAheadDstY;
    float smoothLookVelocityY;
    public float lookSmoothTimeY;
    // test end

    bool lookAheadStopped;

    float nextTimeToSearch;

	GameMaster gm;

    void Start() {
		if (targetPlayer == null) {
			FindPlayer();
		}
        target = targetPlayer.GetComponent<Controller2D>();
        focusArea = new FocusArea(target.collider_jl.bounds, focusAreaSize);
		gm = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();

    }

    void LateUpdate() {

		if (targetPlayer == null) {
			FindPlayer ();
		} else {
			focusArea.Update(target.collider_jl.bounds);

			Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

			if (focusArea.velocity.x != 0) {
				lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
				if (Mathf.Sign(targetPlayer.input.x) == Mathf.Sign(focusArea.velocity.x) && targetPlayer.input.x != 0) {
					lookAheadStopped = false;
					targetLookAheadX = lookAheadDirX * lookAheadDstX;
				} else {
					if (!lookAheadStopped) {
						lookAheadStopped = true;
						targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
					}
				}
			}

			currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

			if (simpleFocusY) {
				focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
			} else {
				// this isn't great. might want to consider nixing except in cases of fast falling
				if (focusArea.velocity.y != 0) {
					lookAheadDirY = Mathf.Sign(focusArea.velocity.y);
					if (Mathf.Sign(targetPlayer.velocity.y) == Mathf.Sign(focusArea.velocity.y) && targetPlayer.velocity.y != 0) {
						lookAheadStoppedY = false;
						targetLookAheadY = lookAheadDirY * lookAheadDstY;
					} else {
						if (!lookAheadStoppedY) {
							lookAheadStoppedY = true;
							targetLookAheadY = currentLookAheadY + (lookAheadDirY * lookAheadDstY - currentLookAheadY) / 4f;
						}
					}
				}
				currentLookAheadY = Mathf.SmoothDamp(currentLookAheadY, targetLookAheadY, ref smoothLookVelocityY, lookSmoothTimeY);

				focusPosition += Vector2.up * currentLookAheadY;
			}

			focusPosition += Vector2.right * currentLookAheadX;
			transform.position = (Vector3)focusPosition + Vector3.forward * -10;
		}
    }

    void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
            if (searchResult != null) {
                targetPlayer = searchResult.GetComponent<Player>();
                target = targetPlayer.GetComponent<Controller2D>();
            }
            nextTimeToSearch = Time.time + 0.5f;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(0, 1, 0, .25f);
        Gizmos.DrawCube(focusArea.centre, focusAreaSize);
    }

    struct FocusArea {
        public Vector2 centre;
        public Vector2 velocity;
        float left, right;
        float top, bottom;


        public FocusArea(Bounds targetBounds, Vector2 size) {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void Update(Bounds targetBounds) {
            float shiftX = 0;
            if (targetBounds.min.x < left) {
                shiftX = targetBounds.min.x - left;
            } else if (targetBounds.max.x > right) {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom) {
                shiftY = targetBounds.min.y - bottom;
            } else if (targetBounds.max.y > top) {
                shiftY = targetBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}