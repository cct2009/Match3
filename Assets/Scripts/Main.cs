using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public enum ProgramState 
{
    Animation = 0,
    Input = 1,
    InitData = 2,

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
    public Match3 match32;
    public FileData file;
    public LineRenderer lr1, lr2;
    public ProgramState programState;
    public GridLayer gridLayer;
    public int DirectionVersion=1;
    
    private void Awake() {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);        
        if (!file.OnLoadData()) {
            Debug.Log("Can't Load Data File");
            return;
        }
        gridLayer = new GridLayer();
        gridLayer.backgrounds = new Background[file.boxData.rows, file.boxData.columns];
        gridLayer.maxX = file.boxData.columns;
        gridLayer.maxY = file.boxData.rows;

    }
    void Start()
    {
        matchPanel.CreateGrid(gridLayer);

        match32.Init(file);
        match32.LoadJelly();
        match32.LoadDirection(DirectionVersion);
        
        programState = ProgramState.InitData;
        
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
                    Debug.Log("Hit "+background.pos+": Data "+gridLayer.backgrounds[background.pos.x, background.pos.y] );

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
                Debug.Log(background.jelly);
                StartCoroutine(background.SwitchItem(swipeDirection));
                    


            }

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
