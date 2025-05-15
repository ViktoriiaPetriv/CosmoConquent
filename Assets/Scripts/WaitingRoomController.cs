using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

public class WaitingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject waitingScreen;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private GameObject notEnoughPlayersObject;
    [SerializeField] private int playerId;

    [Header("Events")]
    public UnityEvent onTimerComplete;

    private float currentTime;
    private bool isTimerRunning = false;
    private int connectedPlayers = 0;
    private const int REQUIRED_PLAYERS = 2;
    private int gameId = 0;

    private void Awake()
    {
        playerId = PlayerPrefs.GetInt("player_id", 0);
        if (waitingScreen != null)
            waitingScreen.SetActive(false);
    }

    public void ShowWaitingScreen()
    {
        if (waitingScreen != null)
        {
            waitingScreen.SetActive(true);
            StartCoroutine(CheckStartGame());
        }
    }

    public void CheckPlayerCount()
    {
        if (connectedPlayers >= REQUIRED_PLAYERS)
        {
            timerObject.SetActive(true);
            notEnoughPlayersObject.SetActive(true);
            statusText.text = $"Connected: {connectedPlayers}/5";
            statusText.color = Color.green;
        }
        else
        {
            timerObject.SetActive(false);
            notEnoughPlayersObject.SetActive(true);
            statusText.text = "Waiting for players to connect...";
            statusText.color = Color.red;
        }
    }

    private void StartTimer(float startFrom)
    {
        currentTime = startFrom;
        isTimerRunning = true;
        StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        float startTime = Time.realtimeSinceStartup;
        while (currentTime > 0 && isTimerRunning)
        {
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            currentTime = Mathf.Max(0f, currentTime - elapsedTime);
            UpdateTimerDisplay();
            startTime = Time.realtimeSinceStartup;

            yield return null;
        }

        if (isTimerRunning)
        {
            isTimerRunning = false;
            StartCoroutine(SendStartRequest());
        }
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

    public void HideWaitingScreen()
    {
        if (waitingScreen != null)
            waitingScreen.SetActive(false);
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public int id;
        public string username;
    }

    [System.Serializable]
    public class StartGameResponse
    {
        public bool start;
        public int game_id;
        public int players;
        public string start_time;
        public string error;
        public List<PlayerInfo> player_list;
    }

    private IEnumerator SendStartRequest()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", playerId);
        form.AddField("force_start", "true");

        UnityWebRequest www = UnityWebRequest.Post("https://unity-server-sdfo.onrender.com/start_game.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            TimerComplete();
        }
        else
        {
            statusText.text = "Failed to start game.";
            statusText.color = Color.red;
        }
    }

    private IEnumerator CheckStartGame()
    {

        timerObject.SetActive(false);
        notEnoughPlayersObject.SetActive(true);
        bool wasTimerVisible = false;
        statusText.text = "";
        int previousPlayerCount = 0;

        while (true)
        {
            playerId = SessionData.playerId;

            WWWForm form = new WWWForm();
            form.AddField("player_id", playerId);

            UnityWebRequest www = UnityWebRequest.Post("https://unity-server-sdfo.onrender.com/start_game.php", form);
            yield return www.SendWebRequest();

            string responseText = www.downloadHandler.text;

            if (responseText.Contains("Fatal error") || responseText.Contains("<"))
            {
                Debug.LogError("Server returned an error: " + responseText);
                statusText.text = "Server error. Please try again.";
                statusText.color = Color.red;
                yield return new WaitForSeconds(5f);
                continue;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                bool jsonParseSuccess = false;
                StartGameResponse data = null;

                try
                {
                    data = JsonUtility.FromJson<StartGameResponse>(responseText);
                    jsonParseSuccess = true;
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON parsing error: " + e.Message);
                    statusText.text = "Communication error. Retrying...";
                    statusText.color = Color.red;
                }

                if (jsonParseSuccess && data != null)
                {
                    if (!string.IsNullOrEmpty(data.error))
                    {
                        statusText.text = "Error: " + data.error;
                        statusText.color = Color.red;
                    }
                    else
                    {
                        gameId = data.game_id;
                        connectedPlayers = data.players;
                        SessionData.gameId = gameId;

                        // Only update UI when player count changes to prevent flickering
                        if (connectedPlayers != previousPlayerCount)
                        {
                            previousPlayerCount = connectedPlayers;
                            CheckPlayerCount();
                        }

                        if (connectedPlayers >= REQUIRED_PLAYERS)
                        {
                            if (!string.IsNullOrEmpty(data.start_time))
                            {
                                DateTime serverTimeUtc = DateTime.UtcNow;
                                DateTime startTimeUtc = DateTime.Parse(data.start_time).ToUniversalTime();

                                float timeRemaining = (float)(startTimeUtc - serverTimeUtc).TotalSeconds;

                                if (timeRemaining > 0)
                                {
                                    if (!isTimerRunning)
                                    {
                                        StartTimer(timeRemaining);
                                        wasTimerVisible = true;
                                    }
                                }
                                else
                                {
                                    TimerComplete();
                                    break;
                                }
                            }
                        }
                        else if (wasTimerVisible)
                        {
                            // Player count dropped below required while timer was visible
                            wasTimerVisible = false;
                            isTimerRunning = false;
                            CheckPlayerCount();
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Network error: " + www.error);
                statusText.text = "Network error. Retrying...";
                statusText.color = Color.red;
            }

            yield return new WaitForSeconds(5f);
        }
    }
}