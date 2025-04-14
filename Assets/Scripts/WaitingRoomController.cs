using UnityEngine;
using TMPro;
using System.Collections;

public class WaitingRoomManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject registerPanel;
    public GameObject waitingPanel;

    [Header("UI Texts")]
    public TMP_Text timerText;
    public TMP_Text statusText;

    [Header("Game Settings")]
    public int requiredPlayers = 2;
    public float waitTime = 180f; 

    private int currentPlayers = 0;
    private Vector3 originalScale;

    private void Start()
    {
        if (waitingPanel == null)
            waitingPanel = GameObject.Find("WaitingWindow");

        if (registerPanel == null)
            registerPanel = GameObject.Find("RegisterWindow");

        if (timerText == null)
            timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();

        if (statusText == null)
            statusText = GameObject.Find("StatusText").GetComponent<TMP_Text>();

        if (timerText != null)
            originalScale = timerText.transform.localScale;

        waitingPanel.SetActive(false);
    }

    public void OnPlayerRegistered()
    {
        currentPlayers++;

        if (!registerPanel.activeSelf && currentPlayers >= requiredPlayers)
        {
            StartCoroutine(StartCountdown());
        }
        else
        {
            ShowWaitingPanel("Not enough players");
        }
    }

    public void OnRegisterPanelClosed()
    {
        registerPanel.SetActive(false);

        if (currentPlayers >= requiredPlayers)
        {
            StartCoroutine(StartCountdown());
        }
        else
        {
            ShowWaitingPanel("Not enough players");
        }
    }

    private void ShowWaitingPanel(string message)
    {
        waitingPanel.SetActive(true);
        statusText.text = message;
        timerText.text = "";
    }

    private IEnumerator StartCountdown()
    {
        waitingPanel.SetActive(true);
        statusText.text = "Waiting for players...";

        float timeRemaining = waitTime;

        while (timeRemaining > 0)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            AnimateTimer();
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        waitingPanel.SetActive(false);
        StartGame();
    }

    private void AnimateTimer()
    {
        if (timerText != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.05f;
            timerText.transform.localScale = originalScale * pulse;
        }
    }

    private void StartGame()
    {
        Debug.Log("Game is starting!");
    }
}
