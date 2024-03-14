using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Match3:MonoBehaviour
{
    public Jelly PrefabJelly;
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
        gridLayer = Main.Instance.gridLayer;
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
            gridLayer.backgrounds[bif.x, bif.y].jelly = createJelly(background, bif.subType);
            
        }

        // create randomize box 
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Background background = gridLayer.backgrounds[x,y];
                if (!background.jelly)
                {
                    BoxSubType subType = GetSubTypeNotMatch3(file.boxData.rows, file.boxData.columns,y, x, gridLayer.backgrounds);
                    gridLayer.backgrounds[x, y].jelly =  createJelly(background, subType);

                }

            }
        }

    }
    
    private BoxSubType GetSubTypeNotMatch3(int rows, int cols, int row, int col, Background[,] background)
    {
        
        for (;;)
        {
            BoxSubType subType = (BoxSubType) Random.Range((int) BoxSubType.JellyRed, (int) BoxSubType.JellyOrange+1);
            
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
            if (background[pos.x,pos.y].jelly == null) return count;
            if (background[pos.x,pos.y].jelly.subType == subType) count++;
            else
                return count;
        }
        
    }

    
    private Jelly createJelly(Background background, BoxSubType subType)
    {
        GameObject panel = GameObject.Find("Panel");
            if (background)
            {
                Jelly jelly = Instantiate(PrefabJelly, background.transform.position, Quaternion.identity, panel.transform);
                
                SpriteRenderer sr3 = jelly.GetComponent<SpriteRenderer>();
                sr3.sprite = file.GetSprite(subType);
                sr3.sortingOrder = 2;
                jelly.name = "Jelly " + background.pos.x +","+ background.pos.y;
                jelly.subType = subType;

                jelly.transform.localScale = background.transform.localScale;
                jelly.background = background;
                
                return jelly;
            }
            return null;
    }

    public Jelly NewJellyRandom(Background background)
    {

          BoxSubType subType = (BoxSubType) Random.Range((int) BoxSubType.JellyRed, (int) BoxSubType.JellyOrange+1);

          return createJelly(background, subType);
    }
}
