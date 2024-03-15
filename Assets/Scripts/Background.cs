using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;



public class Background : MonoBehaviour
{
    public Vector2Int pos;
    public Box box;
    public Vector2Int flow;
    
    static GridLayer gridLayer;
    static Background[,] backgrounds;
    
    private void Start() {
        gridLayer = Global.Instance.gridLayer;
        backgrounds = gridLayer.backgrounds; 
               
    }

    public IEnumerator SwitchItem(Vector2Int swipeDirection)
    {
        Vector2Int item2Pos = pos+swipeDirection;
        if (ValidPos(item2Pos)) 
        {
            Background background2 = backgrounds[item2Pos.x, item2Pos.y];
            
            if (box.move && background2.box.move) 
            {
                box.transform.DOMove(background2.transform.position,0.3f).SetEase(Ease.OutCubic);
                background2.box.transform.DOMove(transform.position, 0.3f).SetEase( Ease.OutCubic);
                SwapPosition(background2);    
                yield return new WaitForSeconds(0.3f);

                List<MatchInfo> mifList = new List<MatchInfo>();
                box.checkJellyMatch(ref mifList);
                background2.box.checkJellyMatch(ref mifList);

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
static Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};
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
                    foreach(Vector2Int dir in dirs)
                    {
                        Vector2Int pos1 = pos+dir;   // อันที่อยู่ตำแหน่งซ้าย ขวา บน ล่าง
                        if (ValidPos(pos1)) // ต้องอยู่ในกริด
                        {
                            canMoveBackground = backgrounds[pos1.x,pos1.y];
                            if (canMoveBackground.box && canMoveBackground.box.move)  // ต้องไม่ใช่อันว่าง
                            {
                                Vector2Int destPos = pos1+ canMoveBackground.flow; // บวกกับทิศทาง flow
                                if (ValidPos(destPos) &&  destPos == pos) // ถ้าเท่ากันแสดงว่า ไหลจากตำแหน่ง pos1 มาที่ destPos
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
            foreach(Vector2Int dir in dirs)
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
        if (!ValidPos(pos)) return null;
        Box expBox = backgrounds[pos.x,pos.y].box;
        if (expBox.isEffect(expBox.subType)) 
            return expBox;
        return null;
        
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
            }
                
        }

        box.AnimationMatch(boxList);
        yield return new WaitForSeconds(0.6f);
        foreach(Box box in boxList)
        {
            if (box.boxState == EBoxState.Die)
            {
                box.DestroyBox();
//                Debug.Log("DESTROY : "+ box.subType);
            }
                
        }
        
    }
    private bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }

}
