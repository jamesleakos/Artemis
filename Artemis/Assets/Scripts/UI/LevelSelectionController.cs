using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour {


    // other UI elements
    GameMaster gm;
    public MenuSystem menuSystem;

    Text bestTimeTextLittleGirl;
    Text bestTimeTextDiety;
    LevelScroller levelScroller;

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
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
            levelBlocksScroller = GameObject.FindGameObjectWithTag("LevelBlocksScroller").transform;
        
            startingPos = levelBlocksScroller.position;
            targetXPos = startingPos.x;
            oldXPos = startingPos.x;

            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).name == "BestTimeLittleGirl")
                    bestTimeTextLittleGirl = transform.GetChild(i).GetComponentInChildren<Text>();
                if (transform.GetChild(i).name == "BestTimeDiety")
                    bestTimeTextDiety = transform.GetChild(i).GetComponentInChildren<Text>();
                if (transform.GetChild(i).name == "LevelScroller")
                    levelScroller = transform.GetChild(i).GetComponentInChildren<LevelScroller>();
            }
        }
    }

    void OnEnable() {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
            levelBlocksScroller = GameObject.FindGameObjectWithTag("LevelBlocksScroller").transform;

            startingPos = levelBlocksScroller.position;
            targetXPos = startingPos.x;
            oldXPos = startingPos.x;

            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).name == "BestTimeLittleGirl")
                    bestTimeTextLittleGirl = transform.GetChild(i).GetComponentInChildren<Text>();
                if (transform.GetChild(i).name == "BestTimeDiety")
                    bestTimeTextDiety = transform.GetChild(i).GetComponentInChildren<Text>();
                if (transform.GetChild(i).name == "LevelScroller")
                    levelScroller = transform.GetChild(i).GetComponentInChildren<LevelScroller>();
            }
        }
    }

    void Update() {
        float bestTime = PlayerPrefs.GetFloat("BestTimeLevel" + levelScroller.levelToLoad, 0f);
        if (bestTime == 0f) {
            bestTimeTextLittleGirl.text = "Little Girl: Not Completed";
            bestTimeTextDiety.text = "Diety: Not Completed";
        } else {
            bestTimeTextLittleGirl.text = "Little Girl: " + PlayerPrefs.GetFloat("BestTimeLevel" + levelScroller.levelToLoad, 0f).ToString("0.#");
            bestTimeTextDiety.text = "Diety: " + PlayerPrefs.GetFloat("BestTimeLevel" + levelScroller.levelToLoad, 0f).ToString("0.#");
        }

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
        Vector3 currentPos = levelBlocksScroller.localPosition;
        levelBlocksScroller.localPosition = new Vector3(0,currentPos.y,currentPos.z);
    }
    #endregion
}
