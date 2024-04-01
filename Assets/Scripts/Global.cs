using UnityEngine;
using System.Collections.Generic;

public class Global : MonoBehaviour
{
     public static Global Instance;
     public static GridLayer gridLayer;
     public static Background[,] backgrounds;
    public  FileData file;
    public SpriteMask spm;
    static public Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};
    private void Awake() {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);        
        if (!file.onLoadBoxData(1)) {
            return;
        }
        gridLayer = new GridLayer();
        gridLayer.backgrounds = new Background[file.boxData.rows, file.boxData.columns];
        backgrounds = gridLayer.backgrounds;
        gridLayer.enterPoints = new List<Background>();
        gridLayer.exitPoints = new List<Background>();

        gridLayer.maxX = file.boxData.columns;
        gridLayer.maxY = file.boxData.rows;

    }
    public static bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }

}
