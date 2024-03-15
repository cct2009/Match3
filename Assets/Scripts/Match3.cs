using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Match3:MonoBehaviour
{
    public Box PrefabJelly;
    private FileData file;
    private int rows, columns;
    GridLayer gridLayer;


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
        gridLayer = Global.Instance.gridLayer;
    }

    public void LoadDirection(int DirectionVersion)
    {
        if (file.onLoadDirection(DirectionVersion))
        {
            foreach(BoxInfo bif in file.boxData.layer1)
            {
                Background background = gridLayer.backgrounds[bif.x,bif.y];
                switch(bif.subType)
                {
                    case BoxSubType.ArrowDown:
                        background.flow = Vector2Int.down;
                    break;
                    case BoxSubType.ArrowLeft:
                        background.flow = Vector2Int.left;
                    break;
                    case BoxSubType.ArrowRight:
                        background.flow = Vector2Int.right;
                    break;
                    case BoxSubType.ArrowUp:
                        background.flow = Vector2Int.up;
                    break;
                    
                }
                SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
                sr.sprite = file.GetSprite(bif.subType);

            }
            Debug.Log("Load Direction successed");
        }

    }
    public void LoadJelly()
    {
        foreach(BoxInfo bif in file.boxData.layer1)
        {
            Background background = gridLayer.backgrounds[bif.x,bif.y];
            gridLayer.backgrounds[bif.x, bif.y].box = createJelly(background, bif.subType);
            
        }

        // create randomize box 
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Background background = gridLayer.backgrounds[x,y];
                if (!background.box)
                {
                    BoxSubType subType = GetSubTypeNotMatch3(file.boxData.rows, file.boxData.columns,y, x, gridLayer.backgrounds);
                    gridLayer.backgrounds[x, y].box =  createJelly(background, subType);

                }

            }
        }

    }
    
    private BoxSubType GetSubTypeNotMatch3(int rows, int cols, int row, int col, Background[,] background)
    {
        
        for (;;)
        {
            BoxSubType subType = (BoxSubType) Random.Range((int) BoxSubType.JellyRed, (int) BoxSubType.JellyCyan+1);
            
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


    private int countSubType(Vector2Int direction,int rows, int cols, int row, int col, Background[,] background, BoxSubType subType)
    {
        Vector2Int pos = new Vector2Int(col,row);
        int count = 0;
        for (pos = pos+direction;;pos = pos+direction)
        {
            if (pos.x < 0 || pos.y < 0) return count;
            if (pos.x >= cols || pos.y >= rows) return count;
            if (background[pos.x,pos.y].box == null) return count;
            if (background[pos.x,pos.y].box.subType == subType) count++;
            else
                return count;
        }
        
    }

    
    private Box createJelly(Background background, BoxSubType subType)
    {
        GameObject panel = GameObject.Find("Panel");
            if (background)
            {
                Box box = Instantiate(PrefabJelly, background.transform.position, Quaternion.identity, panel.transform);
                
                SpriteRenderer sr3 = box.GetComponent<SpriteRenderer>();
                sr3.sprite = file.GetSprite(subType);
                sr3.sortingOrder = 2;
                box.name = "Box " + background.pos.x +","+ background.pos.y;
                box.subType = subType;
                box.live = 1;
                box.move = file.GetMove(subType);
                box.boxState = EBoxState.Normal;

                box.transform.localScale = background.transform.localScale;
                box.background = background;
                
                return box;
            }
            return null;
    }

    public Box NewJellyRandom(Background background)
    {

          BoxSubType subType = (BoxSubType) Random.Range((int) BoxSubType.JellyRed, (int) BoxSubType.JellyCyan+1);

          return createJelly(background, subType);
    }
}
