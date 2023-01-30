using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPopupController : MonoBehaviour
{

    private float disappearTimer = 1f;
    public TextMesh popText;
    private Color txtColor;
    private float moveYSpeed = 2f;
    private float disa = 1;

    public void Setup(int dmgAmount)
    {
        
        popText.text = "-" + dmgAmount.ToString();
        txtColor = popText.color;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;

        if (disappearTimer <= 0)
        {
            txtColor.a -= disa * Time.deltaTime;
            popText.color = txtColor;

            if (txtColor.a < 0)
            {
                Destroy(this.gameObject);
            }
        }

    }

    

}
