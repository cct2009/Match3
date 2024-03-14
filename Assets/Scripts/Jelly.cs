using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Linq;

public class Jelly : MonoBehaviour
{
    public bool shortModule=true;
    public BoxSubType subType;
    GridLayer gridLayer;
    Background[,] backgrounds;
    public Background background;
    
    private void Start() {
        if (!shortModule)
        {
            gridLayer = Main.Instance.gridLayer;
            backgrounds = gridLayer.backgrounds;    

        }
    }

    
  

    // Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};
    // static int count = 20;
    // void FillJellyInBlank(List<Background> list)
    // {
    //     Background bBackground;
    //     List<Background> bBackgrounds = new List<Background>();
    //     foreach(Background background in list)
    //     {
    //         if (FillJellyInBlank(background,out bBackground))
    //             bBackgrounds.Add(bBackground);
    //     }
    //     // ลบอันที่ถูกแทนที่แล้วออก
    //     List<Background> movedList = new List<Background>();
    //     foreach(Background background in list)
    //     {
    //         if (background.jelly)
    //             movedList.Add(background);

    //     }
    //     list = list.Except(movedList).ToList();
    //     list = list.Union(bBackgrounds).ToList();
    //     count--;
    //     if (count > 0)
    //     {
    //         FillJellyInBlank(list);
    //     }
            
    // }

    
    // bool FillJellyInBlank(Background background,out Background bBackground)
    // {
    //     Vector2Int pos = background.pos;
    //     foreach(Vector2Int dir in dirs)
    //     {
    //         Vector2Int pos1 = pos+dir;   // อันที่อยู่ตำแหน่งซ้าย ขวา บน ล่าง
    //         if (ValidPos(pos1)) // ต้องอยู่ในกริด
    //         {
    //             Background canMoveBackground = backgrounds[pos1.x,pos1.y];
    //             if (canMoveBackground.jelly)  // ต้องไม่ใช่อันว่าง
    //             {
    //                 Vector2Int destPos = pos1+ canMoveBackground.flow; // บวกกับทิศทาง flow
    //                 if (ValidPos(destPos) &&  destPos == pos) // ถ้าเท่ากันแสดงว่า ไหลจากตำแหน่ง pos1 มาที่ destPos
    //                 {   
    //                     bBackground = canMoveBackground;
    //                     canMoveBackground.jelly.MoveToBlank(destPos);
    //                     return true;

    //                 }

    //             }

    //         }
    //     }
    //     bBackground = null;
    //     return false;
    // }

    // void MoveToBlank(Vector2Int destPos) 
    // { // ย้าย jelly ไปที่ตำแหน่ง destPos 
    //     Background background2 = backgrounds[destPos.x, destPos.y];
    // //    if (background2.jelly != null) yield return null; // ตำแหน่งที่ย้ายไป ต้องเป็น blank
    //     transform.DOMove(background2.transform.position, 0.3f);

        
    //     background2.jelly =  this;
    //     background = background2;
    //     background.jelly = null;

    // }
    


    public void DestoryJelly()
    {
        background.jelly = null;
        background = null; // จะได้ไ่ม่มีการใช้ jelly อันนี้อีก
        
        Destroy(gameObject,1f);
    }

    public void  AnimationMatch(List<Jelly> list)
    {
        foreach(Jelly jelly in list)
        {
            jelly.transform.DOScale(0,0.5f);
        }
//        yield return new WaitForSeconds(0.6f);

    }

    public void DrawBorder(List<Jelly> list,LineRenderer lr)
    {
        int min, max;
        lr.sortingOrder  = 4;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = 5;

        SpriteRenderer sr = backgrounds[list[0].background.pos.x,list[0].background.pos.y].GetComponent<SpriteRenderer>();
        Debug.Log("Bound size:"+sr.bounds.size.x+","+sr.bounds.size.y);
        if (list[0].background.pos.x == list[1].background.pos.x) // vertical border
        {
            min = max = list[0].background.pos.y;

            foreach(Jelly jelly in list)
            {
                if (jelly.background.pos.y < min) min = jelly.background.pos.y;
                if (jelly.background.pos.y > max) max = jelly.background.pos.y;
            }
            Debug.Log("From X:" + list[0].background.pos.x + " To Y:"+ min+","+max);
            Vector3 point1 = backgrounds[list[0].background.pos.x, min].transform.position-new Vector3(sr.bounds.size.x/2,sr.bounds.size.y/2,0);
            Vector3 point2 = backgrounds[list[0].background.pos.x, max].transform.position-new Vector3(sr.bounds.size.x/2,-sr.bounds.size.y/2,0);
            lr.SetPosition(0,point1);
            lr.SetPosition(1,point2);
            Vector3 point3 = point2+ new Vector3(sr.bounds.size.x,0,0);
            lr.SetPosition(2,point3);
            Vector3 point4 = new Vector3(point3.x, point1.y,0);
            lr.SetPosition(3,point4);
            lr.SetPosition(4,point1);
            
        }
        else
        {
            min = max = list[0].background.pos.x;

            foreach(Jelly jelly in list)
            {
                if (jelly.background.pos.x < min) min = jelly.background.pos.x;
                if (jelly.background.pos.x > max) max = jelly.background.pos.x;
            }
            Debug.Log("From Y:" + list[0].background.pos.y + " To X:"+ min+","+max);
            Vector3 point1 = backgrounds[min,list[0].background.pos.y].transform.position+new Vector3(-sr.bounds.size.x/2,sr.bounds.size.y/2,0);
            Vector3 point2 = backgrounds[max,list[0].background.pos.y].transform.position+new Vector3(sr.bounds.size.x/2,sr.bounds.size.y/2,0);;
            lr.SetPosition(0,point1);
            lr.SetPosition(1,point2);
            Vector3 point3 = point2 - new Vector3(0, sr.bounds.size.y,0);
            Vector3 point4 = new Vector3(point1.x,point3.y);
            lr.SetPosition(2,point3);
            lr.SetPosition(3,point4);
            lr.SetPosition(4,point1);
        }
//            lr.SetPosition(i,ly.transform.position);

    }
   void PrintLayerList(List<Jelly> list,string pos)
   {
        string s = pos + " "+ list.Count+"-";
        int i =0;
        foreach(Jelly jelly in list)
        {
            s += jelly.subType + "["+ jelly + "],";
            i++;
        }
            
        Debug.Log(s);
   }

    public bool checkMatch(out List<Jelly> matchList)
    {
        List<Jelly> list1, list2;
        list1 = matchAlong(Vector2Int.left, Vector2Int.right);
        list2 = matchAlong(Vector2Int.up, Vector2Int.down);
        if (list1.Count >= 2 || list2.Count >= 2)
        {
            if (list1.Count >= 2 && list2.Count >=2)
            {
                matchList =  list1.Union(list2).Distinct().ToList();
                PrintLayerList(matchList, "HOR+VER");
            }
                
            else if (list1.Count >= 2)
            {
                matchList = list1;
                PrintLayerList(matchList, "HOR");
            }
            else
            {
                matchList = list2;
                PrintLayerList(matchList, "VER");
            }
                
            matchList.Add(this);
            return true;
        }
        list1.Clear();
        list2.Clear();
        matchList = list1;
        return false;
    }
    private List<Jelly> matchAlong(Vector2Int direction1, Vector2Int direction2)
    {
        return matchOn(direction1).
                    Union(matchOn(direction2)).Distinct().ToList();
    }
    private List<Jelly> matchOn(Vector2Int direction)
    {
        List<Jelly> matchList = new List<Jelly>();
        Vector2Int checkPos = background.pos+direction;
        while (ValidPos(checkPos))
        {
            Jelly  checkItem = backgrounds[checkPos.x,checkPos.y].jelly;
            if (checkItem == null) break;
            if (subType != checkItem.subType) break;
            matchList.Add(checkItem);
            checkPos = checkPos+direction;
        }
        return matchList;
    }
    private bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }


    

}
