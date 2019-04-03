using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelScroller : MonoBehaviour {

    #region UI elements
    // other UI elements
    GameMaster gm;
    public MenuSystem menuSystem;
    public RectTransform scrollerBox;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    #endregion

    #region Level Ints
    // level ints
    public int minLevel;
    public int maxLevel;
    int levelToLoad;
    #endregion

    #region Movement Params
    // movement parameters
    public bool simpleMovement = true;
    public float easeAmount = 0.5f;
    public float moveSpeed = 500f;
    // private movement vars
    Vector3 startingPos;
    Transform targetTransform;
    float distanceBetweenLevelBoxes = 1400;
    float percentBetweenWaypoints;
    float targetXPos;
    float oldXPos;
    int movesToMake = 0;
    #endregion

    #region Start and Update
    void Start() {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        minLevel = minLevel + gm.levelPaddingBesidesMain;
        maxLevel = maxLevel + gm.levelPaddingBesidesMain;
        levelToLoad = minLevel;

        CheckSideButtons();
        startingPos = gameObject.transform.position;
        targetXPos = startingPos.x;
        oldXPos = startingPos.x;
        targetTransform = gameObject.transform;
    }

    void Update() {
        if (!simpleMovement) {
            if (targetXPos != targetTransform.position.x) {
                Move();
            } else {
                oldXPos = targetTransform.position.x;
                percentBetweenWaypoints = 0;
            }
        }
    }
    #endregion

    #region Useful Functions
    void CheckSideButtons() {
        if (levelToLoad < minLevel) {
            rightArrow.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
        }

        if (levelToLoad == minLevel) {
            leftArrow.gameObject.SetActive(false);
        } else {
            leftArrow.gameObject.SetActive(true);
        }

        if (levelToLoad == maxLevel) {
            rightArrow.gameObject.SetActive(false);
        } else {
            rightArrow.gameObject.SetActive(true);
        }
    }

    public void PlayLevel() {
        menuSystem.setLevelToLoad(levelToLoad);
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

        targetTransform.position = new Vector3(newPos, startingPos.y, startingPos.z);
    }

    public void MoveRightNew() {
        targetXPos -= distanceBetweenLevelBoxes;

        levelToLoad += 1;
        CheckSideButtons();
    }

    public void MoveLeftNew() {
        targetXPos += distanceBetweenLevelBoxes;

        levelToLoad -= 1;
        CheckSideButtons();
    }
    #endregion

    #region Old Movement Functions
    public void MoveRight() {
        Vector3 scrollerX = scrollerBox.localPosition;
        scrollerX.x = scrollerX.x - distanceBetweenLevelBoxes;
        scrollerBox.localPosition = scrollerX;

        levelToLoad += 1;
        CheckSideButtons();
    }

    public void MoveLeft() {
        Vector3 scrollerX = scrollerBox.localPosition;
        scrollerX.x = scrollerX.x + distanceBetweenLevelBoxes;
        scrollerBox.localPosition = scrollerX;

        levelToLoad -= 1;
        CheckSideButtons();
    }
    #endregion
}
