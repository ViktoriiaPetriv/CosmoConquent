using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreCalc : MonoBehaviour
{
    private int gameId;
    private int localPlayerId;

    public Transform resultsTableParent;
    public GameObject resultRowPrefab;
    private List<PlayerData> players;
    private bool calculationDone = false;

    private List<int> winningTeamsGlobal;

    [Header("UI Elements")]
    [SerializeField] private ResultsWindow myResultsWindow;
    [SerializeField] private Button menuButton; // нове поле для кнопки menu

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Planets")]
    public List<GameObject> planetObjects;

    [Header("Effects")]
    public GameObject planetWinEffectPrefab;

    public void Start()
    {
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
            menuButton.gameObject.SetActive(false); // сховати кнопку при старті
        }
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void BeginCheckingMoves()
    {
        localPlayerId = SessionData.playerId;
        gameId = SessionData.gameId;
        if (gameId != -1)
        {
            StartCoroutine(CheckAllPlayersMovesRoutine(gameId));
        }
        else
        {
            Debug.LogError("Game ID is not found in PlayerPrefs.");
        }
    }

    private IEnumerator CheckAllPlayersMovesRoutine(int gameId)
    {
        while (true)
        {
            yield return StartCoroutine(GetPlayersDataFromServer(gameId));

            if (calculationDone)
                break;

            yield return new WaitForSeconds(5f);
        }
    }

    public IEnumerator GetPlayersDataFromServer(int gameId)
    {
        string url = $"https://unity-server-sdfo.onrender.com/get_results.php?game_id={gameId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                PlayerDataList playerDataList = JsonUtility.FromJson<PlayerDataList>("{\"players\":" + jsonResponse + "}");
                players = new List<PlayerData>(playerDataList.players);

                bool allPlayersSubmitted = players.All(p =>
                    (p.kronus + p.lyrion + p.mystara + p.eclipsia + p.fiora) > 0
                );

                if (allPlayersSubmitted && !calculationDone)
                {
                    CalculateTeamScores(players);
                    calculationDone = true;
                }
                else if (!allPlayersSubmitted)
                {
                    Debug.Log("Expecting other players...");
                }
            }
        }
    }

    [Serializable]
    public class PlayerData
    {
        public int player_id;
        public string username;
        public int kronus;
        public int lyrion;
        public int mystara;
        public int eclipsia;
        public int fiora;
        public int score;
    }

    [Serializable]
    public class PlayerDataList
    {
        public PlayerData[] players;
    }

    [Serializable]
    public class PlayerDataListWrapper
    {
        public List<PlayerData> players;
    }

    public void CalculateTeamScores(List<PlayerData> players)
    {
        int teamCount = players.Count;
        int[] teamScores = new int[teamCount];
        List<int[]> teamDroneDistributions = new List<int[]>();

        foreach (var player in players)
        {
            teamDroneDistributions.Add(new int[] { player.kronus, player.lyrion, player.mystara, player.eclipsia, player.fiora });
        }

        string[] planets = { "Kronus", "Lyrion", "Mystara", "Eclipsia", "Fiora" };

        for (int firstTeam = 0; firstTeam < teamCount - 1; firstTeam++)
        {
            for (int secondTeam = firstTeam + 1; secondTeam < teamCount; secondTeam++)
            {
                int firstTeamPlanetWins = 0;
                int secondTeamPlanetWins = 0;

                for (int planetIndex = 0; planetIndex < planets.Length; planetIndex++)
                {
                    int firstTeamDrones = teamDroneDistributions[firstTeam][planetIndex];
                    int secondTeamDrones = teamDroneDistributions[secondTeam][planetIndex];

                    if (firstTeamDrones > secondTeamDrones)
                        firstTeamPlanetWins++;
                    else if (secondTeamDrones > firstTeamDrones)
                        secondTeamPlanetWins++;
                }

                if (firstTeamPlanetWins > secondTeamPlanetWins)
                {
                    teamScores[firstTeam] += 2;
                    teamScores[secondTeam] += 0;
                }
                else if (firstTeamPlanetWins < secondTeamPlanetWins)
                {
                    teamScores[firstTeam] += 0;
                    teamScores[secondTeam] += 2;
                }
                else
                {
                    teamScores[firstTeam] += 1;
                    teamScores[secondTeam] += 1;
                }
            }
        }

        int maxScore = teamScores.Max();
        List<int> winningTeams = new List<int>();

        for (int team = 0; team < teamCount; team++)
        {
            if (teamScores[team] == maxScore)
            {
                winningTeams.Add(team);
            }

            Debug.Log($"Player {players[team].username} total score: {teamScores[team]}");
        }

        List<string> winningPlayerNames = winningTeams.Select(index => players[index].username).ToList();
        Debug.Log($"Player(s) with the highest score: {string.Join(", ", winningPlayerNames)} with score: {maxScore}");
        StartCoroutine(UpdatePlayerScoresOnServer(players, teamScores));
        PopulateResultsTable(players, teamScores);
        winningTeamsGlobal = winningTeams;
        OpenResultsWindow(teamScores, winningTeams);
    }

    private void OpenResultsWindow(int[] scores, List<int> winningTeams)
    {
        if (menuButton != null)
            menuButton.gameObject.SetActive(false);
        string winnerText;

        List<string> winningPlayerNames = winningTeams.Select(index => players[index].username).ToList();

        if (winningPlayerNames.Count > 1)
        {
            winnerText = "Players " + string.Join(", ", winningPlayerNames) + " Win!";
        }
        else
        {
            winnerText = $"Player {winningPlayerNames[0]} Wins!";
        }

        myResultsWindow.gameObject.SetActive(true);
        myResultsWindow.CloseButton.onClick.AddListener(CloseClicked);
        myResultsWindow.winnerText.text = winnerText;

        StartCoroutine(OpenWindowAnimation());
        StartCoroutine(AnimateWinnerText());
    }

    private void ShowWinningPlanetsEffectsForPlayer(int playerIndex)
    {
        int[] drones = new int[]
        {
            players[playerIndex].kronus,
            players[playerIndex].lyrion,
            players[playerIndex].mystara,
            players[playerIndex].eclipsia,
            players[playerIndex].fiora
        };

        for (int i = 0; i < drones.Length; i++)
        {
            if (drones[i] > 0 && i < planetObjects.Count)
            {
                GameObject effect = Instantiate(planetWinEffectPrefab, 
                    planetObjects[i].transform.position, Quaternion.identity);
            }
        }
    }

    private void CloseClicked()
    {
        if (menuButton != null)
            menuButton.gameObject.SetActive(true);
        myResultsWindow.gameObject.SetActive(false);
        PlayClickSound();

       foreach (int winnerIndex in winningTeamsGlobal)
        {
            if (players[winnerIndex].player_id == localPlayerId)
            {
                ShowWinningPlanetsEffectsForPlayer(winnerIndex);
                break;
            }
        }

    }

    private void OnMenuClicked()
    {
        PlayClickSound();
        SceneManager.LoadScene("MainMenu");
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private IEnumerator AnimateWinnerText()
    {
        Vector3 originalScale = myResultsWindow.winnerText.transform.localScale;
        Color originalColor = myResultsWindow.winnerText.color;

        while (myResultsWindow.gameObject.activeSelf)
        {
            float scaleFactor = Mathf.PingPong(Time.time * 0.2f, 0.1f) + 1f;
            myResultsWindow.winnerText.transform.localScale = originalScale * scaleFactor;

            float colorValue = Mathf.PingPong(Time.time * 1.5f, 1f);
            Color color = Color.Lerp(Color.green, Color.yellow, colorValue);
            myResultsWindow.winnerText.color = new Color(color.r, color.g, color.b, originalColor.a);

            yield return null;
        }
    }

    private IEnumerator OpenWindowAnimation()
    {
        Vector3 originalScale = myResultsWindow.transform.localScale;
        myResultsWindow.transform.localScale = Vector3.zero;

        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            myResultsWindow.transform.localScale = originalScale * scale;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        myResultsWindow.transform.localScale = originalScale;
    }

    void PopulateResultsTable(List<PlayerData> players, int[] scores)
    {
        foreach (Transform child in resultsTableParent)
        {
            Destroy(child.gameObject);
        }

        GameObject headerRow = Instantiate(resultRowPrefab, resultsTableParent);
        TextMeshProUGUI teamHeader = headerRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI pointsHeader = headerRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        teamHeader.text = "PLAYER";
        pointsHeader.text = "POINTS";
        teamHeader.color = Color.green;
        pointsHeader.color = Color.green;
        teamHeader.fontStyle = FontStyles.Underline;
        pointsHeader.fontStyle = FontStyles.Underline;

        for (int i = 0; i < scores.Length; i++)
        {
            GameObject row = Instantiate(resultRowPrefab, resultsTableParent);
            row.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = players[i].username;
            row.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = scores[i].ToString();
        }
    }

    IEnumerator UpdatePlayerScoresOnServer(List<PlayerData> players, int[] scores)
    {
        for (int i = 0; i < players.Count; i++)
        {
            WWWForm form = new WWWForm();
            form.AddField("player_id", players[i].player_id);
            form.AddField("score", scores[i]);

            using (UnityWebRequest www = UnityWebRequest.Post("https://unity-server-sdfo.onrender.com/update_score.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error score update: " + www.error);
                }
            }
        }
    }
}
