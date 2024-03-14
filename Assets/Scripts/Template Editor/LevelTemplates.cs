using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using Unity.Collections;


public class LevelTemplates : MonoBehaviour
{
    public TMP_InputField tColumns;
    public TMP_InputField tRows;
    
    public BoxManager box;
    public SpriteRenderer panel;

    public RectTransform PanelJelly;
    public TMP_Dropdown dropdown;
    public BoxData boxData;
    public SubTypeSprite[] subTypeSprite;

     string saveFilePath;

    

    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/BoxData.json";        
        ChangeBoxSize();
        onSelectDropDown();
    }
    public void ChangeBoxSize()
    {
        int rows = Convert.ToInt32(tRows.text);
        int columns = Convert.ToInt32(tColumns.text);
        CreateGrid(rows,columns, panel, box);
    }

    private void CreateGrid(int rows, int columns,SpriteRenderer Panel,BoxManager  box)
    {
         SpriteRenderer  sr = box.GetComponent<SpriteRenderer>();
         BoxCollider2D rg = box.GetComponent<BoxCollider2D>();
         
         box.subType = (int) BoxSubType.JellyRandom;
        // delete all child with name SR ...
        foreach(SpriteRenderer r in Panel.GetComponentsInChildren<SpriteRenderer>())
            if (r.name.IndexOf("BOX") >= 0)
                Destroy(r.gameObject);

        float width = Panel.bounds.size.x / columns;
        float height = Panel.bounds.size.y;
        Debug.Log(rows.ToString()+","+columns.ToString());

        sr.size = new Vector2(width, width);
        rg.size = sr.size;

        width = width * columns;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                
                SpriteRenderer sr1 = Instantiate(sr,  new Vector2(-width/2 + sr.bounds.size.x * x + sr.bounds.size.x/2,
                    -height/2+ sr.bounds.size.y*y + sr.bounds.size.y/2),Quaternion.identity, panel.transform);
                sr1.name = "BOX1 " + (y.ToString()) + "," + x.ToString();

            }
    
    }
    public void onSelectDropDown()
    {
        RectTransform rect = dropdown.GetComponent<RectTransform>();
        if (dropdown.value == 0)
            PanelJelly.localPosition =  new Vector3(rect.localPosition.x,rect.localPosition.y-50,0);
    }

    public void onSave()
    {
        BoxData boxData = new BoxData();

            boxData.rows = Convert.ToInt32(tRows.text);;
            boxData.columns = Convert.ToInt32(tColumns.text);
            boxData.layer1 = new List<BoxInfo>();
            boxData.layer2 = new List<BoxInfo>();
            boxData.layer3 = new List<BoxInfo>();
            for (int y=0; y < boxData.rows;  y++)
                for (int x = 0; x < boxData.columns; x++)
                {
                    BoxInfo bf = new BoxInfo();
                    string name = "BOX1 "+(y.ToString()) + "," + x.ToString();
                    BoxManager box = GameObject.Find(name).GetComponent<BoxManager>();
                    if (box != null && box.subType != (int) BoxSubType.JellyRandom)
                    {
                        bf.subType = (BoxSubType) box.subType;
                        bf.x = x;
                        bf.y = y;
                        boxData.layer1.Add(bf);
                    }
                }
            for (int y=0; y < boxData.rows;  y++)
                for (int x = 0; x < boxData.columns; x++)
                {
                    BoxInfo bf = new BoxInfo();
                    string name = "BOX2 "+(y.ToString()) + "," + x.ToString();
                    GameObject gm = GameObject.Find(name);
                    
                    if (gm != null)
                    {
                        box = gm.GetComponent<BoxManager>();
                        bf.subType = (BoxSubType) box.subType;
                        bf.x = x;
                        bf.y = y;
                        boxData.layer2.Add(bf);
                    }
                }            
            for (int y=0; y < boxData.rows;  y++)
                for (int x = 0; x < boxData.columns; x++)
                {
                    BoxInfo bf = new BoxInfo();
                    string name = "BOX3 "+(y.ToString()) + "," + x.ToString();
                    GameObject gm = GameObject.Find(name);
                    
                    if (gm != null)
                    {
                        box = gm.GetComponent<BoxManager>();
                        bf.subType = (BoxSubType) box.subType;
                        bf.x = x;
                        bf.y = y;
                        boxData.layer3.Add(bf);
                    }
                }            
            
            SaveData(boxData);
        
    }
    public void SaveData(BoxData boxData)
    {
        string  data = JsonUtility.ToJson(boxData);
       

        File.WriteAllText(saveFilePath, data);
        Debug.Log("Save file created at: " + saveFilePath);
        
    }
    
    void OnLoadData()
    {
        StartCoroutine(OnLoadDataCoroutine());
    }
    IEnumerator  OnLoadDataCoroutine()
    {
         if (File.Exists(saveFilePath))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath);
            boxData = JsonUtility.FromJson<BoxData>(loadPlayerData);

            CreateGrid(boxData.rows, boxData.columns, panel, box);
            yield return null;
            LoadGrid();
        }
    }

    void LoadGrid()
    {
                    
            foreach (BoxInfo bif in boxData.layer1)
            {
                string name = "BOX1 " +(bif.y.ToString()) + "," + bif.x.ToString();
                GameObject gm1 = GameObject.Find(name);
                if (gm1)
                {
                    
                    BoxManager box1 = gm1.GetComponent<BoxManager>();
                    SpriteRenderer sr1 = gm1.GetComponent<SpriteRenderer>();
                    box1.subType = (int) bif.subType;
                    sr1.sprite = GetSprite(bif.subType);
  
                }
            }
            foreach (BoxInfo bif in boxData.layer2)
            {
                string name = "BOX1 " +(bif.y.ToString()) + "," + bif.x.ToString();
                GameObject gm1 = GameObject.Find(name);
                if (gm1)
                {
                    string name2 = "BOX2 " +(bif.y.ToString()) + "," + bif.x.ToString();
                    GameObject gm2 = Instantiate(gm1, gm1.transform.position, gm1.transform.rotation, gm1.transform.parent);

                    BoxManager box2 = gm2.GetComponent<BoxManager>();
                    SpriteRenderer sr2 = gm2.GetComponent<SpriteRenderer>();
                    box2.subType = (int) bif.subType;
                    sr2.sprite = GetSprite(bif.subType);
                    sr2.sortingOrder = 2;
                    gm2.name = name2;
                }
            }

    }

    Sprite GetSprite(BoxSubType subType)
    {
        foreach(SubTypeSprite ss in subTypeSprite)
        {
            if (ss.subType == subType)
                return ss.sr;
        }
        return null;
    }
}
