using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Runtime.Serialization.Formatters;


public class Background : MonoBehaviour
{
    public Vector2Int pos;
    public Box jelly;
    public Vector2Int flow;
    private float speed = 0.05f;
    
    GridLayer gridLayer;
    Background[,] backgrounds;
    
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
            
            jelly.transform.DOMove(background2.transform.position,0.3f).SetEase(Ease.OutCubic);
            background2.jelly.transform.DOMove(transform.position, 0.3f).SetEase( Ease.OutCubic);
            SwapPosition(background2);    
            yield return new WaitForSeconds(0.3f);

            List<Box> matchList1, matchList2;

            bool jelly1Match = jelly.checkMatch(out matchList1);
            bool jelly2Match = background2.jelly.checkMatch(out matchList2);
            // if (jelly1Match) jelly.DrawBorder(matchList1,Main.Instance.lr1);
            // if (jelly2Match) background2.jelly.DrawBorder(matchList2,Main.Instance.lr2);

            if (!jelly1Match && !jelly2Match)
            { 
                
                SwapPosition(background2);  
                jelly.transform.DOMove(transform.position,0.3f).SetEase(Ease.OutCubic);
                background2.jelly.transform.DOMove(background2.transform.position, 0.3f).SetEase( Ease.OutCubic);
                

                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                List<Box> matchList = new List<Box>();
                if (jelly1Match)
                    matchList = matchList.Union(matchList1).ToList();
                if (jelly2Match)
                    matchList = matchList.Union(matchList2).ToList();
                DestroyMatch(matchList); // ทำลายทั้งหมดในครั้งเดียวจะได้ไม่มี wait animation หลายครั้ง
                
                yield return FlowJellyToBlank();
            }
        } 

        Main.Instance.programState = ProgramState.Input;
    
    }

    public void SwapPosition(Background background2)
    {
        Box temp = jelly;
        jelly = background2.jelly;
        background2.jelly = temp;
        
        Background tempB = jelly.background;
        jelly.background = background2.jelly.background;
        background2.jelly.background = tempB;

    }
    IEnumerator FlowJellyToBlank()
    {
        Background canMoveBackground, blankBackground;
        while (CanFlow(out canMoveBackground, out blankBackground) ) {
            yield return canMoveBackground.FillBlank(blankBackground);
        }
        FillInBlank();
        List<Box> list = new List<Box>();
        Background background;
        if (CanMatchJellys(out background,out list))
        {
            do
            {
                background.DestroyMatch(list);
            }
            while (CanMatchJellys(out background,out list));
            
            yield return FlowJellyToBlank();
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
                if (background.jelly == null) {
                    background.jelly = Main.Instance.match3.NewJellyRandom(background);
                }
            }
        }
    }

    bool CanMatchJellys(out Background background, out List<Box> list)
    {
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                background = backgrounds[x,y];
                if (background.jelly != null) {
                    if (background.jelly.checkMatch(out list))
                        return true;
                }
            }
        }
        background = null;
        list = null;
        return false;

    }
Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};
    bool CanFlow(out Background canMoveBackground,out Background blankBackground)
    {
        for (int y = 0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                blankBackground = backgrounds[x,y];
                if (blankBackground.jelly == null)
                {
                    Vector2Int pos = blankBackground.pos;
                    foreach(Vector2Int dir in dirs)
                    {
                        Vector2Int pos1 = pos+dir;   // อันที่อยู่ตำแหน่งซ้าย ขวา บน ล่าง
                        if (ValidPos(pos1)) // ต้องอยู่ในกริด
                        {
                            canMoveBackground = backgrounds[pos1.x,pos1.y];
                            if (canMoveBackground.jelly)  // ต้องไม่ใช่อันว่าง
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
    
    IEnumerator FillBlank( Background blankBackground)
    {
        jelly.transform.DOMove(blankBackground.transform.position, speed);
        blankBackground.jelly =  jelly;
        jelly = null;
        blankBackground.jelly.background = blankBackground;
        yield return new WaitForSeconds(speed);
        
        
    }

    public void DestroyMatch(List<Box> list)
    {
        List<Background> bList = new List<Background>();
        jelly.AnimationMatch(list);
        foreach(Box jelly in list)
        {
            bList.Add(this);
            jelly.DestoryJelly();
            
        }
        
        
        
    }
    private bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }

}
