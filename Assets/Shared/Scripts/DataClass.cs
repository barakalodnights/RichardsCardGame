using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataClass 
{
    //public static int Kills { get; set; }
    //public static int Deaths { get; set; }
    //public static int Assists { get; set; }
    //public static float Points { get; set; }
    public static int playerPoints, enemyPoints;
    public static List<int[]> playerSpellHistory; // list of all the turn results
    public static List<int[]> opponentSpellHistory; // list of all the turn results

    public static bool[] pAchievements;
    public static bool[] oAchievements;

    public static void ResetData()
    {
        playerPoints = 0;
        enemyPoints = 0;
        playerSpellHistory = new List<int[]>();
        opponentSpellHistory = new List<int[]>();

        pAchievements = new bool[5];
        for(int i=0;i<5;i++)
        {
            pAchievements[i] = false;
        }
        oAchievements = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            oAchievements[i] = false;
        }

    }

    

}
