using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;

public class ScoreCalc : MonoBehaviour
{

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
        int winningTeam = Array.IndexOf(teamScores, maxScore);

        for (int team = 0; team < teamCount; team++)
        {
            Debug.Log($"Team {team + 1} total score: {teamScores[team]}");
        }
        Debug.Log($"Team {winningTeam + 1} has the highest score: {maxScore}");
    }
}