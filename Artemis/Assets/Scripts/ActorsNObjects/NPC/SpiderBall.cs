using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBall : MonoBehaviour
{
    // tracking size
    float targetSize;
    float oldSize;
    float percentBetweenWaypoints;

    // size params
    public float pulseMultiple;

    float startingSize;
    float bigSize;

    // pulse speed params
    public float easeAmount;
    public float zoomSpeed;

    // gameobjects
    SpiderShooter spiderShooter;
    public Shockwave shockwave;
    Shockwave shockwaveClone;

    void Start() {
        spiderShooter = gameObject.transform.root.GetComponent<SpiderShooter>();
        startingSize = gameObject.transform.localScale.x;
        bigSize = startingSize * pulseMultiple;
        targetSize = 0;
    }

    void Update() {

        if (targetSize != gameObject.transform.localScale.x) {
            CalculateZoomMovement();
        } else {
            percentBetweenWaypoints = 0;
            oldSize = gameObject.transform.localScale.x;
            BallBehaviors();
        }
    }

    #region Behaviors
    void BallBehaviors() {
        if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.alertActivating) {
            targetSize = startingSize;
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.alertFiring) {
            if (targetSize == startingSize) {
                targetSize = bigSize;
                oldSize = startingSize;
            } else {
                targetSize = startingSize;
                oldSize = bigSize;
            }
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.alertLoaded) {
            if (targetSize == startingSize) {
                targetSize = bigSize;
                oldSize = startingSize;
            } else {
                targetSize = startingSize;
                oldSize = bigSize;
            }
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.deactivating) {
            targetSize = 0;
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.dead) {
            targetSize = 0;
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.dying) {
            targetSize = 0;
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.inactive) {
            targetSize = 0;
        } else if (spiderShooter.spiderShooterState == SpiderShooter.SpiderShooterState.shocking) {
            targetSize = 0;
        }
    }



    #endregion

    #region Movement Calcs
    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void CalculateZoomMovement() {

        float distanceBetweenWaypoints = Mathf.Abs(targetSize - oldSize);
        percentBetweenWaypoints += Time.deltaTime * zoomSpeed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        float newFloat = Mathf.Lerp(oldSize, targetSize, easedPercentBetweenWaypoints);
        if (float.IsNaN(newFloat)) {
            newFloat = 0f;
        }
        Vector3 newsize = new Vector3(newFloat, newFloat, 1);
        gameObject.transform.localScale = newsize;
    }
    #endregion

    public void ReleaseShockwave() {
        shockwaveClone = Instantiate(shockwave, gameObject.transform.position, gameObject.transform.rotation);
        Vector3 newsize = new Vector3(0, 0, 1);
        gameObject.transform.localScale = newsize;
        targetSize = 0;
    }

}
