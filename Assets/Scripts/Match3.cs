using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public enum EPointType 
{
    Enter = 0,
    Exit = 1,
    Flow = 2,
    Close = 3,
    Unknown = 4,

}
public class Match3:MonoBehaviour
{
    public Box PrefabBox;
    private FileData file;
    private int rows, columns;
    GridLayer gridLayer;
    Background[,] backgrounds;

    private void Awake() {
        DOTween.Init();   
        
    }
    public void Init (FileData file1  )
    {
        file = file1;
        rows = file1.boxData.rows;
        columns = file1.boxData.columns;
        
    }

    private void Start() {
        gridLayer = Global.gridLayer;
        backgrounds = gridLayer.backgrounds;
    }


    public void FillInBlank()
    {
        List<Box>[] sortBox = new List<Box>[gridLayer.enterPoints.Count];
        int i = 0;
        Box box;
        SpriteRenderer sr;
        float size=0f;
        foreach(Background background in gridLayer.enterPoints)
        {
            sortBox[i] = new List<Box>();
            if (background.box != null) { i++; continue; }
            sr = background.GetComponent<SpriteRenderer>();
            size = sr.size.x;
            Vector2 incr1 = background.flow * new Vector2(-1,-1) * sr.bounds.size.x;
            while ( (box = GetInnerMost(background)) != null)
                sortBox[i].Add(box);
            
            Vector3 pos = background.transform.position;
            Vector3 newPos = pos;
            // ย้ายตำแหน่ง 
            foreach(Box box1 in sortBox[i])
            {
                newPos += new Vector3(incr1.x,incr1.y,0);
                box1.transform.position = newPos;
            }
            i++;
        }

        for (i=0; i < sortBox.Length; i++)
        {
            if (sortBox[i].Count > 0){
                StartCoroutine(FillByRows(sortBox[i], gridLayer.enterPoints[i]));
                Debug.Log("FillByRows with "+sortBox[i].Count+" items");
            }
                
        }
        
        
    }
    IEnumerator FillByRows(List<Box> boxList, Background runPoint)
    {
        float time = 0.1f;
        List<Background> backList = new List<Background>();


        SpriteRenderer sr = runPoint.GetComponent<SpriteRenderer>();
        Vector2 incr = runPoint.flow * new Vector2(1,1) * sr.bounds.size.x;

        for (int i = 0; i < boxList.Count; i++)
        {
            backList.Add(runPoint);
            backList.Reverse();
            for (int j = 0; j < boxList.Count; j++)
            {
                Box box = boxList[j];
                Vector3 pos;
                if (j < backList.Count)
                    pos = backList[j].transform.position;
                else
                    pos = box.transform.position + new Vector3(incr.x, incr.y,0);
                
                box.transform.DOMove(pos, time);
            }
            backList.Reverse();
            runPoint = runPoint.getNext();
            if (runPoint == null)
                Debug.Log("Program Bug!! it must not be null");
            yield return new WaitForSeconds(time);

        }
        yield return null;
            // goto next runpoint
    }
    IEnumerator FillByRows_1(List<Box> boxList, Background runPoint)
    {
        float time = 0.5f;


        foreach(Box box in boxList)
        {
            box.transform.DOMove(box.background.transform.position,time);

        }
        yield return null;
            // goto next runpoint
    }
    private Box GetInnerMost(Background background)
    {
        Vector2Int pos;
        Background inner = background;
        while (background.box == null)
        {
            inner = background;
            pos  = background.pos + background.flow;
            background = backgrounds[pos.x,pos.y];
        }
        if (inner.box == null)
        {
            inner.box = Main.Instance.match3.NewJellyRandom(inner);
            return inner.box;
        }
        return null;
    }
    public void FillInBlank_1()
    {
        Background background;
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                background = backgrounds[x,y];
                if (background.box == null) {
                    background.box = Main.Instance.match3.NewJellyRandom(background);
                    background.type = EBackgroundType.Fill;
                }
            }
        }
    }
    public void LoadDirection(int DirectionVersion)
    {
        if (file.onLoadDirection(DirectionVersion))
        {
            foreach(BoxInfo bif in file.boxData.layer1)
            {
                Background background = gridLayer.backgrounds[bif.x,bif.y];
                switch(bif.type)
                {
                    case BoxType.ArrowDown:
                        background.flow = Vector2Int.down;
                    break;
                    case BoxType.ArrowLeft:
                        background.flow = Vector2Int.left;
                    break;
                    case BoxType.ArrowRight:
                        background.flow = Vector2Int.right;
                    break;
                    case BoxType.ArrowUp:
                        background.flow = Vector2Int.up;
                    break;
                    
                }
                SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
                sr.sprite = file.GetSprite(bif.type);

            }
            Debug.Log("Load Direction successed");
            InitPointType();
        }

    }

    private void InitPointType()
    {

        // reset all type
        for (int y=0; y < gridLayer.maxY; y++)
        {
            for (int x=0; x< gridLayer.maxX; x++)
            {
                Background background = backgrounds[x,y];
                background.ptype = background.type == EBackgroundType.Close? EPointType.Close: EPointType.Unknown;
                if (background.ptype == EPointType.Unknown)
                {
                    if (!onePointTo(background))
                    {
                        gridLayer.enterPoints.Add(background);
                        background.ptype = EPointType.Enter;
                    }
                        
                    else if (!onePointFrom(background))
                    {
                        gridLayer.exitPoints.Add(background);
                        background.ptype = EPointType.Exit;
                    }
                        
                    else   
                        background.ptype = EPointType.Flow;
                }
            }
        }
    }
    private bool onePointTo(Background background)
    {
        foreach(Vector2Int dir in Global.dirs)
        {
            Vector2Int newPos = background.pos + dir;
            if (!Global.ValidPos(newPos)) continue;
            Background newBackground = backgrounds[newPos.x,newPos.y];
            if (newBackground.type == EBackgroundType.Close) continue;
            if (newPos + newBackground.flow == background.pos)
                return true;

        }
        return false;
    }
    private bool onePointFrom(Background background)
    {   
        foreach(Vector2Int dir in Global.dirs)
        {
            Vector2Int newPos = background.pos + dir;
            if (!Global.ValidPos(newPos)) continue;
            Background newBackground = backgrounds[newPos.x,newPos.y];
            if (newBackground.type == EBackgroundType.Close) continue;
            if (background.pos + background.flow == newBackground.pos)
                return true;

        }
        return false;
    }
    public void DrawBox()
    {
        foreach(BoxInfo bif in file.boxData.layer1)
        {
            Background background = gridLayer.backgrounds[bif.x,bif.y];
            gridLayer.backgrounds[bif.x, bif.y].box = createABox(background, bif.type,bif.dir,bif.start);
            
        }

        // create randomize box 
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Background background = gridLayer.backgrounds[x,y];
                if (!background.box)
                {
                    BoxType subType = GetSubTypeNotMatch3(file.boxData.rows, file.boxData.columns,y, x, gridLayer.backgrounds);
                    gridLayer.backgrounds[x, y].box =  createBox(background, subType);

                }

            }
        }

    }
    
    private BoxType GetSubTypeNotMatch3(int rows, int cols, int row, int col, Background[,] background)
    {
        
        for (;;)
        {
            BoxType subType = (BoxType) Random.Range((int) BoxType.JellyRed, (int) BoxType.JellyCyan+1);
            
            if (countSubType(Vector2Int.right, rows, cols, row, col, background,subType) +
                countSubType(Vector2Int.left, rows, cols, row, col, background,subType) < 2)
            {
                if (countSubType(Vector2Int.up, rows, cols, row, col, background,subType) + 
                    countSubType(Vector2Int.down, rows, cols, row, col, background,subType) < 2)
                {
                    return subType;
                }
            }

            

        }
        
    }


    private int countSubType(Vector2Int direction,int rows, int cols, int row, int col, Background[,] background, BoxType type)
    {
        Vector2Int pos = new Vector2Int(col,row);
        int count = 0;
        for (pos = pos+direction;;pos = pos+direction)
        {
            if (pos.x < 0 || pos.y < 0) return count;
            if (pos.x >= cols || pos.y >= rows) return count;
            if (background[pos.x,pos.y].box == null) return count;
            if (background[pos.x,pos.y].box.type == type) count++;
            else
                return count;
        }
        
    }

    private void ResetCookieTray(Background background)
    {
        Vector2Int[] pos = {Vector2Int.right, Vector2Int.down, Vector2Int.right+Vector2Int.down};

        for (int i = 0; i< pos.Length; i++)
        {
            Vector2Int pos1 = background.pos+pos[i];
            if (Global.ValidPos(pos1))
            {
                Background background1 = backgrounds[pos1.x,pos1.y];
                if (background1.type == EBackgroundType.Protected)
                {
                    background1.type = EBackgroundType.Vacant;
                    background1.box = null;
                }
                
            }
        }

    }

    public Box createABox(Background background, BoxType type, Vector2Int dir, int start )
    {
         if (type == BoxType.CookieTray)
            background.box = CreateCookieTray(background);
        else if (type == BoxType.DisplayCard)
            background.box = CreateDisplayCard(background, dir, start);
        else
            background.box = createBox(background, type);
        return background.box;

    }
    public Box createBox(Background background, BoxType type,int start=0)
    {
        GameObject panel = GameObject.Find("Panel");
            if (background)
            {
                Box box;
                if (background.box == null)
                    box = Instantiate(PrefabBox, background.transform.position, Quaternion.identity, panel.transform);
                else {
                    box = background.box;
                    if (box.type == BoxType.CookieTray)
                        ResetCookieTray(background);
                }
                    
                SpriteRenderer sr = box.GetComponent<SpriteRenderer>();
                sr.sprite = file.GetSprite(type,start);
                sr.sortingOrder = 2;
                box.name = "Box " + background.pos.x +","+ background.pos.y;
                box.type = type;
                box.live = file.GetLive(type);
                box.move = file.GetMove(type);
                box.boxState = EBoxState.Normal;
                
                box.transform.position = background.transform.position;
                box.transform.localScale = background.transform.localScale;
                box.background = background;

                box.DieAnimate = file.GetDieAnimate(type);

                // add for type = Close
                SpriteRenderer sr2 = background.GetComponent<SpriteRenderer>();
                if (box.type == BoxType.Close)
                {
                    sr2.color = new Color(255,255,255,0);
                    background.type = EBackgroundType.Close;
                }
                else
            {
                sr2.color = new Color(255, 255, 255, 255);
                background.type = EBackgroundType.Fill;
        //        createSpriteMask(background);

            }
            box.dir = Vector2Int.right;
                box.start = 0;
                return box;
            }
            return null;
    }

    private void createSpriteMask(Background background)
    {
        GameObject panel = GameObject.Find("Panel");
        SpriteMask sm = Instantiate(Global.Instance.spm, background.transform.position, Quaternion.identity, panel.transform);
        sm.transform.position = background.transform.position;
        sm.transform.localScale = background.transform.localScale;
    }

    private Box CreateDisplayCard(Background background, Vector2Int dir, int start)
    {
        Box box1;
        int maxBox = 8- start;

        box1 = createBox(background, BoxType.DisplayCard, start);
        SpriteRenderer sr1 = background.GetComponent<SpriteRenderer>();
        SpriteRenderer sr2 = box1.GetComponent<SpriteRenderer>();
        float equal = sr2.bounds.size.x / sr1.bounds.size.x;
        if (dir == Vector2Int.left) 
            box1.transform.RotateAround(box1.transform.position, Vector3.back, 180);
        else if (dir == Vector2Int.up)
            box1.transform.RotateAround(box1.transform.position, Vector3.back, 270);        
        else if (dir == Vector2Int.down)
            box1.transform.RotateAround(box1.transform.position, Vector3.back, 90);        
        if (dir == Vector2Int.left)
            box1.transform.position = new Vector3( box1.transform.position.x+(equal-1)/2*sr1.bounds.size.x+0.05f,
                                                    box1.transform.position.y, 0);
        else if (dir == Vector2Int.right)                                         
                    box1.transform.position = new Vector3( box1.transform.position.x-(equal-1)/2*sr1.bounds.size.x-0.05f,
                                                    box1.transform.position.y, 0);
        else if (dir == Vector2Int.up)
            box1.transform.position = new Vector3( box1.transform.position.x,
                                                    box1.transform.position.y - (equal-1)/2*sr1.bounds.size.y-0.05f, 0);
        else if (dir == Vector2Int.down)                                                    
                    box1.transform.position = new Vector3( box1.transform.position.x,
                                                    box1.transform.position.y + (equal-1)/2*sr1.bounds.size.y+0.05f, 0);

        box1.dir = dir;
        box1.start = start;
        box1.live = 7-start;
        // 6-1 , 5-2, 4-3, 3-4,
            
        sr2.sortingOrder = 2;                                            

        Vector2Int pos1 =  background.pos;
        for (int i=0; i < maxBox; i++)
        {
            pos1 = pos1 + ( dir * new Vector2Int(-1,-1));
            Background background1 = backgrounds[pos1.x,pos1.y];
            Box box2 = background1.box;
            if (box2 != null)
                Destroy(box2.gameObject);
            background1.box = box1;
            background1.type = EBackgroundType.Protected;
     //       createSpriteMask(background1);

        }
        return box1;
    }
    private Box CreateCookieTray(Background background)
    {
        
        Vector2Int pos1 = background.pos;
        Vector2Int[] pos = new Vector2Int[3];
        pos[0] = pos1+Vector2Int.right;
        pos[1] = pos1+Vector2Int.down;             
        pos[2] = pos1 + (Vector2Int.right+Vector2Int.down);

        if (!Global.ValidPos(pos[0]) || !Global.ValidPos(pos[1]) || !Global.ValidPos(pos[2]))
            return null;
        Box box1;

        box1 = createBox(background, BoxType.CookieTray);
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        box1.transform.position = new Vector3(  box1.transform.position.x+sr.bounds.size.x/2, 
                                                box1.transform.position.y - sr.bounds.size.y/2,0);

        for (int i=0; i < 3; i++)
        {
            Background background1 = backgrounds[pos[i].x,pos[i].y];
            Box box = background1.box;
            if (box != null)
                Destroy(box.gameObject);
            background1.box = box1;
            background1.type = EBackgroundType.Protected;
      //      createSpriteMask(background1);
            
        }
        return box1;

    }
    public Box NewJellyRandom(Background background)
    {

          BoxType subType = (BoxType) Random.Range((int) BoxType.JellyRed, (int) BoxType.JellyCyan+1);

          return createBox(background, subType);
    }

    public void ShowCanMatch()
    {
        Background background;
        bool exit = false;
        for (int y=0; y < gridLayer.maxY && !exit; y++)
        {
            for (int x=0; x < gridLayer.maxX && !exit; x++)
            {
                background = backgrounds[x,y];
                if (!background.box.isJelly()) continue;
                foreach(Vector2Int dir in Global.dirs)
                {
                    Vector2Int nextPos = background.pos+dir;
                    if (Global.ValidPos(nextPos)) {
                        Background nextBackground = backgrounds[nextPos.x,nextPos.y];
                        if (!nextBackground.box.isJelly()) continue;
                        
                        background.SwapPosition(nextBackground);

                        List<MatchInfo> mifList = new List<MatchInfo>();
                        background.box.checkJellyMatch(ref mifList, false);
                        if (mifList.Count > 0) {
                            PrintMIFList(mifList);
                            background.SwapPosition(nextBackground);
                            exit = true;
                            break;
                        }
                        background.SwapPosition(nextBackground);

                            
//                        background2.box.checkJellyMatch(ref mifList);  
                    }

                }

            }
        }
        
    }
    void PrintMIFList(List<MatchInfo> mifList)
    {
        int i =0;
        foreach(MatchInfo mif in mifList)
        {
            i++;
            Debug.Log("List "+i);
            PrintMatchInfo(mif);
            
        }
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
