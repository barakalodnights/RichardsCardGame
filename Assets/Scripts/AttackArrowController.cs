using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArrowController : MonoBehaviour
{
    float timer = 0f;
    float timerMax = 1f;
    bool appear = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (appear)
        {
            timer += Time.deltaTime;

            if (timer > timerMax)
            {
                appear = false;
                this.gameObject.SetActive(false);
                timer = 0f;
            }
        }

        
    }

    public void AnimateAttack(Vector3 pos1, Vector3 pos2)
    {
        float phi = Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x);
        
        //transform.position -= Vector3.up;
        transform.SetPositionAndRotation(( pos1 + pos2) / 2, new Quaternion(0,0,Mathf.Sin(phi/2),Mathf.Cos(phi/2)));

        appear = true;
        this.gameObject.SetActive(true);
    }
}
