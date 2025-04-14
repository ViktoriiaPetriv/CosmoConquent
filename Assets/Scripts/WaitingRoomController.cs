using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class WaitingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject waitingScreen;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private GameObject notEnoughPlayersObject;

    [Header("Timer Settings")]
    [SerializeField] private float waitingTime = 180f;

    [Header("Events")]
    public UnityEvent onTimerComplete;

    private float currentTime;
    private bool isTimerRunning = false;
    private int connectedPlayers = 0;
    private const int REQUIRED_PLAYERS = 2;

    private void Awake()
    {
        if (waitingScreen != null)
            waitingScreen.SetActive(false);
    }

    public void ShowWaitingScreen()
    {
        if (waitingScreen != null)
        {
            waitingScreen.SetActive(true);

            connectedPlayers++;

            CheckPlayerCount();
        }
    }

    public void CheckPlayerCount()
    {
        if (connectedPlayers >= REQUIRED_PLAYERS)
        {
            timerObject.SetActive(true);
            notEnoughPlayersObject.SetActive(false);

            if (!isTimerRunning)
            {
                StartTimer();
            }
        }
        else
        {
            timerObject.SetActive(false);
            notEnoughPlayersObject.SetActive(true);
            statusText.text = "Waiting for players to connect...";
        }
    }

    public void PlayerConnected()
    {
        connectedPlayers++;
        CheckPlayerCount();
    }
    public void TestAddPlayer()
    {
        PlayerConnected();
    }
    public void PlayerDisconnected()
    {
        if (connectedPlayers > 0)
            connectedPlayers--;

        CheckPlayerCount();

        if (connectedPlayers < REQUIRED_PLAYERS && isTimerRunning)
        {
            StopTimer();
        }
    }

    private void StartTimer()
    {
        currentTime = waitingTime;
        isTimerRunning = true;
        StartCoroutine(CountdownTimer());
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        StopAllCoroutines();
    }

    private IEnumerator CountdownTimer()
    {
        while (currentTime > 0 && isTimerRunning)
        {
            UpdateTimerDisplay();

            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        if (isTimerRunning)
        {
            TimerComplete();
        }
    }
    public void HideWaitingScreen()
    {
        if (waitingScreen != null)
            waitingScreen.SetActive(false);
    }
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void TimerComplete()
    {
        isTimerRunning = false;
        waitingScreen.SetActive(false);
        onTimerComplete?.Invoke();
    }

    public void SimulateRegistration()
    {
        ShowWaitingScreen();
    }
}