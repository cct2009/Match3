using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;
using System.Linq;




[Serializable]
public enum BoxType
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

    CookieTray = 51,
    DisplayCard = 52,
    ArrowDown = 100,
    ArrowLeft = 101,
    ArrowRight = 102,
    ArrowUp = 103

}
[Serializable]
public class BoxInfo 
{
    public int x, y;
    public BoxType type;
    public Vector2Int dir;
    public int start;

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
    ROW = 4,
    COL = 5,
}
public struct MatchInfo {
    public EMatchType type;
    public List<Box> boxList;

}
public class Box : MonoBehaviour
{
    public BoxType type;
    static GridLayer gridLayer;
    static Background[,] backgrounds;
    public Background background;
    public bool move;
    public int live;
    public GameObject DieAnimate;

    public EBoxState boxState;
    
    public Vector2Int dir;
    public int start;
    private void Start() {
            gridLayer = Global.gridLayer;
            backgrounds = gridLayer.backgrounds;    
    }
    
    private void TranzformMatch(ref MatchInfo mif)
    {   // เปลี่ยน jelly เป็น powerup กรณี match 4, 5 
        // ตั้งค่า boxState เป็น Die หรือ Live
//        mif.boxList.Add(this);
        switch(mif.boxList.Count)
        {
            case 4: if (mif.type ==  EMatchType.SQUARE)
                        ReplaceJelly(BoxType.PowerUpPoint);
                    else if (mif.type == EMatchType.VER || mif.type == EMatchType.HOR)
                        ReplaceJelly(RandomPower4());
            boxState =  EBoxState.Live;
            break;
            case 5: 
            if (mif.type == EMatchType.VER || mif.type == EMatchType.HOR)
                ReplaceJelly(BoxType.PowerUpGlobe);
            boxState =  EBoxState.Live;
            break;
        }
        foreach(Box box  in mif.boxList)
        {
            if (box.boxState != EBoxState.Live)
                box.boxState = EBoxState.Die;
        }
        

    }
    public void checkPower4Match(ref List<MatchInfo> mifList)
    {
        if (!IsPower4()) return;
        if (type == BoxType.PowerUpVer)
            matchEntireCol(ref mifList);
        else
            matchEntireRow(ref mifList);
    }

    private void matchEntireCol(ref List<MatchInfo> mifList)
    {
        List<Box> boxList = new List<Box>();
        for (int row = 0; row < gridLayer.maxY; row++)
        {
            Box box1 = backgrounds[background.pos.x, row].box;
            if (box1 && box1.background.type == EBackgroundType.Fill)
            {
                if (box1.isEffect(box1.type))
                    box1.boxState = EBoxState.Minus;
                else 
                    box1.boxState = EBoxState.Die;
                boxList.Add(box1);
            }
        }
        MatchInfo mif = new MatchInfo();
        mif.type = EMatchType.COL;
        mif.boxList = boxList;
        mifList.Add(mif);


    }

    private void matchEntireRow(ref List<MatchInfo> mifList)
    {
        List<Box> boxList = new List<Box>();
        for (int col = 0; col < gridLayer.maxX; col++)
        {
            Box box1 = backgrounds[col, background.pos.y].box;
            if (box1 && box1.background.type == EBackgroundType.Fill)
            {
                if (box1.isEffect(box1.type))
                    box1.boxState = EBoxState.Minus;
                else
                    box1.boxState = EBoxState.Die;
                boxList.Add(box1);
            }
        }
        MatchInfo mif = new MatchInfo();
        mif.type = EMatchType.COL;
        mif.boxList = boxList;
        mifList.Add(mif);

        
    }
    public void checkJellyMatch(ref List<MatchInfo> mifList,bool tranzform=true)
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
                mif.boxList.Add(this);
                if (tranzform)
                    TranzformMatch(ref mif);
                mifList.Add(mif);
                PrintMatchInfo(mif);
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

        if (!checkType(dir1)) return false;
        if (!checkType(dir2)) return false;
        if (!checkType(sum)) return false;

        boxList.Add(GetBoxAt(dir1));
        boxList.Add(GetBoxAt(dir2));
        boxList.Add(GetBoxAt(sum));
        return true;
    }
    public Box GetBoxAt(Vector2Int dir)
    {
        Vector2Int pos = background.pos + dir;
        return backgrounds[pos.x,pos.y].box;
    }
    bool checkType(Vector2Int dir)
    {
        Vector2Int pos = background.pos + dir;
        if (!Global.ValidPos(pos)) return false;
        return backgrounds[pos.x,pos.y].box.type == type;
        
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
        while (Global.ValidPos(checkPos))
        {
            Box  checkItem = backgrounds[checkPos.x,checkPos.y].box;
            if (checkItem == null) break;
            if (type != checkItem.type) break;
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
            {
                GameObject prefab = Global.Instance.file.GetDieAnimate(box.type);
                if (prefab == null) prefab = Main.Instance.animate;
                GameObject go = Instantiate(prefab, box.transform.position, box.transform.rotation);
                box.transform.DOScale(0,Main.Instance.dieSpeed);
                Destroy(go,1);
            }
                
        }
//        yield return new WaitForSeconds(0.6f);

    }
    public bool isEffect(BoxType type)
    {
        return (type == BoxType.ObstructWood ||
                type == BoxType.CookieTray  ||
                type == BoxType.ObstructStone ||
                type == BoxType.DisplayCard);
    }
    public bool isJelly()
    {
        return (type >= BoxType.JellyRed && type <= BoxType.JellyCyan);
    }
    private void ReplaceJelly(BoxType type)
    {
        this.type = type;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = Global.Instance.file.GetSprite(type);

    }
    private BoxType RandomPower4()
    {
        int r = UnityEngine.Random.Range(0,2);
        return (r == 0? BoxType.PowerUpVer: BoxType.PowerUpHor);
    }

    public bool IsPower4()
    {
        return (type == BoxType.PowerUpVer || type == BoxType.PowerUpHor);
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
   public void PrintMatchInfo(MatchInfo mif)
   {
        string s = mif.type + " "+ mif.boxList.Count+"-";
        int i =0;
        foreach(Box box in mif.boxList)
        {
            s += box.background.pos + "["+box.type + "],";
            i++;
        }
            
        Debug.Log(s);
   }

    

}
