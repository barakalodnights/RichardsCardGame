using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EndSceneController : MonoBehaviour
{

    public Text pLogText, oLogText, titleText;
   

    public void Start()
    {

        if (DataClass.playerPoints > DataClass.enemyPoints)
        {
            titleText.text = "You Win!";
        }
        else
        {
            titleText.text = "You Lose!";
        }

        pLogText.text = "Spells Cast:\n";

        int[] spellRollup = new int[5];

        foreach(int[] i5 in DataClass.playerSpellHistory)
        {
            for(int i = 0; i < 5; i++)
            {
                spellRollup[i] += i5[i];
            }
        }

       
        pLogText.text += "Heat: " + spellRollup[0] + "\n";
        pLogText.text += "Cold: " + spellRollup[1] + "\n";
        pLogText.text += "Force: " + spellRollup[2] + "\n";
        pLogText.text += "Bio: " + spellRollup[3] + "\n";
        pLogText.text += "Light: " + spellRollup[4] + "\n";

        oLogText.text += "\nOpponent Spells Cast:\n";


        spellRollup = new int[5];
        foreach (int[] i5 in DataClass.opponentSpellHistory)
        {
            for (int i = 0; i < 5; i++)
            {
                spellRollup[i] += i5[i];
            }
        }

        oLogText.text += "Heat: " + spellRollup[0] + "\n";
        oLogText.text += "Cold: " + spellRollup[1] + "\n";
        oLogText.text += "Force: " + spellRollup[2] + "\n";
        oLogText.text += "Bio: " + spellRollup[3] + "\n";
        oLogText.text += "Light: " + spellRollup[4] + "\n";



    }

    public void LoadMenu()
    {
        DataClass.ResetData();
        SceneManager.LoadScene("MenuScene");
    }

}
