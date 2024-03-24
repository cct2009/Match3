using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
     public static Global Instance;
     public static GridLayer gridLayer;
     public static Background[,] backgrounds;
    public  FileData file;
    static public Vector2Int[] dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};
    private void Awake() {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);        
        if (!file.onLoadBoxData(4)) {
            return;
        }
        gridLayer = new GridLayer();
        gridLayer.backgrounds = new Background[file.boxData.rows, file.boxData.columns];
        backgrounds = gridLayer.backgrounds;

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
