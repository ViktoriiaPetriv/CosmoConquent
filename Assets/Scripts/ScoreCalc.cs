using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using TMPro;
using System.Collections;

public class ScoreCalc : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private ResultsWindow myResultsWindow;

    public Transform resultsTableParent; 
    public GameObject resultRowPrefab;  


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public List<int[]> teamDroneDistributions = new List<int[]>();
    public string[] planets = { "Kronus", "Lyrion", "Mystara", "Eclipsia", "Fiora" };
    public int teamCount = 5;



    public void SetPlayerTeamDrones(int kronus, int lyrion, int mystara, int eclipsia, int fiora)
    {

        teamDroneDistributions.Clear();

        teamDroneDistributions.Add(new int[] { 350, 200, 200, 150, 100 });
        teamDroneDistributions.Add(new int[] { 400, 300, 200, 100, 0 });   
        teamDroneDistributions.Add(new int[] { 250, 220, 180, 180, 170 });
        teamDroneDistributions.Add(new int[] { 300, 250, 190, 160, 100 });
        teamDroneDistributions.Add(new int[] { kronus, lyrion, mystara, eclipsia, fiora });


        teamCount = teamDroneDistributions.Count;

        CalculateTeamScores();
    }


    public void CalculateTeamScores()
    {
        int[] teamScores = new int[teamCount];

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

        PopulateResultsTable(teamScores);
        OpenResultsWindow(teamScores, winningTeams);
    }

     private void OpenResultsWindow(int[] scores, List<int> winningTeams)
    {
        string winnerText;
        if (winningTeams.Count > 1)
        {
            winnerText = "Teams " + string.Join(", ", winningTeams) + " Win!";
        }
        else
        {
            winnerText = $"Team {winningTeams[0]} Wins!";
        }

        myResultsWindow.gameObject.SetActive(true);
        myResultsWindow.CloseButton.onClick.AddListener(CloseClicked);
        myResultsWindow.winnerText.text = winnerText;

        StartCoroutine(OpenWindowAnimation());
        StartCoroutine(AnimateWinnerText());
    }


    private void CloseClicked(){
        myResultsWindow.gameObject.SetActive(false);
        PlayClickSound();
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

    while (true)
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

    teamHeader.color = Color.green;
    pointsHeader.color = Color.green;
    teamHeader.fontStyle = FontStyles.Underline;
    pointsHeader.fontStyle = FontStyles.Underline;

    for (int team = 0; team < scores.Length; team++)
    {
        GameObject row = Instantiate(resultRowPrefab, resultsTableParent);
        row.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Team {team + 1}";
        row.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = scores[team].ToString();
    }
}

}

