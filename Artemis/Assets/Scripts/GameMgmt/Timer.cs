using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Text timerText;
    float runningTime;

    bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        runningTime = 0;
        timerText = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning) {
            runningTime += Time.deltaTime;
        }
        timerText.text = runningTime.ToString();
    }

    public void StartTimer() {
        isRunning = true;
    }

    public void EndTimer() {
        isRunning = false;
    }
}
