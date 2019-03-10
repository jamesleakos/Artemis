using UnityEngine;
using System.Collections;

[RequireComponent (typeof(SpriteRenderer))]

public class Tiling : MonoBehaviour {

	public int offsetX = 2;			// the offset so that we don't get any weird errors

	// these are used for checking if we need to instantiate stuff
	public bool hasARightBuddy = false;
	public bool hasALeftBuddy = false;
    public Tiling rightBuddy;
    public Tiling leftBuddy;

	public bool reverseScale = false;	// used if the object is not tilable

	private float spriteWidth = 0f;		// the width of our element
	private Camera cam;
	private Transform myTransform;

	void Awake () {
		cam = Camera.main;
		myTransform = gameObject.transform;
	}

	// Use this for initialization
	void Start () {
        SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
        spriteWidth = Mathf.Abs(sRenderer.bounds.max.x - sRenderer.bounds.min.x);
    }
	
	// Update is called once per frame
	void Update () {

        float camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

        // calculate the x position where the camera can see the edge of the sprite (element)
        float edgePositionRight = (myTransform.position.x + spriteWidth / 2);
        float edgePositionLeft = (myTransform.position.x - spriteWidth / 2);

        // does it still need buddies? If not do nothing
        if (hasALeftBuddy == false || hasARightBuddy == false) {
            // checking if we can see the edge of the element and then calling MakeNewBuddy if we can
            if (cam.transform.position.x + camHorizontalExtend >= edgePositionRight - offsetX && hasARightBuddy == false)
			{
				MakeNewBuddy (1);
				hasARightBuddy = true;
			}
			else if (cam.transform.position.x - camHorizontalExtend <= edgePositionLeft + offsetX && hasALeftBuddy == false)
			{
				MakeNewBuddy (-1);
				hasALeftBuddy = true;
			}
        }

        // is it out of sight range? if so, kill it!
        if (cam.transform.position.x - camHorizontalExtend > edgePositionRight + 2 * offsetX) {
            DestroyTile();
        } else if (cam.transform.position.x + camHorizontalExtend < edgePositionLeft - 2 * offsetX) {
            DestroyTile();
        }


    }

	// a function that creates a buddy on the side required
	void MakeNewBuddy (int rightOrLeft) {
		// calculating the new position for our new buddy
		Vector3 newPosition = new Vector3 (myTransform.position.x + spriteWidth * (float)rightOrLeft, myTransform.position.y, myTransform.position.z);
		// instantating our new body and storing him in a variable
		Transform newBuddy = Instantiate (myTransform, newPosition, myTransform.rotation) as Transform;

		// if not tilable let's reverse the x size og our object to get rid of ugly seams
		if (reverseScale == true) {
			newBuddy.localScale = new Vector3 (newBuddy.localScale.x*-1, newBuddy.localScale.y, newBuddy.localScale.z);
		}

		newBuddy.parent = myTransform.parent;
		if (rightOrLeft > 0) {
			newBuddy.GetComponent<Tiling>().hasALeftBuddy = true;
            newBuddy.GetComponent<Tiling>().leftBuddy = this;
            rightBuddy = newBuddy.GetComponent<Tiling>();
		}
		else {
			newBuddy.GetComponent<Tiling>().hasARightBuddy = true;
            newBuddy.GetComponent<Tiling>().rightBuddy = this;
            leftBuddy = newBuddy.GetComponent<Tiling>();
        }
	}

    void DestroyTile() {
        Destroy(gameObject);
        if (hasALeftBuddy) {
            leftBuddy.hasARightBuddy = false;
        }
        if (hasARightBuddy) {
            rightBuddy.hasALeftBuddy = false;
        }
    }
}
