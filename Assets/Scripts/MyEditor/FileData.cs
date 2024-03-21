using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
[System.Serializable]
public struct SubTypeSprite
{
    public BoxType subType;
    public Sprite[] pic;
    public bool move;
    public Sprite sr;
    public GameObject DieAnimate;

}
public class FileData : MonoBehaviour
{
    public BoxData boxData;
    public SubTypeSprite[] subTypeSprite;
    private string saveFilePath;
    private string directionFilePath1;

    private void Awake() {
        saveFilePath = Application.persistentDataPath + "/level";    
        directionFilePath1 = Application.persistentDataPath + "/direction";
    }
    public  bool  onLoadBoxData(int level)
    {
        string saveFilePath1 = saveFilePath + level + ".json";
         if (File.Exists(saveFilePath1))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath1);
            boxData = JsonUtility.FromJson<BoxData>(loadPlayerData);
            return true;
        }
        Debug.Log("Can't Load BoxData "+ saveFilePath1);
        return false;
    }

    public bool onLoadDirection(int version)
    {
        string directionFilePath = directionFilePath1 + version.ToString() + ".json";
        if (File.Exists(directionFilePath))
        {
            string data = File.ReadAllText(directionFilePath);
            boxData = JsonUtility.FromJson<BoxData>(data);
            return true;
        }
        return false;
    }
    public Sprite GetSprite(BoxType type)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == type)
                return (ss.pic.Length < 1? null : ss.pic[0]);
        }
        return null;
    }
    public Sprite GetSpriteForLive(Box box)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == box.type)
            {
                return ss.pic[ss.pic.Length - box.live];
            }
        }
        return null;
    }
    public bool GetMove(BoxType type)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == type)
                return ss.move;
        }
        return true;
    }
    public int GetLive(BoxType type)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == type)
                return ss.pic.Length;
        }
        return 1;
    }
    public GameObject GetDieAnimate(BoxType type)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == type)
                return (ss.DieAnimate == null? GetDieAnimate(BoxType.JellyBlue): ss.DieAnimate);
        }
        return null;
    }
}
