using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public enum ProgramState 
{
    Animation = 0,
    Input = 1,
    InitData = 2,

}
[System.Serializable]
public struct GoalData
{
    public BoxType boxType;
    public int times;
}

public struct GridLayer
{
    public int maxX, maxY;
    public Background[,] backgrounds;
}
public class Main : MonoBehaviour
{

    public static Main Instance;
    public MatchPanel matchPanel;
    public Match3 match3;
  
    public Global global;
    public LineRenderer lr1, lr2;
    public ProgramState programState;
    private GridLayer gridLayer;
    private Background[,] backgrounds;
    public int DirectionVersion=1;
    public TMP_InputField level;
    public float flowSpeed = 0.06f;
    public float dieSpeed = 0.5f;
    public TMP_Text times;
    public TMP_Text goalTimes;
    public ParticleSystem  part;
    public GameObject animate;
    
    public GoalData[] goals;

    private void Awake() {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);        
        gridLayer = Global.gridLayer;
        backgrounds = gridLayer.backgrounds;

        

    }
    void Start()
    {

        matchPanel.CreateGrid(gridLayer);

        match3.Init(Global.Instance.file);
        match3.DrawBox();
        match3.LoadDirection(DirectionVersion);
        DrawDynamicBorder();

        programState = ProgramState.InitData;
        
    }
    public void onLoadLevel()
    {
        if (Global.Instance.file.onLoadBoxData(int.Parse(level.text)))
        {
            matchPanel.CreateGrid(gridLayer);
            match3.DrawBox();
            match3.LoadDirection(DirectionVersion);
            DrawDynamicBorder();
              programState = ProgramState.InitData;
        }
        
        
    }
    // Update is called once per frame
    void Update()
    {

        switch( programState)
        {
            case ProgramState.InitData:  
                programState = ProgramState.Input;
            break;
            case ProgramState.Input: ManageInput();
            break;
        }
            
        
    }

    Background background;
    Vector2 pos1,pos2,swipe;
    Vector2Int swipeDirection;
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
                 //   Debug.Log("Hit "+background.pos+": Data "+gridLayer.backgrounds[background.pos.x, background.pos.y] );

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
                programState = ProgramState.Animation;
            //    Debug.Log(background.jelly);
                if (background)
                StartCoroutine(background.SwitchItem(swipeDirection));
            }

        }
        else
        {
            //show box can swipe to match
            // background = backgrounds[0,0];
            // StartCoroutine(background.ShowCanMatch());
        }
    }
    public void AddTimes(int added)
    {
        int tm = int.Parse(times.text);
        tm += added;
        if (tm < 0) tm = 0;
        times.text = tm.ToString();
    }

    public void AddGoalTimes(int added)
    {
        int tm = int.Parse(goalTimes.text);
        tm += added;
        if (tm < 0) tm = 0;
        goalTimes.text = tm.ToString();

    }
    private void DrawDynamicBorder()
    {
        lr1.sortingOrder  = 4;
        lr1.startWidth = 0.02f;
        lr1.endWidth = 0.02f;
        lr1.positionCount = 0;

        if (lr1.transform.childCount > 0)
        {
            foreach (Transform gm in lr1.transform.GetComponentsInChildren<Transform>())
            {
                if (gm != lr1.transform)
                    Destroy(gm.gameObject);
            }
                
        }

       for (int y = 0; y < gridLayer.maxY;  y++)
       {
            for (int x = 0; x < gridLayer.maxX ; x++)
            {
                Background background = backgrounds[x,y];

                if (background.type != EBackgroundType.Close)
                {
                    WriteBorder(x,y);
                }
            }
       }
    }
    private void WriteBorder(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x,y);
        Background background = backgrounds[x,y];
        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();

        Debug.Log("Write Border "+pos+"["+background.type+"]");
        if (!ValidBorder(pos, Vector2Int.left)) {
            Vector3 point1 = new Vector3(background.transform.position.x - sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0);
            Vector3 point2 = new Vector3(background.transform.position.x - sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0);
            DrawLine(pos,point1, point2);
            
        }

        if (!ValidBorder(pos, Vector2Int.down)) {
            Vector3 point1 = new Vector3(background.transform.position.x - sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0);
            Vector3 point2 = new Vector3(background.transform.position.x + sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0);
            DrawLine(pos,point1, point2);
            
        }

        if (!ValidBorder(pos, Vector2Int.right)) {
            Vector3 point1 = new Vector3(background.transform.position.x + sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0);
            Vector3 point2 = new Vector3(background.transform.position.x+ sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0);
            DrawLine(pos,point1, point2);
            
        }

        if (!ValidBorder(pos, Vector2Int.up)) {
            Vector3 point1 = new Vector3(background.transform.position.x + sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0);
            Vector3 point2 = new Vector3(background.transform.position.x- sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0);
            DrawLine(pos,point1, point2);
            
        }
        
    }

    private bool ValidBorder(Vector2Int pos, Vector2Int dir)
    {
        if (!Global.ValidPos(pos+dir)) return false;
        Vector2Int pos1 = pos+dir;
        Background background = backgrounds[pos1.x,pos1.y];
        return background.type != EBackgroundType.Close;
    }

    private void DrawLine(Vector2Int pos,Vector3 point1, Vector3 point2)
    {
        lr2.sortingOrder  = 4;
        lr2.startWidth = 0.02f;
        lr2.endWidth = 0.02f;
        lr2.positionCount = 2;
        LineRenderer lr = Instantiate(lr2,lr1.transform);
        lr.name = "LR " + pos;
        lr.positionCount = 2;
        lr.SetPosition(0,point1);
        lr.SetPosition(1,point2);

    }
    private void DrawBorder()
    {
        Background background;
        lr1.sortingOrder  = 4;
        lr1.startWidth = 0.02f;
        lr1.endWidth = 0.02f;
        lr1.positionCount = 0;
        Vector2Int start = new Vector2Int(0,0);
        
        background = backgrounds[start.x,start.y];
        Vector2Int nextPos = start;
        while (Global.ValidPos(nextPos))
        {
            DrawBorderBottom(nextPos);
            nextPos = nextPos + Vector2Int.right;
        }
        nextPos = nextPos - Vector2Int.right;
        while (Global.ValidPos(nextPos))
        {
            DrawBorderRight(nextPos);
            nextPos = nextPos + Vector2Int.up;
        }
        nextPos = nextPos - Vector2Int.up ;
        while (Global.ValidPos(nextPos))
        {
            DrawBorderTop(nextPos);
            nextPos = nextPos + Vector2Int.left;
        }
        nextPos = nextPos - Vector2Int.left ;
        while (Global.ValidPos(nextPos))
        {
            DrawBorderLeft(nextPos);
            nextPos = nextPos + Vector2Int.down;
        }
        
    }

    private  void DrawBorderLeft(Vector2Int pos)
    {
        Background background = backgrounds[pos.x,pos.y];

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        
        if (!Global.ValidPos(pos+Vector2Int.left)) {
            if (lr1.positionCount == 0)
            {
                lr1.positionCount = 1;
                lr1.SetPosition(0, new Vector3(background.transform.position.x - sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0));
            }
            
            lr1.positionCount++;
            lr1.SetPosition(lr1.positionCount-1, new Vector3(background.transform.position.x- sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0));

        }
    }
    private  void DrawBorderBottom(Vector2Int pos)
    {
        Background background = backgrounds[pos.x,pos.y];

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        if (!Global.ValidPos(pos+Vector2Int.down)) {
            if (lr1.positionCount == 0)
            {
                lr1.positionCount = 1;
                lr1.SetPosition(0, new Vector3(background.transform.position.x - sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0));
            }
            
            lr1.positionCount++;
            
            lr1.SetPosition(lr1.positionCount-1, new Vector3(background.transform.position.x+ sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0));

        }
    }
    
    private  void DrawBorderRight(Vector2Int pos)
    {
        Background background = backgrounds[pos.x,pos.y];

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        if (!Global.ValidPos(pos+Vector2Int.right)) {
            if (lr1.positionCount == 0)
            {
                lr1.positionCount = 1;
                lr1.SetPosition(0, new Vector3(background.transform.position.x + sr.bounds.size.x/2f, background.transform.position.y-sr.bounds.size.y/2f,0));
            }
            
            lr1.positionCount++;
            
            lr1.SetPosition(lr1.positionCount-1, new Vector3(background.transform.position.x+ sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0));

        }
    }
    private  void DrawBorderTop(Vector2Int pos)
    {
        Background background = backgrounds[pos.x,pos.y];

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
         if (!Global.ValidPos(pos+Vector2Int.up)) {
            if (lr1.positionCount == 0)
            {
                lr1.positionCount = 1;
                lr1.SetPosition(0, new Vector3(background.transform.position.x + sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0));
            }
            
            lr1.positionCount++;
            
            lr1.SetPosition(lr1.positionCount-1, new Vector3(background.transform.position.x- sr.bounds.size.x/2f, background.transform.position.y+sr.bounds.size.y/2f,0));

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
