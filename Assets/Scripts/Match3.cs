using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

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
        }

    }
    public void LoadBox()
    {
        foreach(BoxInfo bif in file.boxData.layer1)
        {
            Background background = gridLayer.backgrounds[bif.x,bif.y];
            gridLayer.backgrounds[bif.x, bif.y].box = createABox(background, bif.type);
            
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

    public Box createABox(Background background, BoxType type)
    {
         if (type == BoxType.CookieTray)
            background.box = CreateCookieTray(background);
        else
            background.box = createBox(background, type);
        return background.box;

    }
    public Box createBox(Background background, BoxType type)
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
                    
                background.type = EBackgroundType.Fill;
                SpriteRenderer sr3 = box.GetComponent<SpriteRenderer>();
                sr3.sprite = file.GetSprite(type);
                sr3.sortingOrder = 2;
                box.name = "Box " + background.pos.x +","+ background.pos.y;
                box.type = type;
                box.live = file.GetLive(type);
                box.move = file.GetMove(type);
                box.boxState = EBoxState.Normal;
                
                box.transform.position = background.transform.position;
                box.transform.localScale = background.transform.localScale;
                box.background = background;
                
                return box;
            }
            return null;
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
            Box box = backgrounds[pos[i].x,pos[i].y].box;
            if (box != null)
                Destroy(box.gameObject);
            backgrounds[pos[i].x,pos[i].y].box = box1;
            backgrounds[pos[i].x,pos[i].y].type = EBackgroundType.Protected;
            
        }
        return box1;

    }
    public Box NewJellyRandom(Background background)
    {

          BoxType subType = (BoxType) Random.Range((int) BoxType.JellyRed, (int) BoxType.JellyCyan+1);

          return createBox(background, subType);
    }
}
