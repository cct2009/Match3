using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class Global : MonoBehaviour
{
     public static Global Instance;
     public static GridLayer gridLayer;
     public static Background[,] backgrounds;
    public  FileData file;
    public SpriteMask spm;
    public static List<MatchInfo> mifList;
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
        gridLayer.dupList = new List<List<Background>>();
        gridLayer.linesList = new List<List<Background>>();
        

        gridLayer.maxX = file.boxData.columns;
        gridLayer.maxY = file.boxData.rows;
        mifList = new List<MatchInfo>();

    }
    public static bool ValidPos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0) return false;
        if (pos.x >= gridLayer.maxX || pos.y >= gridLayer.maxY) return false;
        return true;
    }

    public static void ResetFromTo()
    {
        for (int y=0; y < gridLayer.maxY; y++)
        {
            for (int x =0; x < gridLayer.maxX; x++)
            {
                Background background = backgrounds[x,y];
                background.moveFrom = null;
                background.moveTo = null;
            }
        }
    }
    private static List<Background> GetLines(Background enterPoint)
    {   // get list of backgrounds in the same line from enterPoint to exitPoint
        List<Background> lines = new List<Background>();
        Vector2Int pos = enterPoint.pos;

        while (Global.ValidPos(pos)) {
            lines.Add(enterPoint);
            pos = pos + enterPoint.flow;
            if (Global.ValidPos(pos))
                enterPoint = backgrounds[pos.x,pos.y];

        }
        return lines;
    }
    public static  void createPointList()
    {
        List<List<Background>> allList = new List<List<Background>>();
        List<List<Background>> dupList = Global.gridLayer.dupList;
        List<List<Background>> linesList = Global.gridLayer.linesList;
        foreach (Background background in gridLayer.enterPoints)
        {
            List<Background> lines;
            lines = GetLines(background);
            allList.Add(lines);
        }

        // 
        for (int i = 0; i < allList.Count; i++)
        {
            for (int j = i + 1; j < allList.Count; j++)
            {
                var result = allList[i].Where(x => allList[j].Contains(x));
                if (result.Count() > 0)
                {
                    if (!dupList.Contains(allList[i])) dupList.Add(allList[i]);
                    if (!dupList.Contains(allList[j])) dupList.Add(allList[j]);
                }
            }
        }
        for (int i = 0; i < allList.Count; i++)
        {
            if (dupList.Contains(allList[i])) continue;
            linesList.Add(allList[i]);
        }
        foreach (List<Background> list in dupList)
        {
            list.Reverse(); // so run from exit point to enter point
        }
        foreach (List<Background> list in linesList)
        {
            list.Reverse();
        }
    }

}
