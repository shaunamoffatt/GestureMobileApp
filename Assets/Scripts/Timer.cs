using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 120;
    public bool timerIsRunning = false;
    public float timeForBoss = 60f;
    public TMP_Text timeText;
    public TMP_Text bossGoText;
    public GameObject bossTarget;

    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning && bossTarget != null)
        {
            if (timeRemaining > 0)
            {
                
                timeRemaining -= Time.deltaTime;
               
                DisplayTime(timeRemaining);
                if(timeRemaining <= timeForBoss)
                {
                    Debug.Log("Timefor the boss!");
                    bossTarget.SetActive(true);
                    //Set the text that appears when boss appears
                    //TODO add in some sound
                    bossGoText.transform.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;   
               // LevelController.paused = true;
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}