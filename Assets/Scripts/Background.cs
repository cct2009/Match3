using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;
using System.IO;


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
    public EBackgroundType type;
    static GridLayer gridLayer;
    static Background[,] backgrounds;
    
    private void Start() {
        gridLayer = Global.gridLayer;
        backgrounds = gridLayer.backgrounds; 
               
    }
    public IEnumerator ShowCanMatch()
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
                            exit = true;
                            break;
                        }

                            
//                        background2.box.checkJellyMatch(ref mifList);  
                    }

                }

            }
        }
        yield return null;
    }

    void PrintMIFList(List<MatchInfo> mifList)
    {
        int i =0;
        foreach(MatchInfo mif in mifList)
        {
            i++;
            Debug.Log("List "+i);
            box.PrintMatchInfo(mif);
            
        }
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
                    SwitchPower4(box, background2.box,ref mifList);
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
    
    void SwitchPower4(Box box1, Box box2, ref List<MatchInfo> mifList)
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
    IEnumerator FlowBoxToBlank()
    {
        Background canMoveBackground, blankBackground;
        while (CanFlow(out canMoveBackground, out blankBackground) ) {
            yield return canMoveBackground.FlowBlank(blankBackground);
        }
        FillInBlank();
        List<MatchInfo> mifList = new List<MatchInfo>();

        if (GetAllMatch(ref mifList))
        {
            yield return DestroyMatch(mifList);
            yield return FlowBoxToBlank();
        }
            
        
    }
    void FillInBlank()
    {
        Background background;
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                background = backgrounds[x,y];
                if (background.box == null) {
                    background.box = Main.Instance.match3.NewJellyRandom(background);
                }
            }
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
    
    IEnumerator FlowBlank( Background blankBackground)
    {
        box.transform.DOMove(blankBackground.transform.position, Main.Instance.flowSpeed);
        blankBackground.box =  box;
        box = null;
        blankBackground.box.background = blankBackground;
        yield return new WaitForSeconds(Main.Instance.flowSpeed);
        
        
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
        if (expBox.isEffect(expBox.type)) 
            return expBox;
        return null;
        
    }
    private void ReplaceSprite(Box box)
    {
        SpriteRenderer sr = box.GetComponent<SpriteRenderer>();
        sr.sprite = Global.Instance.file.GetSpriteForLive(box);
        
        GameObject prefab = Global.Instance.file.GetDieAnimate(box.type);
        if (prefab == null) prefab = Main.Instance.animate;
        GameObject go = Instantiate(prefab, box.transform.position, box.transform.rotation);
        Destroy(go,1);
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
                    box.boxState = EBoxState.Die;
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
