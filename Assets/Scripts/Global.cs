using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
     public static Global Instance;
     public GridLayer gridLayer;
    public FileData file;
    private void Awake() {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);        
        if (!file.onLoadBoxData()) {
            Debug.Log("Can't Load Data File");
            return;
        }
        gridLayer = new GridLayer();
        gridLayer.backgrounds = new Background[file.boxData.rows, file.boxData.columns];
        gridLayer.maxX = file.boxData.columns;
        gridLayer.maxY = file.boxData.rows;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
