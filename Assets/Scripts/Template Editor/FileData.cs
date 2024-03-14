using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
[System.Serializable]
public struct SubTypeSprite
{
    public BoxSubType subType;
    public Sprite sr;
}
public class FileData : MonoBehaviour
{
    public BoxData boxData;
    public SubTypeSprite[] subTypeSprite;
    private string saveFilePath;
    private string directionFilePath1;

    private void Awake() {
        saveFilePath = Application.persistentDataPath + "/BoxData.json";    
        directionFilePath1 = Application.persistentDataPath + "/direction";
    }
    public  bool  OnLoadData()
    {
        
         if (File.Exists(saveFilePath))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath);
            boxData = JsonUtility.FromJson<BoxData>(loadPlayerData);
            return true;
        }
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
    public Sprite GetSprite(BoxSubType subType)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == subType)
                return ss.sr;
        }
        return null;
    }
}
