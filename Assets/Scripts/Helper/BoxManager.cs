using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using UnityEditor.Experimental.GraphView;


public class BoxManager : MonoBehaviour
{
    public Image img;
    public int subType;
    public bool main;
    public TMP_Text subTypeIn;    
    public int level;



private void Update() {
    if (Input.GetMouseButtonDown(0))
    {
        RaycastHit2D hit;

        hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (hit.transform == transform)
        {
            ShowBoxDetails();
            BoxType curSubType = (BoxType) int.Parse(subTypeIn.text);
            if (GetSubTypePos(curSubType) >= 0) return;
            BoxManager box = GetComponent<BoxManager>();
            SpriteRenderer sr1 = box.GetComponent<SpriteRenderer>();
            switch(curSubType)
            {
                
                case BoxType.ArmerChain:
                case BoxType.ArmerIce:
                    GameObject gm2 = Instantiate(transform.gameObject, transform.position,Quaternion.identity, transform.parent.transform);
                    BoxManager box1 = gm2.GetComponent<BoxManager>();
                    box1.level = box.level + 1;
                    box1.subType = (int) curSubType;
                    gm2.name = "BOX"+box1.level.ToString()+ box.name.Substring(4);

                    SpriteRenderer sr2 = gm2.GetComponent<SpriteRenderer>();
                    sr2.sortingOrder = box1.level;
                    sr2.sprite = img.sprite;
                    sr2.color = img.color;    
                    

                break;
                case BoxType.CookieTray:
                    Bounds bound1 = sr1.bounds;
                    transform.position = new Vector3(transform.position.x+bound1.size.x/2 , transform.position.y -bound1.size.y/2,0);
                    transform.localScale = new Vector3(transform.localScale.x * 2 , transform.localScale.y * 2, 0);
                    main = true;
                    SetOtherBox(Vector2Int.right);
                    SetOtherBox(Vector2Int.down);
                    SetOtherBox(Vector2Int.down+Vector2Int.right);
                if (box.level == 1)
                    Level1SubType();
                break;
                default :
                if (box.level == 1)
                    Level1SubType();
                  break;

            }
            ShowBoxDetails();

        }

    }
}
void SetOtherBox(Vector2Int dir)
{
    
}
int GetSubTypePos(BoxType subType)
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
            box2.subType = (int) BoxType.Vacant;
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



