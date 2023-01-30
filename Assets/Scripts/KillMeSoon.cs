using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillMeSoon : MonoBehaviour
{
    public float killTimer=1f;
    // Start is called before the first frame update
    public void Setup(int kTime)
    {
        killTimer = kTime;
    }

    // Update is called once per frame
    void Update()
    {
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            Destroy(this.gameObject);
            //Color col = this.transform.GetComponent<SpriteRenderer>().color; 
            //col.a -= Time.deltaTime;
            //this.transform.GetComponent<SpriteRenderer>().color = col;
            //if (col.a <= 0) Destroy(this.gameObject);
        }
    }
}
