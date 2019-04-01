using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Text timerText;
    public float runningTime;

    enum TimerState { isRunning, beforeStart, finished }
    TimerState timerState;
    bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        runningTime = 0;
        timerText = gameObject.GetComponent<Text>();
        timerState = TimerState.beforeStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerState == TimerState.isRunning) {
            runningTime += Time.deltaTime;
            timerText.text = runningTime.ToString("0.#");
        } else if (timerState == TimerState.beforeStart) {
            runningTime = 0;
            timerText.text = "";
        }
        
    }

    public void StartTimer() {
        timerState = TimerState.isRunning;
    }

    public void EndTimer() {
        timerState = TimerState.finished;

    }
}
