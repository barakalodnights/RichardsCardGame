using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour
{
    public Image myImg;
    public Text nameTxt;
    public Text unitTxt;

    public Canvas cardFront;
    public Canvas cardBack;
    public List<Sprite> cardSprites;
    public List<Sprite> sourceSprites;

    //public ParticleSystem selectEffects;
    //public ParticleSystem effectClone;
    public GameObject gameManagerObj;
    public GameObject highlightPrefab;
    public GameObject attackPrefab;
    public GameObject damagePrefab;

    public int myUnits;
    public string mySource;
    public bool playerCard = true;

    public List<AudioClip> cardSounds;

    public bool moving = false;
    public Vector3 movingTo;
    private float vel = 100f;

    public bool inTurn = false;
    public bool attacking = false;

    public Vector3 posAttacking;
    private float attackTimer = 0, maxAttackTime=4f;

    public bool beingAttacked = false;
    private int damageInt=0;

    private BoxCollider2D coll;
    //private bool canMove;
    //private bool dragging;
    private Vector2 offset = Vector2.zero;
    private AudioSource audioS;

    public List<AudioClip> attackClips;

    private Explodable _explodable;

    // Start is called before the first frame update
    void Start()
    {
        //highlightPrefab.SetActive(false);
        coll = transform.GetComponent<BoxCollider2D>();
        // canMove = false;
        //dragging = false;
        // prep the audio
        audioS = GetComponent<AudioSource>();
        audioS.volume = 0.8f;

       
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingTo, Time.deltaTime * vel);
            transform.position = FloatZ(transform.position);
            if (((Vector2)transform.position - (Vector2)movingTo).magnitude < 0.1f)
            {
                moving = false;
            }
        }

        if(attacking)
        {
            if (attackPrefab.transform.GetChild(GetSourceInt()).gameObject.activeSelf)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer > maxAttackTime)
                {
                    TurnOffAttack();
                    attackTimer = 0f;
                }
            }
        }

        if (beingAttacked)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer > maxAttackTime)
            {
                TurnOffDamage();
                attackTimer = 0f;
            }
        }

    }

    public void SetSourceType(string lSourceType)
    {
        mySource = lSourceType;
        nameTxt.text = lSourceType;
        SetImage(sourceSprites[GetSourceInt()]);
    }

    public void SetSourceType(int srcInt)
    {
        string srcStr;
        switch (srcInt)
        {
            case 0:
                srcStr = "Heat";
                break;
            case 1:
                srcStr = "Cold";
                break;
            case 2:
                srcStr = "Force";
                break;
            case 3:
                srcStr = "Bio";
                break;
            case 4:
                srcStr = "Light";
                break;
            default:
                srcStr = "Heat";
                break;
        }

        mySource = srcStr;
        nameTxt.text = srcStr;
        SetImage(sourceSprites[GetSourceInt()]);
    }



    public void SetUnits(int lUnits)
    {
        myUnits = lUnits;
        unitTxt.text = lUnits.ToString();
    }

    public void SetImage(Sprite lSprite)
    {
        myImg.transform.GetComponent<Image>().sprite = lSprite;

    }

    public void HighlightCard(int attackIndx)
    {
        this.transform.GetComponent<SpriteRenderer>().sprite = cardSprites[1];

        if(attackIndx==1) this.transform.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
        else if(attackIndx==2) this.transform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        else this.transform.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0.5f);

    }

    public void UnHighlightCard()
    {
        this.transform.GetComponent<SpriteRenderer>().sprite = cardSprites[0];
        this.transform.GetComponent<SpriteRenderer>().color = new Color(0.26f, 0.26f, 0.26f, 1f);
    }

    Vector3 CalcCardPos(Vector2 mPos, Vector2 off)
    {
        Vector3 rPos = mPos - offset;
        rPos.z = -1;
        return rPos;
    }

    Vector3 CalcParticlePos(Vector2 mPos, Vector2 off)
    {
        Vector3 rPos = mPos - offset;
        rPos.z = 0;
        return rPos;
    }

    private void OnMouseDown()
    {
        UnHighlightCard();
        //Debug.Log(transform.parent.name);
        audioS.clip = cardSounds[0];
        audioS.Play();

        if (playerCard)
        {
            if (transform.parent.name == "SourceSlots")
            {
                gameManagerObj.transform.GetComponent<GameController>().AddSource2Minds(this.gameObject);
            }

            else if (transform.parent.transform.parent != null) { 
                if (transform.parent.transform.parent.name == "MindSlots")
                {
                    gameManagerObj.transform.GetComponent<GameController>().MoveFromMinds(this.gameObject);
                }
            }
        }

        if (!playerCard)
        {
            if (transform.parent.name == "EnemySourceSlots")
            {
                //gameManagerObj.transform.GetComponent<GameController>().AddSource2MindsO(this.gameObject);
            }
            else if (transform.parent.transform.parent != null)
            {
                if (transform.parent.transform.parent.name == "EnemyMindSlots")
                {
                    // gameManagerObj.transform.GetComponent<GameController>().MoveFromMindsO(this.gameObject);
                }
            }
        }

       // KillCard();

    }

    private void OnMouseOver()
    {

        if (!moving && playerCard)
        {
            TurnOnEffect();
            HighlightCard(2);
        }
            
    }

    private void OnMouseExit()
    {
        if (playerCard)
        {
            if (transform.parent.name == "SourceSlots")
            {
                TurnOffEffect();
                UnHighlightCard();
            }
            if (transform.parent.transform.parent != null)
            {
                if (transform.parent.transform.parent.name != "MindSlots")
                {
                    TurnOffEffect();
                    
                }
                UnHighlightCard();
            }
        }
    }

    private Vector3 FloatZ(Vector3 pos)
    {
        Vector3 posZ = pos;
        posZ.z = -1;
        pos = posZ;
        return pos;
    }

    public void FlipCard()
    {
        if (cardFront.gameObject.activeSelf)
        {
            cardFront.gameObject.SetActive(false);
            cardBack.gameObject.SetActive(true);
        }
        else
        {
            cardBack.gameObject.SetActive(false);
            cardFront.gameObject.SetActive(true);
        }

    }

    public int GetSourceInt()
    {
        int sInt = 0;
        if (mySource == "Cold")
        {
            sInt = 1;
        }
        else if (mySource == "Force")
        {
            sInt = 2;
        }
        else if (mySource == "Bio")
        {
            sInt = 3;
        }
        else if (mySource == "Light")
        {
            sInt = 4;
        }
        return sInt;
    }

    public void TurnOnEffect()
    {
        //highlightPrefab.SetActive(true);
        highlightPrefab.transform.GetChild(GetSourceInt()).gameObject.SetActive(true);
    }

    public void TurnOffEffect()
    {
        //highlightPrefab.SetActive(false);
        highlightPrefab.transform.GetChild(GetSourceInt()).gameObject.SetActive(false);
    }

    public void TurnOnAttack(Vector3 pos, float tMax)
    {
        audioS.clip = attackClips[GetSourceInt()];
        audioS.Play();
        //;
           
        //maxAttackTime = tMax;
        attacking = true;
        //TurnOffEffect();
        highlightPrefab.SetActive(true);
        ParticleSystem ps = attackPrefab.transform.GetChild(GetSourceInt()).GetComponent<ParticleSystem>();
        ParticleSystem.CollisionModule cm = ps.collision;
        
        cm.collidesWith = LayerMask.GetMask("TargetCard");
        
        attackPrefab.transform.GetChild(GetSourceInt()).gameObject.SetActive(true);

        ParticleSystem.VelocityOverLifetimeModule vm = ps.velocityOverLifetime;
        Vector3 diffPos = -(attackPrefab.transform.position - pos).normalized;
        ParticleSystem.MainModule mm = ps.main;

        if (GetSourceInt() == 0)
        {

            vm.x = 10 * diffPos.x;
            vm.y = 10 * diffPos.y;
        }

        if (GetSourceInt() == 1)
        {

            vm.x = 30 * diffPos.x;
            vm.y = 30 * diffPos.y;
        }

        if (GetSourceInt() == 2)
        {

            vm.x = 5 * diffPos.x;
            vm.y = 5 * diffPos.y;
            mm.startRotation = -Mathf.Atan2(diffPos.x, diffPos.y);
        }


        if (GetSourceInt() == 3)
        {
            vm.x = 7 * diffPos.x;
            vm.y = 7 * diffPos.y;
            vm.orbitalX = 3f * diffPos.x;
            vm.orbitalY = 3f * diffPos.y;
        }

        if (GetSourceInt() == 4)
        {
            ParticleSystem ps2 = ps.transform.GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem.VelocityOverLifetimeModule vm2 = ps2.velocityOverLifetime;
            mm.startRotation = Mathf.Atan2(diffPos.x, diffPos.y);
            vm.x = 10 * diffPos.x;
            vm.y = 10 * diffPos.y;
            vm2.orbitalX = 1f * diffPos.x/Mathf.Sqrt(diffPos.x*diffPos.x+diffPos.y*diffPos.y);
            vm2.orbitalY = 1f * diffPos.y/Mathf.Sqrt(diffPos.x * diffPos.x + diffPos.y * diffPos.y);

        }
    }

    public void TurnOffAttack()
    {
        attackPrefab.transform.GetChild(GetSourceInt()).gameObject.SetActive(false);
        attacking = false;
    }

    public void TurnOnDamage(int dmgInt)
    {
        beingAttacked = true;
        damageInt = dmgInt;
        //Debug.Log(dmgInt);
        damagePrefab.transform.GetChild(damageInt).gameObject.SetActive(true);
    }

    public void TurnOffDamage()
    {
        damagePrefab.transform.GetChild(damageInt).gameObject.SetActive(false);
        beingAttacked = false;
    }

    public void KillCard()
    {

        // set up the ability to explode when destroyed
        _explodable = this.gameObject.AddComponent<Explodable>();
        _explodable.shatterType = Explodable.ShatterType.Voronoi;
        _explodable.extraPoints = 8;
        _explodable.allowRuntimeFragmentation = true;
        _explodable.deleteFragments();
        _explodable.explode();
       

        foreach (GameObject go in _explodable.fragments)
        {
            
            go.transform.GetComponent<MeshRenderer>().sortingOrder = 1;
            go.transform.GetComponent<Rigidbody2D>().drag = 3;
            go.transform.GetComponent<Rigidbody2D>().angularDrag = 3;
            go.transform.GetComponent<Rigidbody2D>().gravityScale = 0.2f;
            go.transform.GetComponent<Rigidbody2D>().mass = this.transform.GetComponent<Rigidbody2D>().mass / _explodable.fragments.Count;
            go.AddComponent<KillMeSoon>();
            go.GetComponent<KillMeSoon>().Setup(1);
        }

    }

}





//Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


//if (Input.GetMouseButtonDown(0))
//{
//    if(coll == Physics2D.OverlapPoint(mousePos)){

//        canMove = true;
//        offset = mousePos - new Vector2(transform.position.x, transform.position.y);
//        audioS.clip = cardSounds[0];
//        audioS.Play();

//        // this needs to move with the card, but not be attached as a child (to get the flames to drift) 
//        if(effectClone==null)
//        effectClone = Instantiate(selectEffects, CalcParticlePos(mousePos, offset), Quaternion.identity);
//    }
//    else{

//        canMove = false;

//    }

//    if(canMove){
//        dragging = true;
//    }
//}

//if(dragging){
//    transform.position = CalcCardPos(mousePos,offset);
//}

//if(Input.GetMouseButtonUp(0))
//{
//    canMove = false;
//    dragging = false;
//    // if there's a particle effect, kill it
//    if (effectClone != null) Destroy(effectClone.gameObject);
//    audioS.clip = cardSounds[1];
//    audioS.Play();
//}

//if (dragging == true)
//{
//    effectClone.transform.position = CalcParticlePos(mousePos,offset);
//}
