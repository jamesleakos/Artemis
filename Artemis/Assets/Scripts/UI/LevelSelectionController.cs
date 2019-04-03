using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour {

    #region UI elements
    // other UI elements
    GameMaster gm;
    public MenuSystem menuSystem;
    #endregion

    #region Movement Params
    // movement parameters
    public bool simpleMovement = true;
    public float easeAmount = 0.5f;
    public float moveSpeed = 500f;
    // private movement vars
    Vector3 startingPos;
    public float distanceBetweenLevelBoxes;
    float percentBetweenWaypoints;
    float targetXPos;
    float oldXPos;
    int movesToMake = 0;
    #endregion

    Transform levelBlocksScroller;

    #region Start and Update
    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            levelBlocksScroller = GameObject.FindGameObjectWithTag("LevelBlocksScroller").transform;
        }
        startingPos = levelBlocksScroller.position;
        targetXPos = startingPos.x;
        oldXPos = startingPos.x;
    }

    void Update() {
        if (!simpleMovement) {
            if (targetXPos != levelBlocksScroller.position.x) {
                Move();
            } else {
                oldXPos = levelBlocksScroller.position.x;
                percentBetweenWaypoints = 0;
            }
        }
    }
    #endregion

    #region Fancy Movement Functions
    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void Move() {

        float distanceBetweenWaypoints = Mathf.Abs(targetXPos - oldXPos);
        percentBetweenWaypoints += Time.deltaTime * moveSpeed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        float newPos = Mathf.Lerp(oldXPos, targetXPos, easedPercentBetweenWaypoints);

        levelBlocksScroller.position = new Vector3(newPos, startingPos.y, startingPos.z);
    }

    public void MoveRightNew() {
        targetXPos -= distanceBetweenLevelBoxes;
    }

    public void MoveLeftNew() {
        targetXPos += distanceBetweenLevelBoxes;
    }
    #endregion

    #region Old Movement Functions
    public void MoveRight() {
        Vector3 scrollerX = levelBlocksScroller.position;
        scrollerX.x = scrollerX.x - distanceBetweenLevelBoxes;
        levelBlocksScroller.position = scrollerX;
    }

    public void MoveLeft() {
        Vector3 scrollerX = levelBlocksScroller.position;
        scrollerX.x = scrollerX.x + distanceBetweenLevelBoxes;
        levelBlocksScroller.position = scrollerX;
    }

    public void ResetPosition() {
        levelBlocksScroller.position = startingPos;
    }
    #endregion
}
