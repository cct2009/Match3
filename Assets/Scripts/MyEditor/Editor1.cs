using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TMPro;
using System.IO;
using Unity.VisualScripting;

public class Editor1 : MonoBehaviour
{
    public MatchPanel panel;
    public Global global;
    public Match3 match3;
    public TMP_Text subTypeIn;
    public TMP_InputField level;
    public TMP_InputField tColumns;
    public TMP_InputField tRows;
    private GridLayer gridLayer;
    public TMP_Text dirIn;
    public TMP_InputField start;
    private Background[,] backgrounds;
    private string saveFilePath1;
    private BoxData boxData;

    void Start()
    {
        match3.Init(Global.Instance.file);
        panel.CreateGrid(Global.gridLayer);
        gridLayer = Global.gridLayer;
        backgrounds = gridLayer.backgrounds;
        saveFilePath1 = Application.persistentDataPath + "/Level";    
        InitRandomBox();    

    }

void InitRandomBox()
{
    for (int y = 0 ; y < int.Parse(tRows.text); y++)
    {
        for (int x = 0; x < int.Parse(tColumns.text); x++)
        {
            Background background = gridLayer.backgrounds[x,y];    
            background.box = match3.createBox(background, BoxType.JellyRandom);              
        }

    }   

}
Vector2 pos1;
Background background;
    void Update()
    {
      if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.began)
            {
                pos1 = touch.screenPosition;
                RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(pos1));
                if (hit.transform != null)
                {
                    Vector2Int dir = GetDirectionFromBoxType((BoxType) int.Parse(dirIn.text));
                    Debug.Log("dir="+dir);
                    int start1 = int.Parse(start.text);
                    background = hit.transform.GetComponent<Background>();
      //              Debug.Log("Hit "+background.pos+": Data "+Global.backgrounds[background.pos.x, background.pos.y] );
                    BoxType subType = (BoxType) int.Parse(subTypeIn.text);

                    if (background.box != null && background.box.type == subType)
                        Debug.Log("Don't do anything , same subType");
                    else if (background.type == EBackgroundType.Protected)
                        Debug.Log("Protected don't do anything");
                    else
                        background.box = match3.createABox(background, (BoxType) int.Parse(subTypeIn.text), dir, start1);

                }

            }
        }     
    }

    Vector2Int GetDirectionFromBoxType(BoxType type)
    {
        switch(type)
        {
            case BoxType.ArrowDown: return Vector2Int.down;
            case BoxType.ArrowUp : return Vector2Int.up;
            case BoxType.ArrowLeft: return Vector2Int.left;
            default : return Vector2Int.right;
        }
    }
    public void onSave()
    {
        BoxData boxData = new BoxData();

            boxData.rows = gridLayer.maxY;
            boxData.columns = gridLayer.maxX;
            boxData.layer1 = new List<BoxInfo>();
            for (int y=0; y < boxData.rows;  y++)
                for (int x = 0; x < boxData.columns; x++)
                {
                    BoxInfo bf = new BoxInfo();
                    Background background = backgrounds[x,y];
                    if (background.box && background.type != EBackgroundType.Protected && background.box.type != BoxType.JellyRandom)
                    {
                        Box box = background.box;
                        bf.type = box.type;
                        bf.dir = box.dir;
                        bf.start = box.start;
                        bf.x = x;
                        bf.y = y;
                        boxData.layer1.Add(bf);
                    }
                    
                }
            
            string  data = JsonUtility.ToJson(boxData);
            string saveFilePath = saveFilePath1 + int.Parse(level.text.ToString())+".json";
            File.WriteAllText(saveFilePath, data);
            Debug.Log("Write to "+saveFilePath);
        
    }
    
    public void  OnLoadData()
    {
        string saveFilePath = saveFilePath1 + int.Parse(level.text.ToString())+".json";
         if (File.Exists(saveFilePath))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath);
            boxData = JsonUtility.FromJson<BoxData>(loadPlayerData);
            panel.CreateGrid(Global.gridLayer);
            DrawBox(boxData);
        }
    }
    void DrawBox(BoxData boxData)
    {
        for (int y = 0 ; y < boxData.rows; y++)
        {
            for (int x = 0; x < boxData.columns; x++)
            {
                Background background = gridLayer.backgrounds[x,y];    
                background.box = match3.createBox(background, BoxType.JellyRandom);              
            }

        }   
        foreach(BoxInfo bif in boxData.layer1)
        {
            Background background = gridLayer.backgrounds[bif.x,bif.y];

            background.box = match3.createABox(background, bif.type,bif.dir,bif.start);          

        }
    }
        private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

}
