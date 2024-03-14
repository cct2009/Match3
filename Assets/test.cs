using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;


public class test : MonoBehaviour
{
    public SpriteRenderer panel;


    private void Start() {
        float wWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect ;
        Debug.Log("wWidth="+wWidth);
        panel.size = new Vector2(5,5);
        
    }
 

}
