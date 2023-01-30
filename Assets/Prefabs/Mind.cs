using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mind : MonoBehaviour
{
    public int[] affinity;
    public List<Sprite> spriteList;
    public Image affinityImage;
    public Sprite noneImage;
    public bool isFilled = false;

    

    // Start is called before the first frame update
    void Start()
    {
        affinity = new int[] { 0,0,0,0,0};

        DisplayAffinity();
        
    }

    public int[] GetAffinityVector()
    {
        return affinity;
    }

    public int GetAffinity(int indx)
    {
        return affinity[indx];
    }

    public void AddAffinity(int addAff, int sourceInt)
    {

        affinity[sourceInt] += addAff;
        if (affinity[sourceInt] > 3)
        {
            affinity[sourceInt] = 3;
        }
 
        DisplayAffinity();
    }

    void DisplayAffinity()
    {

        Transform mPanT = this.gameObject.transform.GetChild(0).transform.GetChild(0);

        
        //int bIndx = 0;
        //int aVal = 0;
        
            for (int i = 0; i < affinity.Length; i++)
            {
                if (affinity[i] > 0)
                {
                    for(int j=0; j < affinity[i]; j++)
                    {
                        mPanT.GetChild(i).GetChild(j).gameObject.SetActive(true);
                    }
                    //
                }
            
        //        if (affinity[i] > aVal)
        //        {
        //            aVal = affinity[i];
        //            bIndx = i;
        //        }
            }

        //// set the new affinity image
        //if (aVal > 0)
        //{
        //    affinityImage.sprite = spriteList[bIndx];
        //}
        //else
        //{
        //    affinityImage.sprite = noneImage;
        //}
    }

    private void OnMouseOver()
    {
               
        DisplayAffinity();


    }

}
