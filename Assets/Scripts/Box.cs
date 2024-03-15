using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public enum EBoxState {
    Normal = 0,
    Live = 1,
    Die = 2,
    Minus = 3,
}
public enum EMatchType {
    HOR=0,
    VER=1,
    SQUARE = 2,
    BOMB = 3,
}
public struct MatchInfo {
    public EMatchType type;
    public List<Box> boxList;

}
public class Box : MonoBehaviour
{
    public BoxSubType subType;
    static GridLayer gridLayer;
    static Background[,] backgrounds;
    public Background background;
    public bool move;
    public int live;

    public EBoxState boxState;
    
    private void Start() {
            gridLayer = Global.Instance.gridLayer;
            backgrounds = gridLayer.backgrounds;    
    }
    
    private void TranzformMatch(ref MatchInfo mif)
    {   // เปลี่ยน jelly เป็น powerup กรณี match 4, 5 
        // ตั้งค่า boxState เป็น Die หรือ Live
        mif.boxList.Add(this);
        switch(mif.boxList.Count)
        {
            case 4: if (mif.type ==  EMatchType.SQUARE)
                        ReplaceJelly(BoxSubType.PowerUpPoint);
                    else
                        ReplaceJelly(RandomPower4());
            boxState =  EBoxState.Live;
            break;
            case 5: ReplaceJelly(BoxSubType.PowerUpGlobe);
            boxState =  EBoxState.Live;
            break;
        }
        foreach(Box box  in mif.boxList)
        {
            if (box.boxState != EBoxState.Live)
                box.boxState = EBoxState.Die;
        }
        

    }
    
    public void checkJellyMatch(ref List<MatchInfo> mifList)
    {
        if (!isJelly()) return;

        List<Box> boxList;
        for (int i = 0; i < 3 ; i++)
        {
            switch(i)
            {
                case 0:
                    boxList = matchAlong(Vector2Int.left, Vector2Int.right);
                break;

                case 1:
                    boxList = matchAlong(Vector2Int.up, Vector2Int.down);
                break;
                default:
                    boxList = matchSquare();
                break;
            }
                
            if (boxList.Count >= 2)
            {   // แสดงว่า Match 3 อย่างน้อง
                MatchInfo mif = new MatchInfo();
                mif.type = i==0? EMatchType.HOR: i==1? EMatchType.VER : EMatchType.SQUARE;
                mif.boxList = boxList;
                TranzformMatch(ref mif);
                mifList.Add(mif);
                PrintLayerList(mif);
            }

        }

    }
    private List<Box> matchSquare()
    {
        List<Box> boxList = new List<Box>();

        if (!matchSquareOn(Vector2Int.left, Vector2Int.up,ref boxList))
        if (!matchSquareOn(Vector2Int.right, Vector2Int.up,ref boxList))
        if (!matchSquareOn(Vector2Int.left, Vector2Int.down,ref boxList)) 
        if (!matchSquareOn(Vector2Int.right, Vector2Int.down,ref boxList)) 
               return boxList;
        return boxList;
    }
    private bool matchSquareOn(Vector2Int dir1, Vector2Int dir2, ref List<Box> boxList)
    {
        Vector2Int sum = dir1+dir2;

        if (!checkSubType(dir1)) return false;
        if (!checkSubType(dir2)) return false;
        if (!checkSubType(sum)) return false;

        boxList.Add(GetBoxAt(dir1));
        boxList.Add(GetBoxAt(dir2));
        boxList.Add(GetBoxAt(sum));
        return true;
    }
    Box GetBoxAt(Vector2Int dir)
    {
        Vector2Int pos = background.pos + dir;
        return backgrounds[pos.x,pos.y].box;
    }
    bool checkSubType(Vector2Int dir)
    {
        Vector2Int pos = background.pos + dir;
        if (!ValidPos(pos)) return false;
        return backgrounds[pos.x,pos.y].box.subType == subType;
        
    }
    private List<Box> matchAlong(Vector2Int direction1, Vector2Int direction2)
    {
        return matchOn(direction1).
                    Union(matchOn(direction2)).Distinct().ToList();
    }
    private List<Box> matchOn(Vector2Int direction)
    {
        List<Box> matchList = new List<Box>();
        Vector2Int checkPos = background.pos+direction;
        while (ValidPos(checkPos))
        {
            Box  checkItem = backgrounds[checkPos.x,checkPos.y].box;
            if (checkItem == null) break;
            if (subType != checkItem.subType) break;
            matchList.Add(checkItem);
            checkPos = checkPos+direction;
        }
        return matchList;
    }
    

    public void DestroyBox()
    {
        if (background)
        {
            background.box = null;
            background = null; // จะได้ไ่ม่มีการใช้ jelly อันนี้อีก
        }
        
        Destroy(gameObject,1);
    }

    public void  AnimationMatch(List<Box> list)
    {
        foreach(Box box in list)
        {
            if (box.boxState == EBoxState.Die)
                box.transform.DOScale(0,Main.Instance.dieSpeed);
        }
//        yield return new WaitForSeconds(0.6f);

    }

    public bool isJelly()
    {
        return (subType >= BoxSubType.JellyRed && subType <= BoxSubType.JellyCyan);
    }
    private void ReplaceJelly(BoxSubType subType)
    {
        this.subType = subType;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = Global.Instance.file.GetSprite(subType);

    }
    private BoxSubType RandomPower4()
    {
        int r = Random.Range(0,2);
        return (r == 0? BoxSubType.PowerUpVer: BoxSubType.PowerUpHor);
    }

    private bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }

  
    
public void DrawBorder(List<Box> list,LineRenderer lr)
    {
        int min, max;
        lr.sortingOrder  = 4;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = 5;

        SpriteRenderer sr = backgrounds[list[0].background.pos.x,list[0].background.pos.y].GetComponent<SpriteRenderer>();

        if (list[0].background.pos.x == list[1].background.pos.x) // vertical border
        {
            min = max = list[0].background.pos.y;

            foreach(Box box in list)
            {
                if (box.background.pos.y < min) min = box.background.pos.y;
                if (box.background.pos.y > max) max = box.background.pos.y;
            }
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

            foreach(Box box in list)
            {
                if (box.background.pos.x < min) min = box.background.pos.x;
                if (box.background.pos.x > max) max = box.background.pos.x;
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
   void PrintLayerList(MatchInfo mif)
   {
        string s = mif.type + " "+ mif.boxList.Count+"-";
        int i =0;
        foreach(Box box in mif.boxList)
        {
            s += box.subType + ",";
            i++;
        }
            
        Debug.Log(s);
   }

    

}
