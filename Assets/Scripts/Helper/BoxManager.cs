using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BoxType 
{
    Vacant = 0,
    Jelly =10,
    Powerup = 20,
    Obstruct = 30,
    armer = 40,
    CookieTray = 50,
    Grass = 70,

}
public enum BoxSubType
{
    Vacant = 0,
    Close = 1,

    JellyRandom = 10,
    JellyRed = 11,
    JellyGreen = 12,
    JellyBlue = 13,
    JellyOrange = 14,
    JellyPink =15,
    JellyCyan = 16,



    PowerUpVer = 21,
    PowerUpHor = 22,
    PowerUpPoint = 23,
    PowerUpBomp = 24,
    PowerUpGlobe = 25,

    ObstructWood = 31,
    ObstructPenguine = 32,
    ObstructStone = 33,
    ObstructBread = 34,

    ArmerIce = 41,
    ArmerChain = 42,

    ArrowDown = 100,
    ArrowLeft = 101,
    ArrowRight = 102,
    ArrowUp = 103

}
[Serializable]
public class BoxInfo 
{
    public int x, y;
    public BoxSubType subType;

}
[Serializable]
public class BoxData
{
    public int rows;
    public int columns;
    public List<BoxInfo> layer1;
    public List<BoxInfo> layer2;
    public List<BoxInfo> layer3;
}

public class BoxManager : MonoBehaviour
{
    public Image img;
    public int subType;
    public TMP_Text subTypeIn;    
    public int level;
    private void Start() {
        
    }


private void Update() {
    if (Input.GetMouseButtonDown(0))
    {
        RaycastHit2D hit;

        hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (hit.transform == transform)
        {
            ShowBoxDetails();
            BoxSubType curSubType = (BoxSubType) int.Parse(subTypeIn.text);
            if (GetSubTypePos(curSubType) >= 0) return;
            BoxManager oldBox = GetComponent<BoxManager>();
            switch(curSubType)
            {
                
                case BoxSubType.ArmerChain:
                case BoxSubType.ArmerIce:
                    GameObject gm2 = Instantiate(transform.gameObject, transform.position,Quaternion.identity, transform.parent.transform);
                    BoxManager box1 = gm2.GetComponent<BoxManager>();
                    box1.level = oldBox.level + 1;
                    box1.subType = (int) curSubType;
                    gm2.name = "BOX"+box1.level.ToString()+ oldBox.name.Substring(4);

                    SpriteRenderer sr2 = gm2.GetComponent<SpriteRenderer>();
                    sr2.sortingOrder = box1.level;
                    sr2.sprite = img.sprite;
                    sr2.color = img.color;    
                    

                break;

                default :
                if (oldBox.level == 1)
                    Level1SubType();
                  break;

            }
            ShowBoxDetails();

        }

    }
}

int GetSubTypePos(BoxSubType subType)
{
    for (int i = 1; i <= 5; i++)
    {
        GameObject gm;
        string name = "BOX"+ i.ToString();
        
        gm = GameObject.Find(name);
        if (gm)
        {   
            BoxManager box1 = gm.GetComponent<BoxManager>();
            if (box1.subType == (int) subType)
            {
                return i;
            }
                
        }
    }
    return -1;
}

void ShowBoxDetails()
{
    string name = transform.name;

    for (int i = 1; i <= 5; i++)
    {
        SpriteRenderer sr1,sr2;
        GameObject gm;
        BoxManager box1,box2;

        name = "BOX"+ i.ToString() + name.Substring(4);
      
        gm = GameObject.Find(name);
        string name2 = "BOX" +i.ToString();
        sr2 = GameObject.Find(name2).GetComponent<SpriteRenderer>();
        box2 = GameObject.Find(name2).GetComponent<BoxManager>();
        if (gm)
        {   
            box1 = gm.GetComponent<BoxManager>();
            sr1 = gm.GetComponent<SpriteRenderer>();
            if (sr2 != null)
            {
                sr2.sprite = sr1.sprite ;
                box2.subType = box1.subType;
                box2.level = box1.level;
            }
                
        }
        else
        {
            sr2.sprite = null;
            box2.subType = (int) BoxSubType.Vacant;
            box2.level = 0;
        }
    }
    

    
}
void Level1SubType()
{
    if (transform.name.IndexOf("BOX1") >= 0)
    {
        SpriteRenderer sr;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = img.sprite;
        sr.color = img.color;    
        subType = int.Parse(subTypeIn.text);

    }

}

}



