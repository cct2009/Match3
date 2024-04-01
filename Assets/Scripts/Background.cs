using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;



public enum EBackgroundType{
    Fill = 0,
    Vacant = 1,
    Protected = 2,
    Close = 3,
}
public class Background : MonoBehaviour
{
    public Vector2Int pos;
    public Box box;
    public Vector2Int flow;
    public EPointType ptype;
    public EBackgroundType type;
    public Background moveFrom;
    public Background moveTo;
    static GridLayer gridLayer;
    static Background[,] backgrounds;
    
    private void Start() {
        gridLayer = Global.gridLayer;
        backgrounds = gridLayer.backgrounds; 
        moveTo = moveFrom = null;
               
    }
  

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log(name+" trigger by "+other.name);
        if (box != null && this.type != EBackgroundType.Close)
            box.DestroyAnimateBox();
        
    }
    public IEnumerator SwitchItem(Vector2Int swipeDirection)
    {
        Vector2Int item2Pos = pos+swipeDirection;
        if (Global.ValidPos(item2Pos)) 
        {
            Background background2 = backgrounds[item2Pos.x, item2Pos.y];
            
            if (box.move && background2.box.move) 
            {
                ParticleSystem part = Instantiate(Main.Instance.part,transform.position, Quaternion.identity);
                part.Play();
                ParticleSystem part2 = Instantiate(Main.Instance.part,background2.box.transform.position, Quaternion.identity);
                part2.Play();

                Destroy(part.gameObject,2);
                Destroy(part2.gameObject,2);

                box.transform.DOMove(background2.transform.position,0.3f).SetEase(Ease.OutCubic);
                background2.box.transform.DOMove(transform.position, 0.3f).SetEase( Ease.OutCubic);
                SwapPosition(background2);    
                yield return new WaitForSeconds(0.3f);

                List<MatchInfo> mifList = new List<MatchInfo>();

                if (box.IsPower4() || background2.box.IsPower4())
                {
                    yield return SwitchPower4(box, background2.box);
                    // yield return FlowBoxToBlank();
                }
                    
                else
                {
                    box.checkJellyMatch(ref mifList);
                    background2.box.checkJellyMatch(ref mifList);
                }


                if (mifList.Count == 0)
                {
                    // SwapPosition(background2);  
                    // box.transform.DOMove(transform.position,0.3f).SetEase(Ease.OutCubic);
                    // background2.box.transform.DOMove(background2.transform.position, 0.3f).SetEase( Ease.OutCubic);
                    // yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    yield return DestroyMatch(mifList); 
                        
                    yield return FlowBoxToBlank();
                    Main.Instance.AddTimes(-1);

                }

            }
            
        } 
        ResetBoxState();
        Main.Instance.programState = ProgramState.Input;
    
    }
    
    IEnumerator SwitchPower4(Box box1, Box box2)
    {   
        if (box1.IsPower4() && box2.IsPower4())
        {
            Debug.Log("SwitchPower4 when both are power4");
        }
        else if (box1.IsPower4())
        {
            // box2.checkJellyMatch(ref mifList);
            yield return box1.launchPower4();
        }
        else if (box2.IsPower4())
        {
            // box1.checkJellyMatch(ref mifList);
            yield return box2.launchPower4();
        }
        

    }
    void SwitchPower4_0(Box box1, Box box2, ref List<MatchInfo> mifList)
    {   
        if (box1.IsPower4() && box2.IsPower4())
        {
            Debug.Log("SwitchPower4 when both are power4");
        }
        else if (box1.IsPower4())
        {
            box2.checkJellyMatch(ref mifList);
            box1.checkPower4Match(ref mifList);
        }
        else if (box2.IsPower4())
        {
            box1.checkJellyMatch(ref mifList);
            box2.checkPower4Match(ref mifList);
        }

    }
    
    void ResetBoxState()
    {
        Background background;
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                background = backgrounds[x,y];
                if (background.box != null) {
                    background.box.boxState = EBoxState.Normal;
                }
            }
        }
    }
    public void SwapPosition(Background background2)
    {
        Box temp = box;
        box = background2.box;
        background2.box = temp;
        
        Background tempB = box.background;
        box.background = background2.box.background;
        background2.box.background = tempB;

    }
    public IEnumerator FlowBoxToBlank()
    {
        List<List<Background>> allList = new List<List<Background>>();
        List<List<Background>> dupList = new List<List<Background>>();
        List<List<Background>> linesList = new List<List<Background>>();
        foreach (Background background in gridLayer.enterPoints)
        {
            List<Background> lines;
            lines = GetLines(background);
            allList.Add(lines);
        }

        // 
        for (int i=0; i < allList.Count ;i++)
        {
            for (int j=i+1; j <allList.Count ; j++)
            {
                var result = allList[i].Where(x => allList[j].Contains(x));
                if (result.Count() > 0)
                {
                    if (!dupList.Contains(allList[i])) dupList.Add(allList[i]);
                    if (!dupList.Contains(allList[j])) dupList.Add(allList[j]);
                }
            }
        }
        for (int i =0; i < allList.Count;i++)
        {
            if (dupList.Contains(allList[i])) continue;
            linesList.Add(allList[i]);
        }
        foreach(List<Background> list in dupList)
        {
            list.Reverse(); // so run from exit point to enter point
        }
        foreach(List<Background> list in linesList)
        {
            list.Reverse();
        }
        ResetFromTo();
        foreach(List<Background> list in linesList)
        {
            StartCoroutine(flowPipe(list));
        }
        foreach(List<Background> list in dupList)
        {
            yield return flowPipe(list);
            ResetFromTo();
//            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(0.2f);
        Main.Instance.match3.FillInBlank();
        List<MatchInfo> mifList = new List<MatchInfo>();

        if (GetAllMatch(ref mifList))
        {
            yield return DestroyMatch(mifList);
            yield return FlowBoxToBlank();
        }
        // yield return null;
    }
    void ResetFromTo()
    {
        for (int y=0; y < gridLayer.maxY; y++)
        {
            for (int x =0; x < gridLayer.maxX; x++)
            {
                Background background = backgrounds[x,y];
                background.moveFrom = null;
                background.moveTo = null;
            }
        }
    }
    IEnumerator flowPipe(List<Background> lineList)
    {
        List<Background> listMove = new List<Background>();
        foreach(Background background in lineList)
        {
            if ((background.box == null && background.moveFrom == null)|| background.moveTo != null){
                background.moveFrom = nextAvailBox(background, lineList);
                if (background.moveFrom) {
                    background.moveFrom.moveTo = background;
                    listMove.Add(background);
                }
            }
        }

        foreach(Background background in listMove)
        {
           StartCoroutine(moveOnebyOne(background,background.moveFrom));
        }
         yield return null;

    }
    IEnumerator moveOnebyOne(Background background, Background moveFrom)
    {
        while (moveFrom != background && moveFrom )
        {
            moveFrom = moveFrom.moveNext();
        }
        yield return null;
        
    }
    private Background moveNext()
    {
        Background next = getNext();
        if (box && next){
            box.transform.DOMove(next.transform.position,0.2f);
            next.box = box;
            box.background = next;
            box = null;
            
        }
            
        return next;
    }
    Background nextAvailBox(Background background,List<Background> lineList)
    {
        int i;
        for (i=0; i < lineList.Count; i++)
            if (lineList[i] == background) break;
        while (background.box == null || background.moveTo != null && i < lineList.Count)
        {
            i++;
            if ( i >= lineList.Count) break;
            background = lineList[i];
            
        }
        return background;
    }
    Background getPrev()
    {
        Background background;
        Vector2Int pos1 = pos - flow;
        if (Global.ValidPos(pos1))
            background = backgrounds[pos1.x,pos1.y];
        else    
            return null;

        return background;
    }
    Background getNext()
    {
        Background background;
        Vector2Int pos1 = pos + flow;
        if (Global.ValidPos(pos1))
            background = backgrounds[pos1.x,pos1.y];
        else    
            return null;
        return background;
    }
    private List<Background> GetLines(Background enterPoint)
    {   // get list of backgrounds in the same line from enterPoint to exitPoint
        List<Background> lines = new List<Background>();
        Vector2Int pos = enterPoint.pos;

        while (Global.ValidPos(pos)) {
            lines.Add(enterPoint);
            pos = pos + enterPoint.flow;
            if (Global.ValidPos(pos))
                enterPoint = backgrounds[pos.x,pos.y];

        }
        return lines;
    }
    
    public IEnumerator FlowBoxToBlank_1()
    {
        Background canMoveBackground, blankBackground;
        while (CanFlow(out canMoveBackground, out blankBackground) ) {
            yield return canMoveBackground.FlowBlank(blankBackground);
        }
        Main.Instance.match3.FillInBlank();
        List<MatchInfo> mifList = new List<MatchInfo>();

        if (GetAllMatch(ref mifList))
        {
            yield return DestroyMatch(mifList);
            yield return FlowBoxToBlank_1();
        }
        
    }

    bool GetAllMatch(ref List<MatchInfo> mifList)
    {
        Background background;
        mifList.Clear();
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                background = backgrounds[x,y];
                if (background.box != null &&
                    !ContainIn(background.box, mifList)) {
                    background.box.checkJellyMatch(ref mifList);
                }
            }
        }
        
        return mifList.Count > 0;
    }
    bool ContainIn(Box box, List<MatchInfo> mifList)
    {
        foreach(MatchInfo mif in mifList)
        {
            if (mif.boxList.Contains(box))
                return true;
        }
        return false;
    }

    bool CanFlow(out Background canMoveBackground,out Background blankBackground)
    {
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                blankBackground = backgrounds[x,y];
                if (blankBackground.box == null)
                {
                    Vector2Int pos = blankBackground.pos;
                    foreach(Vector2Int dir in Global.dirs)
                    {
                        Vector2Int pos1 = pos+dir;   // อันที่อยู่ตำแหน่งซ้าย ขวา บน ล่าง
                        if (Global.ValidPos(pos1)) // ต้องอยู่ในกริด
                        {
                            canMoveBackground = backgrounds[pos1.x,pos1.y];
                            if (canMoveBackground.box && canMoveBackground.box.move)  // ต้องไม่ใช่อันว่าง
                            {
                                Vector2Int destPos = pos1+ canMoveBackground.flow; // บวกกับทิศทาง flow
                                if (Global.ValidPos(destPos) &&  destPos == pos) // ถ้าเท่ากันแสดงว่า ไหลจากตำแหน่ง pos1 มาที่ destPos
                                {   
                               //     Debug.Log("CanFillBlank:"+ canMoveBackground +","+blankBackground);
                                    return true;
                                }
                            }
                        }
                    }

                }
            }
        }
        canMoveBackground = null;
        blankBackground = null;
        return false;
    }
    static int ypos=-1;
    IEnumerator FlowBlank( Background blankBackground)
    {
        box.transform.DOMove(blankBackground.transform.position, Main.Instance.flowSpeed);
        blankBackground.box =  box;
        blankBackground.type = EBackgroundType.Fill;
        box = null;
        blankBackground.box.background = blankBackground;
        if (ypos != -1 && blankBackground.pos.y != ypos)
        {
            ypos = blankBackground.pos.y;
            yield return new WaitForSeconds(Main.Instance.flowSpeed);
            
        }
        
        // yield return new WaitForSeconds(Main.Instance.flowSpeed);
        
        
    }

    void ExpandRange(ref List<Box> boxList)
    {
        List<Box> expandList = new List<Box>();
        foreach(Box box in boxList)
        {
            Box expBox;
            foreach(Vector2Int dir in Global.dirs)
            {
                expBox = passCondition(box, dir);
                if (expBox != null) {
                    expBox.boxState = EBoxState.Minus;
                    expandList.Add(expBox);
                   
                } 
            }
        }
        boxList = boxList.Union(expandList).Distinct().ToList();
    }
    Box passCondition(Box box, Vector2Int dir)
    {
        Vector2Int pos = box.background.pos + dir;
        if (!Global.ValidPos(pos)) return null;
        Box expBox = backgrounds[pos.x,pos.y].box;
        if (expBox == null) return null;
        if (expBox.isEffect(expBox.type)) 
            return expBox;
        return null;
        
    }
    private void SetSpritePosition(Box box)
    {
        
        SpriteRenderer sr = box.background.GetComponent<SpriteRenderer>();
        Vector2 dir = box.dir * new Vector2(-1,-1) * -sr.bounds.size.x/2f;
        box.transform.position = box.transform.position + new Vector3(dir.x,dir.y,0);
        // if (box.dir == Vector2Int.right)
        //     box.transform.position = new Vector3(box.transform.position.x + sr.bounds.size.x/2f,box.transform.position.y,0);
        // else if (box.dir == Vector2Int.left)
        //     box.transform.position = new Vector3(box.transform.position.x - sr.bounds.size.x/2f,box.transform.position.y,0);
        DeleteTail(box);

    }
    private void DeleteTail(Box box)
    {
        Vector2Int dirt = box.dir * new Vector2Int(-1,-1);
        Vector2Int pos = box.background.pos;
        Background background;
        do
        {
            pos = pos+ dirt;
            if (!Global.ValidPos(pos)) break;
            background = backgrounds[pos.x,pos.y];

        } while (background.box == box);
        pos = pos - dirt;
        background = backgrounds[pos.x,pos.y];
        background.type = EBackgroundType.Vacant;
        background.box = null;

    }
    private void ClearDisplayCard(Box box)
    {
        DeleteTail(box);
        DeleteTail(box);
        
    }
    private void ReplaceSprite(Box box)
    {
        SpriteRenderer sr = box.GetComponent<SpriteRenderer>();
        sr.sprite = Global.Instance.file.GetSpriteForLive(box);
        if (box.type == BoxType.DisplayCard)
            SetSpritePosition(box);
        
        GameObject prefab = Global.Instance.file.GetDieAnimate(box.type);
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, box.transform.position, box.transform.rotation);
            Destroy(go,1);
        }
    }
  
    public IEnumerator DestroyMatch(List<MatchInfo> mifList)
    {
        List<Box> boxList = new List<Box>();
        // รวม matchInfo ทั้งหมดเข้าด้วยกัน เป็นรายการของ box ทั้งหมดที่จะโดนทำลายหรือไม่โดนเลย (เป็น live)
        foreach(MatchInfo mif in mifList)
        {
            boxList = boxList.Union(mif.boxList).Distinct().ToList();
        }
        ExpandRange(ref boxList);

        foreach(Box box in boxList)
        {
            if (box.boxState == EBoxState.Minus)
            {
                box.live--;
                if (box.live == 0)
                {
                    if (box.type == BoxType.DisplayCard)
                        ClearDisplayCard(box);
                    box.boxState = EBoxState.Die;
                }
                
                else
                    ReplaceSprite(box);
            }
                
        }

        box.AnimationMatch(boxList);
        yield return new WaitForSeconds(0.6f);
        foreach(Box box in boxList)
        {
            if (box.boxState == EBoxState.Die)
            {
                if (box.type == BoxType.CookieTray)
                {  // set box ที่เป็น protected ที่ติดกัน เป็น null
                    Background background = box.background;
                    background.GetAtDir(Vector2Int.right).box = null;
                    background.GetAtDir(Vector2Int.down).box = null;
                    background.GetAtDir(Vector2Int.down + Vector2Int.right).box = null;
                }
                box.DestroyBox();
//                Debug.Log("DESTROY : "+ box.subType);
            }
                
        }
        
    }
    private Background  GetAtDir(Vector2Int dir)
    {
        Vector2Int pos1 = pos+dir;
        return backgrounds[pos1.x,pos1.y];
    }

}
