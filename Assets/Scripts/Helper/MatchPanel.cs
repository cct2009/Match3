using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class MatchPanel : MonoBehaviour
{
    public SpriteRenderer panel;
    public Sprite square1;
    public Sprite square2;
    public Background PrefabBackground;
    public int percentWidth=90;
    private Vector2 lastScreenSize;
    private void Start() {
    }
//  void Update() {
//         Vector2 screenSize = new Vector2(Screen.width, Screen.height); 

//         if (this.lastScreenSize != screenSize) {
//             this.lastScreenSize = screenSize;
//             OnScreenSizeChange(Screen.width, Screen.height);                        //  Launch the event when the screen size change

//         }
//     }

    void OnScreenSizeChange(float width, float height)
    {
        float wWidth, wHeight;
        GridLayer gridLayer;

        wWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect ;
        wHeight = Camera.main.orthographicSize * 2;
        Debug.Log("Resolution - Width:"+width+ " Height:"+height);
        Debug.Log("World - Width:"+ wWidth + " Height:"+ wHeight);

        // update panel size by World width change
        gridLayer = new GridLayer();
        gridLayer.maxX = 9; gridLayer.maxY = 10;
        gridLayer.backgrounds = new Background[gridLayer.maxX, gridLayer.maxY];
        
        CreateGrid(gridLayer);

    }

    public void CreateGrid( GridLayer gridLayer)
    {
        float wWidth;
        int rows = gridLayer.maxY;
        int cols = gridLayer.maxX;

        wWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect ;
        float panelWidth = percentWidth / 100f * wWidth;
        float width = panelWidth;
        float width1 = width / cols;
        float height = width1 * rows;
        float height1 = height / rows;
        panel.size = new Vector2(panelWidth,height);

        Sprite square;

        panel.color = new Color(panel.color.r, panel.color.g,panel.color.b, 0);
        // delete all child of panel
        if (panel.transform.childCount > 0)
        {
            foreach (Transform gm in panel.transform.GetComponentsInChildren<Transform>())
            {
                if (gm != panel.transform)
                    Destroy(gm.gameObject);
            }
                
        }
        for (int y = 0 ; y < rows; y++)
        {
            if (y % 2 == 0) square = square1;
            else
                square = square2;
            for (int x = 0; x < cols; x++)
            {
            
                Vector3 pos = new Vector3(-width/2 + x * width1 + width1/2,-height/2+ y*height1 +height1/2,0);
                Background background = Instantiate(PrefabBackground, pos, Quaternion.identity, panel.transform);
                
                background.name = "BG "+ x +","+y;
                background.pos.x = x; background.pos.y = y;
                gridLayer.backgrounds[x,y] = background;
                background.transform.localScale = new Vector3(width1,width1,width1);
                background.type = EBackgroundType.Vacant;
                
                SpriteRenderer  sr1 = background.GetComponent<SpriteRenderer>();
                
                sr1.sprite = square;  
                if (square == square1) 
                    square = square2;
                else
                    square = square1;

                BoxCollider2D box = background.GetComponent<BoxCollider2D>();
                box.size = new Vector2(1,1);
            }
        }

    }
}
