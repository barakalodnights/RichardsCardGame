using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera cam;

    public GameObject cardPrefab;
    public GameObject mindSlotPrefab;
    

    // player objects
    public List<GameObject> sourceDeck;
    public List<GameObject> mindSlotsDeck;
    public GameObject sourceDeckObj;
    public GameObject mindsDeckObj;
    public Text playerPointsText;
    
    private List<GameObject> aAttackList;
    private int aAttackInt=0;
    private bool[] pSourceInMind;

    // opponent objects
    public List<GameObject> sourceDeckO;
    public List<GameObject> mindSlotsDeckO;
    public GameObject sourceDeckObjO;
    public GameObject mindsDeckObjO;
    public Text enemyPointsText;
    
    private List<GameObject> dAttackList;
    private int dAttackInt = 0;
    private bool[] oSourceInMind;

    public Button castButton;
    private AudioSource audioS;
    public List<AudioClip> gameClips;

    //public GameObject AttackArrowObj;

    List<int> randArray;
    List<float> tArray;
    //float gameTimer = 0f; 
    float turnTimer = 0f;
    float attackTimer = 2f, attackTimeMax = 1f;// pauseTimer = 0f, pauseTimerMax = 1f;

    public enum SourceType { Heat, Cold, Force, Bio, Light };

    private enum GameState { Choosing, PlayerAttack, EnemyAttack, Decomposition };
    private GameState gS;

    private bool[,] winMat;

    public int[] spellLog; // fill during each turn
    private int maxPoints = 30;

    public Text messageText;
    public List<string> messageLog;
    private float messageTimer = 0f,messageTimerMax=5f;

    public Text gameStateText;
    public GameObject txtPrefab;

    private int[,] decompMat;

    void Start()
    {
        // initialize the achievement log, reset all the log data
        DataClass.ResetData();
        
        // set up UI sounds
        audioS = GetComponent<AudioSource>();
        audioS.volume = 0.8f;

        winMat = new bool[5, 5] { { false, true, true, false, false }, { false, false, true, true, false }, { false, false, false, true, true }, { true, false, false, false, true}, { true, true, false, false, false } };
        decompMat = new int[5, 5] { { 0, 1, 1, 0, 0 }, { 0, 0, 1, 1, 0 }, { 0, 0, 0, 1, 1 }, { 1, 0, 0, 0, 1 }, { 1, 1, 0, 0, 0 } };
        castButton.interactable = false;
        // set up the player's gameboard
        // deal cards to player
        for (int i = 0; i < 8; i++)
        {
            sourceDeck.Add(CreateRandomCard(cardPrefab, Vector3.zero, sourceDeckObj.transform.transform, cam));
            sourceDeck[i].layer = LayerMask.NameToLayer("Cards");
        }

        // define starting with just two minds
        mindSlotsDeck.Add(CreateNewMind(mindSlotPrefab, mindsDeckObj.transform));
        mindSlotsDeck.Add(CreateNewMind(mindSlotPrefab, mindsDeckObj.transform));

        // distribute the minds
        DistributeMindSlots(mindSlotsDeck,mindsDeckObj.transform);
        // distribute source deck
        DistributeSourceDeck(sourceDeck, sourceDeckObj.transform);

        // set up the opponent gameboard
        for (int i = 0; i < 8; i++)
        {
            sourceDeckO.Add(CreateRandomCard(cardPrefab, Vector3.zero, sourceDeckObjO.transform.transform, cam));
            sourceDeckO[i].transform.GetComponent<CardUIController>().FlipCard();
            sourceDeckO[i].transform.GetComponent<CardUIController>().playerCard = false;
            sourceDeckO[i].layer = LayerMask.NameToLayer("EnemyCards");
        }

        // define starting with just two minds
        mindSlotsDeckO.Add(CreateNewMind(mindSlotPrefab, mindsDeckObjO.transform));
        mindSlotsDeckO.Add(CreateNewMind(mindSlotPrefab, mindsDeckObjO.transform));
        
        // distribute the minds
        DistributeMindSlots(mindSlotsDeckO, mindsDeckObjO.transform);
        // distribute source deck
        DistributeSourceDeck(sourceDeckO, sourceDeckObjO.transform);

        // move two enemy cards to the minds (random)
        ChooseOpponentSpells();

        // initialize the attack lists
        aAttackList = new List<GameObject>();
        dAttackList = new List<GameObject>();

        gS = GameState.Choosing;

    }

    // Update is called once per frame
    void Update()
    {
        

        //gameTimer += Time.deltaTime;
        turnTimer += Time.deltaTime;
        if (gS == GameState.Choosing)
        {
            // after some time, have the enemy pick a card
            if (tArray.Count > 0)
            {
                if (turnTimer > tArray[tArray.Count-1])
                {
                    AddSource2MindsO(sourceDeckO[randArray[randArray.Count - 1]]);

                    tArray.RemoveAt(tArray.Count - 1);
                    randArray.RemoveAt(randArray.Count - 1);

                    if (tArray.Count == 0) castButton.interactable = true;
                }
            }

        } else if(gS == GameState.PlayerAttack)
        {
            if (attackTimer < attackTimeMax)
            {
                attackTimer += Time.deltaTime;
            }
            else
            {
                PrepareOpponentCast();
            }
           
        } else if(gS == GameState.EnemyAttack)
        {
            //AttackSequence();
            if (attackTimer < attackTimeMax)
            {
                attackTimer += Time.deltaTime;
            }
            else
            {
                EndAttackPhase();
            }
        }

        HandleMessages();

    }

    public GameObject CreateCard(GameObject pf, int srcInt, int units, Vector3 pos,Transform par, Camera camM)
    {
        GameObject cardClone = Instantiate(pf, pos, Quaternion.identity, par);
        cardClone.transform.localPosition = pos;
        cardClone.transform.GetChild(1).transform.GetComponent<Canvas>().worldCamera = camM;

        
        // pick a source type
        cardClone.transform.GetComponent<CardUIController>().SetSourceType(srcInt);
        cardClone.transform.GetComponent<CardUIController>().SetUnits(units);
        //Debug.Log("SrcInt: " + srcInt);
        
        cardClone.transform.GetComponent<CardUIController>().gameManagerObj = this.gameObject;

        return cardClone;
    }

    public GameObject CreateRandomCard(GameObject pf, Vector3 pos, Transform par, Camera camM)
    {
        GameObject cardClone = Instantiate(pf, pos, Quaternion.identity, par);
        cardClone.transform.localPosition = pos;
        cardClone.transform.GetChild(1).transform.GetComponent<Canvas>().worldCamera = camM;
        
        // pick a source type
        SourceType rST = (SourceType)Random.Range(0, 5);
        cardClone.transform.GetComponent<CardUIController>().SetSourceType(rST.ToString());
        cardClone.transform.GetComponent<CardUIController>().SetUnits(Random.Range(40,50));
        //cardClone.transform.GetComponent<CardUIController>().SetImage(sourceSprites[((int)rST)]);
        cardClone.transform.GetComponent<CardUIController>().gameManagerObj = this.gameObject;
        //Debug.Log(LayerMask.NameToLayer("Cards"));
        

        return cardClone;
    }

    public GameObject CreateNewMind(GameObject pf, Transform par)
    {
        GameObject mindClone = Instantiate(pf, Vector3.zero, Quaternion.identity, par);
        mindClone.transform.localPosition = Vector3.zero;

        return mindClone;
    }

    private void DistributeMindSlots(List<GameObject> mtDeck, Transform par)
    {
        Vector2 sz = par.GetComponent<SpriteRenderer>().size;
        float wid = sz.x;
        float deckSz = mtDeck.Count;
        for(int i = 0; i < deckSz; i++)
        {
            mtDeck[i].transform.localPosition = new Vector3((i+1) * wid / (deckSz + 1)-wid/2, 0, 0);
        }

    }

    private void DistributeSourceDeck(List<GameObject> srcDeck, Transform par)
    {
        Vector2 sz = par.GetComponent<SpriteRenderer>().size;
        float wid = sz.x;
        float deckSz = srcDeck.Count;
        for (int i = 0; i < deckSz; i++)
        {
            //srcDeck[i].transform.localPosition = new Vector3((i + 1) * wid / (deckSz + 1) - wid / 2, 0, 0);
            
            Vector3 lPos = new Vector3((i + 1) * wid / (deckSz + 1) - wid / 2, 0, 0); // these are local coordinates
            srcDeck[i].transform.GetComponent<CardUIController>().movingTo = par.TransformPoint(lPos);
            srcDeck[i].transform.GetComponent<CardUIController>().moving = true;
        }
    }

    public void AddSource2Minds(GameObject go)
    {
        audioS.clip = gameClips[1];
        audioS.Play();
        // refresh the list of empty/full minds
        bool[] indx = CheckMindSlots(mindSlotsDeck);
        //Debug.Log(indx);
        // only move if there is an empty mind available
        for(int i = 0; i < indx.Length; i++) {

            if (!indx[i]) {
                
                sourceDeck.Remove(go);
                go.transform.parent = mindsDeckObj.transform.GetChild(i).transform;
                

                go.transform.GetComponent<CardUIController>().moving = true;
            // find first available empty slot
    
                go.transform.GetComponent<CardUIController>().movingTo = mindSlotsDeck[i].transform.position;
                mindSlotsDeck[i].transform.GetComponent<Mind>().isFilled = true;
                break;
            }
        }

        go.transform.GetComponent<CardUIController>().TurnOnEffect();     

        DistributeSourceDeck(sourceDeck, sourceDeckObj.transform);
    }

    public void AddSource2MindsO(GameObject go)
    {
        audioS.clip = gameClips[1];
        audioS.Play();
        // refresh the list of empty/full minds
        bool[] indx = CheckMindSlots(mindSlotsDeckO);
        //Debug.Log(indx);
        // only move if there is an empty mind available
        for (int i = 0; i < indx.Length; i++)
        {

            if (!indx[i])
            {  
                sourceDeckO.Remove(go);
                go.transform.parent = mindsDeckObjO.transform.GetChild(i).transform;

                go.transform.GetComponent<CardUIController>().moving = true;
                // find first available empty slot

                go.transform.GetComponent<CardUIController>().movingTo = mindSlotsDeckO[i].transform.position;

                
                //go.transform.GetComponent<CardUIController>().FlipCard();
                break;
            }
        }

        DistributeSourceDeck(sourceDeckO, sourceDeckObjO.transform);

    }

    public void MoveFromMinds(GameObject go)
    {
        audioS.clip = gameClips[1];
        audioS.Play();
        go.transform.parent.transform.GetComponent<Mind>().isFilled = false;

        sourceDeck.Add(go);
        go.transform.parent = sourceDeckObj.transform;

        // get the old position
        Vector3 oldPos = go.transform.position;
        // this will make room for the card back in the source deck (actually places it there)
        DistributeSourceDeck(sourceDeck, sourceDeckObj.transform);
        // get teh newly calculated position (current position)
        Vector3 currPos = go.transform.position;
        // set teh current positino to where it was before
        go.transform.position = oldPos;

        // now animate the move from the old to the new
        //go.transform.GetComponent<CardUIController>().moving = true;
        //go.transform.GetComponent<CardUIController>().movingTo = currPos;
        
        // turn off the particle effect
        go.transform.GetComponent<CardUIController>().TurnOffEffect();
    }

    public void SendCard2Source(GameObject go)
    {
        audioS.clip = gameClips[1];
        audioS.Play();

        sourceDeck.Add(go);
        go.transform.parent = sourceDeckObj.transform;

        // get the old position
        Vector3 oldPos = go.transform.position;
        // this will make room for the card back in the source deck (actually places it there)
        DistributeSourceDeck(sourceDeck, sourceDeckObj.transform);
        // get teh newly calculated position (current position)
        Vector3 currPos = go.transform.position;
        // set teh current positino to where it was before
        go.transform.position = oldPos;

        // turn off the particle effect
        go.transform.GetComponent<CardUIController>().TurnOffEffect();
    }


    public void MoveFromMindsO(GameObject go) // make this take multiple game objects
    {
        audioS.clip = gameClips[1];
        audioS.Play();
        sourceDeckO.Add(go);
        go.transform.parent = sourceDeckObjO.transform;

        // get the old position
        Vector3 oldPos = go.transform.position;
        // this will make room for the card back in the source deck (actually places it there)
        DistributeSourceDeck(sourceDeckO, sourceDeckObjO.transform);
        // get teh newly calculated position (current position)
        Vector3 currPos = go.transform.position;
        // set teh current positino to where it was before
        go.transform.position = oldPos;

        // now animate the move from the old to the new
        //go.transform.GetComponent<CardUIController>().moving = true;
        //go.transform.GetComponent<CardUIController>().movingTo = currPos;
    }

    private bool[] CheckMindSlots(List<GameObject> mSD)
    {
        bool[] indx = new bool[mSD.Count];
        for(int i=0;i<mSD.Count;i++)
        {
            if (mSD[i].transform.childCount == 1)
            {
                indx[i] = false;
                mSD[i].transform.GetComponent<Mind>().isFilled = false;
            }
            else if (mSD[i].transform.childCount > 1)
            {
                indx[i] = true;
                mSD[i].transform.GetComponent<Mind>().isFilled = true;
                //Debug.Log(i);
            }
            
        }

        return indx;
    }

    public void KillCardFromMinds(GameObject go)
    {
        go.transform.GetComponent<CardUIController>().KillCard();
        go.transform.parent.transform.GetComponent<Mind>().isFilled = false;
    }

    private List<int> GenerateIntArray(int min, int max, int len)
    {
        List<int> usedInts = new List<int>();
        int val = Random.Range(min, max);
        usedInts.Add(val);

        if (len > 1)
        {
            while (usedInts.Count < len)
            {
                val = Random.Range(min, max);
                if (!usedInts.Contains(val))
                {
                    usedInts.Add(val);
                }
            }
        }

        usedInts.Sort();

        return usedInts;
    }

    public void PrepareCast()
    {
        spellLog = new int[5];
        for(int i = 0; i < 5; i++)
        {
            spellLog[i] = 0;
        }

        audioS.clip = gameClips[0];
        audioS.Play();

        castButton.interactable = false;

        // make sure the timers are at zero
        //dAttackInt = 0;
        //aAttackInt = 0;

        playerPointsText.text = "Player Points: " + DataClass.playerPoints.ToString();
        enemyPointsText.text = "Enemy Points: " + DataClass.enemyPoints.ToString();

        // check where all the cards are (is there a card in each mind?)
        pSourceInMind = CheckMindSlots(mindSlotsDeck);
        oSourceInMind = CheckMindSlots(mindSlotsDeckO);

        aAttackList.Clear();

        for (int i = 0; i < mindSlotsDeck.Count; i++)
        {
            if(pSourceInMind[i])
            aAttackList.Add(mindSlotsDeck[i].transform.GetChild(1).gameObject);
        }

        dAttackList.Clear();
        for (int i = 0; i < mindSlotsDeckO.Count; i++)
        {
            if (oSourceInMind[i])
            {
                dAttackList.Add(mindSlotsDeckO[i].transform.GetChild(1).gameObject);
                // this is too fast unless I can pause the action for an effect!!!
                mindSlotsDeckO[i].transform.GetChild(1).GetComponent<CardUIController>().FlipCard();
                mindSlotsDeckO[i].transform.GetChild(1).GetComponent<CardUIController>().TurnOnEffect();
            }
        }

        if (aAttackList.Count > 0)
        {
            gS = GameState.PlayerAttack;
            gameStateText.text = "Player Attacks!";
            AttackSequence2();

        } // else tell the player to choose!

    }

    public void PrepareOpponentCast()
    {
        for (int j = 0; j < dAttackList.Count; j++)
        {
            // clean up the cards that got attacked
            dAttackList[j].transform.GetComponent<CardUIController>().beingAttacked = false;
            dAttackList[j].layer = LayerMask.NameToLayer("EnemyCards");

        }

        // record the log and reset
        DataClass.playerSpellHistory.Add(spellLog);

        // check if the player just won
        CheckEndGame();

        spellLog = new int[5];
        for (int i = 0; i < 5; i++)
        {
            spellLog[i] = 0;
        }
        // reset the attack timer
        attackTimer = 0f;

        // make sure the timers are at zero
        //dAttackInt = 0;
        //aAttackInt = 0;

        // update the points if needed
        playerPointsText.text = "Player Points: " + DataClass.playerPoints.ToString();
        enemyPointsText.text = "Enemy Points: " + DataClass.enemyPoints.ToString();

        // check where all the cards are (is there a card in each mind?)
        pSourceInMind = CheckMindSlots(mindSlotsDeck);
        oSourceInMind = CheckMindSlots(mindSlotsDeckO);

        aAttackList.Clear();

        for (int i = 0; i < mindSlotsDeckO.Count; i++)
        {
            if (oSourceInMind[i])
                aAttackList.Add(mindSlotsDeckO[i].transform.GetChild(1).gameObject);
        }

        dAttackList.Clear();
        for (int i = 0; i < mindSlotsDeck.Count; i++)
        {
            if (pSourceInMind[i])
            {
                dAttackList.Add(mindSlotsDeck[i].transform.GetChild(1).gameObject);
                // this is too fast unless I can pause the action for an effect!!!
                
            }
        }

        if (aAttackList.Count > 0)
        {
            gameStateText.text = "Opponent Attacks!";
            gS = GameState.EnemyAttack;
            AttackSequence2();
        }
        else
        {
            gameStateText.text = "Choose Cards";
            gS = GameState.Choosing;
        } 
    }

    private bool WinCheck(GameObject g1,GameObject g2)
    {
        int i1 = g1.transform.GetComponent<CardUIController>().GetSourceInt();
        int i2 = g2.transform.GetComponent<CardUIController>().GetSourceInt();
        //Debug.Log(" Wincheck i1: " + i1 + " i2: " + i2);

        return winMat[i1, i2];
    }
   
    private void EndAttackPhase()
    {
        for (int j = 0; j < dAttackList.Count; j++)
        {
            // clean up the cards that got attacked
            dAttackList[j].transform.GetComponent<CardUIController>().beingAttacked = false;
            dAttackList[j].layer = LayerMask.NameToLayer("Cards");
        }

        // record the log and reset
        DataClass.opponentSpellHistory.Add(spellLog);
        CheckAchievements();
        spellLog = new int[5];


        // check if the opponent just won
        CheckEndGame();

        // send the player and enemy cards back to their decks
        foreach (GameObject go in aAttackList)
        {
            // check for damage below zero
            if (go.transform.GetComponent<CardUIController>().myUnits <= 0)
            {
                // remove from mindSlotsDeck
                KillCardFromMinds(go);

            }
            else
            {
                // remove markers
                go.transform.GetComponent<CardUIController>().FlipCard();
                go.transform.GetComponent<CardUIController>().TurnOffEffect();
                MoveFromMindsO(go);
            }
        }


        foreach (GameObject go in dAttackList)
        {
            MoveFromMinds(go);
        }

        dAttackInt = 0;
        aAttackInt = 0;
        aAttackList.Clear();
        dAttackList.Clear();

        // create random time array to move enemy cards to minds deck
        ChooseOpponentSpells();

        // restart the attack timer
        attackTimer = 0f;
        turnTimer = 0f;

        gS = GameState.Choosing;
        gameStateText.text = "Choose Spells to Cast";
    }

    private void AttackSequence2()
    {
        string msg="";
        int tempPts = 0;
        // go through the list of attacking cards
        for (int i = 0; i < aAttackList.Count; i++)
        {
            CardUIController aCardCont = aAttackList[i].transform.GetComponent<CardUIController>();
            for (int j = 0; j < dAttackList.Count; j++)
            {
                CardUIController dCardCont = dAttackList[j].transform.GetComponent<CardUIController>();
                if (!dAttackList[j].transform.GetComponent<CardUIController>().beingAttacked)
                {
                    if (WinCheck(aAttackList[i], dAttackList[j]))
                    {
                        //Debug.Log(i + ", " + j);
                        // place the card being attacked in the layer that can be attacked
                        dAttackList[j].layer = LayerMask.NameToLayer("TargetCard");

                        // store the position of the card being attacked
                        Vector3 oPos = dAttackList[j].transform.position;

                        // watch for winners
                        spellLog[aCardCont.GetSourceInt()] += 1;

                        //activate the attacking card's particle system 
                        aCardCont.TurnOnAttack(oPos, attackTimeMax);
                        dCardCont.TurnOnDamage(aCardCont.GetSourceInt());

                        // let the card know it has already been attacked
                        dCardCont.beingAttacked = true;

                        // add affinity to the current mind
                        Mind aMind = aAttackList[i].transform.parent.gameObject.transform.GetComponent<Mind>();
                        //aMind.affinity[aCardCont.GetSourceInt()] += 1;
                        aMind.AddAffinity(1, aCardCont.GetSourceInt());

                        // dock units from the attacked card
                         DamagePopup(aCardCont, aMind, dCardCont, dAttackList[j].transform.position);

                       

                        // add one point
                        AddPoint();
                        tempPts += 1;
                        // send a message about this point

                        msg += aAttackList[i].transform.GetComponent<CardUIController>().mySource;
                        msg += " attacked " + dAttackList[j].transform.GetComponent<CardUIController>().mySource;
                        msg += "!\n";
                        

                        // break out of the inner loop
                        break;
                    }
                }
            }
        }

        if (tempPts == 0)
        {
            msg = "No attacks possible!";
        }
        messageLog.Add(msg);

        attackTimer = 0f;
        attackTimeMax = 5f;
 
    }

    private void AddPoint() {
        Debug.Log("Point: " + gS);
        if (gS == GameState.PlayerAttack)
        {
            DataClass.playerPoints += 1;
            playerPointsText.text = "Player Points: " + DataClass.playerPoints.ToString();
        }
        else if(gS == GameState.EnemyAttack)
        {
            DataClass.enemyPoints += 1;
            enemyPointsText.text = "Enemy Points: " + DataClass.enemyPoints.ToString();
        }
    }

    private void CheckEndGame()
    {
        if (sourceDeck.Count<=1 || sourceDeckO.Count<=1)
        {
            SceneManager.LoadScene("EndScene");
        }
    }

    private void CheckAchievements()
    {
        if (!DataClass.pAchievements[0])
        {
            // go through the lists for achievements
            int[] spCheck = DataClass.playerSpellHistory[DataClass.playerSpellHistory.Count - 1];
            // check for the double achievement
            int summ = 0;
            foreach (int i in spCheck)
            {
                summ += i;
            }
            if (summ == 2)
            {
                DataClass.pAchievements[0] = true;
                // add a new mind
                mindSlotsDeck.Add(CreateNewMind(mindSlotPrefab, mindsDeckObj.transform));
                DistributeMindSlots(mindSlotsDeck, mindsDeckObj.transform);

                //messageText.gameObject.SetActive(true);
                //messageText.text = "Achievement Unlocked!\nTwo spells in one turn\n+1 Mind";

                messageLog.Add("Achievement Unlocked!\nTwo spells in one turn\n+1 Mind");
            }
        }

        if (!DataClass.pAchievements[1])
        {
            // if they've used all five sources, add a mind
            int[] spellRollup = new int[5];

            foreach (int[] i5 in DataClass.playerSpellHistory)
            {
                for (int i = 0; i < 5; i++)
                {
                    spellRollup[i] += i5[i];
                }
            }
            int summ = 0;
            for (int i = 0; i < 5; i++)
            {
                if (spellRollup[i] > 0)
                {
                    summ += 1;
                }
            }
            if (summ == 5)
            {
                DataClass.pAchievements[1] = true;
                // add a new mind
                mindSlotsDeck.Add(CreateNewMind(mindSlotPrefab, mindsDeckObj.transform));
                DistributeMindSlots(mindSlotsDeck, mindsDeckObj.transform);

                messageLog.Add("Achievement Unlocked!\nUse every spell once");
                //messageText.gameObject.SetActive(true);
                //messageText.text = "Achievement Unlocked!\nUse every spell once";
            }
        }

        if (!DataClass.oAchievements[0])
        {
            // go through the lists for achievements
            int[] spCheck = DataClass.opponentSpellHistory[DataClass.opponentSpellHistory.Count - 1];
            // check for the double achievement
            int summ = 0;
            foreach (int i in spCheck)
            {
                summ += i;
            }
            if (summ == 2)
            {
                DataClass.oAchievements[0] = true;
                // add a new mind
                mindSlotsDeckO.Add(CreateNewMind(mindSlotPrefab, mindsDeckObjO.transform));
                DistributeMindSlots(mindSlotsDeckO, mindsDeckObjO.transform);

                messageLog.Add("Opponent Achievement Unlocked!\nTwo spells in one turn\n+1 Mind");
            }
        }

        if (!DataClass.oAchievements[1])
        {
            // if they've used all five sources, add a mind
            int[] spellRollup = new int[5];

            foreach (int[] i5 in DataClass.opponentSpellHistory)
            {
                for (int i = 0; i < 5; i++)
                {
                    spellRollup[i] += i5[i];
                }
            }
            int summ = 0;
            for (int i = 0; i < 5; i++)
            {
                if (spellRollup[i] > 0)
                {
                    summ += 1;
                }
            }
            if (summ == 5)
            {
                DataClass.oAchievements[1] = true;
                // add a new mind
                mindSlotsDeckO.Add(CreateNewMind(mindSlotPrefab, mindsDeckObjO.transform));
                 DistributeMindSlots(mindSlotsDeckO, mindsDeckObjO.transform);
                messageLog.Add("Opponent Achievement Unlocked!\nUse every spell once");

            }
        }

    }

    public void HandleMessages()
    {
        if (messageLog.Count > 0)
        {
            //Debug.Log("Achievement message");
            messageTimer += Time.deltaTime;

            if (!messageText.gameObject.activeSelf)
            {
                messageText.text = messageLog[0];
                messageText.gameObject.SetActive(true);    
            }
            else
            {

                if (messageTimer > messageTimerMax)
                {
                    //Debug.Log("turn off message");
                    messageLog.RemoveAt(0);
                    messageText.gameObject.SetActive(false);
                    messageTimer = 0f;
                }
            }
        }

    }

    private void ChooseOpponentSpells()
    {
        // create random time array to move enemy cards to minds deck
        randArray = GenerateIntArray(0, sourceDeckO.Count - 1, mindSlotsDeckO.Count);
       // Debug.Log("randArray length: " + randArray.Count);

        //tArray = GenerateIntArray(1, sourceDeckO.Count, mindSlotsDeckO.Count);
        tArray = new List<float>();
        
        foreach (int i in randArray)
        {
            tArray.Add(Random.Range(1f, 3f));
        }
    }

    private int CalcUnitDamage(CardUIController aCard, Mind aMind, CardUIController dCard)
    {
        // damage depends on the strength of the card (units) and the affinity of the mind
        int dmg = (int)(aCard.myUnits / 10f + aMind.affinity[aCard.GetSourceInt()]*2);

        return dmg;
    }

    private void DamagePopup(CardUIController aCard, Mind aMind, CardUIController dCard, Vector2 pos)
    {
        int dockUnits = CalcUnitDamage(aCard, aMind, dCard);
        dCard.SetUnits(dCard.myUnits - dockUnits);
        Vector2 txtPos = pos + new Vector2(2, 2);
        GameObject tmpTxt = Instantiate(txtPrefab, txtPos, Quaternion.identity);
        //Debug.Log("Dmg: " + dockUnits.ToString());
        tmpTxt.transform.GetComponent<TextPopupController>().Setup(dockUnits);
    }


    public void PlayerDecompose()
    {
        // make sure there is at least one card available to decompose
        bool[] mSC = CheckMindSlots(mindSlotsDeck);
        int bSum = 0;
        foreach(bool b in mSC)
        {
            if (b) bSum += 1;
        }

        if (bSum > 1) Decompose(mindSlotsDeck);
        else messageLog.Add("You need at least two cards to decompose!");
    }
    public void Decompose(List<GameObject> mSD)
    {
        List<GameObject> goList = new List<GameObject>();
        int[] srcArray = new int[5] {0,0,0,0,0};
        int[] probArray = new int[5] { 0, 0, 0, 0, 0 };
        int[] affinitySum = new int[5] { 0, 0, 0, 0, 0 };

        int sumUnits = 0;
        foreach (GameObject go in mSD)
        {
            if (go.transform.GetChild(1) != null)
            {
                GameObject listObj = go.transform.GetChild(1).gameObject;
                goList.Add(listObj);
                srcArray[listObj.GetComponent<CardUIController>().GetSourceInt()] += 1;
                sumUnits += listObj.GetComponent<CardUIController>().myUnits;
            }
            for (int ii = 0; ii < 5; ii++)
            {
                affinitySum[ii] += go.transform.GetComponent<Mind>().GetAffinity(ii);
            }
        }

            // this is matrix multiplication of the array of sources that are being decomposed
            int sumProb = 0;
        
        for (int ii = 0; ii < 5; ii++)
        {
            for (int jj = 0; jj < 5; jj++)
            {
                probArray[ii] += srcArray[jj] * decompMat[ii, jj];
            }
            // the affinity adds to the probability array of that source
            probArray[ii] += affinitySum[ii];
            sumProb += probArray[ii];
        }

        int rr = Random.Range(0, sumProb - 1);
        int iter = 0;

        foreach(int ii in probArray)
        {
            iter++;
            if ((rr -= ii) < 0) break;
        }

        foreach (GameObject go in goList)
        {
            KillCardFromMinds(go);
        }

        // the iteration is actually one too many
        iter -= 1;

        GameObject nGO = CreateCard(cardPrefab, iter, sumUnits, -Vector3.forward, mindsDeckObj.transform, cam);
        nGO.layer = LayerMask.NameToLayer("Cards");
        SendCard2Source(nGO);



        //DistributeSourceDeck(sourceDeck, sourceDeckObj.transform);

        Debug.Log("Sum: " + sumProb);
        for (int ii = 0; ii < 5; ii++)
        {
            Debug.Log("SA: " + srcArray[ii]);
            Debug.Log("PA: " + probArray[ii]);
        }

        Debug.Log("indx: " + iter);

        gS = GameState.Decomposition;
        
    }

    /*
    private string IntSrc2Str(int srcInt)
    {
        string srcStr;

        switch (srcInt)
        {
            case 0:
                srcStr = "heat";
                break;
            case 1:
                srcStr = "cold";
                break;
            case 2:
                srcStr = "force";
                break;
            case 3:
                srcStr = "bio";
                break;
            case 4:
                srcStr = "light";
                break;
            default:
                srcStr = "heat";
                break;
        }

        return srcStr;
    }
    */

}
