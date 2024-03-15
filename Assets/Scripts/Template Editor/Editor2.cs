using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TMPro;

public class Editor2 : MonoBehaviour
{
    public MatchPanel panel;
    public FileData  file;
    public Box PrefabJelly;
    public TMP_Text subTypeIn;    
    public TMP_InputField version;
    private BoxData boxData;
    GridLayer gridLayer;
    Background[,] backgrounds;
   string saveFilePath1;

    

    void Start()
    {
        gridLayer = new GridLayer();
        
        gridLayer.maxX = 9;
        gridLayer.maxY = 9;
        gridLayer.backgrounds = new Background[gridLayer.maxX, gridLayer.maxY];
        backgrounds = gridLayer.backgrounds;

        panel.CreateGrid(gridLayer);
        saveFilePath1 = Application.persistentDataPath + "/Direction";        
    }
    Background background;
    Vector2 pos1,pos2,swipe;
    Vector2Int swipeDirection;

    private void Update() {
        ManageInput();
    }
    void ManageInput()
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
                    background = hit.transform.GetComponent<Background>();
                    Debug.Log("Hit "+background.pos+": Data "+gridLayer.backgrounds[background.pos.x, background.pos.y] );
                    background.box = createJelly(background, (BoxSubType) int.Parse(subTypeIn.text));

                }

            }
            if (touch.ended)
            {
                pos2 = touch.screenPosition;
                swipe = pos2-pos1;

                swipe.Normalize();

                // Swipe up
                if (swipe.y > 0 && swipe.x > -0.5f && swipe.x < 0.5f) {
                    swipeDirection = Vector2Int.up;
                    // Swipe down
                } else if (swipe.y < 0 && swipe.x > -0.5f && swipe.x < 0.5f) {
                    swipeDirection = Vector2Int.down;
                    // Swipe left
                } else if (swipe.x < 0 && swipe.y > -0.5f && swipe.y < 0.5f) {
                    swipeDirection = Vector2Int.left;
                    // Swipe right
                } else if (swipe.x > 0 && swipe.y > -0.5f && swipe.y < 0.5f) {
                    swipeDirection = Vector2Int.right;
                }
                
                    


            }

        }
    }
   
    private Box createJelly(Background background, BoxSubType subType)
    {
        Box jelly;
            if (background.box == null)
                 jelly = Instantiate(PrefabJelly, background.transform.position, Quaternion.identity, background.transform);
            else    
                jelly = background.box;
                
            SpriteRenderer sr3 = jelly.GetComponent<SpriteRenderer>();
            sr3.sprite = file.GetSprite(subType);
            sr3.sortingOrder = 2;
            jelly.name = "Jelly " + background.pos.x +","+ background.pos.y;
            jelly.subType = subType;

            //jelly.transform.localScale = background.transform.localScale;
            jelly.background = background;
            
            return jelly;
    }

    public void Init()
    {
        for (int y=0; y < gridLayer.maxY; y++)
        {
            for (int x = 0; x < gridLayer.maxX; x++)
            {
                Background background = gridLayer.backgrounds[x,y];

                    background.box = createJelly(background, BoxSubType.ArrowDown);
            }
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
                    if (background.box)
                    {
                        Box jelly = background.box;
                        bf.subType = jelly.subType;
                        bf.x = x;
                        bf.y = y;
                    }
                    boxData.layer1.Add(bf);
                }
            
            string  data = JsonUtility.ToJson(boxData);
            string saveFilePath = saveFilePath1 + int.Parse(version.text.ToString())+".json";
            File.WriteAllText(saveFilePath, data);
            Debug.Log("Write to "+saveFilePath);
        
    }
    
    public void  OnLoadData()
    {
        string saveFilePath = saveFilePath1 + int.Parse(version.text.ToString())+".json";
         if (File.Exists(saveFilePath))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath);
            boxData = JsonUtility.FromJson<BoxData>(loadPlayerData);
            CreateDirection(boxData);
        }
    }

    void CreateDirection(BoxData boxData)
    {
        foreach(BoxInfo bif in boxData.layer1)
        {
            Background background = gridLayer.backgrounds[bif.x,bif.y];

            background.box = createJelly(background, bif.subType);          

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
