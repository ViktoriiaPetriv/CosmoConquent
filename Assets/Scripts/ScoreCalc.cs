using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ScoreCalc : MonoBehaviour
{
    private int gameId; 
    public Transform resultsTableParent;
    public GameObject resultRowPrefab;
    private List<PlayerData> players;
    private bool calculationDone = false;

    public void Start()
    {
        

    }

    public void BeginCheckingMoves()
    {
        gameId = SessionData.gameId;
        if (gameId != -1)
        {
            StartCoroutine(CheckAllPlayersMovesRoutine(gameId));
        }
        else
        {
            Debug.LogError("Game ID is not found ó PlayerPrefs.");
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
        string url = $"https://6c0a-213-109-232-105.ngrok-free.app/get_results.php?game_id={gameId}";

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
                winningTeams.Add(team + 1);
            }

            Debug.Log($"Team {team + 1} total score: {teamScores[team]}");
        }

        Debug.Log($"Team(s) with the highest score: {string.Join(", ", winningTeams)} with score: {maxScore}");
        StartCoroutine(UpdatePlayerScoresOnServer(players, teamScores));
        PopulateResultsTable(teamScores);
    }

    void PopulateResultsTable(int[] scores)
    {
        foreach (Transform child in resultsTableParent)
        {
            Destroy(child.gameObject);
        }

        GameObject headerRow = Instantiate(resultRowPrefab, resultsTableParent);
        TextMeshProUGUI teamHeader = headerRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI pointsHeader = headerRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        teamHeader.text = "TEAM";
        pointsHeader.text = "POINTS";

        for (int team = 0; team < scores.Length; team++)
        {
            GameObject row = Instantiate(resultRowPrefab, resultsTableParent);
            row.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Team {team + 1}";
            row.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = scores[team].ToString();
        }
    }

    IEnumerator UpdatePlayerScoresOnServer(List<PlayerData> players, int[] scores)
    {
        for (int i = 0; i < players.Count; i++)
        {
            WWWForm form = new WWWForm();
            form.AddField("player_id", players[i].player_id);
            form.AddField("score", scores[i]);

            using (UnityWebRequest www = UnityWebRequest.Post("https://6c0a-213-109-232-105.ngrok-free.app/update_score.php", form))
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
